using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.PartyCreation
{
    [TempleDllLocation(0x102f7938)]
    internal class StatsSystem : IChargenSystem
    {
        public string Name => "TAG_CHARGEN_STATS";

        private static readonly string[] AttributeIdSuffixes =
        {
            "str", "dex", "con", "int", "wis", "cha"
        };

        public ChargenStages Stage => ChargenStages.CG_Stage_Stats;

        [TempleDllLocation(0x10c44c50)]
        private int[] charGenRolledStats = new int[6];

        private AbilityScoreValueWidget[] charGenRolledStatsWidgets = new AbilityScoreValueWidget[6];

        public WidgetContainer Container { get; private set; }

        [TempleDllLocation(0x10C44C48)]
        private WidgetButton _togglePointBuyButton;

        [TempleDllLocation(0x10C45310)]
        private WidgetButton[] _increaseButtons;

        [TempleDllLocation(0x10C44DA8)]
        private WidgetButton[] _decreaseButtons;

        [TempleDllLocation(0x10C45460)]
        private WidgetButton _rerollButton;

        private WidgetText _rerollsLabel;

        private CharEditorSelectionPacket _pkt;

        private WidgetText _titleLabel;

        private WidgetText _draggedAbilityScoreLabel;

        private WidgetContainer _pointBuyInfo;

        private WidgetText _pointBuyPointsAvailable;

        [TempleDllLocation(0x10C453F4)]
        private int pointBuyPoints;

        [TempleDllLocation(0x1018c740)]
        public StatsSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/stats.json");
            Container = doc.TakeRootContainer();

            _pointBuyInfo = doc.GetContainer("pointBuyInfo");
            _pointBuyPointsAvailable = doc.GetTextContent("pointBuyPointsAvailable");

            _titleLabel = doc.GetTextContent("title");
            _rerollButton = doc.GetButton("reroll");
            _rerollButton.SetClickHandler(RerollStats);
            _rerollsLabel = doc.GetTextContent("rerollsLabel");

            _togglePointBuyButton = doc.GetButton("togglePointBuy");
            _togglePointBuyButton.SetClickHandler(TogglePointBuy);
            _increaseButtons = new WidgetButton[6];
            _decreaseButtons = new WidgetButton[6];
            for (var i = 0; i < 6; i++)
            {
                var abilityIndex = i;
                _increaseButtons[i] = doc.GetButton($"increase-{AttributeIdSuffixes[i]}");
                _increaseButtons[i].SetClickHandler(() => IncreaseStat(abilityIndex));

                _decreaseButtons[i] = doc.GetButton($"decrease-{AttributeIdSuffixes[i]}");
                _decreaseButtons[i].SetClickHandler(() => DecreaseStat(abilityIndex));

                var assignedValContainer = doc.GetContainer($"assigned-val-{AttributeIdSuffixes[i]}");
                var assignedVal = new AbilityScoreValueWidget(
                    assignedValContainer.GetSize(),
                    () => _pkt.abilityStats[abilityIndex],
                    value => _pkt.abilityStats[abilityIndex] = value,
                    true
                );
                assignedValContainer.Add(assignedVal);
                assignedVal.SetMouseMsgHandler(msg => AbilityScoreMouseHandler(msg, assignedVal));

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
                    rolledStatContainer.GetSize(),
                    () => charGenRolledStats[index],
                    value => charGenRolledStats[index] = value,
                    false
                );
                charGenRolledStatsWidgets[i] = rolledStatWidget;
                rolledStatContainer.Add(rolledStatWidget);
                rolledStatWidget.SetMouseMsgHandler(msg => AbilityScoreMouseHandler(msg, rolledStatWidget));
            }
        }

        private bool AbilityScoreMouseHandler(MessageMouseArgs msg, AbilityScoreValueWidget widget)
        {
            if (Globals.UiManager.GetMouseCaptureWidget() == widget)
            {
                if ((msg.flags & MouseEventFlag.LeftReleased) != 0)
                {
                    Tig.Mouse.SetCursorDrawCallback(null);
                    Globals.UiManager.UnsetMouseCaptureWidget(widget);
                    widget.IsDragging = false;

                    var widgetUnderCursor = Globals.UiManager.GetWidgetAt(msg.X, msg.Y);
                    if (widgetUnderCursor is AbilityScoreValueWidget otherAbilityScoreValue)
                    {
                        // Swap the two values
                        var tmp = otherAbilityScoreValue.Value;
                        otherAbilityScoreValue.Value = widget.Value;
                        widget.Value = tmp;

                        OnAbilityScoresChanged();
                    }
                }

                return true;
            }

            if (widget.Value == -1)
            {
                // Do not allow interaction with unassigned ability scores
                return true;
            }

            // Allow quickly swapping values between the two columns, but only when we actually have rolled values
            // (not in point buy mode)
            if (!_pkt.isPointbuy && (msg.flags & MouseEventFlag.RightClick) != 0)
            {
                var destinationPool = widget.IsAssigned ? charGenRolledStats : _pkt.abilityStats;
                for (var i = 0; i < destinationPool.Length; i++)
                {
                    if (destinationPool[i] == -1)
                    {
                        destinationPool[i] = widget.Value;
                        widget.Value = -1;
                        OnAbilityScoresChanged();
                        return true;
                    }
                }
            }
            else if ((msg.flags & MouseEventFlag.LeftDown) != 0)
            {
                if (!Globals.UiManager.SetMouseCaptureWidget(widget))
                {
                    // Something else has the mouse capture right now (how are we getting this message then...?)
                    return true;
                }

                // Figure out where in the widget we got clicked so we can draw the dragged text with the proper offset
                var globalContentArea = widget.GetContentArea(true);
                var localX = msg.X - globalContentArea.X;
                var localY = msg.Y - globalContentArea.Y;
                _draggedAbilityScoreLabel.SetText(widget.Value.ToString());
                widget.IsDragging = true;

                // This will draw the ability score being dragged under the mouse cursor
                Tig.Mouse.SetCursorDrawCallback((x, y, arg) =>
                {
                    var point = new Point(x, y);
                    point.Offset(-localX, -localY);
                    var contentArea = new Rectangle(point, widget.GetSize());

                    _draggedAbilityScoreLabel.SetContentArea(contentArea);
                    _draggedAbilityScoreLabel.Render();
                });
            }

            return true;
        }

        private void OnAbilityScoresChanged()
        {
            UpdateButtons();
            UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Stats);
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
                        charGenRolledStats[i] = 18;
                    }

                    _pkt.rerollString = "@1#{pc_creation:10002}";
                }
                else if (_pkt.numRerolls > 100000)
                {
                    for (var i = 0; i < 6; i++)
                    {
                        _pkt.abilityStats[i] = -1;
                        charGenRolledStats[i] = 3;
                    }

                    _pkt.rerollString = "@1#{pc_creation:10003}";
                }
                else
                {
                    if (Globals.GameLib.IsIronmanGame)
                    {
                        _rerollButton.SetDisabled(true);
                        RollIronmanStats();
                    }
                    else
                    {
                        _pkt.rerollString = $"@0#{{pc_creation:10001}}@1 {_pkt.numRerolls:D5}";
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
                charGenRolledStats[i] = sum;
            }
        }

        [TempleDllLocation(0x1018b860)]
        private void RollIronmanStats()
        {
            int highestAttribute, modifierSum;
            do
            {
                RollStats();

                highestAttribute = charGenRolledStats.Max();
                modifierSum = charGenRolledStats.Sum(D20StatSystem.GetModifierForAbilityScore);
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

            if (pointBuyPoints >= cost && (abilityLvl < 18 || Globals.Config.laxRules))
            {
                pointBuyPoints -= cost;
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

            if (pointBuyPoints < Globals.Config.PointBuyBudget && (abilityLvl > 8 || Globals.Config.laxRules))
            {
                pointBuyPoints += cost;
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
                pkt.rerollString = "@1#{pc_creation:10004}"; // Iron Man
            }
            else
            {
                pkt.rerollString = $"@0#{{pc_creation:10001}}@1 {pkt.numRerolls:D5}";
            }

            for (var i = 0; i < 6; i++)
            {
                charGenRolledStats[i] = -1;
                pkt.abilityStats[i] = -1;
            }

            _rerollButton.SetDisabled(false);
            UiPcCreationStatSetPointbuyState(false);
        }

        [TempleDllLocation(0x1011e2c0)]
        private void UiPcCreationStatSetPointbuyState(bool pointbuyState)
        {
            _pkt.isPointbuy = pointbuyState;
            _titleLabel.SetText(_pkt.isPointbuy ? "#{pc_creation:1100}" : "#{pc_creation:1000}");
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
                pointBuyPoints = Globals.Config.PointBuyBudget;
                for (int i = 0; i < 6; i++)
                {
                    _pkt.abilityStats[i] = 8;
                }
            }
            else
            {
                pointBuyPoints = 0;
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
            _pointBuyPointsAvailable.SetText($"{pointBuyPoints}@1/{pointBuyBudget}");

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

                _togglePointBuyButton.SetText(isPointBuyMode ? "#{pc_creation:10005}" : "#{pc_creation:10006}");
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
                    if (pointBuyPoints < cost || (abLvl == 18 && !Globals.Config.laxRules))
                        incBtnId.SetDisabled(true);
                    else
                        incBtnId.SetDisabled(false);
                    incBtnId.Visible = isPointBuyMode;
                }

                // dec btn
                {
                    var decBtnId = _decreaseButtons[i];
                    var cost = 1;
                    if (abLvl >= 17)
                        cost = 3;
                    else if (abLvl >= 15)
                        cost = 2;

                    if (pointBuyPoints >= Globals.Config.PointBuyBudget || (abLvl == 8 && !Globals.Config.laxRules) ||
                        abLvl <= 5)
                        decBtnId.SetDisabled(true);
                    else
                        decBtnId.SetDisabled(false);
                    decBtnId.Visible = isPointBuyMode;
                }
            }

            _pointBuyInfo.Visible = isPointBuyMode;

            foreach (var rolledStatWidget in charGenRolledStatsWidgets)
            {
                rolledStatWidget.Visible = !isPointBuyMode;
            }

            _rerollButton.Visible = !isPointBuyMode;
            _rerollsLabel.Visible = !isPointBuyMode;
            _rerollsLabel.SetText(_pkt.rerollString);
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

            return pointBuyPoints == 0;
        }

        [TempleDllLocation(0x1018acd0)]
        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody handle)
        {
            if (handle != null)
            {
                GameSystems.Object.Destroy(handle);
                handle = null;
            }
        }

        [TempleDllLocation(0x1018b2c0)]
        public void ChargenStatsBtnEntered()
        {
            UiSystems.PCCreation.UiPcCreationButtonEnteredHandler(Name);
        }
    }

