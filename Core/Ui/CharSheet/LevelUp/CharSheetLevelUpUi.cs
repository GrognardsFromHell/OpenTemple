using OpenTemple.Core.Ui.PartyCreation;
using OpenTemple.Core.Ui.Widgets;
using System;
using System.Collections.Generic;

namespace OpenTemple.Core.Ui.CharSheet.LevelUp
{
    public class CharSheetLevelUpUi : IDisposable
    {
        [TempleDllLocation(0x102FA6C8)]
        private List<ICharEditorSystem> _levelUpSystems = new List<ICharEditorSystem>();

        [TempleDllLocation(0x10BE9310)]
        private WidgetContainer char_ui_main_nav_editor_window;

        [TempleDllLocation(0x10BE9970)]
        private bool freeze;

        public CharSheetLevelUpUi()
        {

        }

        public void Dispose()
        {
            Stub.TODO();
        }


        public void Show()
        {
            // TODO: part of the normal char_ui_show method
            Stub.TODO();



            //char_ui_main_nav_editor_window
            /*
             * ui_widget_set_hidden(*(_DWORD *)(char_ui_main_nav_editor_window_widget + 12), 0);
                    ui_window_bring_to_front(*(_DWORD *)(char_ui_main_nav_editor_window_widget + 12));
                    v2 = *(_DWORD *)(char_ui_main_nav_editor_window_widget + 12);
                    ui_widget_set_hidden(v2, 0);
            */
            ResetData(UiSystems.PCCreation.charEdSelPkt);
            freeze = false;
            /*
                    
                    ui_widget_set_hidden(char_ui_levelup_window_id, 0);
                    ui_window_bring_to_front(char_ui_levelup_window_id);
                    if ( dword_10BE8D38 >= 0 )
                    {
                        v4 = (void (*)(void))chargen_systems_sheet[dword_10BE8D34].hide;
                        if ( v4 )
                            v4();
                        dword_10BE8D34 = 0;
                        if ( chargen_systems_sheet[0].activate )
                            chargen_systems_sheet[0].activate();
                        sub_10148B70(0);
                        v5 = (void (*)(void))chargen_systems_sheet[dword_10BE8D34].show;
                        if ( v5 )
                            v5();
                        if ( sub_10130300() )
                            sub_10130310(0);
                    }
             */
        }

        public void Hide()
        {
            freeze = true;

            // TODO ui_widget_set_hidden(char_ui_levelup_window_id, 1);
            Stub.TODO();

            if (UiSystems.CharSheet.State == CharInventoryState.LevelUp)
            {
                foreach (var system in _levelUpSystems)
                {
                    system.Hide();
                }
            }
        }

        public void Reset()
        {
            foreach (var system in _levelUpSystems)
            {
                system.ResetSystem();
            }
        }

        public void ResetData(CharEditorSelectionPacket selPkt)
        {
            foreach (var system in _levelUpSystems)
            {
                system.Reset(selPkt);
            }
        }
    }
}