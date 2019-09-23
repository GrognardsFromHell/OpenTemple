
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

namespace Scripts.Spells
{
    [SpellScript(472)]
    public class SummonMonsterVI : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Monster VI OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Summon Monster VI OnSpellEffect");
            var teststr = "; summon monster 6\n"; // change this to the header line for the spell in spells_radial_menu_options.mes
            var options = get_options_from_mes(teststr);
            spell.duration = 1 * spell.casterLevel;
            // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (!(options).Contains(spell_arg))
            {
                var x = RandomRange(0, options.Count - 1);
                spell_arg = options[x];
            }

            // create monster, monster should be added to target_list
            spell.SummonMonsters(true, spell_arg);
            var target_item = spell.Targets[0];
            AttachParticles("sp-Summon Monster V", target_item.Object);
            SummonMonsterTools.SummonMonster_Rectify_Initiative(spell, spell_arg); // Added by S.A. - sets iniative to caster's initiative -1, so that it gets to act in the same round
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Summon Monster VI OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Summon Monster VI OnEndSpellCast");
        }
        public static List<GameObjectBody> get_options_from_mes(FIXME teststr)
        {
            Logger.Info("get_options_from_mes");
            var options = new List<GameObjectBody>();
            var i_file = open("data\\mes\\spells_radial_menu_options.mes", "r");
            var s = i_file.readline/*Unknown*/();
            while (s != teststr && s != "")
            {
                s = i_file.readline/*Unknown*/();
            }

            s = i_file.readline/*Unknown*/();
            var str_list = s.split/*Unknown*/();
            var num_str = str_list[1];
            num_str = num_str.strip/*Unknown*/();
            num_str = num_str.replace/*Unknown*/("{", "");
            num_str = num_str.replace/*Unknown*/("}", "");
            var num_options = (int)(num_str);
            var i = 0;
            while (i < num_options)
            {
                s = i_file.readline/*Unknown*/();
                str_list = s.split/*Unknown*/();
                num_str = str_list[1];
                num_str = num_str.strip/*Unknown*/();
                num_str = num_str.replace/*Unknown*/("{", "");
                num_str = num_str.replace/*Unknown*/("}", "");
                options.Add((int)(num_str));
                i = i + 1;
            }

            i_file.close/*Unknown*/();
            return options;
        }

    }
}
