using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Assets;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7938)]
class AbilityScoreSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_STATS";

    private static readonly string[] AttributeIdSuffixes =
    {
        "str", "dex", "con", "int", "wis", "cha"
    };

    public ChargenStage Stage => ChargenStage.Stats;

    [TempleDllLocation(0x10c44c50)]
    private readonly int[] _charGenRolledStats = new int[6];

    private readonly AbilityScoreValueWidget[] _charGenRolledStatsWidgets = new AbilityScoreValueWidget[6];

    public WidgetContainer Container { get; }

    [TempleDllLocation(0x10C44C48)]
    private readonly WidgetButton _togglePointBuyButton;

    [TempleDllLocation(0x10C45310)]
    private readonly WidgetButton[] _increaseButtons;

    [TempleDllLocation(0x10C44DA8)]
    private readonly WidgetButton[] _decreaseButtons;

    [TempleDllLocation(0x10C45460)]
    private readonly WidgetButton _rerollButton;

    private readonly WidgetText _rerollsLabel;

    private CharEditorSelectionPacket _pkt;

    private readonly WidgetText _titleLabel;

    private readonly WidgetText _draggedAbilityScoreLabel;

    private readonly WidgetContainer _pointBuyInfo;

    private readonly WidgetText _pointBuyPointsAvailable;

    [TempleDllLocation(0x10C453F4)]
    private int _pointBuyPoints;

    [TempleDllLocation(0x1018c740)]
    public AbilityScoreSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/stats_ui.json");
        Container = doc.GetRootContainer();

        _pointBuyInfo = doc.GetContainer("pointBuyInfo");
        _pointBuyPointsAvailable = doc.GetTextContent("pointBuyPointsAvailable");

        _titleLabel = doc.GetTextContent("title");
        _rerollButton = doc.GetButton("reroll");
        _rerollButton.AddClickListener(RerollStats);
        _rerollsLabel = doc.GetTextContent("rerollsLabel");

        _togglePointBuyButton = doc.GetButton("togglePointBuy");
        _togglePointBuyButton.AddClickListener(TogglePointBuy);
        _increaseButtons = new WidgetButton[6];
        _decreaseButtons = new WidgetButton[6];
        for (var i = 0; i < 6; i++)
        {
            var abilityIndex = i;
            _increaseButtons[i] = doc.GetButton($"increase-{AttributeIdSuffixes[i]}");
            _increaseButtons[i].AddClickListener(() => IncreaseStat(abilityIndex));

            _decreaseButtons[i] = doc.GetButton($"decrease-{AttributeIdSuffixes[i]}");
            _decreaseButtons[i].AddClickListener(() => DecreaseStat(abilityIndex));

            var assignedValContainer = doc.GetContainer($"assigned-val-{AttributeIdSuffixes[i]}");
            var assignedVal = new AbilityScoreValueWidget(
                () => _pkt.abilityStats[abilityIndex],
                value => _pkt.abilityStats[abilityIndex] = value,
                true
            )
            {
                PixelSize = assignedValContainer.GetSize()
            };
            assignedValContainer.Add(assignedVal);
            InstallAbilityScoreBehavior(assignedVal);

            // Displays the modifier for the assigned attribute
            var assignedModContainer = doc.GetContainer($"assigned-mod-{AttributeIdSuffixes[i]}");
            var assignedMod = new AbilityScoreModifierWidget(
                assignedValContainer.GetSize(),
                () => _pkt.abilityStats[abilityIndex]
            );
            assignedModContainer.Add(assignedMod);
        }

        // This label is used to draw the ability score currently being dragged
        _draggedAbilityScoreLabel = new WidgetText("", "charGenAssignedStat");

        for (var i = 0; i < 6; i++)
        {
            var index = i;
            var rolledStatContainer = doc.GetContainer("rolledAttribute" + i);
            var rolledStatWidget = new AbilityScoreValueWidget(
                () => _charGenRolledStats[index],
                value => _charGenRolledStats[index] = value,
                false
            )
            {
                PixelSize = rolledStatContainer.GetSize()
            };
            _charGenRolledStatsWidgets[i] = rolledStatWidget;
            rolledStatContainer.Add(rolledStatWidget);
            InstallAbilityScoreBehavior(rolledStatWidget);
        }
    }

    private void InstallAbilityScoreBehavior(AbilityScoreValueWidget widget)
    {
        widget.OnMouseDown += e =>
        {
            if (widget.Value == -1)
            {
                // Do not allow interaction with unassigned ability scores
                return;
            }

            // Allow quickly swapping values between the two columns, but only when we actually have rolled values
            // (not in point buy mode)
            if (!_pkt.isPointbuy && e.Button == MouseButton.Right)
            {
                var destinationPool = widget.IsAssigned ? _charGenRolledStats : _pkt.abilityStats;
                for (var i = 0; i < destinationPool.Length; i++)
                {
                    if (destinationPool[i] == -1)
                    {
                        destinationPool[i] = widget.Value;
                        widget.Value = -1;
                        OnAbilityScoresChanged();
                        return;
                    }
                }
            }
            else if (e.Button == MouseButton.Left && widget.SetMouseCapture())
            {
                // Figure out where in the widget we got clicked so we can draw the dragged text with the proper offset
                var globalContentArea = widget.GetContentArea(true);
                var localX = (int) (e.X - globalContentArea.X);
                var localY = (int) (e.Y - globalContentArea.Y);
                _draggedAbilityScoreLabel.Text = widget.Value.ToString();
                widget.IsDragging = true;

                // This will draw the ability score being dragged under the mouse cursor
                Tig.Mouse.SetCursorDrawCallback((x, y, arg) =>
                {
                    var point = new Point(x, y);
                    point.Offset(-localX, -localY);
                    var contentArea = new RectangleF(point, widget.GetSize());

                    _draggedAbilityScoreLabel.SetBounds(contentArea);
                    _draggedAbilityScoreLabel.Render();
                });
            }
        };

        widget.OnMouseUp += e =>
        {
            if (widget.HasMouseCapture && e.Button == MouseButton.Left)
            {
                Tig.Mouse.SetCursorDrawCallback(null);
                widget.ReleaseMouseCapture();
                widget.IsDragging = false;

                var widgetUnderCursor = Globals.UiManager.PickWidget(e.X, e.Y);
                if (widgetUnderCursor is AbilityScoreValueWidget otherAbilityScoreValue)
                {
                    // Swap the two values
                    var tmp = otherAbilityScoreValue.Value;
                    otherAbilityScoreValue.Value = widget.Value;
                    widget.Value = tmp;

                    OnAbilityScoresChanged();
                }
            }
        };
    }

    private void OnAbilityScoresChanged()
    {
        UpdateButtons();
        UiSystems.PCCreation.ResetSystemsAfter(ChargenStage.Stats);
    }

    [TempleDllLocation(0x1018bcb0)]
    private void RerollStats()
    {
        // Ironman does not allow re-rolling stats
        if (!Globals.GameLib.IsIronmanGame || _pkt.numRerolls == 0)
        {
            _pkt.numRerolls++;
            if (_pkt.numRerolls == 100000)
            {
                for (var i = 0; i < 6; i++)
                {
                    _pkt.abilityStats[i] = -1;
                    _charGenRolledStats[i] = 18;
                }

                _pkt.rerollString = CreateRerollText("pc_creation:10002");
            }
            else if (_pkt.numRerolls > 100000)
            {
                for (var i = 0; i < 6; i++)
                {
                    _pkt.abilityStats[i] = -1;
                    _charGenRolledStats[i] = 3;
                }

                _pkt.rerollString = CreateRerollText("pc_creation:10003");
            }
            else
            {
                if (Globals.GameLib.IsIronmanGame)
                {
                    _rerollButton.Disabled = true;
                    RollIronmanStats();
                }
                else
                {
                    _pkt.rerollString = CreateRerollText("pc_creation:10001", _pkt.numRerolls);
                    RollStats();
                }

                OnAbilityScoresChanged();
            }
        }
    }

    /// <summary>
    /// Re-Rolls the attributes for a normal character.
    /// We use the following rule:
    /// Roll 4d6, discard the lowest.
    /// </summary>
    [TempleDllLocation(0x1018b860)]
    private void RollStats()
    {
        for (var i = 0; i < 6; i++)
        {
            var sum = 0;

            var lowestDiceRoll = 7;
            for (var j = 0; j < 4; j++)
            {
                var roll = Dice.D6.Roll();
                lowestDiceRoll = Math.Min(lowestDiceRoll, roll);
                sum += roll;
            }

            sum -= lowestDiceRoll;

            _pkt.abilityStats[i] = -1;
            _charGenRolledStats[i] = sum;
        }
    }

    [TempleDllLocation(0x1018b860)]
    private void RollIronmanStats()
    {
        int highestAttribute, modifierSum;
        do
        {
            RollStats();

            highestAttribute = _charGenRolledStats.Max();
            modifierSum = _charGenRolledStats.Sum(D20StatSystem.GetModifierForAbilityScore);
        } while (highestAttribute < 14 || modifierSum <= 0);
    }

    [TempleDllLocation(0x1018b940)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp")]
    private void IncreaseStat(int statIndex)
    {
        var abilityLvl = _pkt.abilityStats[statIndex];
        var cost = 1;
        if (abilityLvl >= 16)
            cost = 3;
        else if (abilityLvl >= 14)
            cost = 2;

        if (_pointBuyPoints >= cost && (abilityLvl < 18 || Globals.Config.laxRules))
        {
            _pointBuyPoints -= cost;
            _pkt.abilityStats[statIndex]++;
            OnAbilityScoresChanged();
        }
    }

    [TempleDllLocation(0x1018b9b0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp")]
    private void DecreaseStat(int statIndex)
    {
        var abilityLvl = _pkt.abilityStats[statIndex];
        var cost = 1;
        if (abilityLvl >= 17)
            cost = 3;
        else if (abilityLvl >= 15)
            cost = 2;

        if (_pointBuyPoints < Globals.Config.PointBuyBudget && (abilityLvl > 8 || Globals.Config.laxRules))
        {
            _pointBuyPoints += cost;
            _pkt.abilityStats[statIndex]--;
            OnAbilityScoresChanged();
        }
    }

    [TempleDllLocation(0x1018abc0)]
    public void Reset(CharEditorSelectionPacket pkt)
    {
        _pkt = pkt;
        pkt.numRerolls = 0;
        if (Globals.GameLib.IsIronmanGame)
        {
            pkt.rerollString = CreateRerollText("pc_creation:10004"); // Iron Man
        }
        else
        {
            pkt.rerollString = CreateRerollText("pc_creation:10001", pkt.numRerolls);
        }

        for (var i = 0; i < 6; i++)
        {
            _charGenRolledStats[i] = -1;
            pkt.abilityStats[i] = -1;
        }

        _rerollButton.Disabled = false;
        UiPcCreationStatSetPointbuyState(false);
    }

    [TempleDllLocation(0x1011e2c0)]
    private void UiPcCreationStatSetPointbuyState(bool pointbuyState)
    {
        _pkt.isPointbuy = pointbuyState;
        _titleLabel.Text = _pkt.isPointbuy ? "#{pc_creation:1100}" : "#{pc_creation:1000}";
    }

    [TempleDllLocation(0x1018b680)]
    public void Dispose()
    {
        Container.Dispose();
    }

    [TempleDllLocation(0x1018c720)]
    public void Resize(Size resizeArgs)
    {
    }

    [TempleDllLocation(0x1018b1e0)]
    public void Hide()
    {
        Container.Visible = false;
    }

    [TempleDllLocation(0x1018b910)]
    public void Show()
    {
        Container.Visible = true;
        Container.BringToFront();
        UpdateButtons();
    }

    [TempleDllLocation(0x1018b500)]
    private void TogglePointBuy()
    {
        var isPointbuy = UiSystems.PCCreation.IsPointBuy;
        UiPcCreationStatSetPointbuyState(!isPointbuy);
        if (UiSystems.PCCreation.IsPointBuy)
        {
            _pointBuyPoints = Globals.Config.PointBuyBudget;
            for (int i = 0; i < 6; i++)
            {
                _pkt.abilityStats[i] = 8;
            }
        }
        else
        {
            _pointBuyPoints = 0;
            for (int i = 0; i < 6; i++)
            {
                _pkt.abilityStats[i] = -1;
            }
        }

        OnAbilityScoresChanged();
    }

    [TempleDllLocation(0x1018b570)]
    private void UpdateButtons()
    {
        var pointBuyBudget = Globals.Config.PointBuyBudget;
        var pointBuyText = new ComplexInlineElement();
        pointBuyText.AppendContent(_pointBuyPoints.ToString());
        pointBuyText.AppendContent($"/{pointBuyBudget}", PartyCreationStyles.AccentColor);
        _pointBuyPointsAvailable.Content = pointBuyText;

        var isPointBuyMode = UiSystems.PCCreation.IsPointBuy;

        // hide/show basic/advanced toggle button
        if (Globals.GameLib.IsIronmanGame)
        {
            if (isPointBuyMode)
            {
                TogglePointBuy(); // pointbuy toggle
            }

            _togglePointBuyButton.Visible = false; // hide toggle button
        }
        else
        {
            _togglePointBuyButton.Visible = true; // show toggle button

            _togglePointBuyButton.Text = isPointBuyMode ? "#{pc_creation:10005}" : "#{pc_creation:10006}";
        }

        for (var i = 0; i < 6; i++)
        {
            var abLvl = _pkt.abilityStats[i];

            // increase btn
            {
                var incBtnId = _increaseButtons[i];
                var cost = 1;
                if (abLvl >= 16)
                    cost = 3;
                else if (abLvl >= 14)
                    cost = 2;
                if (_pointBuyPoints < cost || (abLvl == 18 && !Globals.Config.laxRules))
                    incBtnId.Disabled = true;
                else
                    incBtnId.Disabled = false;
                incBtnId.Visible = isPointBuyMode;
            }

            // dec btn
            {
                var decBtnId = _decreaseButtons[i];

                if (_pointBuyPoints >= Globals.Config.PointBuyBudget || (abLvl == 8 && !Globals.Config.laxRules) ||
                    abLvl <= 5)
                    decBtnId.Disabled = true;
                else
                    decBtnId.Disabled = false;
                decBtnId.Visible = isPointBuyMode;
            }
        }

        _pointBuyInfo.Visible = isPointBuyMode;

        foreach (var rolledStatWidget in _charGenRolledStatsWidgets)
        {
            rolledStatWidget.Visible = !isPointBuyMode;
        }

        _rerollButton.Visible = !isPointBuyMode;
        _rerollsLabel.Visible = !isPointBuyMode;
        _rerollsLabel.Content = _pkt.rerollString;
    }

    [TempleDllLocation(0x1018adb0)]
    public bool CheckComplete()
    {
        foreach (var score in _pkt.abilityStats)
        {
            if (score == -1)
            {
                return false;
            }
        }

        return _pointBuyPoints == 0;
    }

    [TempleDllLocation(0x1018acd0)]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        if (playerObj != null)
        {
            GameSystems.Object.Destroy(playerObj);
            playerObj = null;
        }
    }

    [TempleDllLocation(0x1018b2c0)]
    public void ChargenStatsBtnEntered()
    {
        UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
    }

    public bool CompleteForTesting(Dictionary<string, object> props)
    {
        UiPcCreationStatSetPointbuyState(false);
        RerollStats();
        for (var i = 0; i < 6; i++)
        {
            _pkt.abilityStats[i] = _charGenRolledStats[i];
        }

        OnAbilityScoresChanged();
        return true;
    }

    private static InlineElement? CreateRerollText(string labelTranslationId, int count = -1)
    {
        var label = new ComplexInlineElement();
        label.AppendContent(Globals.UiAssets.ApplyTranslation($"#{{{labelTranslationId}}}"), "charGenRerollsLabel");
        if (count >= 0)
        {
            label.AppendContent($" {count:D5}", "charGenRerollsCount");
        }

        return label;
    }
}