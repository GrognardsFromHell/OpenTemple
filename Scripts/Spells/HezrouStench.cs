
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(735)]
    public class HezrouStench : BaseSpellScript
    {
        //
        //Stench (Ex)
        //
        //sp-Hezrou Stench
        //	OnEnter:
        //		If first time:
        //			- Do saving throw (Con, DC24)
        //			Success:
        //				- Apply Sickened
        //			Failure:
        //				- Apply Nauseated
        //	D20Signal (S_Spell_End): data1    DONE
        //		0x100D3430 AoESpellRemove(DCA)
        //	OnConditionAdd:   
        //		BeginHezrouStench:
        //			similar to stinking cloud
        //			
        //	(no ImmunityTrigger, no CountDown, no Q_Critter_Has_Spell_Active)
        //sp-Hezrou Stench Hit
        //	OnLeave:
        //		if Nauseated: 
        //			change duration to 1d4
        //	Countdown expired: (relevant only for 1d4 really)
        //		if Nauseated:
        //			if inside AoE:
        //				demote to Sickened
        //			if outside AoE:
        //				nothing
        //	TurnbasedInit:
        //		if Nauseated:
        //			reduce hourglass to 1
        //	AoOPossible:
        //		if Nauseated:
        //			SetTo0
        //	Attack Rolls:
        //		-2 penalty
        //	Damage Rolls:
        //		-2 penalty
        //	Saving Throws:
        //		-2 penalty
        //	Skill checks:
        //		-2 penalty
        //	Ability checks:
        //		-2 penalty
        //		
        //
        //		
        //Add to ImmunityHandler (type 13/14 I think)
        //		
        //A hezrou???s skin produces a foul-smelling, toxic liquid whenever it fights. 
        //
        //Any living creature (except other demons) within 10 feet must succeed on a DC 24 Fortitude save or be nauseated for as long as it remains within the affected area and for 1d4 rounds afterward. 
        //
        //Creatures that successfully save are sickened for as long as they remain in the area. 
        //A creature that successfully saves cannot be affected again by the same hezrou???s stench for 24 hours. 
        //A delay poison or neutralize poison spell removes either condition from one creature. 
        //Creatures that have immunity to poison are unaffected, and creatures resistant to poison receive their normal bonus on their saving throws. The save DC is Constitution-based.
        //
        //
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hezrou Stench OnBeginSpellCast id= {0}", spell.spellId);
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster); // change to stinking cloud?
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Hezrou Stench OnSpellEffect");
            spell.dc = 100;
            spell.duration = 1000;
            // processStench(spell.caster, spell.id) # fuck youuuuuuuu
            spell.caster.AddCondition("Hezrou Stench", spell.spellId, spell.duration, 0);
        }
        // spell.spell_end(spell.id)

        public override void OnAreaOfEffectHit(SpellPacketBody spell)
        {
            Logger.Info("Hezrou Stench OnAreaOfEffectHit: ");
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Hezrou Stench OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Hezrou Stench OnEndSpellCast");
            spell.EndSpell();
        }

    }
}