//
// [TempleDllLocation(0x102f7964)]
// internal class RaceSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_RACE";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Race;
//
//         public WidgetContainer Container { get; private set; }
//
// [TempleDllLocation(0x1018a590)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:63")]
// public void   Reset(CharEditorSelectionPacket pkt)
// {
//   pkt.raceId = 7;
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:63
// */
// [TempleDllLocation(0x1018ab30)]
// public void SystemInit()
// {
//   int result;
//
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.flags = 8;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.field2c = -1;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.textColor = &uiPcCreationWhite/*0x102fe4f8*/;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.shadowColor = &uiPcCreationBlack/*0x102fe508*/;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.colors4 = &uiPcCreationWhite/*0x102fe4f8*/;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.colors2 = &uiPcCreationWhite/*0x102fe4f8*/;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.field0 = 0;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.kerning = 1;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.leading = 0;
//   uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/.tracking = 3;
//   if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\racebox.tga", &uiPcCreationRacebox/*0x10c43b74*/) )
//   {
//     result = 0;
//   }
//   else
//   {
//     result = ChargenRaceWidgetsInit/*0x1018a900*/(conf.width, conf.height) != 0;
//   }
//   return result;
// }
// [TempleDllLocation(0x1018a820)]
// public void Dispose()
// {
//   int *v0;
//
//   v0 = uiPcCreationRaceBtnIds/*0x10c43e0c*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v0);
//     ++v0;
//   }
//   while ( (int)v0 < (int)&uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/ );
//   return ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationRaceWndId/*0x10c43e78*/);
// }
// [TempleDllLocation(0x1018aaf0)]
// public void Resize(Size a1)
// {
//   int *v1;
//
//   v1 = uiPcCreationRaceBtnIds/*0x10c43e0c*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v1);
//     ++v1;
//   }
//   while ( (int)v1 < (int)&uiPcCreationRaceStyle_bigBtn/*0x10c43e28*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationRaceWndId/*0x10c43e78*/);
//   return ChargenRaceWidgetsInit/*0x1018a900*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x1018a7b0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:62")]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationRaceWndId/*0x10c43e78*/, 1);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:62
// */
// [TempleDllLocation(0x1018a7d0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:61")]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationRaceWndId/*0x10c43e78*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationRaceWndId/*0x10c43e78*/);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:61
// */
// [TempleDllLocation(0x1018a5a0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:64")]
// public bool CheckComplete()
// {
//   return _pkt.raceId != 7;
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:64
// */
// [TempleDllLocation(0x1018a7f0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:65")]
// public void UiPcCreationRaceUpdateScrollbox()
// {
//   uint "TAG_CHARGEN_RACE";
//
//   if ( _pkt.raceId == 7 )
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_RACE");
//   }
//   else
//   {
//     UiPcCreationRaceSetScrollboxShortHelp/*0x1011bae0*/(_pkt.raceId);
//   }
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:65
// */
// }
//
// [TempleDllLocation(0x102f7990)]
// internal class GenderSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_GENDER";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Gender;
//
//         public WidgetContainer Container { get; private set; }
//
//       [TempleDllLocation(0x10189c70)]
//       public void Reset(CharEditorSelectionPacket a1)
//       {
//         a1.genderId = 2;
//       }
//
// [TempleDllLocation(0x1018a420)]
// public GenderSystem()
// {
//   string v1;
//   string v2;
//   bool v3;
//   string v4;
//   CHAR v5;
//   string v6;
//   string v7;
//   string v8;
//   CHAR v9;
//   int result;
//
//   stru_10C435F0/*0x10c435f0*/.flags = 8;
//   stru_10C435F0/*0x10c435f0*/.field2c = -1;
//   stru_10C435F0/*0x10c435f0*/.textColor = (ColorRect *)&unk_102FE390/*0x102fe390*/;
//   stru_10C435F0/*0x10c435f0*/.shadowColor = (ColorRect *)&unk_102FE3A0/*0x102fe3a0*/;
//   stru_10C435F0/*0x10c435f0*/.colors4 = (ColorRect *)&unk_102FE390/*0x102fe390*/;
//   stru_10C435F0/*0x10c435f0*/.colors2 = (ColorRect *)&unk_102FE390/*0x102fe390*/;
//   stru_10C435F0/*0x10c435f0*/.field0 = 0;
//   stru_10C435F0/*0x10c435f0*/.kerning = 1;
//   stru_10C435F0/*0x10c435f0*/.leading = 0;
//   stru_10C435F0/*0x10c435f0*/.tracking = 3;
//   v1 = GetGenderString/*0x10073a30*/(1);
//   v2 = _strdup(v1);
//   v3 = *v2 == 0;
//   dword_10C431B0/*0x10c431b0*/ = v2;
//   v4 = v2;
//   if ( !v3 )
//   {
//     do
//     {
//       *v4 = toupper_0(*v4);
//       v5 = (v4++)[1];
//     }
//     while ( v5 );
//   }
//   v6 = GetGenderString/*0x10073a30*/(0);
//   v7 = _strdup(v6);
//   v3 = *v7 == 0;
//   dword_10C43184/*0x10c43184*/ = v7;
//   v8 = v7;
//   if ( !v3 )
//   {
//     do
//     {
//       *v8 = toupper_0(*v8);
//       v9 = (v8++)[1];
//     }
//     while ( v9 );
//   }
//   if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\male.tga", &dword_10C431C4/*0x10c431c4*/)
//     || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\female.tga", &dword_10C43354/*0x10c43354*/)
//     || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\buttonbox.tga", &dword_10C431A8/*0x10c431a8*/) )
//   {
//     result = 0;
//   }
//   else
//   {
//     result = sub_1018A070/*0x1018a070*/(*(_DWORD *)(a1 + 4), *(_DWORD *)(a1 + 8)) != 0;
//   }
//   return result;
// }
// [TempleDllLocation(0x1018a030)]
// public void Dispose()
// {
//   ui_widget_remove_regard_parent/*0x101f94d0*/(dword_10C431AC/*0x10c431ac*/);
//   ui_widget_remove_regard_parent/*0x101f94d0*/(dword_10C43180/*0x10c43180*/);
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C43284/*0x10c43284*/);
//   free(dword_10C431B0/*0x10c431b0*/);
//   free(dword_10C43184/*0x10c43184*/);
// }
// [TempleDllLocation(0x1018a550)]
// public void Resize(Size a1)
// {
//   ui_widget_remove_regard_parent/*0x101f94d0*/(dword_10C431AC/*0x10c431ac*/);
//   ui_widget_remove_regard_parent/*0x101f94d0*/(dword_10C43180/*0x10c43180*/);
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C43284/*0x10c43284*/);
//   return sub_1018A070/*0x1018a070*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x10189c80)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(dword_10C43284/*0x10c43284*/, 1);
// }
// [TempleDllLocation(0x10189ca0)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(dword_10C43284/*0x10c43284*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(dword_10C43284/*0x10c43284*/);
// }
// [TempleDllLocation(0x10189cc0)]
// public bool CheckComplete()
// {
//   return _pkt.genderId != 2;
// }
// [TempleDllLocation(0x10189cd0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:68")]
// public void Finalize(CharEditorSelectionPacket selpkt, GameObjectBody handleNew)
// {
//   GameObjectBody v2;
//   Stat v3;
//   int aasHandle;
//   GameObjectBody v5;
//   int unk;
//   aas_anim_state animParams;
//
//   v2 = GameSystems.Proto.GetProtoById(2 * selpkt.raceId - selpkt.genderId + 13001);
//   if ( !GameSystems.MapObject.CreateObject(v2, (locXY)0x1E0000001E0, handleNew) )
//   {
//     Logger.Info("pc_creation.c: FATAL ERROR, could not create player");
//     exit(0);
//   }
//   v3 = 0;
//   do
//   {
//     GameSystems.Stat.SetBasicStat(*handleNew, v3, selpkt.abilityStats[v3]);
//     ++v3;
//   }
//   while ( (int)v3 < 6 );
//   aasHandle = UiSystems.PCCreation.charEditorObjHnd.GetOrCreateAnimHandle();
//   Aas_10262C10/*0x10262c10*/(aasHandle, 1065353216, 0, 0, &animParams, &unk);
//   v5 = *handleNew;
//   if ( selpkt.isPointbuy )
//   {
//     v5.SetInt32(obj_f.pc_roll_count, -25);
//   }
//   else
//   {
//     v5.SetInt32(obj_f.pc_roll_count, selpkt.numRerolls);
//   }
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:68
// */
// [TempleDllLocation(0x10189db0)]
// public void ChargenGenderBtnEntered()
// {
//   uint "TAG_CHARGEN_GENDER";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_GENDER");
// }
// }
//
// [TempleDllLocation(0x102f79bc)]
// internal class HeightSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_HEIGHT";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Height;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10189b60)]
//         public HeightSystem()
//         {
//           int result;
//
//           stru_10C42E30/*0x10c42e30*/.flags = 8;
//           stru_10C42E30/*0x10c42e30*/.field2c = -1;
//           stru_10C42E30/*0x10c42e30*/.textColor = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42E30/*0x10c42e30*/.shadowColor = &stru_102FE1D0/*0x102fe1d0*/;
//           stru_10C42E30/*0x10c42e30*/.colors4 = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42E30/*0x10c42e30*/.colors2 = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42E30/*0x10c42e30*/.field0 = 0;
//           stru_10C42E30/*0x10c42e30*/.kerning = 1;
//           stru_10C42E30/*0x10c42e30*/.leading = 0;
//           stru_10C42E30/*0x10c42e30*/.tracking = 3;
//           stru_10C42DB8/*0x10c42db8*/.flags = 8;
//           stru_10C42DB8/*0x10c42db8*/.field2c = -1;
//           stru_10C42DB8/*0x10c42db8*/.textColor = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42DB8/*0x10c42db8*/.shadowColor = &stru_102FE1D0/*0x102fe1d0*/;
//           stru_10C42DB8/*0x10c42db8*/.colors4 = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42DB8/*0x10c42db8*/.colors2 = &stru_102FE1C0/*0x102fe1c0*/;
//           stru_10C42DB8/*0x10c42db8*/.field0 = 0;
//           stru_10C42DB8/*0x10c42db8*/.kerning = 1;
//           stru_10C42DB8/*0x10c42db8*/.leading = 0;
//           stru_10C42DB8/*0x10c42db8*/.tracking = 3;
//           if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\height_bar.tga", &dword_10C42DB0/*0x10c42db0*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\height_slider.tga", &dword_10C42E20/*0x10c42e20*/) )
//           {
//             result = 0;
//           }
//           else
//           {
//             result = UiPcCreationHeightWidgetsInit/*0x10189a70*/(*(_DWORD *)(a1 + 4), *(_DWORD *)(a1 + 8)) != 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10189520)]
//         public void Dispose()
//         {
//           return ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationHeightWndId/*0x10c43160*/);
//         }
//
//         [TempleDllLocation(0x101892b0)]
// public void ChargenHeightActivate()
// {
//   chargenHeightActivated/*0x10c43178*/ = 1;
// }
//
// [TempleDllLocation(0x101896c0)]
// public void   Reset(CharEditorSelectionPacket pkt)
// {
//   CharEditorSelectionPacket result;
//
//   uiPcCreationHeight_10C42EC0/*0x10c42ec0*/ = 82;
//   uiPcCreationHeightSliderValue/*0x10c42e28*/ = 82;
//   UiPcCreationUpdateCharacterScale/*0x10189530*/();
//   result = pkt;
//   if ( pkt == &_pkt )
//   {
//     chargenHeightActivated/*0x10c43178*/ = 0;
//   }
//   LODWORD(pkt.modelScale) = 0;
//   pkt.height = 0;
//   pkt.weight = 0;
//   return result;
// }
//
// [TempleDllLocation(0x10189c50)]
// public void Resize(Size a1)
// {
//   ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationHeightWndId/*0x10c43160*/);
//   return UiPcCreationHeightWidgetsInit/*0x10189a70*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x101892c0)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationHeightWndId/*0x10c43160*/, 1);
// }
// [TempleDllLocation(0x10189700)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:115")]
// public void Show()
// {
//   int v0;
//   int v1;
//
//   ChargenUpdateMinMaxHeights/*0x10189350*/(
//     minimumHeights/*0x102efce0*/[_pkt.genderId + 2 * _pkt.raceId],
//     maximumHeights/*0x102efd18*/[_pkt.genderId + 2 * _pkt.raceId]);
//   v0 = _pkt.genderId + 2 * _pkt.raceId;
//   v1 = maximumWeights/*0x102efd88*/[v0];
//   uiPcCreationMinWeight/*0x10c42e24*/ = minimumWeights/*0x102efd50*/[v0];
//   uiPcCreationMaxWeight/*0x10c42e08*/ = v1;
//   UiPcCreationUpdateCharacterScale/*0x10189530*/();
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationHeightWndId/*0x10c43160*/, 0);
//   WidgetBringToFront/*0x101f8e40*/(uiPcCreationHeightWndId/*0x10c43160*/);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:115
// */
// [TempleDllLocation(0x101892e0)]
// public bool CheckComplete()
// {
//   return chargenHeightActivated/*0x10c43178*/;
// }
// [TempleDllLocation(0x101892f0)]
// public void Finalize(CharEditorSelectionPacket a1, GameObjectBody a2)
// {
//   *a2.SetInt32(obj_f.critter_height, *(_DWORD *)(a1 + 172));
//   *a2.SetInt32(obj_f.critter_weight, *(_DWORD *)(a1 + 176));
//   return *a2.SetInt32(obj_f.model_scale, (ulong)(*(float *)(a1 + 180) * 100.0));
// }
// [TempleDllLocation(0x10189450)]
// public void ChargenHeightBtnEntered()
// {
//   uint "TAG_CHARGEN_HEIGHT";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_HEIGHT");
// }
// }
//
// [TempleDllLocation(0x102f79e8)]
// internal class HairSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_HAIR";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Hair;
//
//         public WidgetContainer Container { get; private set; }
//
// [TempleDllLocation(0x10188a30)]
// public void   Reset(CharEditorSelectionPacket selPkt)
// {
//   CharEditorSelectionPacket result;
//
//   result = selPkt;
//   selPkt.hair0 = -1;
//   selPkt.hair1 = 0;
//   return result;
// }
// [TempleDllLocation(0x10189240)]
// public HairSystem()
// {
//   int result;
//
//   if ( !MesFuncsOpen/*0x101e6d00*/("rules\\pc_creation_hair.mes", (int *)&dword_10C42D8C/*0x10c42d8c*/)
//     || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\hairswatches.tga", &dword_10C42D88/*0x10c42d88*/) )
//   {
//     result = 0;
//   }
//   else
//   {
//     result = UiPcCreationHairWidgetsInit/*0x10188f10*/(*(_DWORD *)(a1 + 4), *(_DWORD *)(a1 + 8)) != 0;
//   }
// }
// [TempleDllLocation(0x10188ef0)]
// public void Dispose()
// {
//   sub_10188B20/*0x10188b20*/();
//   return MesFuncsClose/*0x101e6360*/(dword_10C42D8C/*0x10c42d8c*/);
// }
// [TempleDllLocation(0x10188ee0)]
// public int ChargenHairActivate()
// {
//   return ChargenHairUpdateBtnTextures/*0x10188bd0*/();
// }
// [TempleDllLocation(0x10189290)]
// public void Resize(Size a1)
// {
//   sub_10188B20/*0x10188b20*/();
//   return UiPcCreationHairWidgetsInit/*0x10188f10*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x10188a50)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationHairWndId/*0x10c42700*/, 1);
// }
// [TempleDllLocation(0x10188a70)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationHairWndId/*0x10c42700*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationHairWndId/*0x10c42700*/);
// }
// [TempleDllLocation(0x10188a90)]
// public bool CheckComplete()
// {
//   return _pkt.hair0 != -1 && _pkt.hair1 != -1;
// }
// [TempleDllLocation(0x10188ab0)]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody a2)
// {
//   int v2;
//
//   v2 = GetHairStyleId/*0x100e17b0*/(selPkt.raceId, selPkt.genderId, selPkt.hair0, selPkt.hair1, 0);
//   *a2.SetInt32(obj_f.critter_hair_style, v2);
//   return GameSystems.Critter.UpdateModelEquipment(*a2);
// }
// [TempleDllLocation(0x10188b00)]
// public void ChargenHairBtnEntered()
// {
//   uint "TAG_CHARGEN_HAIR";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_HAIR");
// }
// }
//
// [TempleDllLocation(0x102f7a14)]
// internal class ClassSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_CLASS";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Class;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10188910)]
//         [TemplePlusLocation("ui_pc_creation_hooks.cpp:131")]
//         public ClassSystem()
//         {
//           int result;
//           int v2;
//           string v3;
//           string v4;
//           CHAR v5;
//
//           if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\buttonbox.tga", &chargenButtonbox/*0x10c4193c*/) )
//           {
//             result = 0;
//           }
//           else
//           {
//             chargenClassBtnTextStyle/*0x10c40ff8*/.flags = 8;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.field2c = -1;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.textColor = &classBtnColorRect/*0x102fde20*/;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.shadowColor = &classBtnShadowColor/*0x102fde30*/;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.colors4 = &classBtnColorRect/*0x102fde20*/;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.colors2 = &classBtnColorRect/*0x102fde20*/;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.field0 = 0;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.kerning = 1;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.leading = 0;
//             chargenClassBtnTextStyle/*0x10c40ff8*/.tracking = 3;
//             v2 = 0;
//             do
//             {
//               v3 = GameSystems.Stat.GetStatName((Stat)(v2 + 7));
//               v4 = _strdup(v3);
//               chargenUpperClassNames/*0x10c41048*/[v2] = v4;
//               if ( *v4 )
//               {
//                 do
//                 {
//                   *v4 = toupper_0(*v4);
//                   v5 = (v4++)[1];
//                 }
//                 while ( v5 );
//               }
//               ++v2;
//             }
//             while ( v2 < 11 );
//             result = ChargenClassWidgetsInit/*0x10188630*/(a1.width, a1.height) != 0;
//           }
//           return result;
//         }
//
//     /* Orphan comments:
//     TP Replaced @ ui_pc_creation_hooks.cpp:131
//     */
//         [TempleDllLocation(0x101885e0)]
//         [TemplePlusLocation("ui_pc_creation_hooks.cpp:132")]
//         public void Dispose()
//         {
//           int *v0;
//           void **v1;
//
//           v0 = uiPcCreationClassBtnIds/*0x10c41aa0*/;
//           do
//           {
//             ui_widget_remove_regard_parent/*0x101f94d0*/(*v0);
//             ++v0;
//           }
//           while ( (int)v0 < (int)&uiPcCreationClassWndId/*0x10c41acc*/ );
//           ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationClassWndId/*0x10c41acc*/);
//           v1 = (void **)chargenUpperClassNames/*0x10c41048*/;
//           do
//           {
//             free(*v1);
//             ++v1;
//           }
//           while ( (int)v1 < (int)&unk_10C41074/*0x10c41074*/ );
//         }
//
//         [TempleDllLocation(0x101b05d0)]
// public void   Reset(CharEditorSelectionPacket selPkt)
// {
//   selPkt.classCode = 0;
// }
//
// [TempleDllLocation(0x101885d0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:133")]
// public void   ChargenClassActivate()
// {
//   UiChargenClassActivate/*0x10188550*/();
// }
//
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:132
// */
// [TempleDllLocation(0x101889f0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:134")]
// public void Resize(Size a1)
// {
//   int *v1;
//
//   v1 = uiPcCreationClassBtnIds/*0x10c41aa0*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v1);
//     ++v1;
//   }
//   while ( (int)v1 < (int)&uiPcCreationClassWndId/*0x10c41acc*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationClassWndId/*0x10c41acc*/);
//   return ChargenClassWidgetsInit/*0x10188630*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:134
// */
// [TempleDllLocation(0x101880d0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:136")]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationClassWndId/*0x10c41acc*/, 1);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:136
// */
// [TempleDllLocation(0x101880f0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:135")]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationClassWndId/*0x10c41acc*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationClassWndId/*0x10c41acc*/);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:135
// */
// [TempleDllLocation(0x101b0620)]
// [TemplePlusLocation("ui_char_editor.cpp:3270")]
// public bool CheckComplete()
// {
//   return _pkt.classCode >= 7 && _pkt.classCode < 18;
// }
// /* Orphan comments:
// TP Replaced @ ui_char_editor.cpp:3270
// */
// [TempleDllLocation(0x10188110)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:137")]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody handle)
// {
//   ClearArrayField/*0x1009e860*/(*handle, obj_f.critter_level_idx);
//   *handle.SetInt32(obj_f.critter_level_idx, 0, selPkt.classCode);
//   GameSystems.D20.Status.D20StatusRefresh(*handle);
//   GameSystems.Critter.GenerateHp(*handle);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:137
// */
// [TempleDllLocation(0x10188260)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:139")]
// public void ChargenClassBtnEntered()
// {
//   uint "TAG_CHARGEN_CLASS";
//
//   if ( _pkt.classCode )
//   {
//     ChargenClassScrollboxTextSet/*0x1011b920*/(_pkt.classCode);
//   }
//   else
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_CLASS");
//   }
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:139
// */
// }
//
// [TempleDllLocation(0x102f7a40)]
// internal class AlignmentSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_ALIGNMENT";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Alignment;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10187f30)]
//         public AlignmentSystem()
//         {
//           int result;
//           int v2;
//           string v3;
//           string v4;
//           CHAR v5;
//           string meslineValue;
//           int meslineKey;
//
//           meslineKey = 16000;
//           if ( !Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline)
//                || (dword_10C409D0/*0x10c409d0*/ = (int)meslineValue,
//                  RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\buttonbox.tga", &dword_10C409D4/*0x10c409d4*/)) )
//           {
//             result = 0;
//           }
//           else
//           {
//             stru_10C40CC0/*0x10c40cc0*/.field2c = -1;
//             stru_10C40FA8/*0x10c40fa8*/.field2c = -1;
//             stru_10C40CC0/*0x10c40cc0*/.flags = 8;
//             stru_10C40CC0/*0x10c40cc0*/.textColor = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40CC0/*0x10c40cc0*/.shadowColor = (ColorRect *)&unk_102FDCB4/*0x102fdcb4*/;
//             stru_10C40CC0/*0x10c40cc0*/.colors4 = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40CC0/*0x10c40cc0*/.colors2 = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40CC0/*0x10c40cc0*/.field0 = 0;
//             stru_10C40CC0/*0x10c40cc0*/.kerning = 1;
//             stru_10C40CC0/*0x10c40cc0*/.leading = 0;
//             stru_10C40CC0/*0x10c40cc0*/.tracking = 3;
//             stru_10C40FA8/*0x10c40fa8*/.flags = 0;
//             stru_10C40FA8/*0x10c40fa8*/.textColor = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40FA8/*0x10c40fa8*/.shadowColor = (ColorRect *)&unk_102FDCB4/*0x102fdcb4*/;
//             stru_10C40FA8/*0x10c40fa8*/.colors4 = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40FA8/*0x10c40fa8*/.colors2 = (ColorRect *)&dword_102FDC94/*0x102fdc94*/;
//             stru_10C40FA8/*0x10c40fa8*/.field0 = 0;
//             stru_10C40FA8/*0x10c40fa8*/.kerning = 1;
//             stru_10C40FA8/*0x10c40fa8*/.leading = 0;
//             stru_10C40FA8/*0x10c40fa8*/.tracking = 3;
//             v2 = 0;
//             do
//             {
//               v3 = GameSystems.Stat.GetAlignmentName(charUiAlignments/*0x102fdc60*/[v2]);
//               v4 = _strdup(v3);
//               *(_DWORD *)&dword_10C40AE8/*0x10c40ae8*/[v2 * 4] = v4;
//               if ( *v4 )
//               {
//                 do
//                 {
//                   *v4 = toupper_0(*v4);
//                   v5 = (v4++)[1];
//                 }
//                 while ( v5 );
//               }
//               ++v2;
//             }
//             while ( v2 < 9 );
//             result = ChargenAlignmentWidgetsInit/*0x10187c80*/(conf.width, conf.height) != 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10187810)]
// public void   Reset(CharEditorSelectionPacket selPkt)
// {
//   selPkt.alignment = -1;
// }
//
// [TempleDllLocation(0x10187c20)]
// public void   ChargenAlignmentActivate()
// {
//   ChargenAlignmentActivateImpl/*0x10187b10*/();
// }
//
// [TempleDllLocation(0x10187c30)]
// public void Dispose()
// {
//   int v0;
//   int v1;
//
//   v0 = 0;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(uiPcCreationAlignmentBtnIds/*0x10c40308*/[v0]);
//     ++v0;
//   }
//   while ( v0 < 9 );
//   ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationAlignmentWndId/*0x10c409cc*/);
//   v1 = 0;
//   do
//   {
//     free(*(void **)&dword_10C40AE8/*0x10c40ae8*/[v1]);
//     v1 += 4;
//   }
//   while ( v1 < 0x24 );
// }
// [TempleDllLocation(0x10188090)]
// public void Resize(Size resizeArgs)
// {
//   int v1;
//
//   v1 = 0;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(uiPcCreationAlignmentBtnIds/*0x10c40308*/[v1]);
//     ++v1;
//   }
//   while ( v1 < 9 );
//   ui_widget_and_window_remove/*0x101f9010*/(uiPcCreationAlignmentWndId/*0x10c409cc*/);
//   return ChargenAlignmentWidgetsInit/*0x10187c80*/(resizeArgs.rect1.width, resizeArgs.rect1.height);
// }
// [TempleDllLocation(0x10187820)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationAlignmentWndId/*0x10c409cc*/, 1);
// }
// [TempleDllLocation(0x10187840)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationAlignmentWndId/*0x10c409cc*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationAlignmentWndId/*0x10c409cc*/);
// }
// [TempleDllLocation(0x10187860)]
// public bool CheckComplete()
// {
//   return _pkt.alignment != -1;
// }
// [TempleDllLocation(0x10187870)]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody handle)
// {
//   *handle.SetInt32(obj_f.critter_alignment, selPkt.alignment);
// }
// [TempleDllLocation(0x101878a0)]
// public void   ChargenAlignmentButtnEntered()
// {
//   int v0;
//   uint "TAG_CHARGEN_ALIGNMENT";
//
//   if ( _pkt.alignment == -1 )
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_ALIGNMENT");
//   }
//   else
//   {
//     v0 = 0;
//     do
//     {
//       if ( charUiAlignments/*0x102fdc60*/[v0] == _pkt.alignment )
//       {
//         sub_1011BA00/*0x1011ba00*/(charUiAlignments/*0x102fdc60*/[v0]);
//       }
//       ++v0;
//     }
//     while ( v0 < 9 );
//   }
// }
// }
//
// [TempleDllLocation(0x102f7a6c)]
// internal class DeitySystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_DEITY";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Deity;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10187690)]
//         public DeitySystem()
//         {
//           int result;
//           int v2;
//           string v3;
//           string v4;
//           CHAR v5;
//
//           stru_10C3EED8/*0x10c3eed8*/.flags = 8;
//           stru_10C3EED8/*0x10c3eed8*/.field2c = -1;
//           stru_10C3EED8/*0x10c3eed8*/.textColor = (ColorRect *)&unk_102FDAF8/*0x102fdaf8*/;
//           stru_10C3EED8/*0x10c3eed8*/.shadowColor = (ColorRect *)&unk_102FDB08/*0x102fdb08*/;
//           stru_10C3EED8/*0x10c3eed8*/.colors4 = (ColorRect *)&unk_102FDAF8/*0x102fdaf8*/;
//           stru_10C3EED8/*0x10c3eed8*/.colors2 = (ColorRect *)&unk_102FDAF8/*0x102fdaf8*/;
//           stru_10C3EED8/*0x10c3eed8*/.field0 = 0;
//           stru_10C3EED8/*0x10c3eed8*/.kerning = 1;
//           stru_10C3EED8/*0x10c3eed8*/.leading = 0;
//           stru_10C3EED8/*0x10c3eed8*/.tracking = 3;
//           if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deitybox.tga", &dword_10C40300/*0x10c40300*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deity_button.tga", &dword_10C3EED4/*0x10c3eed4*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deity_button_hovered.tga", &dword_10C3F304/*0x10c3f304*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deity_button_clicked.tga", &dword_10C3F1BC/*0x10c3f1bc*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deity_button_selected.tga", &dword_10C3EED0/*0x10c3eed0*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\deity_button_disabled.tga", &dword_10C3F300/*0x10c3f300*/) )
//           {
//             result = 0;
//           }
//           else
//           {
//             v2 = 0;
//             do
//             {
//               v3 = GameSystems.Deity.GetName(v2);
//               v4 = _strdup(v3);
//               (&dword_10C3EE30/*0x10c3ee30*/)[4 * v2] = v4;
//               if ( *v4 )
//               {
//                 do
//                 {
//                   *v4 = toupper_0(*v4);
//                   v5 = (v4++)[1];
//                 }
//                 while ( v5 );
//               }
//               ++v2;
//             }
//             while ( v2 < 20 );
//             result = sub_101873F0/*0x101873f0*/(*(_DWORD *)(a1 + 4), *(_DWORD *)(a1 + 8)) != 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10187050)]
// public void   Reset(CharEditorSelectionPacket selPkt)
// {
//   CharEditorSelectionPacket result;
//
//   result = selPkt;
//   selPkt.deityId = 20;
//   selPkt.alignmentChoice = 0;
//   return result;
// }
//
// [TempleDllLocation(0x10187390)]
// public int ChargenDeityActivate()
// {
//   return ChargenDeityBtnStates/*0x10187340*/();
// }
//
// [TempleDllLocation(0x101873a0)]
// public void Dispose()
// {
//   int *v0;
//   void **v1;
//
//   v0 = chargenDeityBtnIds/*0x10c3ee80*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v0);
//     ++v0;
//   }
//   while ( (int)v0 < (int)&dword_10C3EED0/*0x10c3eed0*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C3F448/*0x10c3f448*/);
//   v1 = (void **)&dword_10C3EE30/*0x10c3ee30*/;
//   do
//   {
//     free(*v1);
//     ++v1;
//   }
//   while ( (int)v1 < (int)chargenDeityBtnIds/*0x10c3ee80*/ );
// }
// [TempleDllLocation(0x101877d0)]
// public void Resize(Size a1)
// {
//   int *v1;
//
//   v1 = chargenDeityBtnIds/*0x10c3ee80*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v1);
//     ++v1;
//   }
//   while ( (int)v1 < (int)&dword_10C3EED0/*0x10c3eed0*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C3F448/*0x10c3f448*/);
//   return sub_101873F0/*0x101873f0*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x10187070)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(dword_10C3F448/*0x10c3f448*/, 1);
// }
// [TempleDllLocation(0x10187090)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(dword_10C3F448/*0x10c3f448*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(dword_10C3F448/*0x10c3f448*/);
// }
// [TempleDllLocation(0x101870b0)]
// public bool CheckComplete()
// {
//   return _pkt.deityId != 20;
// }
// [TempleDllLocation(0x101870c0)]
// public void Finalize(CharEditorSelectionPacket a1, GameObjectBody a2)
// {
//   return *a2.SetInt32(obj_f.critter_deity, a1.deityId);
// }
// [TempleDllLocation(0x101870f0)]
// public void ChargenDeityBtnEntered()
// {
//   uint v0;
//   uint "TAG_CHARGEN_DEITY";
//
//   if ( _pkt.deityId == 20 )
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_DEITY");
//   }
//   else
//   {
//     v0 = (&off_102FDA68/*0x102fda68*//*ELFHASH*/[4 * _pkt.deityId]);
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/(v0);
//   }
// }
// }
//
// [TempleDllLocation(0x102f7a98)]
// internal class AbilitiesSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_ABILITIES";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Abilities;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10186f90)]
//         public AbilitiesSystem()
//         {
//           bool result;
//
//           if ( ChargenAbilitiesWidgetsInit/*0x10186570*/(conf.width, conf.height) && sub_10186020/*0x10186020*/(conf.width, conf.height) )
//           {
//             result = sub_10186BD0/*0x10186bd0*/(conf.width, conf.height) != 0;
//           }
//           else
//           {
//             result = 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10184b90)]
// public void   Reset(CharEditorSelectionPacket a1)
// {
//   a1.wizSchool = 0;
//   a1.forbiddenSchool1 = 0;
//   a1.forbiddenSchool2 = 0;
//   a1.domain1 = 0;
//   a1.domain2 = 0;
//   a1.feat3 = -1;
//   uiChargenAbilitiesActivated/*0x10c3d0f0*/ = 0;
// }
//
// [TempleDllLocation(0x10185e10)]
// public int ChargenAbilitiesActivate()
// {
//   int result;
//
//   result = ChargenAbilitiesActivateImpl/*0x101856f0*/();
//   uiChargenAbilitiesActivated/*0x10c3d0f0*/ = 1;
//   return result;
// }
//
// [TempleDllLocation(0x10185600)]
// public void Dispose()
// {
//   int *v0;
//
//   sub_10184D50/*0x10184d50*/();
//   v0 = &dword_10C3C968/*0x10c3c968*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v0);
//     ++v0;
//   }
//   while ( (int)v0 < (int)&dword_10C3C98C/*0x10c3c98c*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C3DDC0/*0x10c3ddc0*/);
//   return sub_10185530/*0x10185530*/();
// }
// [TempleDllLocation(0x10186fe0)]
// public void Resize(Size a1)
// {
//   int *v1;
//   int v2;
//
//   sub_10184D50/*0x10184d50*/();
//   v1 = &dword_10C3C968/*0x10c3c968*/;
//   do
//   {
//     ui_widget_remove_regard_parent/*0x101f94d0*/(*v1);
//     ++v1;
//   }
//   while ( (int)v1 < (int)&dword_10C3C98C/*0x10c3c98c*/ );
//   ui_widget_and_window_remove/*0x101f9010*/(dword_10C3DDC0/*0x10c3ddc0*/);
//   sub_10185530/*0x10185530*/();
//   v2 = a1;
//   ChargenAbilitiesWidgetsInit/*0x10186570*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
//   sub_10186020/*0x10186020*/(*(_DWORD *)(v2 + 12), *(_DWORD *)(v2 + 16));
//   return sub_10186BD0/*0x10186bd0*/(*(_DWORD *)(v2 + 12), *(_DWORD *)(v2 + 16));
// }
// [TempleDllLocation(0x10185640)]
// public void Hide()
// {
//   WidgetSetHidden/*0x101f9100*/(chargenAbilitiesWndId/*0x10c3deb8*/, 1);
//   WidgetSetHidden/*0x101f9100*/(dword_10C3DDC0/*0x10c3ddc0*/, 1);
//   return WidgetSetHidden/*0x101f9100*/(dword_10C3E008/*0x10c3e008*/, 1);
// }
// [TempleDllLocation(0x10185670)]
// public void Show()
// {
//   int result;
//
//   if ( _pkt.classCode == 9 )
//   {
//     WidgetSetHidden/*0x101f9100*/(chargenAbilitiesWndId/*0x10c3deb8*/, 0);
//     result = WidgetBringToFront/*0x101f8e40*/(chargenAbilitiesWndId/*0x10c3deb8*/);
//   }
//   if ( _pkt.classCode == 17 )
//   {
//     WidgetSetHidden/*0x101f9100*/(dword_10C3DDC0/*0x10c3ddc0*/, 0);
//     result = WidgetBringToFront/*0x101f8e40*/(dword_10C3DDC0/*0x10c3ddc0*/);
//   }
//   if ( _pkt.classCode == 14 )
//   {
//     WidgetSetHidden/*0x101f9100*/(dword_10C3E008/*0x10c3e008*/, 0);
//     result = WidgetBringToFront/*0x101f8e40*/(dword_10C3E008/*0x10c3e008*/);
//   }
//   return result;
// }
// [TempleDllLocation(0x10184bd0)]
// public bool CheckComplete()
// {
//   if ( _pkt.classCode == 9 )
//   {
//     if ( _pkt.domain1 && _pkt.domain2 )
//     {
//       if ( !_pkt.alignmentChoice )
//       {
//         return 0;
//       }
//       return 1;
//     }
//     return 0;
//   }
//   if ( _pkt.classCode == 17 )
//   {
//     if ( _pkt.wizSchool )
//     {
//       if ( _pkt.forbiddenSchool1 )
//       {
//         if ( _pkt.wizSchool != 3 && !_pkt.forbiddenSchool2 )
//         {
//           return 0;
//         }
//         return 1;
//       }
//       return 0;
//     }
//   }
//   else if ( _pkt.classCode == 14 && _pkt.feat3 == -1 )
//   {
//     return 0;
//   }
//   return 1;
// }
// [TempleDllLocation(0x10184c80)]
// public void Finalize(CharEditorSelectionPacket cePkt, GameObjectBody a2)
// {
//   if ( cePkt.classCode == 9 )
//   {
//     *a2.SetInt32(obj_f.critter_domain_1, cePkt.domain1);
//     *a2.SetInt32(obj_f.critter_domain_2, cePkt.domain2);
//     SetAlignmentChoice/*0x1004aac0*/(*a2, cePkt.alignmentChoice);
//   }
//   if ( cePkt.classCode == 17 )
//   {
//     setWizSchool/*0x100fdec0*/(*a2, cePkt.wizSchool);
//     SetForbiddenSchoolMaybe/*0x100fdf30*/(*a2, 0, cePkt.forbiddenSchool1);
//     SetForbiddenSchoolMaybe/*0x100fdf30*/(*a2, 1, cePkt.forbiddenSchool2);
//   }
//   if ( cePkt.classCode == 14 )
//   {
//     GameSystems.Feat.AddFeat(*a2, cePkt.feat3);
//     GameSystems.D20.Status.D20StatusRefresh(*a2);
//   }
// }
// [TempleDllLocation(0x10184c40)]
// public void ChargenAbilitiesBtnEntered()
// {
//   uint "TAG_HMU_CHAR_EDITOR_WIZARD_SPEC";
//   uint v1;
//
//   if ( _pkt.classCode == 17 )
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_HMU_CHAR_EDITOR_WIZARD_SPEC");
//   }
//   else if ( _pkt.classCode == 9 )
//   {
//     v1 = &emptyString/*0x1026c67b*//*ELFHASH*/;
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/(v1);
//   }
// }
// }
//
// [TempleDllLocation(0x102f7ac4)]
// internal class FeatsSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_FEATS";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Feats;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x101847f0)]
//         [TemplePlusLocation("ui_pc_creation_hooks.cpp:165")]
//         public FeatsSystem()
//         {
//           int result;
//           int v2;
//           string meslineValue;
//           int meslineKey;
//
//           meslineKey = 19000;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_FeatsAvailable/*0x10c3a3ac*/ = (string )meslineValue;
//           meslineKey = 19001;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_Feats/*0x10c38c08*/ = (string )meslineValue;
//           meslineKey = 19002;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_ClassFeats/*0x10c39888*/ = (string )meslineValue;
//           meslineKey = 19003;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_ClassBonusFeat/*0x10c37dec*/ = (string )meslineValue;
//           meslineKey = 19200;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_Accept/*0x10c38ac0*/ = (string )meslineValue;
//           meslineKey = 19201;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           pcCreationLabel_Cancel/*0x10c3c148*/ = (string )meslineValue;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.textColor = &pcCreationDarkGreen/*0x102fd688*/;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.colors4 = &pcCreationDarkGreen/*0x102fd688*/;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.colors2 = &pcCreationDarkGreen/*0x102fd688*/;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.flags = 0x4000;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.field2c = -1;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.field0 = 0;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.kerning = 1;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.leading = 0;
//           pcCreationClassFeatsBtnStyle/*0x10c39890*/.tracking = 3;
//           stru_10C398E0/*0x10c398e0*/.flags = 0x4000;
//           stru_10C398E0/*0x10c398e0*/.field2c = -1;
//           stru_10C398E0/*0x10c398e0*/.textColor = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C398E0/*0x10c398e0*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C398E0/*0x10c398e0*/.colors4 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C398E0/*0x10c398e0*/.colors2 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C398E0/*0x10c398e0*/.field0 = 0;
//           stru_10C398E0/*0x10c398e0*/.kerning = 1;
//           stru_10C398E0/*0x10c398e0*/.leading = 0;
//           stru_10C398E0/*0x10c398e0*/.tracking = 3;
//           stru_10C38908/*0x10c38908*/.flags = 0x4000;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.flags = 0x4000;
//           stru_10C38960/*0x10c38960*/.flags = 0x4000;
//           dword_10C3C178/*0x10c3c178*/ = 0x4000;
//           stru_10C39838/*0x10c39838*/.flags = 0x4000;
//           stru_10C38908/*0x10c38908*/.field2c = -1;
//           stru_10C38908/*0x10c38908*/.textColor = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38908/*0x10c38908*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C38908/*0x10c38908*/.colors4 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38908/*0x10c38908*/.colors2 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38908/*0x10c38908*/.field0 = 0;
//           stru_10C38908/*0x10c38908*/.kerning = 1;
//           stru_10C38908/*0x10c38908*/.leading = 0;
//           stru_10C38908/*0x10c38908*/.tracking = 3;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.field2c = -1;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.textColor = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.colors4 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.colors2 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.field0 = 0;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.kerning = 1;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.leading = 0;
//           pcCreationClassFeatsStyle/*0x10c3ae00*/.tracking = 3;
//           stru_10C38960/*0x10c38960*/.field2c = -1;
//           stru_10C38960/*0x10c38960*/.textColor = (ColorRect *)&unk_102FD668/*0x102fd668*/;
//           stru_10C38960/*0x10c38960*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C38960/*0x10c38960*/.colors4 = (ColorRect *)&unk_102FD668/*0x102fd668*/;
//           stru_10C38960/*0x10c38960*/.colors2 = (ColorRect *)&unk_102FD668/*0x102fd668*/;
//           stru_10C38960/*0x10c38960*/.field0 = 0;
//           stru_10C38960/*0x10c38960*/.kerning = 1;
//           stru_10C38960/*0x10c38960*/.leading = 0;
//           stru_10C38960/*0x10c38960*/.tracking = 3;
//           dword_10C3C17C/*0x10c3c17c*/ = -1;
//           dword_10C3C184/*0x10c3c184*/ = (int)&stru_102FD698/*0x102fd698*/;
//           dword_10C3C18C/*0x10c3c18c*/ = (int)&stru_102FD658/*0x102fd658*/;
//           dword_10C3C190/*0x10c3c190*/ = (int)&stru_102FD698/*0x102fd698*/;
//           dword_10C3C188/*0x10c3c188*/ = (int)&stru_102FD698/*0x102fd698*/;
//           dword_10C3C150/*0x10c3c150*/ = 0;
//           dword_10C3C158/*0x10c3c158*/ = 1;
//           dword_10C3C15C/*0x10c3c15c*/ = 0;
//           dword_10C3C154/*0x10c3c154*/ = 3;
//           stru_10C39838/*0x10c39838*/.field2c = -1;
//           stru_10C39838/*0x10c39838*/.textColor = &stru_102FD678/*0x102fd678*/;
//           stru_10C39838/*0x10c39838*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C39838/*0x10c39838*/.colors4 = &stru_102FD678/*0x102fd678*/;
//           stru_10C39838/*0x10c39838*/.colors2 = &stru_102FD678/*0x102fd678*/;
//           stru_10C39838/*0x10c39838*/.field0 = 0;
//           stru_10C39838/*0x10c39838*/.kerning = 1;
//           stru_10C39838/*0x10c39838*/.leading = 0;
//           stru_10C39838/*0x10c39838*/.tracking = 3;
//           stru_10C38D08/*0x10c38d08*/.flags = 16;
//           stru_10C38D08/*0x10c38d08*/.field2c = -1;
//           stru_10C38D08/*0x10c38d08*/.textColor = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38D08/*0x10c38d08*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C38D08/*0x10c38d08*/.colors4 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38D08/*0x10c38d08*/.colors2 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38D08/*0x10c38d08*/.field0 = 0;
//           stru_10C38D08/*0x10c38d08*/.kerning = 1;
//           stru_10C38D08/*0x10c38d08*/.leading = 0;
//           stru_10C38D08/*0x10c38d08*/.tracking = 3;
//           stru_10C38A70/*0x10c38a70*/.flags = 3088;
//           stru_10C38A70/*0x10c38a70*/.field2c = -1;
//           stru_10C38A70/*0x10c38a70*/.textColor = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38A70/*0x10c38a70*/.shadowColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C38A70/*0x10c38a70*/.colors4 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38A70/*0x10c38a70*/.colors2 = (ColorRect *)&unk_102FD648/*0x102fd648*/;
//           stru_10C38A70/*0x10c38a70*/.bgColor = &stru_102FD658/*0x102fd658*/;
//           stru_10C38A70/*0x10c38a70*/.field0 = 0;
//           stru_10C38A70/*0x10c38a70*/.kerning = 1;
//           stru_10C38A70/*0x10c38a70*/.leading = 0;
//           stru_10C38A70/*0x10c38a70*/.tracking = 3;
//           dword_10C3ADF8/*0x10c3adf8*/ = Globals.UiAssets.LoadImg("art\\interface\\pc_creation\\meta_backdrop.img");
//           if ( dword_10C3ADF8/*0x10c3adf8*/ )
//           {
//             v2 = a1.height;
//             result = UiPcCreationFeatsWidgetsInit/*0x10183e00*/(a1.width) != 0;
//           }
//           else
//           {
//             result = 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10181f40)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:168")]
// public void   Reset(CharEditorSelectionPacket selPkt)
// {
//   JUMPOUT(unk_10181F49);
// }
//
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:168
// */
//
// [TempleDllLocation(0x10182a30)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:167")]
// public int UiPcCreationFeatsActivate()
// {
//   int classFeatCount;
//   int i;
//   FeatId *v2;
//   FeatId v3;
//   FeatId v4;
//   int v5;
//   int v6;
//   int v7;
//   int v8;
//   int v9;
//   string meslineValue;
// int meslineKey;
//
//   charUiFeatBeingAdded/*0x10c3add4*/ = 649;
//   meslineKey = 19101;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3895C/*0x10c3895c*/ = (int)meslineValue;
//   meslineKey = 19102;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3A3A8/*0x10c3a3a8*/ = (int)meslineValue;
//   meslineKey = 19103;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C37DFC/*0x10c37dfc*/ = (int)meslineValue;
//   meslineKey = 19104;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3AF60/*0x10c3af60*/ = (int)meslineValue;
//   meslineKey = 19105;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3C144/*0x10c3c144*/ = (int)meslineValue;
//   meslineKey = 19106;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3C14C/*0x10c3c14c*/ = (int)meslineValue;
//   meslineKey = 19107;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3AE54/*0x10c3ae54*/ = (int)meslineValue;
//   meslineKey = 19108;
//   GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//   dword_10C3C140/*0x10c3c140*/ = (int)meslineValue;
//   classFeatCount = GameSystems.Feat.FeatListElective(UiSystems.PCCreation.charEditorObjHnd, pcCreationFeatListElectives/*0x10c39970*/);
//   i = 0;
//   featListElectiveCount/*0x10c38904*/ = classFeatCount;
//   if ( classFeatCount > 0 )
//   {
//     v2 = pcCreationFeatListElectives/*0x10c39970*/;
//     do
//     {
//       v3 = *v2;
//       if ( _pkt.feat0 == *v2 || _pkt.feat1 == v3 || _pkt.feat2 == v3 )
//       {
//         featListElectiveCount/*0x10c38904*/ = classFeatCount - 1;
//         memcpy(v2, v2 + 1, 4 * (classFeatCount - 1 - i));
//         classFeatCount = featListElectiveCount/*0x10c38904*/;
//       }
//       ++i;
//       ++v2;
//     }
//     while ( i < classFeatCount );
//   }
//   ArraySort/*0x10254750*/(pcCreationFeatListElectives/*0x10c39970*/, classFeatCount, 4, FeatListSorter/*0x101827e0*/);
//   j_WidgetCopy/*0x101f87a0*/(widIdx/*0x10c3ae50*/, (LgcyWidget *)&pcCreationClassFeatsScrollbar/*0x10c3c090*/);
//   pcCreationClassFeatsScrollbar/*0x10c3c090*/.scrollbarY = 0;
//   featsListWidIdx/*0x10c3a394*/ = 0;
//   pcCreationClassFeatsScrollbar/*0x10c3c090*/.yMax = (featListElectiveCount/*0x10c38904*/ - 8) & ((featListElectiveCount/*0x10c38904*/ - 8 < 0) - 1);
//   j_ui_widget_set/*0x101f87b0*/(widIdx/*0x10c3ae50*/, &pcCreationClassFeatsScrollbar/*0x10c3c090*/);
//   pcCreationFeatsAvailCount/*0x10c3996c*/ = 0;
//   v4 = 0;
//   do
//   {
//     LOBYTE(v5) = IsFeatEnabled/*0x1007bbd0*/(v4);
//     if ( !v5 || (LOBYTE(v6) = IsFeatClassOrRacialAutomatic/*0x1007bca0*/(v4), v6) || IsFeatPartOfMultiSelect/*0x1007bc80*/(v4) )
//     {
//       v8 = pcCreationFeatsAvailCount/*0x10c3996c*/;
//     }
//     else
//     {
//       v7 = pcCreationFeatsAvailCount/*0x10c3996c*/;
//       uiPcCreationFeatsAvail/*0x10c38d58*/[pcCreationFeatsAvailCount/*0x10c3996c*/] = v4;
//       v8 = v7 + 1;
//       pcCreationFeatsAvailCount/*0x10c3996c*/ = v8;
//     }
//     ++v4;
//   }
//   while ( (int)v4 < 649 );
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v8] = 650;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v8 + 1] = 651;
//   v9 = v8 + 1;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9++ + 1] = 652;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9 + 1] = 653;
//   v9 += 2;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9++] = 654;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9++] = 655;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9++] = 656;
//   uiPcCreationFeatsAvail/*0x10c38d58*/[v9] = 657;
//   pcCreationFeatsAvailCount/*0x10c3996c*/ = v9 + 1;
//   ArraySort/*0x10254750*/(uiPcCreationFeatsAvail/*0x10c38d58*/, v9 + 1, 4, FeatListSorter/*0x101827e0*/);
//   j_WidgetCopy/*0x101f87a0*/(pcCreationFeatsAvailScrollbarId/*0x10c38d00*/, (LgcyWidget *)&pcCreationFeatsAvailScrollbar/*0x10c38b58*/);
//   pcCreationFeatsAvailScrollbar/*0x10c38b58*/.yMax = pcCreationFeatsAvailCount/*0x10c3996c*/ - 15;
//   return j_ui_widget_set/*0x101f87b0*/(pcCreationFeatsAvailScrollbarId/*0x10c38d00*/, &pcCreationFeatsAvailScrollbar/*0x10c38b58*/);
// }
//
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:167
// */
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:165
// */
// [TempleDllLocation(0x10182d30)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:166")]
// public void Dispose()
// {
//   return PcCreationWidgetsRemove/*0x10182090*/();
// }
// [TempleDllLocation(0x10184b70)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:169")]
// public void Resize(Size a1)
// {
//   int v1;
//
//   PcCreationWidgetsRemove/*0x10182090*/();
//   v1 = *(_DWORD *)(a1 + 16);
//   return UiPcCreationFeatsWidgetsInit/*0x10183e00*/(*(_DWORD *)(a1 + 12));
// }
// [TempleDllLocation(0x10181f60)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:171")]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationFeatsWndId/*0x10c3af5c*/, 1);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:171
// */
// [TempleDllLocation(0x10181f80)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:170")]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationFeatsWndId/*0x10c3af5c*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationFeatsWndId/*0x10c3af5c*/);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:170
// */
// [TempleDllLocation(0x10181fa0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:173")]
// public int ChargenFeatsCheckComplete()
// {
//   int result;
//
//   if ( (_pkt.raceId || _pkt.feat1 != 649) && (_pkt.classCode != 11 || _pkt.feat2 != 649) )
//   {
//     result = _pkt.feat0 != 649;
//   }
//   else
//   {
//     result = 0;
//   }
//   return result;
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:173
// */
// [TempleDllLocation(0x10181fe0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:172")]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody a2)
// {
//   FeatId v2;
//
//   GameSystems.Feat.AddFeat(*a2, selPkt.feat0);
//   GameSystems.D20.Status.D20StatusRefresh(*a2);
//   if ( selPkt.feat1 != 649 )
//   {
//     GameSystems.Feat.AddFeat(*a2, selPkt.feat1);
//     GameSystems.D20.Status.D20StatusRefresh(*a2);
//   }
//   v2 = selPkt.feat2;
//   if ( v2 != 649 )
//   {
//     GameSystems.Feat.AddFeat(*a2, v2);
//     GameSystems.D20.Status.D20StatusRefresh(*a2);
//   }
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:172
// */
// [TempleDllLocation(0x10182070)]
// public void PcCreationBtnExited()
// {
//   uint "TAG_CHARGEN_FEATS";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_FEATS");
// }
// }
//
// [TempleDllLocation(0x102f7af0)]
// internal class SkillsSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_SKILLS";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Skills;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x10181b70)]
//         public SkillsSystem()
//         {
//           int result;
//           string meslineValue;
//           int meslineKey;
//           TigFontMetrics metrics;
//
//           stru_10C371B8/*0x10c371b8*/.flags = 0x4000;
//           stru_10C37858/*0x10c37858*/.flags = 0x4000;
//           stru_10C379C0/*0x10c379c0*/.flags = 0x4000;
//           stru_10C371B8/*0x10c371b8*/.textColor = (ColorRect *)&unk_102FD3E0/*0x102fd3e0*/;
//           stru_10C371B8/*0x10c371b8*/.colors4 = (ColorRect *)&unk_102FD3E0/*0x102fd3e0*/;
//           stru_10C371B8/*0x10c371b8*/.colors2 = (ColorRect *)&unk_102FD3E0/*0x102fd3e0*/;
//           stru_10C379C0/*0x10c379c0*/.textColor = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C379C0/*0x10c379c0*/.colors4 = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C379C0/*0x10c379c0*/.colors2 = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C36910/*0x10c36910*/.textColor = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C36910/*0x10c36910*/.colors4 = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C36910/*0x10c36910*/.colors2 = (ColorRect *)&unk_102FD420/*0x102fd420*/;
//           stru_10C37858/*0x10c37858*/.textColor = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37858/*0x10c37858*/.colors4 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37858/*0x10c37858*/.colors2 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37A18/*0x10c37a18*/.textColor = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37A18/*0x10c37a18*/.colors4 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37A18/*0x10c37a18*/.colors2 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C378C0/*0x10c378c0*/.textColor = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C378C0/*0x10c378c0*/.colors4 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C378C0/*0x10c378c0*/.colors2 = (ColorRect *)&unk_102FD400/*0x102fd400*/;
//           stru_10C37A18/*0x10c37a18*/.flags = 0x4000;
//           stru_10C378C0/*0x10c378c0*/.flags = 0x4000;
//           stru_10C371B8/*0x10c371b8*/.field2c = -1;
//           stru_10C371B8/*0x10c371b8*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C371B8/*0x10c371b8*/.field0 = 0;
//           stru_10C371B8/*0x10c371b8*/.kerning = 1;
//           stru_10C371B8/*0x10c371b8*/.leading = 0;
//           stru_10C371B8/*0x10c371b8*/.tracking = 3;
//           stru_10C37858/*0x10c37858*/.field2c = -1;
//           stru_10C37858/*0x10c37858*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C37858/*0x10c37858*/.field0 = 0;
//           stru_10C37858/*0x10c37858*/.kerning = 1;
//           stru_10C37858/*0x10c37858*/.leading = 0;
//           stru_10C37858/*0x10c37858*/.tracking = 3;
//           stru_10C379C0/*0x10c379c0*/.field2c = -1;
//           stru_10C379C0/*0x10c379c0*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C379C0/*0x10c379c0*/.field0 = 0;
//           stru_10C379C0/*0x10c379c0*/.kerning = 1;
//           stru_10C379C0/*0x10c379c0*/.leading = 0;
//           stru_10C379C0/*0x10c379c0*/.tracking = 3;
//           stru_10C36910/*0x10c36910*/.flags = 0x4000;
//           stru_10C36910/*0x10c36910*/.field2c = -1;
//           stru_10C36910/*0x10c36910*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C36910/*0x10c36910*/.field0 = 0;
//           stru_10C36910/*0x10c36910*/.kerning = 1;
//           stru_10C36910/*0x10c36910*/.leading = 0;
//           stru_10C36910/*0x10c36910*/.tracking = 3;
//           stru_10C37A18/*0x10c37a18*/.field2c = -1;
//           stru_10C37A18/*0x10c37a18*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C37A18/*0x10c37a18*/.field0 = 0;
//           stru_10C37A18/*0x10c37a18*/.kerning = 1;
//           stru_10C37A18/*0x10c37a18*/.leading = 0;
//           stru_10C37A18/*0x10c37a18*/.tracking = 3;
//           stru_10C378C0/*0x10c378c0*/.field2c = -1;
//           stru_10C378C0/*0x10c378c0*/.shadowColor = (ColorRect *)&unk_102FD3F0/*0x102fd3f0*/;
//           stru_10C378C0/*0x10c378c0*/.field0 = 0;
//           stru_10C378C0/*0x10c378c0*/.kerning = 1;
//           stru_10C378C0/*0x10c378c0*/.leading = 0;
//           stru_10C378C0/*0x10c378c0*/.tracking = 3;
//           meslineKey = 20000;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C36854/*0x10c36854*/ = (string )meslineValue;
//           dword_10C36868/*0x10c36868*/ = &stru_10C371B8/*0x10c371b8*/;
//           Tig.Fonts.PushFont(PredefinedFont.PRIORY_12);
//           metrics.height = 0;
//           metrics.width = 0;
//           metrics.text = dword_10C36854/*0x10c36854*/;
//           Tig.Fonts.Measure(dword_10C36868/*0x10c36868*/, &metrics);
//           stru_10C36C1C/*0x10c36c1c*/.width = metrics.width;
//           stru_10C36C1C/*0x10c36c1c*/.x = 431 - metrics.width;
//           stru_10C36C1C/*0x10c36c1c*/.y = 221;
//           stru_10C36C1C/*0x10c36c1c*/.height = metrics.height;
//           meslineKey = 20001;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C379BC/*0x10c379bc*/ = (string )meslineValue;
//           dword_10C36BF4/*0x10c36bf4*/ = &stru_10C37858/*0x10c37858*/;
//           metrics.height = 0;
//           metrics.width = 0;
//           metrics.text = meslineValue;
//           Tig.Fonts.Measure(&stru_10C37858/*0x10c37858*/, &metrics);
//           stru_10C3686C/*0x10c3686c*/.x = 350 - metrics.width;
//           stru_10C3686C/*0x10c3686c*/.y = 27;
//           stru_10C3686C/*0x10c3686c*/.width = metrics.width;
//           stru_10C3686C/*0x10c3686c*/.height = metrics.height;
//           Tig.Fonts.PopFont();
//           meslineKey = 20002;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C378A8/*0x10c378a8*/ = (string )meslineValue;
//           dword_10C3624C/*0x10c3624c*/ = &stru_10C378C0/*0x10c378c0*/;
//           meslineKey = 20003;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C36C60/*0x10c36c60*/ = (string )meslineValue;
//           dword_10C36850/*0x10c36850*/ = &stru_10C378C0/*0x10c378c0*/;
//           meslineKey = 20004;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C36BF8/*0x10c36bf8*/ = (string )meslineValue;
//           dword_10C377E4/*0x10c377e4*/ = &stru_10C378C0/*0x10c378c0*/;
//           meslineKey = 20005;
//           GetLine_Safe/*0x101e65e0*/(pc_creationMes/*0x11e72ef0*/, &mesline);
//           dword_10C3684C/*0x10c3684c*/ = (string )meslineValue;
//           dword_10C36C88/*0x10c36c88*/ = &stru_10C37858/*0x10c37858*/;
//           if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.breakdown.tga", &dword_10C36C3C/*0x10c36c3c*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.availbox.tga", &dword_10C36C84/*0x10c36c84*/)
//                || RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\SkillId.buttonbox.tga", &dword_10C37A68/*0x10c37a68*/) )
//           {
//             result = 0;
//           }
//           else
//           {
//             result = UiPcCreationSkillWidgetsInit/*0x10181700*/(a1.width, a1.height) != 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x10180630)]
// public void Reset(CharEditorSelectionPacket a1)
// {
//   int result;
//
//   result = 0;
//   memset(a1.skillPointsAdded, 0, sizeof(a1.skillPointsAdded));
//   uiPcCreationSkillsActivated/*0x10c36248*/ = 0;
//   return result;
// }
//
// [TempleDllLocation(0x10181380)]
// public int ChargenSkillsActivate()
// {
//   int skillIdx;
//   int unlearnable;
//   int v2;
//   int v3;
//   int v4;
//   int result;
//
//   skillIdxMax/*0x10c37218*/ = 42;
//   chargenSkillsAvailableCount/*0x10c379b8*/ = 0;
//   skillIdx = 0;
//   do
//   {
//     LOBYTE(unlearnable) = UnableToLearnSkill/*0x1007d160*/(skillIdx, _pkt.classCode);
//     if ( !unlearnable )
//     {
//       v2 = chargenSkillsAvailableCount/*0x10c379b8*/;
//       chargenSkills/*0x10c37910*/[chargenSkillsAvailableCount/*0x10c379b8*/] = skillIdx;
//       chargenSkillsAvailableCount/*0x10c379b8*/ = v2 + 1;
//     }
//     ++skillIdx;
//   }
//   while ( skillIdx < 42 );
//   ArraySort/*0x10254750*/(
//     chargenSkills/*0x10c37910*/,
//     chargenSkillsAvailableCount/*0x10c379b8*/,
//     4,
//     (int (  *)(void *, void *))SkillAlphabeticalSortCallback/*0x10180b30*/);
//   j_WidgetCopy/*0x101f87a0*/(uiPcCreationSkillsScrollbarId/*0x10c37a14*/, (LgcyWidget *)&uiPcCreationSkillsScrollbar/*0x10c36780*/);
//   uiPcCreationSkillsScrollbar/*0x10c36780*/.yMax = chargenSkillsAvailableCount/*0x10c379b8*/ - 7;
//   j_ui_widget_set/*0x101f87b0*/(uiPcCreationSkillsScrollbarId/*0x10c37a14*/, &uiPcCreationSkillsScrollbar/*0x10c36780*/);
//   if ( !uiPcCreationSkillsActivated/*0x10c36248*/ )
//   {
//     _pkt.skillPointsSpent = 0;
//     v3 = UiSystems.PCCreation.charEditorObjHnd.GetStat(Stat.intelligence);
//     v4 = 4 * (skillPtsByClass_OFFSET_7_dword_102EB48C/*0x102eb48c*/[_pkt.classCode] + D20StatSystem.GetModifierForAbilityScore(v3));
//     _pkt.availableSkillPts = v4;
//     if ( !_pkt.raceId )
//     {
//       v4 += 4;
//       _pkt.availableSkillPts = v4;
//     }
//     if ( v4 < 4 )
//     {
//       _pkt.availableSkillPts = 4;
//     }
//   }
//   result = UiPcCreationSkillTextDraw/*0x10180eb0*/();
//   uiPcCreationSkillsActivated/*0x10c36248*/ = 1;
//   return result;
// }
//
// [TempleDllLocation(0x10180bc0)]
// public void Dispose()
// {
//   return sub_10180720/*0x10180720*/();
// }
// [TempleDllLocation(0x10181f20)]
// public void Resize(Size a1)
// {
//   sub_10180720/*0x10180720*/();
//   return UiPcCreationSkillWidgetsInit/*0x10181700*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x10180650)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationWndSkillsId/*0x10c36250*/, 1);
// }
// [TempleDllLocation(0x10180670)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationWndSkillsId/*0x10c36250*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationWndSkillsId/*0x10c36250*/);
// }
// [TempleDllLocation(0x10180690)]
// public bool CheckComplete()
// {
//   return uiPcCreationSkillsActivated/*0x10c36248*/ && _pkt.availableSkillPts == _pkt.skillPointsSpent;
// }
// [TempleDllLocation(0x101806b0)]
// public void   ChargenSkillsApply(CharEditorSelectionPacket charEdPkt, GameObjectBody *obj)
// {
//   int i;
//   int *skillPtsAdded;
//
//   i = 0;
//   skillPtsAdded = charEdPkt.skillPointsAdded;
//   do
//   {
//     if ( *skillPtsAdded )
//     {
//       GameSystems.Skill.AddSkillRanks(*obj, i, *skillPtsAdded);
//     }
//     ++i;
//     ++skillPtsAdded;
//   }
//   while ( i < 42 );
// }
// [TempleDllLocation(0x101806f0)]
// public void ChargenSkillsBtnEntered()
// {
//   uint "TAG_CHARGEN_SKILLS";
//
//   if ( skillIdxMax/*0x10c37218*/ == 42 )
//   {
//     UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_SKILLS");
//   }
//   else
//   {
//     sub_1011BBC0/*0x1011bbc0*/(skillIdxMax/*0x10c37218*/);
//   }
// }
// }
//
// [TempleDllLocation(0x102f7b1c)]
// internal class SpellsSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_SPELLS";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Spells;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x101800e0)]
//         [TemplePlusLocation("ui_pc_creation_hooks.cpp:176")]
//         public SpellsSystem()
//         {
//           int v1;
//           int v3;
//           int v4;
//           string meslineValue;
//           int meslineKey;
//
//           v1 = 0;
//           chargenLevelLabelStyle/*0x10c35738*/.textColor = &chargenSpellLevelLabelStyle_0/*0x102fd1a8*/;
//           chargenLevelLabelStyle/*0x10c35738*/.colors4 = &chargenSpellLevelLabelStyle_0/*0x102fd1a8*/;
//           chargenLevelLabelStyle/*0x10c35738*/.colors2 = &chargenSpellLevelLabelStyle_0/*0x102fd1a8*/;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.textColor = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.colors4 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.colors2 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           stru_10C360B8/*0x10c360b8*/.textColor = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           stru_10C360B8/*0x10c360b8*/.colors4 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           stru_10C360B8/*0x10c360b8*/.colors2 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.textColor = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.colors4 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.colors2 = (ColorRect *)&unk_102FD188/*0x102fd188*/;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           chargenLevelLabelStyle/*0x10c35738*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           stru_10C34950/*0x10c34950*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           stru_10C36060/*0x10c36060*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           stru_10C360B8/*0x10c360b8*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.shadowColor = (ColorRect *)&dword_102FD178/*0x102fd178*/;
//           stru_10C34950/*0x10c34950*/.textColor = (ColorRect *)&unk_102FD198/*0x102fd198*/;
//           stru_10C34950/*0x10c34950*/.colors4 = (ColorRect *)&unk_102FD198/*0x102fd198*/;
//           stru_10C34950/*0x10c34950*/.colors2 = (ColorRect *)&unk_102FD198/*0x102fd198*/;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.flags = 0;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.field2c = -1;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.field0 = 0;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.kerning = 0;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.leading = 0;
//           chargenSpellsPerDayStyle/*0x10c34a60*/.tracking = 4;
//           chargenLevelLabelStyle/*0x10c35738*/.flags = 0;
//           chargenLevelLabelStyle/*0x10c35738*/.field2c = -1;
//           chargenLevelLabelStyle/*0x10c35738*/.field0 = 0;
//           chargenLevelLabelStyle/*0x10c35738*/.kerning = 0;
//           chargenLevelLabelStyle/*0x10c35738*/.leading = 0;
//           chargenLevelLabelStyle/*0x10c35738*/.tracking = 4;
//           stru_10C34950/*0x10c34950*/.flags = 0;
//           stru_10C34950/*0x10c34950*/.field2c = -1;
//           stru_10C34950/*0x10c34950*/.field0 = 0;
//           stru_10C34950/*0x10c34950*/.kerning = 0;
//           stru_10C34950/*0x10c34950*/.leading = 0;
//           stru_10C34950/*0x10c34950*/.tracking = 4;
//           stru_10C36060/*0x10c36060*/.flags = 0;
//           stru_10C36060/*0x10c36060*/.field2c = -1;
//           stru_10C36060/*0x10c36060*/.textColor = &stru_102FD1B8/*0x102fd1b8*/;
//           stru_10C36060/*0x10c36060*/.colors4 = &stru_102FD1B8/*0x102fd1b8*/;
//           stru_10C36060/*0x10c36060*/.colors2 = &stru_102FD1B8/*0x102fd1b8*/;
//           stru_10C36060/*0x10c36060*/.field0 = 0;
//           stru_10C36060/*0x10c36060*/.kerning = 0;
//           stru_10C36060/*0x10c36060*/.leading = 0;
//           stru_10C36060/*0x10c36060*/.tracking = 4;
//           stru_10C360B8/*0x10c360b8*/.flags = 0;
//           stru_10C360B8/*0x10c360b8*/.field2c = -1;
//           stru_10C360B8/*0x10c360b8*/.field0 = 0;
//           stru_10C360B8/*0x10c360b8*/.kerning = 0;
//           stru_10C360B8/*0x10c360b8*/.leading = 0;
//           stru_10C360B8/*0x10c360b8*/.tracking = 4;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.flags = 0;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.field2c = -1;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.field0 = 0;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.kerning = 0;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.leading = 0;
//           chargenSpellsPerDayStyle_0/*0x10c361c0*/.tracking = 4;
//           meslineKey = 21000;
//           if ( Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline) )
//           {
//             chargenSpellsAvailableTitle/*0x10c36108*/ = (string )meslineValue;
//             meslineKey = 21001;
//             if ( Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline) )
//             {
//               chargenSpellsChosenTitle/*0x10c0eef8*/ = (string )meslineValue;
//               meslineKey = 21002;
//               if ( Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline) )
//               {
//                 chargenSpellsPerDayTitle/*0x10c34e48*/ = (string )meslineValue;
//                 v3 = 21200;
//                 while ( 1 )
//                 {
//                   meslineKey = v3 - 100;
//                   if ( !Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline) )
//                   {
//                     break;
//                   }
//                   v4 = pc_creationMes/*0x11e72ef0*/;
//                   chargenSpellLevelLabels_0/*0x10c35720*/[v1] = (int)meslineValue;
//                   meslineKey = v3;
//                   if ( !Mesfile_GetLine/*0x101e6760*/(v4, &mesline) )
//                   {
//                     break;
//                   }
//                   ++v3;
//                   chargenLevelLabels/*0x10c34938*/[v1] = (int)meslineValue;
//                   ++v1;
//                   if ( (int)(v3 - 21200) >= 6 )
//                   {
//                     return ChargenSpellsWidgetsInit/*0x1017fcc0*/(a1.width, a1.height) != 0;
//                   }
//                 }
//               }
//             }
//           }
//           return 0;
//         }
//
//         [TempleDllLocation(0x1017eae0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:179")]
// public void   Reset(CharEditorSelectionPacket spec)
// {
//   int result;
//
//   spec.spellEnumsAddedCount = 0;
//   result = 802;
//   memset32(spec.spellEnums, 802, 802);
//   chargenSpellsReseted/*0x10c0eefc*/ = 1;
//   return result;
// }
//
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:179
// */
//
// [TempleDllLocation(0x101804a0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:178")]
// public int ChargenSpellsActivate()
// {
//   int i;
//   int intMod;
//   int result;
//
//   if ( chargenSpellsReseted/*0x10c0eefc*/ )
//   {
//     if ( ((i = 0, _pkt.spellEnumsAddedCount = 0, _pkt.classCode == 8) || _pkt.classCode == 16)
//       && (i = 5,
//           _pkt.spellEnums[0] = 803,
//           _pkt.spellEnums[1] = 802,
//           _pkt.spellEnums[2] = 802,
//           _pkt.spellEnums[3] = 802,
//           _pkt.spellEnums[4] = 802,
//           _pkt.spellEnumsAddedCount = 5,
//           _pkt.classCode == 16)
//       || _pkt.classCode == 17 )
//     {
//       _pkt.spellEnums[i] = 804;
//       _pkt.spellEnums[++_pkt.spellEnumsAddedCount] = 802;
//       _pkt.spellEnums[++_pkt.spellEnumsAddedCount] = 802;
//       ++_pkt.spellEnumsAddedCount;
//       if ( _pkt.classCode == 17 )
//       {
//         intMod = D20StatSystem.GetModifierForAbilityScore(_pkt.abilityStats[3]);
//         if ( intMod > 0 )
//         {
//           do
//           {
//             _pkt.spellEnums[_pkt.spellEnumsAddedCount] = 802;
//             --intMod;
//             ++_pkt.spellEnumsAddedCount;
//           }
//           while ( intMod );
//         }
//       }
//     }
//   }
//   ChargenGetLearnables/*0x101803b0*/();
//   ChargenNumSpellsSet/*0x1017ecd0*/();
//   uiPcCreationSpellsAvailScrollbarId/*0x10c34ab0*/.SetMax(chargenSpellsNumLearnable/*0x10c360b0*/ - 12 < 0 ? 0 : chargenSpellsNumLearnable/*0x10c360b0*/ - 12);
//   uiPcCreationSpellsAvailScrollbarId/*0x10c34ab0*/.SetValue(0);
//   j_WidgetCopy/*0x101f87a0*/(uiPcCreationSpellsAvailScrollbarId/*0x10c34ab0*/, (LgcyWidget *)&uiPcCreationSpellsScrollbar/*0x10c349b0*/);
//   uiPcCreationSpellsChosenScrollbarY/*0x10c34e4c*/ = 0;
//   uiPcCreationSpellsAvailableSpellsScrollbarId/*0x10c0ef80*/.SetMax(_pkt.spellEnumsAddedCount - 12 < 0 ? 0 : _pkt.spellEnumsAddedCount - 12);
//   uiPcCreationSpellsAvailableSpellsScrollbarId/*0x10c0ef80*/.SetValue(0);
//   result = j_WidgetCopy/*0x101f87a0*/(uiPcCreationSpellsAvailableSpellsScrollbarId/*0x10c0ef80*/, (LgcyWidget *)&uiPcCreationSpellsScrollbar2/*0x10c36110*/);
//   uiPcCreationSpellsAvailableSpellsScrollbarY/*0x10c36210*/ = 0;
//   chargenSpellsReseted/*0x10c0eefc*/ = 0;
//   return result;
// }
//
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:178
// */
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:176
// */
// [TempleDllLocation(0x1017f090)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:177")]
// public void Dispose()
// {
//   return ChargenSpellWidgetsFree/*0x1017f040*/();
// }
// [TempleDllLocation(0x10180390)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:180")]
// public void Resize(Size a1)
// {
//   ChargenSpellWidgetsFree/*0x1017f040*/();
//   return ChargenSpellsWidgetsInit/*0x1017fcc0*/(a1.rect1.width, a1.rect1.height);
// }
// [TempleDllLocation(0x1017eb40)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:182")]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationSpellsWndId/*0x10c36058*/, 1);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:182
// */
// [TempleDllLocation(0x1017eb60)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:181")]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationSpellsWndId/*0x10c36058*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationSpellsWndId/*0x10c36058*/);
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:181
// */
// [TempleDllLocation(0x1017eb80)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:184")]
// public bool CheckComplete()
// {
//   int result;
//   int i;
//
//   if ( _pkt.classCode == 16 || _pkt.classCode == 17 || _pkt.classCode == 8 )
//   {
//     i = 0;
//     if ( _pkt.spellEnumsAddedCount <= 0 )
//     {
// LABEL_8:
//       result = chargenSpellsReseted/*0x10c0eefc*/ == 0;
//     }
//     else
//     {
//       while ( _pkt.spellEnums[i] != 802 )
//       {
//         if ( ++i >= _pkt.spellEnumsAddedCount )
//         {
//           goto LABEL_8;
//         }
//       }
//       result = 0;
//     }
//   }
//   else
//   {
//     result = 1;
//   }
//   return result;
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:184
// */
// [TempleDllLocation(0x1017f0a0)]
// [TemplePlusLocation("ui_pc_creation_hooks.cpp:183")]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody a2)
// {
//   CharEditorSelectionPacket selPkt_1;
//   Stat classCode;
//   int numLearnable;
//   int i_1;
//   SpellEntryLevelSpec *lvlSpec;
//   int specLvl;
//   Stat classCode_1;
//   int v9;
//   int dom1;
//   int dom2;
//   int v12;
//   int result;
//   int v14;
//   int *v15;
//   int v16;
//   int v17;
//   int v18;
//   int v19;
//   int v20;
//   int i;
//
//   selPkt_1 = selPkt;
//   classCode = selPkt.classCode;
//   if ( (int)classCode < 9 || (int)classCode > 10 && classCode != 17 )
//   {
//     goto LABEL_25;
//   }
//   numLearnable = GameSystems.Spell.EnumerateLearnableSpells(*a2, chargenLearnables/*0x10c0ef88*/, 802);
//   i_1 = 0;
//   chargenSpellsNumLearnable/*0x10c360b0*/ = numLearnable;
//   i = 0;
//   if ( numLearnable <= 0 )
//   {
//     goto LABEL_25;
//   }
//   do
//   {
//     v20 = 0;
//     if ( !chargenLearnables/*0x10c0ef88*/[i_1].spellLvlsNum )
//     {
//       goto LABEL_24;
//     }
//     lvlSpec = chargenLearnables/*0x10c0ef88*/[i_1].spellLvls;
//     do
//     {
//       specLvl = lvlSpec.spellLevel;
//       if ( specLvl > 1 )
//       {
//         goto LABEL_22;
//       }
//       if ( lvlSpec.classCode & 0x80 && (classCode_1 = selPkt.classCode, (lvlSpec.classCode & 0x7F) == classCode_1) )
//       {
//         if ( classCode_1 != 17
//           || !specLvl
//           && (v9 = GameSystems.Spell.GetSpellSchoolEnum(chargenLearnables/*0x10c0ef88*/[i_1].spellEnum), v9 != _pkt.forbiddenSchool1)
//           && v9 != _pkt.forbiddenSchool2 )
//         {
//           v19 = lvlSpec.spellLevel;
//           v18 = lvlSpec.classCode;
// LABEL_21:
//           GameSystems.Spell.SpellKnownAdd(*a2, chargenLearnables/*0x10c0ef88*/[i_1].spellEnum, v18, v19, 1, 0);
//           goto LABEL_22;
//         }
//       }
//       else if ( selPkt.classCode == 9 && !(lvlSpec.classCode & 0x80) && specLvl == 1 )
//       {
//         dom1 = *a2.GetInt32(obj_f.critter_domain_1);
//         dom2 = *a2.GetInt32(obj_f.critter_domain_2);
//         v12 = lvlSpec.classCode & 0x7F;
//         if ( v12 == dom1 || v12 == dom2 )
//         {
//           v19 = lvlSpec.spellLevel;
//           v18 = lvlSpec.classCode;
//           goto LABEL_21;
//         }
//       }
// LABEL_22:
//       ++lvlSpec;
//       ++v20;
//     }
//     while ( v20 < chargenLearnables/*0x10c0ef88*/[i_1].spellLvlsNum );
//     selPkt_1 = selPkt;
//     numLearnable = chargenSpellsNumLearnable/*0x10c360b0*/;
// LABEL_24:
//     ++i_1;
//     ++i;
//   }
//   while ( i < numLearnable );
// LABEL_25:
//   result = selPkt_1.spellEnumsAddedCount;
//   v14 = 0;
//   if ( result > 0 )
//   {
//     v15 = (int *)selPkt_1.spellEnums;
//     while ( 1 )
//     {
//       v16 = *v15;
//       if ( (int)*v15 <= 802 )
//       {
//         goto LABEL_32;
//       }
//       if ( v16 >= 813 )
//       {
//         break;
//       }
// LABEL_34:
//       result = selPkt_1.spellEnumsAddedCount;
//       ++v14;
//       ++v15;
//       if ( v14 >= result )
//       {
//         return result;
//       }
//     }
//     if ( v16 <= 802 || v16 >= 813 )
//     {
// LABEL_32:
//       v17 = GameSystems.Spell.GetSpellLevelBySpellClass(v16, _pkt.classCode & 0x7F | 0x80);
//     }
//     else
//     {
//       v17 = v16 - 803;
//     }
//     GameSystems.Spell.SpellKnownAdd(*a2, *v15, selPkt_1.classCode & 0x7F | 0x80, v17, 1, 0);
//     goto LABEL_34;
//   }
//   return result;
// }
// /* Orphan comments:
// TP Replaced @ ui_pc_creation_hooks.cpp:183
// */
// [TempleDllLocation(0x1017ebd0)]
// public void ChargenSpellsButtonEntered()
// {
//   uint "TAG_CHARGEN_SPELLS";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_SPELLS");
// }
// }
//
// [TempleDllLocation(0x102f7b48)]
// internal class PortraitSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_PORTRAIT";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Portrait;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x1017ea70)]
//         public PortraitSystem()
//         {
//           int result;
//
//           chargenPortraitCapacity/*0x10c0eaa8*/ = 0;
//           chargenPortraitCount/*0x10c0eb64*/ = 0;
//           chargenPortraitIds/*0x10c0ed28*/ = 0;
//           if ( RegisterUiTexture/*0x101ee7b0*/("art\\interface\\pc_creation\\portrait_frame.tga", &chargenPortrait_portrait_frame/*0x10c0eef0*/) )
//           {
//             result = 0;
//           }
//           else
//           {
//             result = ChargenPortraitWidgetsInit/*0x1017e800*/(a1.width, a1.height) != 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x1017e420)]
// public void   Reset(CharEditorSelectionPacket a1)
//
// {
//   a1.portraitId = 0;
// }
//
// [TempleDllLocation(0x1017e7d0)]
// public int ChargenPortraitActivate()
// {
//   return ChargenPortraitActivateImpl/*0x1017e700*/();
// }
//
// [TempleDllLocation(0x1017e7e0)]
// public void Dispose()
// {
//   sub_1017E4D0/*0x1017e4d0*/();
//   if ( chargenPortraitIds/*0x10c0ed28*/ )
//   {
//     free(chargenPortraitIds/*0x10c0ed28*/);
//   }
// }
// [TempleDllLocation(0x1017eac0)]
// public void Resize(Size a1)
// {
//   sub_1017E4D0/*0x1017e4d0*/();
//   return ChargenPortraitWidgetsInit/*0x1017e800*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x1017e430)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(chargenPortraitWndId/*0x10c0eb60*/, 1);
// }
// [TempleDllLocation(0x1017e450)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(chargenPortraitWndId/*0x10c0eb60*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(chargenPortraitWndId/*0x10c0eb60*/);
// }
// [TempleDllLocation(0x1017e470)]
// public bool CheckComplete()
// {
//   return _pkt.portraitId != 0;
// }
// [TempleDllLocation(0x1017e480)]
// public void Finalize(CharEditorSelectionPacket a1, GameObjectBody a2)
// {
//   return *a2.SetInt32(obj_f.critter_portrait, *(_DWORD *)(a1 + 3640));
// }
// [TempleDllLocation(0x1017e4b0)]
// public void ChargenPortraitBtnEntered()
// {
//   uint "TAG_CHARGEN_PORTRAIT";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_PORTRAIT");
// }
// }
//
// [TempleDllLocation(0x102f7b74)]
// internal class VoiceSystem : IChargenSystem {
//         public string Name => "TAG_CHARGEN_VOICE";
//
//         public ChargenStages Stage => ChargenStages.CG_Stage_Voice;
//
//         public WidgetContainer Container { get; private set; }
//
//         [TempleDllLocation(0x1017e220)]
//         public VoiceSystem()
//         {
//           int result;
//           int v2;
//           string meslineValue;
//           int meslineKey;
//
//           stru_10C0C700/*0x10c0c700*/.textColor = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0C700/*0x10c0c700*/.colors4 = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0C700/*0x10c0c700*/.colors2 = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0CD90/*0x10c0cd90*/.textColor = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0CD90/*0x10c0cd90*/.colors4 = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0CD90/*0x10c0cd90*/.colors2 = (ColorRect *)&unk_102FCDF8/*0x102fcdf8*/;
//           stru_10C0C700/*0x10c0c700*/.flags = 0x4000;
//           stru_10C0C700/*0x10c0c700*/.field2c = -1;
//           stru_10C0C700/*0x10c0c700*/.shadowColor = (ColorRect *)&unk_102FCE18/*0x102fce18*/;
//           stru_10C0C700/*0x10c0c700*/.field0 = 0;
//           stru_10C0C700/*0x10c0c700*/.kerning = 1;
//           stru_10C0C700/*0x10c0c700*/.leading = 0;
//           stru_10C0C700/*0x10c0c700*/.tracking = 3;
//           stru_10C0CD90/*0x10c0cd90*/.flags = 0x4000;
//           stru_10C0CD90/*0x10c0cd90*/.field2c = -1;
//           stru_10C0CD90/*0x10c0cd90*/.shadowColor = (ColorRect *)&unk_102FCE18/*0x102fce18*/;
//           stru_10C0CD90/*0x10c0cd90*/.field0 = 0;
//           stru_10C0CD90/*0x10c0cd90*/.kerning = 1;
//           stru_10C0CD90/*0x10c0cd90*/.leading = 0;
//           stru_10C0CD90/*0x10c0cd90*/.tracking = 3;
//           stru_10C0C7E0/*0x10c0c7e0*/.flags = 0x4000;
//           stru_10C0C7E0/*0x10c0c7e0*/.field2c = -1;
//           stru_10C0C7E0/*0x10c0c7e0*/.textColor = (ColorRect *)&unk_102FCE08/*0x102fce08*/;
//           stru_10C0C7E0/*0x10c0c7e0*/.shadowColor = (ColorRect *)&unk_102FCE18/*0x102fce18*/;
//           stru_10C0C7E0/*0x10c0c7e0*/.colors4 = (ColorRect *)&unk_102FCE08/*0x102fce08*/;
//           stru_10C0C7E0/*0x10c0c7e0*/.colors2 = (ColorRect *)&unk_102FCE08/*0x102fce08*/;
//           stru_10C0C7E0/*0x10c0c7e0*/.field0 = 0;
//           stru_10C0C7E0/*0x10c0c7e0*/.kerning = 1;
//           stru_10C0C7E0/*0x10c0c7e0*/.leading = 0;
//           stru_10C0C7E0/*0x10c0c7e0*/.tracking = 3;
//           stru_10C0C680/*0x10c0c680*/.flags = 0x4000;
//           stru_10C0C680/*0x10c0c680*/.field2c = -1;
//           stru_10C0C680/*0x10c0c680*/.textColor = (ColorRect *)&unk_102FCE28/*0x102fce28*/;
//           stru_10C0C680/*0x10c0c680*/.shadowColor = (ColorRect *)&unk_102FCE18/*0x102fce18*/;
//           stru_10C0C680/*0x10c0c680*/.colors4 = (ColorRect *)&unk_102FCE08/*0x102fce08*/;
//           stru_10C0C680/*0x10c0c680*/.colors2 = (ColorRect *)&unk_102FCE08/*0x102fce08*/;
//           stru_10C0C680/*0x10c0c680*/.field0 = 0;
//           stru_10C0C680/*0x10c0c680*/.kerning = 1;
//           stru_10C0C680/*0x10c0c680*/.leading = 0;
//           stru_10C0C680/*0x10c0c680*/.tracking = 3;
//           dword_10C0C834/*0x10c0c834*/ = Globals.UiAssets.LoadImg("art\\interface\\pc_creation\\bigvoicebox.img");
//           if ( dword_10C0C834/*0x10c0c834*/
//                && (meslineKey = 23000, Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline))
//                && (dword_10C0C7C0/*0x10c0c7c0*/ = (string )meslineValue, meslineKey = 23001, Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline))
//                && (dword_10C0C830/*0x10c0c830*/ = (string )meslineValue, meslineKey = 23002, Mesfile_GetLine/*0x101e6760*/(pc_creationMes/*0x11e72ef0*/, &mesline)) )
//           {
//             v2 = conf.height;
//             dword_10C0D304/*0x10c0d304*/ = (string )meslineValue;
//             result = UiPcCreationNameWidgetsInit/*0x1017de80*/(conf.width, v2) != 0;
//           }
//           else
//           {
//             result = 0;
//           }
//           return result;
//         }
//
//         [TempleDllLocation(0x1017d5a0)]
// public CharEditorSelectionPacket   ChargenVoiceInit(CharEditorSelectionPacket a1)
// {
//   CharEditorSelectionPacket result;
//
//   result = a1;
//   a1.voiceFile[0] = 0;
//   a1.voiceId = -1;
//   return result;
// }
//
// [TempleDllLocation(0x1017dbf0)]
// public int ChargenVoiceActivate()
// {
//   int v0;
//   int i_1;
//   int v2;
//   int v3;
//   int i;
//   CHAR v6;
//   _WORD *v7;
//   CHAR v8;
//
//   voiceFileLen/*0x10c0c570*/ = strlen(_pkt.voiceFile);
//   i = 0;
//   do
//   {
//     v6 = _pkt.voiceFile[i];
//     byte_10C0C9F0/*0x10c0c9f0*/[i++] = v6;
//   }
//   while ( v6 );
//   v7 = &byte_10C0C9F0/*0x10c0c9f0*/[-1];
//   do
//   {
//     v8 = *((_BYTE *)v7 + 1);
//     v7 = (_WORD *)((string )v7 + 1);
//   }
//   while ( v8 );
//   *v7 = consoleMarker/*0x10299060*/;
//   v0 = sub_10034880/*0x10034880*/();
//   i_1 = 0;
//   uiPcCreationNamesScrollbarY/*0x10c0c6e4*/ = 0;
//   for ( dword_10C0C574/*0x10c0c574*/ = 0; i_1 <= v0; ++i_1 )
//   {
//     if ( sub_100347D0/*0x100347d0*/(UiSystems.PCCreation.charEditorObjHnd, i_1) )
//     {
//       voiceIds/*0x10c0c578*/[dword_10C0C574/*0x10c0c574*/] = i_1;
//       v2 = sub_10034840/*0x10034840*/(i_1);
//       v3 = dword_10C0C574/*0x10c0c574*/;
//       *(_DWORD *)&dword_10C0C838/*0x10c0c838*/[4 * dword_10C0C574/*0x10c0c574*/] = v2;
//       dword_10C0C574/*0x10c0c574*/ = v3 + 1;
//     }
//   }
//   j_WidgetCopy/*0x101f87a0*/(uiPcCreationNamesScrollbarId/*0x10c0c9e8*/, (LgcyWidget *)&uiPcCreationNamesScrollbar/*0x10c0c938*/);
//   uiPcCreationNamesScrollbar/*0x10c0c938*/.yMax = (dword_10C0C574/*0x10c0c574*/ - 7) & ((dword_10C0C574/*0x10c0c574*/ - 7 < 0) - 1);
//   uiPcCreationNamesScrollbar/*0x10c0c938*/.scrollbarY = uiPcCreationNamesScrollbar/*0x10c0c938*/.yMin;
//   uiPcCreationNamesScrollbarY/*0x10c0c6e4*/ = uiPcCreationNamesScrollbar/*0x10c0c938*/.yMin;
//   return j_ui_widget_set/*0x101f87b0*/(uiPcCreationNamesScrollbarId/*0x10c0c9e8*/, &uiPcCreationNamesScrollbar/*0x10c0c938*/);
// }
//
// [TempleDllLocation(0x1017dc40)]
// public void Dispose()
// {
//   sub_1017D680/*0x1017d680*/();
//   j__free/*0x101f7cd0*/(dword_10C0C834/*0x10c0c834*/);
// }
// [TempleDllLocation(0x1017e400)]
// public void Resize(Size a1)
// {
//   sub_1017D680/*0x1017d680*/();
//   return UiPcCreationNameWidgetsInit/*0x1017de80*/(*(_DWORD *)(a1 + 12), *(_DWORD *)(a1 + 16));
// }
// [TempleDllLocation(0x1017d5c0)]
// public void Hide()
// {
//   return WidgetSetHidden/*0x101f9100*/(uiPcCreationNamesWndId/*0x10c0c6d0*/, 1);
// }
// [TempleDllLocation(0x1017d5e0)]
// public void Show()
// {
//   WidgetSetHidden/*0x101f9100*/(uiPcCreationNamesWndId/*0x10c0c6d0*/, 0);
//   return WidgetBringToFront/*0x101f8e40*/(uiPcCreationNamesWndId/*0x10c0c6d0*/);
// }
// [TempleDllLocation(0x1017d600)]
// public bool CheckComplete()
// {
//   return _pkt.voiceFile[0] && chargenCurVoiceId/*0x11e73e3c*/ != -1;
// }
// [TempleDllLocation(0x1017d620)]
// public void Finalize(CharEditorSelectionPacket selPkt, GameObjectBody a2)
// {
//   SetPcPlayerNameField/*0x100a0490*/(*a2, obj_f.pc_player_name, selPkt.voiceFile);
//   return PcVoiceSet/*0x100347b0*/(*a2, selPkt.voiceId);
// }
// [TempleDllLocation(0x1017d660)]
// public void ChargenVoiceButtonEntered()
// {
//   uint "TAG_CHARGEN_VOICE";
//   UiPcCreationButtonEnteredHandler/*0x1011b890*/("TAG_CHARGEN_VOICE");
// }
// }
}