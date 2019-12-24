using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Actions
{

	public class D20SpellData
	{
		public int spellEnumOrg;
		public MetaMagicData metaMagicData;
		public int spellClassCode;
		public int itemSpellData;
		public SpontCastType spontCastType; // was 4-bit originally
		public int spellSlotLevel; // was 4-bit originally

        public bool HasItem => itemSpellData != -1;

		[TempleDllLocation(0x10077850)]
		public int SpellEnum
		{
			get
			{
				if (spontCastType != SpontCastType.None)
				{
					return GameSystems.Spell.GetSpontaneousCastSpell(spontCastType, spellSlotLevel);
				}
				else
				{
					return spellEnumOrg;
				}
			}
		}

		public D20SpellData()
		{
		}

		public D20SpellData(SpellStoreData spell)
			: this(spell.spellEnum, spell.classCode, spell.spellLevel, metaMagicData:spell.metaMagicData)
		{
		}

		public D20SpellData(int spellEnumOrg, int spellClassCode, int spellSlotLevel, int itemSpellData = -1,
			MetaMagicData metaMagicData = default, SpontCastType spontCastType = default)
		{
			SetSpellData(spellEnumOrg, spellClassCode, spellSlotLevel, itemSpellData, metaMagicData, spontCastType);
		}

		[TempleDllLocation(0x10077800)]
		public void SetSpellData(int spellEnumOrg, int spellClassCode, int spellSlotLevel, int itemSpellData = -1,
			MetaMagicData metaMagicData = default, SpontCastType spontCastType = default)
		{
			this.metaMagicData = metaMagicData;
			this.spellEnumOrg = spellEnumOrg;
			this.spellClassCode = spellClassCode;
			this.itemSpellData = itemSpellData;
			this.spontCastType = spontCastType;
			this.spellSlotLevel = spellSlotLevel;
		}

	}

	public class D20Action
    {
	    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public D20ActionType d20ActType;
        public int data1; // generic piece of data
        public D20CAF d20Caf; // Based on D20_CAF flags
        public uint field_C; // unknown use
        public GameObjectBody d20APerformer;
        public GameObjectBody d20ATarget;
        public LocAndOffsets destLoc; // action located (usually movement destination)
        public float distTraversed; // distanced traversed by a move action
        public int radialMenuActualArg; // the value chosen by radial menu toggle/slider
        public int rollHistId0 = -1;
        public int rollHistId1 = -1;
        public int rollHistId2 = -1;

        public D20SpellData d20SpellData = new D20SpellData();
        public int spellId;
        public int animID;
        public PathQueryResult path;

        public D20Action()
        {
        }

        public D20Action(D20ActionType type)
        {
            d20ActType = type;
        }

        [TempleDllLocation(0x10093810)]
        public D20Action(D20ActionType type, GameObjectBody performer)
        {
            d20ActType = type;
            d20APerformer = performer;
            destLoc = performer.GetLocationFull();
        }

        [TempleDllLocation(0x10093810)]
        internal void Reset(GameObjectBody performer)
        {
	        d20APerformer = performer;
	        d20ActType = D20ActionType.NONE;
	        data1 = 0;
	        d20ATarget = null;
	        destLoc = performer.GetLocationFull();
	        distTraversed = 0;
	        radialMenuActualArg = 0;
	        spellId = 0;
	        d20Caf = 0;
	        GameSystems.D20.Actions.ReleasePooledPathQueryResult(ref path);
	        d20SpellData = new D20SpellData();
	        animID = 0;
	        rollHistId1 = -1;
	        rollHistId2 = -1;
	        rollHistId0 = -1;
        }

        public bool FilterSpellTargets(SpellPacketBody spellPkt)
        {
	        if (spellPkt.Targets.Length <= 0)
		        return false;

	        var spEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);

	        void blinkSpellHandler(D20Action d20a, SpellEntry spellEntry) {
		        if (!d20a.d20APerformer.HasCondition("sp-Blink"))
		        {
			        return;
		        }

		        if (!spellEntry.IsBaseModeTarget(UiPickerType.Single))
		        {
			        return;
		        }

		        // "Spell failure due to Blink" roll
		        var rollRes = Dice.D100.Roll();
		        if (rollRes >= 50)
		        {
			        GameSystems.RollHistory.AddPercentageCheck(d20a.d20APerformer, d20a.d20ATarget, 50, 111, rollRes, 62, 192);
		        } else {
			        GameSystems.Spell.FloatSpellLine(d20a.d20APerformer, 30015, TextFloaterColor.White);
			        GameSystems.ParticleSys.CreateAtObj("Fizzle", d20a.d20ATarget);
			        GameSystems.RollHistory.AddPercentageCheck(d20a.d20APerformer, d20a.d20ATarget, 50, 111, rollRes, 112, 192); // Miscast (Blink)!
			        GameSystems.D20.Actions.CurrentSequence?.ResetSpell();
		        }
	        }

	        int targetsAftedProcessing = CastSpellProcessTargets(this, spellPkt);
	        blinkSpellHandler(this, spEntry); // originally this cheked the result, but the result was always 0 anyway
	        return SpellTargetsFilterInvalid();
        }

        [TempleDllLocation(0x1008dcf0)]
        private bool SpellTargetsFilterInvalid()
        {
	        var valid = true;

	        var curSeq = GameSystems.D20.Actions.CurrentSequence;
	        var orgTgtCount = curSeq.spellPktBody.Targets.Length;
	        List<GameObjectBody> validTargets = new List<GameObjectBody>(orgTgtCount);

	        for (var i = 0; i < orgTgtCount; i++){
		        var tgt = curSeq.spellPktBody.Targets[i].Object;

		        if (tgt == null){
			        Logger.Warn("Null target handle in spell target list! Filtering out...");
			        continue;
		        }

		        // Check if Critter
		        if (tgt.IsCritter()){
			        if (d20ATarget == null)
			        {
				        d20ATarget = tgt;
			        }

			        // Check Q_CanBeAffected_PerformAction
			        var canBeAffected = GameSystems.D20.D20QueryWithObject(tgt, D20DispatcherKey.QUE_CanBeAffected_PerformAction, this, 1);
			        if (tgt.GetDispatcher() != null && canBeAffected == 0) {
				        if (curSeq.spellPktBody.Targets.Length == 0) // shouldn't be possible but it was there in the code...
					        valid = false;
				        continue;
			        }
		        }

		        validTargets.Add(tgt);
	        }

	        curSeq.spellPktBody.SetTargets(validTargets);

	        return valid && curSeq.spellPktBody.Targets.Length > 0;
        }

		private int CastSpellProcessTargets(D20Action action, SpellPacketBody spellPkt) {

			List<GameObjectBody> targets = new List<GameObjectBody>();

			for (var i = 0u; i < spellPkt.Targets.Length; i++) {
				var tgt = spellPkt.Targets[i].Object;
				if (tgt == null){
					Logger.Warn("CastSpellProcessTargets: Null target! Idx {0}",i);
					continue;
				}

				if (!tgt.IsCritter())
				{
					targets.Add(tgt);
					continue;
				}

				// check target spell immunity
				var dispIoImmunity = DispIoImmunity.Default;
				dispIoImmunity.flag = 1;
				dispIoImmunity.spellPkt = spellPkt;
				if (tgt.Dispatch64ImmunityCheck(dispIoImmunity))
					continue;

				// check spell resistance for hostiles
				if (GameSystems.Critter.IsFriendly(action.d20APerformer, tgt)){
					targets.Add(tgt);
					continue;
				}

				var spEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);
				if (spEntry.spellResistanceCode != SpellResistanceType.Yes)
				{
					targets.Add(tgt);
					continue;
				}

				BonusList casterLvlBonlist = BonusList.Default;
				var casterLvl = spellPkt.casterLevel;
				casterLvlBonlist.AddBonus(casterLvl, 0, 203);
				if (GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.SPELL_PENETRATION) != 0)	{
					casterLvlBonlist.AddBonusFromFeat(2, 0, 114, FeatId.SPELL_PENETRATION);
				}
				if (GameSystems.Feat.HasFeatCountByClass(action.d20APerformer, FeatId.GREATER_SPELL_PENETRATION) != 0) {
					casterLvlBonlist.AddBonusFromFeat(2, 0, 114, FeatId.GREATER_SPELL_PENETRATION);
				}

				// New Spell resistance mod
				spellPkt.caster.DispatchSpellResistanceCasterLevelCheck(tgt, casterLvlBonlist, spellPkt);

				var dispelDc = tgt.Dispatch45SpellResistanceMod(spEntry);
				if (dispelDc <=0){
					targets.Add(tgt);
					continue;
				}

				if (GameSystems.Spell.DispelRoll(action.d20APerformer, casterLvlBonlist, 0, dispelDc, GameSystems.D20.Combat.GetCombatMesLine(5048), out var rollHistId) < 0){
					Logger.Info("CastSpellProcessTargets: spell {0} cast by {1} resisted by target {2}", spellPkt.GetName(), action.d20APerformer, tgt );
					GameSystems.Spell.FloatSpellLine(tgt, 30008, TextFloaterColor.White);
					GameSystems.ParticleSys.CreateAtObj("Fizzle", tgt);

					var text = $"{GameSystems.D20.Combat.GetCombatMesLine(119)}{rollHistId}{GameSystems.D20.Combat.GetCombatMesLine(120)}\n\n"; // Spell ~fails~ to overcome Spell Resistance
					GameSystems.RollHistory.CreateFromFreeText(text);

					GameSystems.Spell.UpdateSpellPacket(spellPkt);
					GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
					continue;
				}
				else
				{
					var text =
						$"{GameSystems.D20.Combat.GetCombatMesLine(121)}{rollHistId}{GameSystems.D20.Combat.GetCombatMesLine(122)}\n\n"; // Spell ~overcomes~ Spell Resistance
					GameSystems.RollHistory.CreateFromFreeText(text);
				}

				GameSystems.Spell.FloatSpellLine(tgt, 30009, TextFloaterColor.Red);
				targets.Add(tgt);

			}

			spellPkt.SetTargets(targets);

			GameSystems.Spell.UpdateSpellPacket(spellPkt);
			GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
			return spellPkt.Targets.Length;
		}

        public D20ADF GetActionDefinitionFlags()
        {
            return GameSystems.D20.Actions.GetActionFlags(d20ActType);
        }

        public bool IsMeleeHit()
        {
            return d20Caf.HasFlag(D20CAF.HIT) && !d20Caf.HasFlag(D20CAF.RANGED);
        }

        public D20Action Copy()
        {
            return new D20Action
            {
                animID = animID,
                d20ActType = d20ActType,
                d20APerformer = d20APerformer,
                d20ATarget = d20ATarget,
                d20SpellData = d20SpellData,
                d20Caf = d20Caf,
                data1 = data1,
                destLoc = destLoc,
                distTraversed = distTraversed,
                field_C = field_C,
                path = path,
                spellId = spellId,
                rollHistId0 = rollHistId0,
                rollHistId1 = rollHistId1,
                rollHistId2 = rollHistId2,
                radialMenuActualArg = radialMenuActualArg
            };
        }



    }
}