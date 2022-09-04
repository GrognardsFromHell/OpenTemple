using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.Camping;

public class CampingUi : ISaveGameAwareUi, IResetAwareSystem, IDisposable
{
    [TempleDllLocation(0x10be2af0)]
    private int uiCampingDaysToRest;

    [TempleDllLocation(0x10be28fc)]
    private int uiCampingHoursToRest;

    private readonly Dictionary<int, string> _translations;

    [TempleDllLocation(0x1012e440)]
    public bool IsHidden => !_mainWindow.IsInTree;

    public bool IsVisible => !IsHidden;

    [TempleDllLocation(0x10be2ac8)]
    private string WindowTitle => _translations[100];

    [TempleDllLocation(0x10be2ba8)]
    private string DaysLabel => _translations[102];

    [TempleDllLocation(0x10be206c)]
    private string HoursLabel => _translations[103];

    [TempleDllLocation(0x10be2304)]
    private string ButtonLabelRest => _translations[107];

    [TempleDllLocation(0x10be2070)]
    private string ButtonLabelPassTime => _translations[108];

    [TempleDllLocation(0x10be2464)]
    private string ButtonLabelCancel => _translations[109];

    [TempleDllLocation(0x10be2454)]
    private string UntilHealedLabel => _translations[104];

    [TempleDllLocation(0x10be2074)]
    private string UntilEveningLabel => _translations[105];

    [TempleDllLocation(0x10be2460)]
    private string UntilMorningLabel => _translations[106];

    [TempleDllLocation(0x10be29e4)]
    private string PassingTimeHintTitle => _translations[1000];

    [TempleDllLocation(0x10be2318)]
    private string PassingTimeHintBody => _translations[1001];

    [TempleDllLocation(0x10be2300)]
    private string PassingTimeHintNeverShowAgain => _translations[1002];

    [TempleDllLocation(0x10be2824)]
    private readonly WidgetContainer _mainWindow;

    [TempleDllLocation(0x10be2924)]
    private readonly WidgetButton _restButton;

    [TempleDllLocation(0x10be22fc)]
    private readonly WidgetButton _cancelButton;

    [TempleDllLocation(0x10be22f0)]
    private readonly WidgetButton _incrementDaysButton;

    [TempleDllLocation(0x10be22dc)]
    private readonly WidgetButton _decrementDaysButton;

    [TempleDllLocation(0x10be245c)]
    private readonly WidgetButton _incrementHoursButton;

    [TempleDllLocation(0x10be2458)]
    private readonly WidgetButton _decrementHoursButton;

    [TempleDllLocation(0x10be2bac)]
    private readonly CampingCheckbox _restUntilHealedCheckbox;

    [TempleDllLocation(0x10be2838)]
    private readonly CampingCheckbox _restUntilNightCheckbox;

    [TempleDllLocation(0x10be2910)]
    private readonly CampingCheckbox _restUntilDayCheckbox;

    private readonly WidgetText _daysToRestText;
    private readonly WidgetText _hoursToRestText;
    private readonly WidgetText _hoursToRestLabelText;
    private readonly WidgetText _daysToRestLabelText;

    // TODO: This is the checkbox state saved from last time
    [TempleDllLocation(0x10be2b38)]
    private int uiCampingDefinition;

    [TempleDllLocation(0x10be2c18)]
    private bool _confirmPassingTime;

