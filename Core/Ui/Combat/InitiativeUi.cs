using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Combat;

public class InitiativeUi
{
    /// <summary>
    ///     The margin between portraits (of any size).
    /// </summary>
    private const int PortraitMargin = 5;

    private static readonly Size PortraitSize = new(59, 56);

    private static readonly Size SmallPortraitSize = new(47, 42);

    private List<PortraitRecord> _currentPortraits = new();

    private readonly WidgetContainer _portraitContainer;

    [TempleDllLocation(0x10BE700C)]
    internal bool draggingPortrait;

    [TempleDllLocation(0x10BE7010)]
    internal bool actorCanChangeInitiative;

    [TempleDllLocation(0x10be7004)]
    internal bool uiPortraitState1;

    [TempleDllLocation(0x102f9af0)]
    internal int initiativeSwapSourceIndex;

    [TempleDllLocation(0x102f9af4)]
    internal int initiativeSwapTargetIndex;

    [TempleDllLocation(0x10be6ff0)]
    private int uiCombatInitiativeNumActivePortraits = 0;

    [TempleDllLocation(0x10be6ff4)]
    private bool uiCombatPortraitsNeedRefresh_10BE6FF4;

    [TempleDllLocation(0x10be6f74)]
    private PortraitRecord[] uiInitiativeMiniPortraits;

    [TempleDllLocation(0x10be6f0c)]
    private PortraitRecord[] uiInitiativeSmallPortraits;

    [TempleDllLocation(0x10be7008)]
    internal bool swapPortraitsForDragAndDrop; // Used for drag&drop methinks

    [TempleDllLocation(0x101430b0)]
    public InitiativeUi()
    {
        _portraitContainer = new WidgetContainer();
        _portraitContainer.PixelSize = new SizeF(
            Globals.UiManager.CanvasSize.Width - 2,
            100 - 12
        );

        _portraitContainer.ZIndex = 90550;
        Globals.UiManager.AddWindow(_portraitContainer);
    }

    [TempleDllLocation(0x101414b0)]
    public void Reset()
    {
        Update();
        // TODO: The original function did a lot more here, but this might actually be enough...
    }

    [TempleDllLocation(0x10143180)]
    public void Update()
    {
        uiCombatPortraitsNeedRefresh_10BE6FF4 = true;
        UpdateIfNeeded();
    }

    [TempleDllLocation(0x10142740)]
    public void UpdateIfNeeded()
    {
        if (!uiCombatPortraitsNeedRefresh_10BE6FF4)
        {
            return;
        }

        var combatantCount = GameSystems.D20.Initiative.Count;

        // Check if we need small portraits or not
        var availableWidth = _portraitContainer.ContentArea.Width;
        var requiredWidthNormal = PortraitSize.Width * combatantCount + (combatantCount - 1) * PortraitMargin;
        var useSmallPortraits = requiredWidthNormal > availableWidth;

        // Reassemble the list of portraits and try to reuse portraits as we go
        Span<bool> reused = stackalloc bool[_currentPortraits.Count];
        var portraits = new List<PortraitRecord>(combatantCount);
        var currentX = 0f;
        foreach (var combatant in GameSystems.D20.Initiative)
        {
            var index = portraits.Count;

            // Try finding an existing portrait
            var reusedPortrait = false;
            for (var i = 0; i < _currentPortraits.Count; i++)
            {
                if (_currentPortraits[i].Combatant == combatant && !reused[i])
                {
                    // Strike! We found one to reuse
                    reusedPortrait = true;
                    _currentPortraits[i].Container.X = currentX;
                    portraits.Add(_currentPortraits[i]);
                    reused[i] = true;
                    break;
                }
            }

            if (!reusedPortrait)
            {
                portraits.Add(CreatePortraitRecord(combatant, useSmallPortraits));
            }

            var preferredSize = portraits[index].Container.ComputePreferredBorderAreaSize();
            currentX += preferredSize.Width + PortraitMargin;
        }

        // Free all portraits that were not reused
        for (var i = 0; i < _currentPortraits.Count; i++)
        {
            if (!reused[i])
            {
                _portraitContainer.Remove(_currentPortraits[i].Container);
            }
        }

        _currentPortraits = portraits;
    }

    private PortraitRecord CreatePortraitRecord(GameObject combatant, bool useSmallPortraits)
    {
        var container = new WidgetContainer();
        container.PixelSize = useSmallPortraits ? SmallPortraitSize : PortraitSize;
        container.Y = 5;
        container.ClipChildren = false;

        var button = new InitiativePortraitButton(combatant, useSmallPortraits);
        container.Add(button);

        _portraitContainer.Add(container);

        return new PortraitRecord(combatant, container, button, useSmallPortraits);
    }

    [TempleDllLocation(0x10141760)]
    public ActionCursor? GetCursor()
    {
        if (draggingPortrait)
        {
            return actorCanChangeInitiative ? ActionCursor.SlidePortraits : ActionCursor.InvalidSelection;
        }
        else
        {
            return null;
        }
    }

    private readonly struct PortraitRecord
    {
        public readonly GameObject Combatant;

        public readonly WidgetContainer Container;

        public readonly InitiativePortraitButton Button;

        public readonly bool SmallPortrait;

        public PortraitRecord(GameObject combatant,
            WidgetContainer container,
            InitiativePortraitButton button,
            bool smallPortrait)
        {
            Combatant = combatant;
            Container = container;
            Button = button;
            SmallPortrait = smallPortrait;
        }
    }

}