
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{

    public class RumorControl
    {

        public static int find_rumor(GameObject pc, GameObject npc)
        {
            var sk_mod = pc.GetSkillLevel(npc, SkillId.gather_information) + pc.GetStat(Stat.level_bard);

            if (((StoryState != GetGlobalVar(22)) && (StoryState <= 6)))
            {
                SetGlobalVar(22, StoryState);
                SetGlobalFlag(211, false);
                SetGlobalFlag(212, false);
                SetGlobalFlag(213, false);
                SetGlobalFlag(214, false);
                SetGlobalFlag(215, false);
                SetGlobalFlag(216, false);
                SetGlobalFlag(217, false);
                SetGlobalFlag(218, false);
                SetGlobalFlag(219, false);
                SetGlobalFlag(220, false);
                SetGlobalFlag(221, false);
                SetGlobalFlag(222, false);
                SetGlobalFlag(223, false);
                SetGlobalFlag(224, false);
                SetGlobalFlag(225, false);
                SetGlobalFlag(226, false);
                SetGlobalFlag(209, false);
                SetGlobalFlag(210, false);
            }

            if (StoryState == 0)
            {
                if (sk_mod == 5)
                {
                    sk_mod = 4;

                }
                else if (sk_mod >= 6)
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState == 1)
            {
                if ((sk_mod == 2))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod == 3) || (sk_mod == 4))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod == 5))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod == 6) || (sk_mod == 7))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 8))
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState == 2)
            {
                if ((sk_mod == 2) || (sk_mod == 3))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod == 4) || (sk_mod == 5))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod == 6))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod == 7) || (sk_mod == 8))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 9))
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState == 3)
            {
                if ((sk_mod >= 1) && (sk_mod <= 4))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod == 5) || (sk_mod == 6))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod == 7))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod == 8) || (sk_mod == 9))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 10))
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState == 4)
            {
                if ((sk_mod >= 1) && (sk_mod <= 5))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod == 6))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod == 7))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod >= 8) && (sk_mod <= 10))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 11))
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState == 5)
            {
                if ((sk_mod >= 1) && (sk_mod <= 5))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod == 6) || (sk_mod == 7))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod == 8) || (sk_mod == 9))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod >= 10) && (sk_mod <= 12))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 13))
                {
                    sk_mod = 5;

                }

            }
            else if (StoryState >= 6)
            {
                if ((sk_mod >= 1) && (sk_mod <= 5))
                {
                    sk_mod = 1;

                }
                else if ((sk_mod >= 6) && (sk_mod <= 10))
                {
                    sk_mod = 2;

                }
                else if ((sk_mod >= 11) && (sk_mod <= 14))
                {
                    sk_mod = 3;

                }
                else if ((sk_mod >= 15) && (sk_mod <= 17))
                {
                    sk_mod = 4;

                }
                else if ((sk_mod >= 18))
                {
                    sk_mod = 5;

                }

            }

            var ss_num = (StoryState) * 200;

            var sk_num = (sk_mod * 30) + 20;

            while ((sk_num >= 0))
            {
                var rumor = ss_num + sk_num;

                if (rumor_valid(rumor, pc, npc) == 1)
                {
                    return rumor;
                }
                else
                {
                    sk_num = (sk_num - 10);

                }

            }

            return -1;
        }
        public static int rumor_valid(int rumor, GameObject pc, GameObject npc)
        {
            var offset = (StoryState) * 200;

            var sk_lookup = ((rumor - offset) / 10);

            if (GetGlobalFlag(209 + sk_lookup))
            {
                return 0;
            }

            if ((npc.GetMap() == 5007))
            {
                if (((rumor == 120) || (rumor == 130) || (rumor == 520) || (rumor == 530)))
                {
                    return 0;
                }

            }
            else if (((rumor == 150) || (rumor == 330) || (rumor == 510) || (rumor == 540) || (rumor == 560)))
            {
                return 0;
            }

            if (((PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL) && (rumor == 550 || rumor == 630)))
            {
                return 0;
            }

            if ((GetQuestState(12) == QuestState.Completed && rumor == 40))
            {
                return 0;
            }

            if ((pc.GetRace() != RaceId.human) && (rumor == 800))
            {
                return 0;
            }

            if (((rumor >= 590 && rumor <= 680) || (rumor >= 800 && rumor <= 830) && (npc.GetArea() == 1)))
            {
                return 1;
            }

            if (((rumor == 860 || rumor == 890 || rumor == 900 || rumor == 1030 || rumor == 1060 || rumor == 1090 || rumor == 1230 || rumor == 1260 || rumor == 1290) && (npc.GetArea() == 1 || npc.GetArea() == 3)))
            {
                return 1;
            }

            if ((rumor >= 690 && npc.GetArea() == 1))
            {
                return 0;
            }

            if ((rumor >= 800 && npc.GetArea() == 3))
            {
                return 0;
            }

            return 1;
        }
        public static void rumor_given_out(int rumor)
        {
            var offset = (StoryState) * 200;

            var sk_lookup = ((rumor - offset) / 10);

            SetGlobalFlag(209 + sk_lookup, true);
            return;
        }


    }
}
