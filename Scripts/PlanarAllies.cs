using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public static class PlanarAllies
    {
        // Special criteria for certain gods
        // TODO: We should be able to retrieve this info from the proto object
        private enum Tag
        {
            Demon,
            Devil,
            Animal,
            Spider,
            Earth,
            Fire,
            Air,
            Water
        }

        private struct PlanarAlly
        {
            public readonly int ProtoId;

            public readonly int HitDice;

            public readonly Alignment Alignment;

            public readonly ISet<Tag> Tags;

            public PlanarAlly(int protoId, int hitDice, Alignment alignment, params Tag[] tags)
            {
                ProtoId = protoId;
                HitDice = hitDice;
                Alignment = alignment;
                Tags = tags.ToImmutableHashSet();
            }
        }

        private static readonly PlanarAlly[] OUTSIDERS =
        {
            new PlanarAlly(14110, 3, Alignment.CHAOTIC_EVIL),
            new PlanarAlly(14111, 15, Alignment.CHAOTIC_EVIL, Tag.Fire),
            new PlanarAlly(14258, 10, Alignment.CHAOTIC_EVIL, Tag.Demon),
            new PlanarAlly(14259, 10, Alignment.CHAOTIC_EVIL, Tag.Demon),
            new PlanarAlly(14263, 12, Alignment.CHAOTIC_EVIL, Tag.Demon),
            new PlanarAlly(14275, 5, Alignment.NEUTRAL_EVIL),
            new PlanarAlly(14286, 20, Alignment.CHAOTIC_EVIL, Tag.Demon),
            new PlanarAlly(14299, 4, Alignment.NEUTRAL, Tag.Animal),
            new PlanarAlly(14340, 10, Alignment.LAWFUL_EVIL, Tag.Fire),
            new PlanarAlly(14384, 4, Alignment.CHAOTIC_EVIL, Tag.Fire),
            new PlanarAlly(14529, 7, Alignment.NEUTRAL_GOOD),
            new PlanarAlly(14530, 12, Alignment.CHAOTIC_EVIL, Tag.Demon, Tag.Spider),
            new PlanarAlly(14531, 6, Alignment.CHAOTIC_GOOD, Tag.Air),
            new PlanarAlly(14537, 7, Alignment.CHAOTIC_GOOD, Tag.Air),
            new PlanarAlly(14540, 4, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14541, 6, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14543, 6, Alignment.NEUTRAL),
            new PlanarAlly(14544, 12, Alignment.NEUTRAL_GOOD, Tag.Animal),
            new PlanarAlly(14545, 3, Alignment.NEUTRAL, Tag.Air),
            new PlanarAlly(14546, 3, Alignment.NEUTRAL, Tag.Air, Tag.Earth),
            new PlanarAlly(14547, 3, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14549, 3, Alignment.NEUTRAL, Tag.Fire),
            new PlanarAlly(14550, 3, Alignment.NEUTRAL, Tag.Fire, Tag.Earth),
            new PlanarAlly(14551, 3, Alignment.NEUTRAL, Tag.Water, Tag.Earth),
            new PlanarAlly(14552, 3, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14553, 3, Alignment.NEUTRAL, Tag.Fire, Tag.Water),
            new PlanarAlly(14554, 3, Alignment.NEUTRAL, Tag.Water),
            new PlanarAlly(14555, 8, Alignment.NEUTRAL_EVIL),
            new PlanarAlly(14563, 9, Alignment.CHAOTIC_EVIL, Tag.Fire),
            new PlanarAlly(14564, 4, Alignment.NEUTRAL_EVIL),
            new PlanarAlly(14565, 3, Alignment.NEUTRAL),
            new PlanarAlly(14566, 3, Alignment.NEUTRAL_EVIL),
            new PlanarAlly(14567, 6, Alignment.LAWFUL_EVIL, Tag.Devil),
            new PlanarAlly(14568, 8, Alignment.LAWFUL_EVIL, Tag.Devil)
        };

        private static readonly PlanarAlly[] FIENDS =
        {
            new PlanarAlly(14399, 4, Alignment.CHAOTIC_EVIL, Tag.Spider),
            new PlanarAlly(14402, 3, Alignment.CHAOTIC_EVIL, Tag.Animal),
            new PlanarAlly(14403, 6, Alignment.CHAOTIC_EVIL, Tag.Animal),
            new PlanarAlly(14404, 3, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14407, 4, Alignment.NEUTRAL_EVIL, Tag.Animal),
            new PlanarAlly(14408, 6, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14472, 6, Alignment.CHAOTIC_EVIL),
            new PlanarAlly(14520, 3, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14521, 7, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14523, 32, Alignment.CHAOTIC_EVIL, Tag.Spider),
            new PlanarAlly(14524, 16, Alignment.CHAOTIC_EVIL, Tag.Spider),
            new PlanarAlly(14525, 11, Alignment.LAWFUL_EVIL, Tag.Animal),
            new PlanarAlly(14527, 8, Alignment.CHAOTIC_EVIL, Tag.Spider)
        };

        private static readonly PlanarAlly[] CELESTIALS =
        {
            new PlanarAlly(14394, 3, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14395, 6, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14503, 12, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14505, 8, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14532, 5, Alignment.LAWFUL_GOOD, Tag.Animal),
            new PlanarAlly(14534, 4, Alignment.NEUTRAL_GOOD, Tag.Animal),
            new PlanarAlly(14535, 4, Alignment.NEUTRAL_GOOD, Tag.Animal),
            new PlanarAlly(14536, 18, Alignment.LAWFUL_GOOD, Tag.Animal)
        };

        private static readonly PlanarAlly[] ELEMENTALS =
        {
            new PlanarAlly(14292, 8, Alignment.NEUTRAL, Tag.Air),
            new PlanarAlly(14296, 8, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14298, 8, Alignment.NEUTRAL, Tag.Fire),
            new PlanarAlly(14302, 8, Alignment.NEUTRAL, Tag.Water),
            new PlanarAlly(14376, 2, Alignment.NEUTRAL, Tag.Air),
            new PlanarAlly(14377, 2, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14378, 2, Alignment.NEUTRAL, Tag.Fire),
            new PlanarAlly(14379, 2, Alignment.NEUTRAL, Tag.Water),
            new PlanarAlly(14380, 4, Alignment.NEUTRAL, Tag.Air),
            new PlanarAlly(14381, 4, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14382, 4, Alignment.NEUTRAL, Tag.Fire),
            new PlanarAlly(14383, 4, Alignment.NEUTRAL, Tag.Water),
            new PlanarAlly(14508, 16, Alignment.NEUTRAL, Tag.Air),
            new PlanarAlly(14509, 16, Alignment.NEUTRAL, Tag.Earth),
            new PlanarAlly(14510, 16, Alignment.NEUTRAL, Tag.Fire),
            new PlanarAlly(14511, 16, Alignment.NEUTRAL, Tag.Water)
        };

        private readonly struct DeityCriteria
        {
            public readonly ISet<Alignment> Outsiders;

            public readonly ISet<Tag> Specials;

            public readonly ISet<Tag> Elementals;

            public DeityCriteria(IEnumerable<Alignment> alignments,
                IEnumerable<Tag> specials = null,
                IEnumerable<Tag> elementals = null)
            {
                Outsiders = alignments.ToImmutableHashSet();
                Specials = specials?.ToImmutableHashSet() ?? ImmutableHashSet<Tag>.Empty;
                Elementals = elementals?.ToImmutableHashSet() ?? ImmutableHashSet<Tag>.Empty;
            }
        }

        private static readonly Tag[] ALL_ELEMS = {Tag.Air, Tag.Earth, Tag.Fire, Tag.Water};

        private static readonly Dictionary<DeityId, DeityCriteria> deity_alignments =
            new Dictionary<DeityId, DeityCriteria>
            {
                {
                    DeityId.HEIRONEOUS, new DeityCriteria(new[] {Alignment.LAWFUL_GOOD})
                },
                {DeityId.ST_CUTHBERT, new DeityCriteria(new[] {Alignment.LAWFUL_GOOD, Alignment.LAWFUL_NEUTRAL})},
                {
                    DeityId.MORADIN, new DeityCriteria(
                        new[] {Alignment.LAWFUL_GOOD},
                        elementals: new[] {Tag.Fire, Tag.Earth})
                },
                {
                    DeityId.LOLTH,
                    new DeityCriteria(
                        new[] {Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL_EVIL},
                        specials: new[] {Tag.Spider})
                },
                {
                    DeityId.IUZ,
                    new DeityCriteria(
                        new[] {Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL_EVIL},
                        elementals: new[] {Tag.Fire}
                    )
                },
                {
                    DeityId.ZUGGTMOY, new DeityCriteria(
                        new[] {Alignment.CHAOTIC_EVIL},
                        new[] {Tag.Demon},
                        new[] {Tag.Water}
                    )
                },
                {
                    DeityId.OLD_FAITH,
                    new DeityCriteria(
                        new[] {Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL},
                        new[] {Tag.Animal},
                        new[] {Tag.Earth, Tag.Water}
                    )
                },
                {
                    DeityId.PELOR,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.LAWFUL_GOOD, Alignment.NEUTRAL_GOOD, Alignment.CHAOTIC_GOOD, Alignment.NEUTRAL
                        },
                        elementals: ALL_ELEMS
                    )
                },
                {
                    DeityId.YONDALLA,
                    new DeityCriteria(
                        new[] {Alignment.LAWFUL_GOOD, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL},
                        ALL_ELEMS
                    )
                },
                {
                    DeityId.WEE_JAS,
                    new DeityCriteria(
                        new[] {Alignment.LAWFUL_EVIL, Alignment.LAWFUL_NEUTRAL, Alignment.NEUTRAL},
                        elementals: ALL_ELEMS
                    )
                },
                {
                    DeityId.VECNA,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.LAWFUL_EVIL, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL
                        },
                        elementals: ALL_ELEMS
                    )
                },
                {
                    DeityId.OLIDAMMARA,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL
                        },
                        elementals: ALL_ELEMS
                    )
                },
                {
                    DeityId.NERULL,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.LAWFUL_EVIL, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL
                        },
                        elementals: ALL_ELEMS
                    )
                },
                {
                    DeityId.KORD,
                    new DeityCriteria(
                        new[] {Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_GOOD},
                        elementals: new[] {Tag.Air, Tag.Water}
                    )
                },
                {DeityId.HEXTOR, new DeityCriteria(new[] {Alignment.LAWFUL_EVIL})},
                {
                    DeityId.GRUUMSH,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.CHAOTIC_EVIL, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_EVIL, Alignment.NEUTRAL
                        },
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.GARL_GLITTERGOLD,
                    new DeityCriteria(new[] {Alignment.CHAOTIC_GOOD, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL},
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.CORELLON_LARETHIAN,
                    new DeityCriteria(new[] {Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL_GOOD},
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.BOCCOB,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.LAWFUL_EVIL,
                            Alignment.CHAOTIC_GOOD
                        },
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.OBAD_HAI,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL,
                            Alignment.LAWFUL_GOOD, Alignment.CHAOTIC_GOOD
                        },
                        specials: new[] {Tag.Animal},
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.FHARLANGHN,
                    new DeityCriteria(
                        new[]
                        {
                            Alignment.NEUTRAL, Alignment.NEUTRAL_GOOD, Alignment.NEUTRAL_EVIL, Alignment.LAWFUL_EVIL,
                            Alignment.CHAOTIC_GOOD
                        }, elementals: ALL_ELEMS)
                },
                {
                    DeityId.ERYTHNUL,
                    new DeityCriteria(new[] {Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL}, new[] {Tag.Fire, Tag.Air})
                },
                {
                    DeityId.EHLONNA,
                    new DeityCriteria(new[] {Alignment.NEUTRAL_GOOD, Alignment.LAWFUL_GOOD, Alignment.CHAOTIC_GOOD},
                        specials: new[] {Tag.Animal},
                        elementals: ALL_ELEMS)
                },
                {
                    DeityId.PROCAN,
                    new DeityCriteria(new[] {Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL},
                        elementals: new[] {Tag.Water})
                },
                {
                    DeityId.RALISHAZ,
                    new DeityCriteria(new[] {Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_EVIL, Alignment.NEUTRAL},
                        elementals: new[] {Tag.Air})
                },
                {
                    DeityId.PYREMIUS,
                    new DeityCriteria(new[] {Alignment.NEUTRAL_EVIL, Alignment.CHAOTIC_EVIL, Alignment.LAWFUL_EVIL},
                        elementals: new[] {Tag.Fire})
                },
                {
                    DeityId.NOREBO,
                    new DeityCriteria(new[] {Alignment.CHAOTIC_GOOD, Alignment.CHAOTIC_NEUTRAL, Alignment.NEUTRAL},
                        elementals: new[] {Tag.Air})
                },
                {DeityId.NONE, new DeityCriteria(Array.Empty<Alignment>(), elementals: ALL_ELEMS)},
            };

        private static int fuzz(DeityId deity)
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

        private static bool match(IEnumerable<Tag> mon_flags, IEnumerable<Tag> target_flags)
        {
            foreach (var flag in target_flags)
            {
                if (!mon_flags.Contains(flag))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<int> choose_among(PlanarAlly[] critters, int max_hd, int num)
        {
            // TODO: The original co8 algo was wrong w.r.t. RAW
            throw new NotImplementedException();
        }

        public static IList<int> choose_allies(GameObjectBody caster, int min_hd, int max_hd, int max_summon)
        {
            var deity = caster.GetDeity();
            var portfolio = deity_alignments[deity];

            var fz = fuzz(deity);

            bool ValidHitDice(PlanarAlly ally) => ally.HitDice >= min_hd && ally.HitDice <= max_hd;

            var outsiders = OUTSIDERS.Concat(FIENDS).Concat(CELESTIALS)
                .Where(ValidHitDice)
                .Where(ally => portfolio.Outsiders.Contains(ally.Alignment))
                .Where(ally => match(ally.Tags, portfolio.Specials))
                .ToArray();

            var elementals = ELEMENTALS.Where(ValidHitDice)
                .Where(ally => ally.Tags.Any(t => portfolio.Elementals.Contains(t)))
                .ToArray();

            var num = 1;
            var p = Dice.D100.Roll();
            // prefer single summons to multiples
            if (p < 11)
            {
                num = Math.Min(max_summon, 3);
            }
            else if (p < 31)
            {
                num = Math.Min(max_summon, 2);
            }

            PlanarAlly[] available;
            if (outsiders.Length == 0)
            {
                available = elementals;
            }
            else if (elementals.Length == 0)
            {
                available = outsiders;
            }
            // prefer outsiders to elementals
            else if (Dice.D100.Roll() < 19)
            {
                available = elementals;
            }
            else
            {
                available = outsiders;
            }

            IList<int> allies;
            if (available.Length == 0)
            {
                allies = new[] {14546}; // dust mephit, this shouldn't happen
            }
            else
            {
                allies = choose_among(available, max_hd + fz, num);
            }

            if (allies.Count == 0)
            {
                allies = choose_among(available, max_hd + fz, max_summon);
            }

            if (allies.Count == 0)
            {
                allies = choose_among(elementals.Concat(outsiders).ToArray(), max_hd, max_summon);
            }

            // if it's still nothing, we have a problem
            if (allies.Count == 0)
            {
                allies = new[] {14550}; // magma mephit this time
            }

            return allies;
        }
    }
}