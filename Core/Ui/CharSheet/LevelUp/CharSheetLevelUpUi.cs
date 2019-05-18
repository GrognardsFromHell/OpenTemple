using System;
using System.Collections.Generic;

namespace SpicyTemple.Core.Ui.CharSheet.LevelUp
{
    public class CharSheetLevelUpUi : IDisposable
    {
        [TempleDllLocation(0x102FA6C8)]
        private List<ICharGenSystem> _levelUpSystems = new List<ICharGenSystem>();

        [TempleDllLocation(0x10BE9970)]
        private bool dword_10BE9970;

        public void Dispose()
        {
            Stub.TODO();
        }


        public void Show()
        {
            // TODO: part of the normal char_ui_show method
            Stub.TODO();
            /*
             * ui_widget_set_hidden(*(_DWORD *)(char_ui_main_nav_editor_window_widget + 12), 0);
                    ui_window_bring_to_front(*(_DWORD *)(char_ui_main_nav_editor_window_widget + 12));
                    v2 = *(_DWORD *)(char_ui_main_nav_editor_window_widget + 12);
                    ui_widget_set_hidden(v2, 0);
                    char_creation_objId.id = objId;
                    v3 = (char *)&chargen_systems_sheet[0].reset;
                    do
                    {
                        if ( *(_DWORD *)v3 )
                            (*(void (__cdecl **)(CharEditorSelectionPacket *))v3)(&chargen_packet);
                        v3 += 44;
                    }
                    while ( (signed int)v3 < (signed int)&unk_102FA7F4 );
                    dword_10BE9970 = 0;
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
            dword_10BE9970 = true;

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
    }
}