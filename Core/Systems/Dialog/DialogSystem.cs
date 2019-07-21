using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Dialog
{
    public class DialogSystem : IGameSystem
    {
        private readonly Dictionary<int, PlayerVoiceSet> _playerVoiceSetsByKey = new Dictionary<int, PlayerVoiceSet>();

        [TempleDllLocation(0x108EC860)]
        private readonly List<PlayerVoiceSet> _playerVoiceSets = new List<PlayerVoiceSet>();

        [TempleDllLocation(0x10036040)]
        public DialogSystem()
        {
            LoadPlayerVoices();
        }

        [TempleDllLocation(0x10035660)]
        private void LoadPlayerVoices()
        {
            var playerVoiceFiles = Tig.FS.ReadMesFile("rules/pcvoice.mes");
            var playerVoiceNames = Tig.FS.ReadMesFile("mes/pcvoice.mes");

            foreach (var (key, filename) in playerVoiceFiles)
            {
                var name = playerVoiceNames[key];
                var voiceSet = new PlayerVoiceSet(key, name, filename);
                _playerVoiceSetsByKey[key] = voiceSet;
                _playerVoiceSets.Add(voiceSet);
            }
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10038470)]
        public void OnAfterTeleport(int targetMapId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100372d0)]
        private void GetNpcVoiceLine(GameObjectBody speaker, GameObjectBody listener,
            out string text, out int soundId, int a5, int a6, int a7)
        {
            if ( !GameSystems.AI.CanTalkTo(speaker, listener) )
            {
                text = null;
                soundId = -1;
            }
            else
            {
                DialogState dialogArgs = new DialogState();
                dialogArgs.npc = speaker;
                dialogArgs.npcId = speaker.id;
                dialogArgs.pc = listener;
                dialogArgs.pcId = listener.id;
                dialogArgs.dialogScriptId = 0;

                throw new NotImplementedException();
                soundId = dialogArgs.speechId;
            }
        }

        /// <summary>
        /// fetches a PC who is not identical to the object. For NPCs this will try to fetch their leader first.
        /// </summary>
        [TempleDllLocation(0x10034A40)]
        public GameObjectBody GetListeningPartyMember(GameObjectBody critter)
        {
            if (critter.IsNPC())
            {
                var leader = GameSystems.Critter.GetLeader(critter);
                if (leader != null)
                {
                    return leader;
                }
            }

            foreach (var otherPlayer in GameSystems.Party.PlayerCharacters)
            {
                if (otherPlayer != critter)
                {
                    return otherPlayer;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100348c0)]
        private void GetPcVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId,
            PlayerVoiceLine line)
        {
            var v9 = speaker.GetInt32(obj_f.pc_voice_idx);

            if (!_playerVoiceSetsByKey.TryGetValue(v9, out var voiceSet))
            {
                text = null;
                soundId = -1;
                return;
            }

            voiceSet.PickLine(line, out text, out soundId);
        }

        [TempleDllLocation(0x100373c0)]
        public bool TryGetOkayVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            Stub.TODO();
            text = null;
            soundId = -1;
            return false;
        }

        [TempleDllLocation(0x10037c60)]
        public void GetDyingVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId)
        {
            text = null;
            soundId = -1;

            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 1500, 1599, 12014);
            }
            else
            {
                GetPcVoiceLine(speaker, listener, out text, out soundId, PlayerVoiceLine.DeathCry);
            }
        }

        [TempleDllLocation(0x10037e40)]
        public void GetLeaderDyingVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId)
        {
            text = null;
            soundId = -1;

            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId,  3500, 3599, 12056);
            }
        }

        [TempleDllLocation(0x100373c0)]
        public bool TryGetWontSellVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text,
            out int soundId)
        {
            Stub.TODO();
            text = null;
            soundId = -1;
            return false;
        }

        [TempleDllLocation(0x10036120)]
        public bool PlayCritterVoiceLine(GameObjectBody obj, GameObjectBody objAddressed, string text, int soundId)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100374e0)]
        public void GetFleeVoiceLine(GameObjectBody speaker, GameObjectBody listener, out string text, out int soundId)
        {
            text = null;
            soundId = -1;
            if (speaker.IsNPC())
            {
                GetNpcVoiceLine(speaker, listener, out text, out soundId, 3000, 3099, 12029);
            }
        }
    }
}