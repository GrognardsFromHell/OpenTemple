using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.Combat
{
    public class InitiativeUi
    {
        /// <summary>
        ///     The margin between portraits (of any size).
        /// </summary>
        private const int PortraitMargin = 5;

        private static readonly Size PortraitSize = new Size(62, 56);

        private static readonly Size SmallPortraitSize = new Size(47, 42);

        private List<PortraitRecord> _currentPortraits = new List<PortraitRecord>();

        private readonly WidgetContainer _portraitContainer;

        [TempleDllLocation(0x10be6ff0)]
        private int uiCombatInitiativeNumActivePortraits = 0;

        [TempleDllLocation(0x10be6ff4)]
        private bool uiCombatPortraitsNeedRefresh_10BE6FF4;

        [TempleDllLocation(0x10be6f74)]
        private PortraitRecord[] uiInitiativeMiniPortraits;

        [TempleDllLocation(0x10be6f0c)]
        private PortraitRecord[] uiInitiativeSmallPortraits;

        [TempleDllLocation(0x10be7008)]
        private bool uiIntgameFlag_10BE7008; // Used for drag&drop methinks

        [TempleDllLocation(0x101430b0)]
        public InitiativeUi()
        {
            _portraitContainer = new WidgetContainer(
                0,
                5,
                Tig.RenderingDevice.GetCamera().ScreenSize.Width - 2,
                100 - 12
            );
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
            var availableWidth = _portraitContainer.GetWidth();
            var requiredWidthNormal = PortraitSize.Width * combatantCount + (combatantCount - 1) * PortraitMargin;
            var useSmallPortraits = requiredWidthNormal > availableWidth;

            // Reassemble the list of portraits and try to reuse portraits as we go
            Span<bool> reused = stackalloc bool[_currentPortraits.Count];
            var portraits = new List<PortraitRecord>(combatantCount);
            var currentX = 0;
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
                        _currentPortraits[i].Container.SetX(currentX);
                        portraits.Add(_currentPortraits[i]);
                        reused[i] = true;
                        break;
                    }
                }

                if (!reusedPortrait)
                {
                    portraits.Add(CreatePortraitRecord(combatant, useSmallPortraits));
                }

                currentX += portraits[index].Container.GetWidth() + PortraitMargin;
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

        private PortraitRecord CreatePortraitRecord(GameObjectBody combatant, bool useSmallPortraits)
        {
            var size = useSmallPortraits ? SmallPortraitSize : PortraitSize;
            var container = new WidgetContainer(size);
            container.ClipChildren = false;
            // ZIndex maxInitiativeSmallPortraits + 90550
            // R ENDER ui_render_combat_initiative_ui/*0x10141780*/

            var button = new InitiativePortraitButton(combatant, useSmallPortraits);
            //render = ui_render_combat_initiative_ui_2/*0x10141810*/;
            //handleMessage = (LgcyWidgetHandleMsgFn)UiCombatInitiativePortraitMsg/*0x101428d0*/;
            //renderTooltip = UiSystems.Tooltip.GetInjuryLevelColor;
            // widget.field98 = 72;
            container.Add(button);

            _portraitContainer.Add(container);

            return new PortraitRecord(combatant, container, button, useSmallPortraits);
        }

        private readonly struct PortraitRecord
        {
            public readonly GameObjectBody Combatant;

            public readonly WidgetContainer Container;

            public readonly InitiativePortraitButton Button;

            public readonly bool SmallPortrait;

            public PortraitRecord(GameObjectBody combatant,
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
}