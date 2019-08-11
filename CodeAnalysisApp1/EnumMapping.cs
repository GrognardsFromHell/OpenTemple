using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Diagnostics;

namespace CodeAnalysisApp1
{
    public static class EnumMapping
    {

        public static Dictionary<string, Dictionary<ulong, string>> Enums = new Dictionary<string, Dictionary<ulong, string>>();

        static EnumMapping()
        {
            Enums["D20CAF"] = new Dictionary<ulong, string>
            {
                {0, "NONE"},
                {0x1, "HIT"},
                {0x2, "CRITICAL"},
                {0x4, "RANGED"},
                {0x8, "ACTIONFRAME_PROCESSED"},
                {0x10, "NEED_PROJECTILE_HIT"},
                {0x20, "NEED_ANIM_COMPLETED"},
                {0x40, "ATTACK_OF_OPPORTUNITY"},
                {0x80, "CONCEALMENT_MISS"},
                {0x100, "TOUCH_ATTACK"},
                {0x200, "FREE_ACTION"},
                {0x400, "CHARGE"},
                {0x800, "REROLL"},
                {0x1000, "REROLL_CRITICAL"},
                {0x2000, "TRAP"},
                {0x4000, "ALTERNATE"},
                {0x8000, "NO_PRECISION_DAMAGE"},
                {0x10000, "FLANKED"},
                {0x20000, "DEFLECT_ARROWS"},
                {0x40000, "FULL_ATTACK"},
                {0x80000, "AOO_MOVEMENT"},
                {0x100000, "BONUS_ATTACK"},
                {0x200000, "THROWN"},
                {0x400000, "SAVE_SUCCESSFUL"},
                {0x800000, "SECONDARY_WEAPON"},
                {0x1000000, "MANYSHOT"},
                {0x2000000, "ALWAYS_HIT"},
                {0x4000000, "COVER"},
                {0x8000000, "COUNTERSPELLED"},
                {0x10000000, "THROWN_GRENADE"},
                {0x20000000, "FINAL_ATTACK_ROLL"},
                {0x40000000, "TRUNCATED"},
                {0x80000000, "UNNECESSARY"},
            };

            Enums["SavingThrowType"] = new Dictionary<ulong, string>
            {
                { 0, "Fortitude" },
                { 1, "Reflex" },
                { 2, "Will" },
            };

            Enums["D20SavingThrowFlag"] = new Dictionary<ulong, string>
            {
                {0x1, "REROLL" },
                {0x2, "CHARM" },
                {0x4, "TRAP" },
                {0x8, "POISON" },
                {0x10, "SPELL_LIKE_EFFECT" },
                {0x20, "SPELL_SCHOOL_ABJURATION" },
                {0x40, "SPELL_SCHOOL_CONJURATION" },
                {0x80, "SPELL_SCHOOL_DIVINATION" },
                {0x100, "SPELL_SCHOOL_ENCHANTMENT" },
                {0x200, "SPELL_SCHOOL_EVOCATION" },
                {0x400, "SPELL_SCHOOL_ILLUSION" },
                {0x800, "SPELL_SCHOOL_NECROMANCY" },
                {0x1000, "SPELL_SCHOOL_TRANSMUTATION" },
                {0x2000, "SPELL_DESCRIPTOR_ACID" },
                {0x4000, "SPELL_DESCRIPTOR_CHAOTIC" },
                {0x8000, "SPELL_DESCRIPTOR_COLD" },
                {0x10000, "SPELL_DESCRIPTOR_DARKNESS" },
                {0x20000, "SPELL_DESCRIPTOR_DEATH" },
                {0x40000, "SPELL_DESCRIPTOR_ELECTRICITY" },
                {0x80000, "SPELL_DESCRIPTOR_EVIL" },
                {0x100000, "SPELL_DESCRIPTOR_FEAR" },
                {0x200000, "SPELL_DESCRIPTOR_FIRE" },
                {0x400000, "SPELL_DESCRIPTOR_FORCE" },
                {0x800000, "SPELL_DESCRIPTOR_GOOD" },
                {0x1000000, "SPELL_DESCRIPTOR_LANGUAGE_DEPENDENT" },
                {0x2000000, "SPELL_DESCRIPTOR_LAWFUL" },
                {0x4000000, "SPELL_DESCRIPTOR_LIGHT" },
                {0x8000000, "SPELL_DESCRIPTOR_MIND_AFFECTING" },
                {0x10000000, "SPELL_DESCRIPTOR_SONIC" },
                {0x20000000, "SPELL_DESCRIPTOR_TELEPORTATION" },
                {0x40000000, "SPELL_DESCRIPTOR_AIR" },
                {0x80000000, "SPELL_DESCRIPTOR_EARTH" },
            };

        }

        internal static ExpressionSyntax MapNumberToEnumSyntax(string name, object value)
        {
            if (name == null || !Enums.TryGetValue(name, out var mapping))
            {
                return null;
            }

            ulong checkAgainst;
            if (value is int intValue)
            {
                checkAgainst = (ulong)intValue;
            }
            else if (value is uint uintValue)
            {
                checkAgainst = uintValue;
            }
            else if (value is long longValue)
            {
                checkAgainst = unchecked((ulong)longValue);
            }
            else if (value is ulong ulongValue)
            {
                checkAgainst = ulongValue;
            }
            else
            {
                throw new ArgumentException();
            }

            ExpressionSyntax v = null;

            for (int i = 0; i < 64; ++i)
            {
                var m = 1UL << i;
                if ((checkAgainst & m) != default)
                {
                    ExpressionSyntax enumLiteralSyntax;
                    if (mapping.TryGetValue(m, out var literalName))
                    {
                        enumLiteralSyntax = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                                            IdentifierName(name),
                                                                                            IdentifierName(literalName));

                    }
                    else
                    {
                        enumLiteralSyntax = LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(m));
                    }

                    if (v != null)
                    {
                        v = BinaryExpression(SyntaxKind.BitwiseOrExpression, v, enumLiteralSyntax);
                    }
                    else
                    {
                        v = enumLiteralSyntax;
                    }
                }
            }

            if (v != null)
            {
                if (v.IsKind(SyntaxKind.BitwiseOrExpression))
                {
                    v = ParenthesizedExpression(v);
                }
                return v;
            }

            // Return 0, because it MUST have been zero!
            Trace.Assert(checkAgainst == 0);
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0));
        }
    }
}
