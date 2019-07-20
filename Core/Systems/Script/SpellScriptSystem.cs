using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.Script
{
    public class SpellScriptSystem
    {

	    private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x100c0180)]
        public void SpellTrigger(int spellId, SpellEvent evt)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100BF770)]
        public bool SpellSoundPlay(SpellPacketBody spellPkt, SpellEvent spellEvt)
        {
            	if (spellEvt > SpellEvent.SpellStruck)
					return false;
				var spellSoundType = 0;
				switch (spellEvt) {

				case SpellEvent.BeginSpellCast:
					spellSoundType = 0;
					break;
				case SpellEvent.EndSpellCast:
					spellSoundType = 1;
					break;
				case SpellEvent.SpellEffect:
					spellSoundType = 2;
					break;
				case SpellEvent.BeginRound:
					spellSoundType = 3;
					break;
				case SpellEvent.BeginProjectile:
					spellSoundType = 4;
					break;
				case SpellEvent.EndProjectile:
					spellSoundType = 5;
					break;
				case SpellEvent.AreaOfEffectHit:
					spellSoundType = 7;
					break;
				case SpellEvent.SpellStruck:
					spellSoundType = 8;
					break;
				default:
					return false;
				}
				var spellSoundId = spellSoundType + 20 * spellPkt.spellEnum + 6000;
				SpellEntry spellEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);
				//UiPickerType modeTarget = pickerResult.GetModeTarget();
				UiPickerType modeTarget = (UiPickerType)(spellEntry.modeTargetSemiBitmask & 0xFF);
				var tgtCount = spellPkt.targetCount;

				switch (spellEvt) {
				case SpellEvent.EndSpellCast:
					GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
					return true;
				case SpellEvent.SpellEffect:
				case SpellEvent.AreaOfEffectHit:
				case SpellEvent.SpellStruck:
					if (spellPkt.spellEnum == 133) { // Dispel Magic
						if (spellPkt.targetCount <= 1) {
							GameSystems.SoundGame.PositionalSound(8660, spellPkt.targetListHandles[0]);
						}
						else {
							GameSystems.SoundGame.PositionalSound(8660, spellPkt.aoeCenter.location);
						}
						return true;
					}

					goto case SpellEvent.BeginRound;
				case SpellEvent.BeginRound:
					switch (modeTarget)
					{
					case UiPickerType.Single:
						if (spellPkt.targetListHandles[0] == null) {
							GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
							return true;
						}
						GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.targetListHandles[0]);
						break;
					case UiPickerType.Multi:
					case UiPickerType.Cone:

						if (tgtCount == 0) {
							GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
							return true;
						}
						for (var i = 0u; i < tgtCount; i++)
						{
							if (spellPkt.targetListHandles[i] != null) {
								GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.targetListHandles[i]);
							}
							else {
								Logger.Debug("SpellSoundPlay: invalid target handle caught!");
								int dummy = 1;
							}
						}
						break;
					case UiPickerType.Area:
						if (tgtCount == 0) {
							GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.aoeCenter.location);
							return true;
						}
						for (var i = 0u; i < tgtCount; i++) {
							if (spellPkt.targetListHandles[i] != null) {
								GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.targetListHandles[i]);
							}
							else {
								Logger.Debug("SpellSoundPlay: invalid target handle caught!");
								int dummy = 1;
							}
						}
						break;
					case UiPickerType.Wall:
					case UiPickerType.Location:
						GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.aoeCenter.location);
						return true;
					case UiPickerType.Personal:
					case UiPickerType.InventoryItem:
					case UiPickerType.Ray:
						GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
						return true;

					default:
						break;
					}
					return true;


				case SpellEvent.BeginSpellCast:
					GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
					return true;
				case SpellEvent.BeginProjectile:
				case SpellEvent.EndProjectile:
					return true;
				default:
					return false;
				}
        }

        public void SpellTriggerProjectile(int spellId, SpellEvent evt, GameObjectBody projectile, int targetIdx)
        {
	        Stub.TODO();
        }

        public void UpdateSpell(int spellId)
        {
	        Stub.TODO();
        }
    }
}