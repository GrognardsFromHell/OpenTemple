using OpenTemple.Core.GameObject;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    [TempleDllLocation(0x102f7990)]
    internal class GenderSystem : IChargenSystem
    {
        public string HelpTopic => "TAG_CHARGEN_GENDER";

        public ChargenStages Stage => ChargenStages.CG_Stage_Gender;

        public WidgetContainer Container { get; }

        private readonly WidgetButton _maleButton;

        private readonly WidgetButton _femaleButton;

        private CharEditorSelectionPacket _pkt;

        [TempleDllLocation(0x1018a420)]
        public GenderSystem()
        {
            var doc = WidgetDoc.Load("ui/pc_creation/gender_ui.json");
            Container = doc.TakeRootContainer();
            Container.Visible = false;

            _maleButton = doc.GetButton("maleButton");
            _maleButton.SetClickHandler(() => ChooseGender(Gender.Male));

            _femaleButton = doc.GetButton("femaleButton");
            _femaleButton.SetClickHandler(() => ChooseGender(Gender.Female));

            // NOTE: Vanilla previously tried showing gender-specific help texts, but those texts actually don't exist
        }

        private void ChooseGender(Gender gender)
        {
            _pkt.genderId = gender;
            UpdateButtons();
            UiSystems.PCCreation.UpdatePlayerDescription();
            UiSystems.PCCreation.ResetSystemsAfter(ChargenStages.CG_Stage_Gender);
        }

        [TempleDllLocation(0x10189c70)]
        public void Reset(CharEditorSelectionPacket pkt)
        {
            pkt.genderId = null;
            _pkt = pkt;
            UpdateButtons();
        }

        [TempleDllLocation(0x10189cc0)]
        public bool CheckComplete()
        {
            return _pkt.genderId.HasValue;
        }

        [TempleDllLocation(0x10189cd0)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:68")]
        public void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody handleNew)
        {
            // GameObjectBody v2;
            // Stat v3;
            // int aasHandle;
            // GameObjectBody v5;
            // int unk;
            // aas_anim_state animParams;
            //
            // v2 = GameSystems.Proto.GetProtoById(2 * selpkt.raceId - selpkt.genderId + 13001);
            // if (!GameSystems.MapObject.CreateObject(v2, (locXY) 0x1E0000001E0, handleNew))
            // {
            //     Logger.Info("pc_creation.c: FATAL ERROR, could not create player");
            //     exit(0);
            // }
            //
            // v3 = 0;
            // do
            // {
            //     GameSystems.Stat.SetBasicStat(*handleNew, v3, selpkt.abilityStats[v3]);
            //     ++v3;
            // } while ((int) v3 < 6);
            //
            // aasHandle = UiSystems.PCCreation.charEditorObjHnd.GetOrCreateAnimHandle();
            // Aas_10262C10 /*0x10262c10*/(aasHandle, 1065353216, 0, 0, &animParams, &unk);
            // v5 = *handleNew;
            // if (selpkt.isPointbuy)
            // {
            //     v5.SetInt32(obj_f.pc_roll_count, -25);
            // }
            // else
            // {
            //     v5.SetInt32(obj_f.pc_roll_count, selpkt.numRerolls);
            // }
        }

        private void UpdateButtons()
        {
            var gender = _pkt.genderId;
            _maleButton.SetActive(gender.HasValue && gender.Value == Gender.Male);
            _femaleButton.SetActive(gender.HasValue && gender.Value == Gender.Female);
        }
    }
}