    public CampingUi()
    {
        _translations = Tig.FS.ReadMesFile("mes/utility_bar.mes");

        var doc = WidgetDoc.Load("ui/camping_ui.json");

        // Begin top level window
        // Created @ 0x1012f29f
        _mainWindow = doc.GetRootContainer();
        // _mainWindow.OnBeforeRender += 0x1012e4d0;
        _mainWindow.PreventsInGameInteraction = true;
        _mainWindow.AddHotkey(UiHotkeys.CloseWindow, Hide);
        _mainWindow.ZIndex = 100000;
        _mainWindow.OnBeforeRender += UpdateCheckboxes;

        var titleLabel = new WidgetText(WindowTitle, "camping-button-text");
        titleLabel.X = 31;
        titleLabel.Y = 11;
        titleLabel.FixedSize = new Size(230, 12);
        _mainWindow.AddContent(titleLabel);

        // Labels for the hours/days to rest

        _daysToRestLabelText = new WidgetText(DaysLabel, "camping-torest-labels");
        _mainWindow.AddContent(_daysToRestLabelText);
        _hoursToRestLabelText = new WidgetText(HoursLabel, "camping-torest-labels");
        _mainWindow.AddContent(_hoursToRestLabelText);
        _daysToRestText = new WidgetText("0", "camping-torest");
        _mainWindow.AddContent(_daysToRestText);
        _hoursToRestText = new WidgetText("0", "camping-torest");
        _mainWindow.AddContent(_hoursToRestText);

        _restButton = doc.GetButton("restButton");
        _restButton.AddClickListener(OnRestClicked);

        _cancelButton = doc.GetButton("cancelButton");
        _cancelButton.Text = ButtonLabelCancel;
        _cancelButton.AddClickListener(Hide);

        _incrementDaysButton = doc.GetButton("incDaysButton");
        _incrementDaysButton.IsRepeat = true;
        _incrementDaysButton.AddClickListener(OnIncrementDays);

        _decrementDaysButton = doc.GetButton("decDaysButton");
        _decrementDaysButton.IsRepeat = true;
        _decrementDaysButton.AddClickListener(OnDecrementDays);

        _incrementHoursButton = doc.GetButton("incHoursButton");
        _incrementHoursButton.IsRepeat = true;
        _incrementHoursButton.AddClickListener(OnIncrementHours);

        _decrementHoursButton = doc.GetButton("decHoursButton");
        _decrementHoursButton.IsRepeat = true;
        _decrementHoursButton.AddClickListener(OnDecrementHours);

        _restUntilHealedCheckbox = new CampingCheckbox(new Rectangle(86, 96, 113, 15), UntilHealedLabel,
            "camping-checkbox-labels");
        _restUntilHealedCheckbox.OnCheckedChange += value => CampingSetTimeUntilHealed();
        _mainWindow.Add(_restUntilHealedCheckbox);

        _restUntilNightCheckbox = new CampingCheckbox(new Rectangle(86, 117, 113, 15), UntilEveningLabel,
            "camping-checkbox-labels");
        _restUntilNightCheckbox.OnCheckedChange += value => UiCampingSetTimeToUntilNighttime();
        _mainWindow.Add(_restUntilNightCheckbox);

        _restUntilDayCheckbox = new CampingCheckbox(new Rectangle(86, 138, 113, 15), UntilMorningLabel,
            "camping-checkbox-labels");
        _restUntilDayCheckbox.OnCheckedChange += value => UiCampingSetTimeToUntilDaytime();
        _mainWindow.Add(_restUntilDayCheckbox);

        // Begin top level window
        // Created @ 0x1019b2c8
        // var @ [TempleDllLocation(0x11e72ad8)]
        var sticky_ui_main_window1 = new WidgetContainer(new Rectangle(0, 0, 0, 0));
        // sticky_ui_main_window1.OnHandleMessage += 0x101f5850;
        // sticky_ui_main_window1.OnBeforeRender += 0x1019a9a0;
        sticky_ui_main_window1.ZIndex = 0;
        sticky_ui_main_window1.Name = "sticky_ui_main_window";
        sticky_ui_main_window1.Visible = false; // TODO
        // Created @ 0x1019b39a
        // var @ [TempleDllLocation(0x11e7277c)]
        var radialmenuslideracceptbutton1 = new WidgetButton(new Rectangle(328, 370, 112, 22));
        // radialmenuslideracceptbutton1.OnHandleMessage += 0x1019af30;
        // radialmenuslideracceptbutton1.OnBeforeRender += 0x1019ac10;
        radialmenuslideracceptbutton1.Name = "radial menu slider accept button";
        sticky_ui_main_window1.Add(radialmenuslideracceptbutton1);
        // Created @ 0x1019b4a1
        // var @ [TempleDllLocation(0x11e72ad4)]
        var radialmenusliderdeclinebutton1 = new WidgetButton(new Rectangle(452, 370, 112, 22));
        // radialmenusliderdeclinebutton1.OnHandleMessage += 0x1019af30;
        // radialmenusliderdeclinebutton1.OnBeforeRender += 0x1019ac10;
        radialmenusliderdeclinebutton1.Name = "radial menu slider decline button";
        sticky_ui_main_window1.Add(radialmenusliderdeclinebutton1);
        // Created @ 0x1019b5a9
        // var @ [TempleDllLocation(0x11e72b9c)]
        var radialmenuslidercheckboxbutton1 = new WidgetButton(new Rectangle(335, 354, 40, 11));
        // radialmenuslidercheckboxbutton1.OnHandleMessage += 0x1019b1d0;
        // radialmenuslidercheckboxbutton1.OnBeforeRender += 0x1019afa0;
        radialmenuslidercheckboxbutton1.Name = "radial menu slider checkbox button";
        sticky_ui_main_window1.Add(radialmenuslidercheckboxbutton1);
    }

