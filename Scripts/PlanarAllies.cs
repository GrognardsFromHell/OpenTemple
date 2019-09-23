
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class PlanarAllies
    {
        // Special criteria for certain gods

        private static readonly int Demon = 0;
        private static readonly int Devil = 1;
        private static readonly int Animal = 2;
        private static readonly int Spider = 3;
        private static readonly int Earth = 4;
        private static readonly int Fire = 5;
        private static readonly int Air = 6;
        private static readonly int Water = 7;
        // desc   hd  align                   flags

        private static readonly List<List<GameObjectBody>> OUTSIDERS = new[] { (14110, 3, Alignment.CHAOTIC_EVIL, new List<GameObjectBody>()), (14111, 15, Alignment.CHAOTIC_EVIL, new[] { Fire }), (14258, 10, Alignment.CHAOTIC_EVIL, new[] { Demon }), (14259, 10, Alignment.CHAOTIC_EVIL, new[] { Demon }), (14263, 12, Alignment.CHAOTIC_EVIL, new[] { Demon }), (14275, 5, Alignment.NEUTRAL_EVIL, new List<GameObjectBody>()), (14286, 20, Alignment.CHAOTIC_EVIL, new[] { Demon }), (14299, 4, Alignment.NEUTRAL, new[] { Animal }), (14340, 10, Alignment.LAWFUL_EVIL, new[] { Fire }), (14384, 4, Alignment.CHAOTIC_EVIL, new[] { Fire }), (14529, 7, Alignment.NEUTRAL_GOOD, new List<GameObjectBody>()), (14530, 12, Alignment.CHAOTIC_EVIL, new[] { Demon, Spider }), (14531, 6, Alignment.CHAOTIC_GOOD, new[] { Air }), (14537, 7, Alignment.CHAOTIC_GOOD, new[] { Air }), (14540, 4, Alignment.LAWFUL_EVIL, new[] { Animal }), (14541, 6, Alignment.LAWFUL_GOOD, new[] { Animal }), (14543, 6, Alignment.NEUTRAL, new List<GameObjectBody>()), (14544, 12, Alignment.NEUTRAL_GOOD, new[] { Animal }), (14545, 3, Alignment.NEUTRAL, new[] { Air }), (14546, 3, Alignment.NEUTRAL, new[] { Air, Earth }), (14547, 3, Alignment.NEUTRAL, new[] { Earth }), (14549, 3, Alignment.NEUTRAL, new[] { Fire }), (14550, 3, Alignment.NEUTRAL, new[] { Fire, Earth }), (14551, 3, Alignment.NEUTRAL, new[] { Water, Earth }), (14552, 3, Alignment.NEUTRAL, new[] { Earth }), (14553, 3, Alignment.NEUTRAL, new[] { Fire, Water }), (14554, 3, Alignment.NEUTRAL, new[] { Water }), (14555, 8, Alignment.NEUTRAL_EVIL, new List<GameObjectBody>()), (14563, 9, Alignment.CHAOTIC_EVIL, new[] { Fire }), (14564, 4, Alignment.NEUTRAL_EVIL, new List<GameObjectBody>()), (14565, 3, Alignment.NEUTRAL, new List<GameObjectBody>()), (14566, 3, Alignment.NEUTRAL_EVIL, new List<GameObjectBody>()), (14567, 6, Alignment.LAWFUL_EVIL, new[] { Devil }), (14568, 8, Alignment.LAWFUL_EVIL, new[] { Devil }) };
        private static readonly List<FIXME> FIENDS = new[] { (14399, 4, Alignment.CHAOTIC_EVIL, new[] { Spider }), (14402, 3, Alignment.CHAOTIC_EVIL, new[] { Animal }), (14403, 6, Alignment.CHAOTIC_EVIL, new[] { Animal }), (14404, 3, Alignment.LAWFUL_EVIL, new[] { Animal }), (14407, 4, Alignment.NEUTRAL_EVIL, new[] { Animal }), (14408, 6, Alignment.LAWFUL_EVIL, new[] { Animal }), (14472, 6, Alignment.CHAOTIC_EVIL, new List<GameObjectBody>()), (14520, 3, Alignment.LAWFUL_EVIL, new[] { Animal }), (14521, 7, Alignment.LAWFUL_EVIL, new[] { Animal }), (14523, 32, Alignment.CHAOTIC_EVIL, new[] { Spider }), (14524, 16, Alignment.CHAOTIC_EVIL, new[] { Spider }), (14525, 11, Alignment.LAWFUL_EVIL, new[] { Animal }), (14527, 8, Alignment.CHAOTIC_EVIL, new[] { Spider }) };
        private static readonly List<FIXME> CELESTIALS = new[] { (14394, 3, Alignment.LAWFUL_GOOD, new[] { Animal }), (14395, 6, Alignment.LAWFUL_GOOD, new[] { Animal }), (14503, 12, Alignment.LAWFUL_GOOD, new[] { Animal }), (14505, 8, Alignment.LAWFUL_GOOD, new[] { Animal }), (14532, 5, Alignment.LAWFUL_GOOD, new[] { Animal }), (14534, 4, Alignment.NEUTRAL_GOOD, new[] { Animal }), (14535, 4, Alignment.NEUTRAL_GOOD, new[] { Animal }), (14536, 18, Alignment.LAWFUL_GOOD, new[] { Animal }) };
        private static readonly List<FIXME> ELEMENTALS = new[] { (14292, 8, Alignment.NEUTRAL, new[] { Air }), (14296, 8, Alignment.NEUTRAL, new[] { Earth }), (14298, 8, Alignment.NEUTRAL, new[] { Fire }), (14302, 8, Alignment.NEUTRAL, new[] { Water }), (14376, 2, Alignment.NEUTRAL, new[] { Air }), (14377, 2, Alignment.NEUTRAL, new[] { Earth }), (14378, 2, Alignment.NEUTRAL, new[] { Fire }), (14379, 2, Alignment.NEUTRAL, new[] { Water }), (14380, 4, Alignment.NEUTRAL, new[] { Air }), (14381, 4, Alignment.NEUTRAL, new[] { Earth }), (14382, 4, Alignment.NEUTRAL, new[] { Fire }), (14383, 4, Alignment.NEUTRAL, new[] { Water }), (14508, 16, Alignment.NEUTRAL, new[] { Air }), (14509, 16, Alignment.NEUTRAL, new[] { Earth }), (14510, 16, Alignment.NEUTRAL, new[] { Fire }), (14511, 16, Alignment.NEUTRAL, new[] { Water }) };
        private static readonly List<FIXME> ALL_ELEMS = new[] { Air, Earth, Fire, Water };
        private static readonly Dictionary<DeityId, List<GameObjectBody>> deity_alignments = new Dictionary<DeityId, List<GameObjectBody>> {
{DeityId.HEIRONEOUS,(new []{Alignment.LAWFUL_GOOD}, new List<GameObjectBody>(), new List<GameObjectBody>())},
{DeityId.ST_CUTHBERT,(new []{Alignment.LAWFUL_GOOD, Alignment.LAWFUL_NEUTRAL}, new List<GameObjectBody>(), new List<GameObjectBody>())},
{DeityId.MORADIN,(new []{Alignment.LAWFUL_GOOD}, new List<GameObjectBody>(), new []{Fire, Earth})},
{DeityId.LOLTH,(new []{Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL_EVIL}, new []{Spider}, new List<GameObjectBody>())},
{DeityId.IUZ,(new []{Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL_EVIL}, new List<GameObjectBody>(), new []{Fire})},
{DeityId.ZUGGTMOY,(new []{Alignment.CHAOTIC_EVIL}, new []{Demon}, new []{Water})},
{DeityId.OLD_FAITH,(new []{Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL}, new []{Animal}, new []{Earth, Water})},
{DeityId.PELOR,(new []{Alignment.LAWFUL_GOOD, Alignment.NEUTRAL_GOOD, Alignment.CHAOTIC_GOOD, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.YONDALLA,(new []{Alignment.LAWFUL_GOOD, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.WEE_JAS,(new []{Alignment.LAWFUL_EVIL, Alignment.LAWFUL_NEUTRAL, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.VECNA,(new []{Alignment.LAWFUL_EVIL, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.OLIDAMMARA,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.NERULL,(new []{Alignment.LAWFUL_EVIL, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.KORD,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_GOOD}, new List<GameObjectBody>(), new []{Air, Water})},
{DeityId.HEXTOR,(new []{Alignment.LAWFUL_EVIL}, new List<GameObjectBody>(), new List<GameObjectBody>())},
{DeityId.GRUUMSH,(new []{Alignment.CHAOTIC_EVIL, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.GARL_GLITTERGOLD,(new []{Alignment.CHAOTIC_GOOD, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.CORELLON_LARETHIAN,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_GOOD}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.BOCCOB,(new []{Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.LAWFUL_EVIL, Alignment.CHAOTIC_GOOD}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.OBAD_HAI,(new []{Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.LAWFUL_GOOD, Alignment.CHAOTIC_GOOD}, new []{Animal}, ALL_ELEMS)},
{DeityId.FHARLANGHN,(new []{Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.LAWFUL_EVIL, Alignment.CHAOTIC_GOOD}, new List<GameObjectBody>(), ALL_ELEMS)},
{DeityId.ERYTHNUL,(new []{Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL}, new List<GameObjectBody>(), new []{Fire, Air})},
{DeityId.EHLONNA,(new []{Alignment.NEUTRAL_GOOD, Alignment.LAWFUL_GOOD, Alignment.CHAOTIC_GOOD}, new []{Animal}, ALL_ELEMS)},
{DeityId.PROCAN,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), new []{Water})},
{DeityId.RALISHAZ,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL}, new List<GameObjectBody>(), new []{Air})},
{DeityId.PYREMIUS,(new []{Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.LAWFUL_EVIL}, new List<GameObjectBody>(), new []{Fire})},
{DeityId.NOREBO,(new []{Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL}, new List<GameObjectBody>(), new []{SpellDescriptor.AIR})},
{DeityId.NONE,(new List<GameObjectBody>(), new List<GameObjectBody>(), ALL_ELEMS)},
}
        ;
        public static int fuzz(FIXME deity)
        {
            var dice = Dice.D2;
            dice = dice.WithModifier(-1);
            // make the non-chooseable deities a little more interesting
            if (deity == DeityId.ZUGGTMOY || deity == DeityId.IUZ)
            {
                dice = dice.WithCount(2);
            }
            else if (deity == DeityId.LOLTH)
            {
                dice = dice.WithSides(3);
            }

            return dice.Roll();
        }
        public static FIXME tails(FIXME l)
        {
            if (l.Count == 0)
            {
                return new List<GameObjectBody>();
            }
            else
            {
                return new[] { (l[0], l) } + tails(l[1..]);
            }

        }
        public static int match(FIXME mon_flags, FIXME target_flags)
        {
            var accept = 1;
            foreach (var flag in target_flags)
            {
                if (!(mon_flags).Contains(flag))
                {
                    accept = 0;
                }

            }

            return accept;
        }
        public static List<GameObjectBody> picks(FIXME n, FIXME max_hd, FIXME l)
        {
            if (max_hd <= 0)
            {
                return new[] { new List<GameObjectBody>() };
            }
            else if (n <= 0)
            {
                // Try to get close to the maximum hit dice limit.
                // We don't want 3 4-hit-dice creatures for the
                // 18-hit-dice greater planar ally.
                if (max_hd < 3)
                {
                    return new[] { new List<GameObjectBody>() };
                }
                else
                {
                    return new List<GameObjectBody>();
                }

            }
            else
            {
                return FIXME[rest + [desc]
                             for ((desc, hd), tl) in tails(l)
                             if hd <= max_hd
                 for rest in picks(n - 1, max_hd - hd, tl) ];
            }

        }
        public static FIXME choose_among(FIXME critters, FIXME max_hd, FIXME num)
        {
            var choices = picks(num, max_hd, critters);
            if (choices.Count == 0)
            {
                return new List<GameObjectBody>();
            }

            var die = Dice.Parse("1d1");
            die = die.WithSides(choices.Count);
            die = die.WithModifier(-1);
            return choices[die.Roll()];
        }
        public static IList<int> choose_allies(GameObjectBody caster, int min_hd, int max_hd, int max_summon)
        {
            var deity = caster.GetDeity();
            var percent = Dice.D100;
            var (alignments, specials, elements) = deity_alignments[deity];
            var fz = fuzz(deity);
            var outsiders = FIXME[(desc, hd)
                          for (desc, hd, align, mon_flags)
                  in OUTSIDERS + FIENDS + CELESTIALS
                          if align in alignments and match(mon_flags, specials)
                          if min_hd <= hd and hd <= max_hd ];
            var elementals = FIXME[(desc, hd)
                           for (desc, hd, align, flags)
                   in ELEMENTALS
                           if flags[0] in elements
                           if min_hd <= hd and hd <= max_hd ];
            var num = 1;
            var p = percent.Roll();
            // prefer single summons to multiples
            if (p < 11)
            {
                num = Math.Min(max_summon, 3);
            }
            else if (p < 31)
            {
                num = Math.Min(max_summon, 2);
            }

            if (outsiders.Count == 0)
            {
                var available = elementals;
            }
            else if (elementals.Count == 0)
            {
                var available = outsiders;
            }
            // prefer outsiders to elementals
            else if (percent.Roll() < 19)
            {
                var available = elementals;
            }
            else
            {
                var available = outsiders;
            }

            if (available == new List<GameObjectBody>())
            {
                var allies = new[] { 14546 }; // dust mephit, this shouldn't happen
            }
            else
            {
                var allies = choose_among(available, max_hd + fz, num);
            }

            if (allies == new List<GameObjectBody>())
            {
                var allies = choose_among(available, max_hd + fz, max_summon);
            }

            if (allies == new List<GameObjectBody>())
            {
                var allies = choose_among(elementals + outsiders, max_hd, max_summon);
            }

            // if it's still nothing, we have a problem
            if (allies == new List<GameObjectBody>())
            {
                var allies = new[] { 14550 }; // magma mephit this time
            }

            return allies;
        }

    }
}
