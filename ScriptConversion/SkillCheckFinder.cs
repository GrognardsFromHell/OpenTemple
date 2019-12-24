using System;
using System.Collections.Generic;
using System.Diagnostics;
using IronPython.Compiler;
using IronPython.Compiler.Ast;
using OpenTemple.Core.Systems.D20;

namespace ScriptConversion
{
    public class SkillCheckFinder : PythonWalker
    {
        public readonly List<SkillCheck> Checks = new List<SkillCheck>();

        // pc.skill_level_get(npc,skill_diplomacy) >= 10
        public override void PostWalk(BinaryExpression node)
        {
            var left = node.Left;
            var right = node.Right;
            if (left is CallExpression callExpression
                && callExpression.Target is MemberExpression targetMemberExpression
                && targetMemberExpression.Name == "skill_level_get"
                && targetMemberExpression.Target is NameExpression callTargetName
                && callTargetName.Name == "pc"
                && right is ConstantExpression constantExpression
                && constantExpression.Value is int skillRef)
            {
                NameExpression skillNameExpression;

                var args = callExpression.Args;
                if (args.Count == 1 && args[0].Expression is NameExpression)
                {
                    skillNameExpression = (NameExpression) args[0].Expression;
                }
                else if (args.Count == 2 && args[1].Expression is NameExpression)
                {
                    if (!(args[0].Expression is NameExpression opponentName) || opponentName.Name != "npc")
                    {
                        Debugger.Break();
                    }

                    skillNameExpression = (NameExpression) args[1].Expression;
                }
                else
                {
                    Debugger.Break();
                    return;
                }

                var skillConstant = skillNameExpression.Name;
                SkillId skill;
                switch (skillConstant)
                {
                    case "skill_bluff":
                        skill = SkillId.bluff;
                        break;
                    case "skill_diplomacy":
                        skill = SkillId.diplomacy;
                        break;
                    case "skill_intimidate":
                        skill = SkillId.intimidate;
                        break;
                    case "skill_sense_motive":
                        skill = SkillId.sense_motive;
                        break;
                    case "skill_gather_information":
                        skill = SkillId.gather_information;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            "Unknown skill constant used for dialog: " + skillConstant);
                }

                if (node.Operator == PythonOperator.GreaterThanOrEqual)
                {
                    Checks.Add(new SkillCheck
                    {
                        Skill = skill,
                        Ranks = skillRef
                    });
                }
                else if (node.Operator == PythonOperator.GreaterThan)
                {
                    Checks.Add(new SkillCheck
                    {
                        Skill = skill,
                        Ranks = skillRef + 1
                    });
                }
            }
        }
    }

    public struct SkillCheck
    {
        public SkillId Skill;

        public int Ranks;
    }
}