    [TempleDllLocation(0x1012ee50)]
    private void OnRestClicked()
    {
        Hide();

        var fadeArgs = FadeArgs.Default;
        fadeArgs.fadeSteps = 48;
        fadeArgs.transitionTime = 1.0f;
        fadeArgs.color = PackedLinearColorA.Black;
        var hoursToRest = UiSystems.Camping.uiCampingHoursToRest
                          + 24 * UiSystems.Camping.uiCampingDaysToRest;
        GameSystems.GFade.PerformFade(ref fadeArgs)
            .ContinueWith(_ => Rest(hoursToRest));
    }

    [TempleDllLocation(0x1010ef00)]
    private bool Rest(int hoursToRest)
    {
        var restedHours = 0;
        var canHeal = true;
        var completedSuccessfully = true;

        if (hoursToRest > 0)
        {
            var healerMod = GetHealingAmountMod();

            while (restedHours < hoursToRest)
            {
                GameSystems.TimeEvent.AddGameTime(TimeSpan.FromHours(1));

                if (GameSystems.RandomEncounter.SleepStatus == SleepStatus.Dangerous)
                {
                    var query = new RandomEncounterQuery(default, RandomEncounterType.Resting);
                    if (GameSystems.RandomEncounter.Query(query, out var encounter))
                    {
                        completedSuccessfully = false;
                        GameSystems.RandomEncounter.CreateEncounter(encounter);
                        break;
                    }
                }

                restedHours++;
            }

            // apply healing
            if (restedHours >= 8)
            {
                if (GameSystems.RandomEncounter.SleepStatus == SleepStatus.PassTimeOnly)
                {
                    canHeal = false;
                }

                var restPeriods = (restedHours - 8) / 24 + 1;

                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    var healAmt = GetHealingAmount(partyMember, restPeriods);
                    if (GameSystems.Map.IsCurrentMapBedrest)
                    {
                        healAmt *= 2;
                    }

                    if (restedHours >= 24)
                    {
                        healAmt += healerMod * restPeriods;
                    }

                    // heal damage
                    if (canHeal)
                    {
                        GameSystems.Spell.SanitizeSpellSlots(partyMember);
                        GameSystems.Spell.PendingSpellsToMemorized(partyMember);
                        partyMember.ClearArray(obj_f.critter_spells_cast_idx);
                        GameSystems.Combat.Heal(partyMember, partyMember, Dice.Constant(healAmt),
                            D20ActionType.NONE);
                    }

                    // heal subdual
                    GameSystems.Critter.HealSubdualSub_100B9030(partyMember, healAmt);

                    // dispatch NewDayRest event(s)
                    if (canHeal)
                    {
                        for (var iRestPeriod = 0; iRestPeriod < restPeriods; iRestPeriod++)
                        {
                            partyMember.DispatchRested();
                        }
                    }
                }
            }
        }

        GameSystems.RandomEncounter.UpdateSleepStatus();

        var fadeArgs = FadeArgs.Default;
        fadeArgs.flags = FadeFlag.FadeIn;
        fadeArgs.fadeSteps = 48;
        fadeArgs.transitionTime = 1.0f;
        GameSystems.GFade.PerformFade(ref fadeArgs);

        int TUTORIAL_MAP_ID = 5117;
        if (GameSystems.Map.GetCurrentMapId() == TUTORIAL_MAP_ID & !GameSystems.Script.GetGlobalFlag(3))
        {
            GameUiBridge.EnableTutorial();
            GameUiBridge.ShowTutorialTopic(TutorialTopic.MultipleCharacters);
            GameSystems.Script.SetGlobalFlag(3, true);
        }

