using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.Script
{
    public class SpellScriptSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly ScriptAssembly _scriptAssembly;

        public SpellScriptSystem(ScriptAssembly scriptAssembly)
        {
            _scriptAssembly = scriptAssembly;
        }

        [TempleDllLocation(0x100c0180)]
        public void SpellTrigger(int spellId, SpellEvent evt)
        {
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var activeSpell))
            {
                Logger.Warn("Trying to run spell event {0} for active spell id {1} which is unknown.",
                    evt, spellId);
                return;
            }

            if (!_scriptAssembly.TryCreateSpellScript(activeSpell.spellEnum, out var spellScript))
            {
                Logger.Warn("Failed to find spell script for spell id {0}", spellId);
                return;
            }

            // The spell effect gives each target a chance to cancel out the entire spell effect
            if (evt == SpellEvent.SpellEffect)
            {
                var cancel = false;
                foreach (var target in activeSpell.Targets)
                {
                    if (GameSystems.Script.ExecuteObjectScript(activeSpell.caster, target.Object, spellId,
                            ObjScriptEvent.SpellCast) == 0)
                    {
                        cancel = true;
                    }
                }

                if (cancel)
                {
                    return;
                }
            }

            switch (evt)
            {
                case SpellEvent.SpellEffect:
                    spellScript.OnSpellEffect(activeSpell);
                    break;
                case SpellEvent.BeginSpellCast:
                    spellScript.OnBeginSpellCast(activeSpell);
                    break;
                case SpellEvent.EndSpellCast:
                    spellScript.OnEndSpellCast(activeSpell);
                    break;
                case SpellEvent.BeginRound:
                    spellScript.OnBeginRound(activeSpell);
                    break;
                case SpellEvent.EndRound:
                    spellScript.OnEndRound(activeSpell);
                    break;
                case SpellEvent.BeginProjectile:
                case SpellEvent.EndProjectile:
                    throw new ArgumentException($"This function cannot be used for the projectile events.");
                case SpellEvent.BeginRoundD20Ping:
                    spellScript.OnBeginRoundD20Ping(activeSpell);
                    break;
                case SpellEvent.EndRoundD20Ping:
                    spellScript.OnEndRoundD20Ping(activeSpell);
                    break;
                case SpellEvent.AreaOfEffectHit:
                    spellScript.OnAreaOfEffectHit(activeSpell);
                    break;
                case SpellEvent.SpellStruck:
                    spellScript.OnSpellStruck(activeSpell);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evt), evt, null);
            }

            SpellSoundPlay(activeSpell, evt);
        }

        [TempleDllLocation(0x100c0390)]
        public void SpellTriggerProjectile(int spellId, SpellEvent evt, GameObject projectile, int projectileIdx)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100BF770)]
        public bool SpellSoundPlay(SpellPacketBody spellPkt, SpellEvent spellEvt)
        {
            if (spellEvt > SpellEvent.SpellStruck)
                return false;
            int spellSoundType;
            switch (spellEvt)
            {
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
            var modeTarget = spellEntry.modeTargetSemiBitmask.GetBaseMode();

            switch (spellEvt)
            {
                case SpellEvent.EndSpellCast:
                    GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
                    return true;
                case SpellEvent.SpellEffect:
                case SpellEvent.AreaOfEffectHit:
                case SpellEvent.SpellStruck:
                    if (spellPkt.spellEnum == 133)
                    {
                        // Dispel Magic
                        if (spellPkt.Targets.Length > 0)
                        {
                            GameSystems.SoundGame.PositionalSound(8660, spellPkt.Targets[0].Object);
                        }
                        else
                        {
                            GameSystems.SoundGame.PositionalSound(8660, spellPkt.aoeCenter.location);
                        }

                        return true;
                    }

                    goto case SpellEvent.BeginRound;
                case SpellEvent.BeginRound:
                    switch (modeTarget)
                    {
                        case UiPickerType.Single:
                            if (spellPkt.Targets.Length > 0)
                            {
                                GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.Targets[0].Object);
                                return true;
                            }

                            GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
                            break;
                        case UiPickerType.Multi:
                        case UiPickerType.Cone:

                            if (spellPkt.Targets.Length == 0)
                            {
                                GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.caster);
                                return true;
                            }

                            foreach (var target in spellPkt.Targets)
                            {
                                GameSystems.SoundGame.PositionalSound(spellSoundId, target.Object);
                            }

                            break;
                        case UiPickerType.Area:
                            if (spellPkt.Targets.Length == 0)
                            {
                                GameSystems.SoundGame.PositionalSound(spellSoundId, spellPkt.aoeCenter.location);
                                return true;
                            }

                            foreach (var target in spellPkt.Targets)
                            {
                                GameSystems.SoundGame.PositionalSound(spellSoundId, target.Object);
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

        [TempleDllLocation(0x100beb80)]
        public void UpdateSpell(int spellId)
        {
            Stub.TODO();
        }
    }
}