        return completedSuccessfully;
    }

    [TempleDllLocation(0x1012ef40)]
    private void OnIncrementDays()
    {
        uiCampingDaysToRest++;
        if (uiCampingDaysToRest > 99)
        {
            uiCampingDaysToRest = 99;
        }

        uiCampingDefinition = 0;
        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012efb0)]
    private void OnDecrementDays()
    {
        if (--uiCampingDaysToRest < 0)
        {
            uiCampingDaysToRest = 0;
        }

        uiCampingDefinition = 0;
        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012ef40)]
    private void OnIncrementHours()
    {
        uiCampingHoursToRest++;
        if (uiCampingHoursToRest > 99)
        {
            uiCampingHoursToRest = 99;
        }

        uiCampingDefinition = 0;
        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012efb0)]
    private void OnDecrementHours()
    {
        if (--uiCampingHoursToRest < 0)
        {
            uiCampingHoursToRest = 0;
        }

        uiCampingDefinition = 0;
        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012e310)]
    public void Reset()
    {
        uiCampingDaysToRest = 0;
        uiCampingHoursToRest = 8;
    }

    [TempleDllLocation(0x1012edd0)]
    public void Dispose()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x1012edf0)]
    [TempleDllLocation(0x1012eef0)]
    public void Hide()
    {
        if (Globals.UiManager.RemoveWindow(_mainWindow))
        {
            GameSystems.TimeEvent.ResumeGameTime();
        }
    }

    [TempleDllLocation(0x1012f0c0)]
    public void Show()
    {
        var sleepStatus = GameSystems.RandomEncounter.SleepStatus;
        if (GameSystems.Combat.IsCombatActive() || sleepStatus == SleepStatus.Impossible)
        {
            return;
        }

        GameSystems.TimeEvent.PauseGameTime();
        UiSystems.HideOpenedWindows(true);

        Globals.UiManager.ShowModal(_mainWindow);

        if (sleepStatus == SleepStatus.PassTimeOnly)
        {
            _restButton.Text = ButtonLabelPassTime;
            throw new NotImplementedException();
//                TODO
//                if (!uiStickyStatePtr /*0x10be2c18*/)
//                {
//                    v0 = PassingTimeHintNeverShowAgain;
//                    v1 = (string) GameSystems.D20.Combat.GetCombatMesLine(6010);
//                    v2 = (string) GameSystems.D20.Combat.GetCombatMesLine(6009);
//                    UiStickyShow /*0x1019a920*/(
//                        PassingTimeHintTitle,
//                        PassingTimeHintBody,
//                        v2,
//                        v1,
//                        v0,
//                        &uiStickyStatePtr /*0x10be2c18*/,
//                        UiStickyOnEndFunc /*0x1012f080*/);
//                }
        }
        else
        {
            _restButton.Text = ButtonLabelRest;
        }

        switch (uiCampingDefinition)
        {
            case 1:
                CampingSetTimeUntilHealed();
                break;
            case 2:
                UiCampingSetTimeToUntilNighttime();
                break;
            case 3:
                UiCampingSetTimeToUntilDaytime();
                break;
        }

        if (uiCampingDaysToRest <= 0 && uiCampingHoursToRest <= 0)
        {
            uiCampingDefinition = 0;
            uiCampingDaysToRest = 0;
            uiCampingHoursToRest = 8;
        }

        UiCampingTimeToRestTextUpdate();
    }

    private void UpdateCheckboxes()
    {
        if (GameSystems.RandomEncounter.SleepStatus == SleepStatus.PassTimeOnly)
        {
            _restUntilHealedCheckbox.Visible = false;
            _restUntilDayCheckbox.Visible = false;
            _restUntilNightCheckbox.Visible = false;
        }
        else
        {
            _restUntilHealedCheckbox.Visible = true;
            _restUntilDayCheckbox.Visible = true;
            _restUntilNightCheckbox.Visible = true;

            _restUntilHealedCheckbox.Checked = false;
            _restUntilDayCheckbox.Checked = false;
            _restUntilNightCheckbox.Checked = false;
            if (uiCampingDefinition == 1)
            {
                _restUntilHealedCheckbox.Checked = true;
            }
            else if (uiCampingDefinition == 2)
            {
                _restUntilNightCheckbox.Checked = true;
            }
            else if (uiCampingDefinition == 3)
            {
                _restUntilDayCheckbox.Checked = true;
            }
        }
    }

    [TempleDllLocation(0x1012ea20)]
    private void UiCampingTimeToRestTextUpdate()
    {
        _daysToRestText.Text = uiCampingDaysToRest.ToString(CultureInfo.InvariantCulture);
        _hoursToRestText.Text = uiCampingHoursToRest.ToString(CultureInfo.InvariantCulture);

        var daysSize = _daysToRestText.GetPreferredSize();
        var hoursSize = _hoursToRestText.GetPreferredSize();

        var dayLabelSize = _daysToRestLabelText.GetPreferredSize();
        var hoursLabelSize = _hoursToRestLabelText.GetPreferredSize();

        var daysLeft = (82 - dayLabelSize.Width - daysSize.Width) / 2;
        _daysToRestText.X = daysLeft + 40;
        _daysToRestText.Y = 85 - daysSize.Height;

        _daysToRestLabelText.X = daysLeft + daysSize.Width + 44;
        _daysToRestLabelText.Y = 85 - dayLabelSize.Height;

        var hoursLeft = (82 - hoursLabelSize.Width - hoursSize.Width) / 2;
        _hoursToRestText.X = hoursLeft + 149;
        _hoursToRestText.Y = 85 - hoursSize.Height;

        _hoursToRestLabelText.X = hoursLeft + hoursSize.Width + 153;
        _hoursToRestLabelText.Y = 85 - hoursLabelSize.Height;

        _incrementDaysButton.Disabled = uiCampingDaysToRest >= 99;
        _decrementDaysButton.Disabled = uiCampingDaysToRest <= 0;

        _incrementHoursButton.Disabled = uiCampingHoursToRest >= 99;
        _decrementHoursButton.Disabled = uiCampingHoursToRest <= 0;
    }

    [TempleDllLocation(0x1012ed50)]
    private void UiCampingSetTimeToUntilNighttime()
    {
        uiCampingDefinition = 2;
        uiCampingDaysToRest = 0;
        uiCampingHoursToRest = 0;

        while (GameSystems.TimeEvent.IsDaytimeInHours(uiCampingHoursToRest))
        {
            uiCampingHoursToRest++;
        }

        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012ed90)]
    private void UiCampingSetTimeToUntilDaytime()
    {
        uiCampingDefinition = 3;
        uiCampingDaysToRest = 0;
        uiCampingHoursToRest = 0;

        while (!GameSystems.TimeEvent.IsDaytimeInHours(uiCampingHoursToRest))
        {
            uiCampingHoursToRest++;
        }

        UiCampingTimeToRestTextUpdate();
    }

    [TempleDllLocation(0x1012ec70)]
    [TemplePlusLocation("ui_systems_hooks.cpp:33")]
    private void CampingSetTimeUntilHealed()
    {
        uiCampingDefinition = 1;
        uiCampingHoursToRest = 0;
        uiCampingDaysToRest = 0;
        if (GameSystems.RandomEncounter.SleepStatus != SleepStatus.PassTimeOnly)
        {
            var healMod = GetHealingAmountMod();
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var curHp = partyMember.GetStat(Stat.hp_current);
                var maxHp = partyMember.GetStat(Stat.hp_max);
                var healQuantum = GetHealingAmount(partyMember, 1) + healMod;

                var hours = 24 * ((healQuantum - curHp + maxHp - 1) / healQuantum);
                if (curHp < maxHp && hours < 24)
                {
                    hours = 24;
                }

                if (hours > uiCampingHoursToRest)
                {
                    uiCampingHoursToRest = hours;
                }
            }

            uiCampingDaysToRest = uiCampingHoursToRest / 24;
            uiCampingHoursToRest %= 24;
        }

        UiCampingTimeToRestTextUpdate();
    }

    private int GetHealingAmountMod()
    {
        var healerMod = 0;
        // safe resting place or camping in wilderness
        if (GameSystems.RandomEncounter.SleepStatus <= SleepStatus.Dangerous)
        {
            // find someone who can cast heal spells
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var maxClrSpellLvl = GameSystems.Spell.GetMaxSpellLevel(partyMember, Stat.level_cleric);
                var maxDrdSpellLvl = GameSystems.Spell.GetMaxSpellLevel(partyMember, Stat.level_druid);
                healerMod = Math.Max(healerMod, Math.Max(maxClrSpellLvl * 5, maxDrdSpellLvl * 4));
            }
        }

        return healerMod;
    }

    /// <summary>
    /// Returns how much the critter heals naturally through resting.
    /// </summary>
    private int GetHealingAmount(GameObject critter, int restPeriods)
    {
        var lvl = critter.GetStat(Stat.level);
        var hdCount = GameSystems.Critter.GetHitDiceNum(critter);
        var healQuantum = Math.Max(hdCount, lvl);
        return healQuantum * restPeriods;
    }

    [TempleDllLocation(0x1012e330)]
    public void SaveGame(SavedUiState savedState)
    {
        savedState.CampingState = new SavedCampingUiState
        {
            CampingType = uiCampingDefinition,
            DaysToRest = uiCampingDaysToRest,
            HoursToRest = uiCampingHoursToRest,
            DontAskToPassTime = !_confirmPassingTime
        };
    }

    [TempleDllLocation(0x1012e3b0)]
    public void LoadGame(SavedUiState savedState)
    {
        var campingState = savedState.CampingState;
        uiCampingDefinition = campingState.CampingType;
        uiCampingDaysToRest = campingState.DaysToRest;
        uiCampingHoursToRest = campingState.HoursToRest;
        _confirmPassingTime = !campingState.DontAskToPassTime;
    }
}