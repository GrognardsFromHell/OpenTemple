using System;
using System.Linq.Expressions;
using SharpDX.DXGI;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Fade;
using OpenTemple.Core.Systems.Teleport;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Script
{
    public static class ScriptUtilities
    {
        public const int DAMAGE_REDUCTION_HALF = 50;
        public const int DAMAGE_REDUCTION_QUARTER = 25;

        // TODO: Need to wire this up with the actual python pickers somehow
        public static GameObjectBody PickedObject => throw new NotImplementedException();

        public static int RandomRange(int from, int toInclusive) => GameSystems.Random.GetInt(from, toInclusive);

        public static void StartTimer(int delay, Expression<Action> callback, bool realTime = false)
        {
            Stub.TODO();
        }

        public const bool SkipDefault = false;

        public const bool RunDefault = true;

        public static Alignment PartyAlignment => GameSystems.Party.PartyAlignment;

        public static GameObjectBody PartyLeader => GameSystems.Party.GetLeader();

        public static GameObjectBody SelectedPartyLeader => GameSystems.Party.GetConsciousLeader();

        public static QuestState GetQuestState(int questId) => GameSystems.Quest.GetState(questId);

        public static void SetQuestState(int questId, QuestState state) => GameSystems.Quest.SetState(questId, state);

        public static void UnbotchQuest(int questId) => GameSystems.Quest.Unbotch(questId);

        [TempleDllLocation(0x1010cb50)]
        public static int GetGlobalVar(int index) => GameSystems.Script.GetGlobalVar(index);

        [TempleDllLocation(0x1010cb70)]
        public static void SetGlobalVar(int index, int value) => GameSystems.Script.SetGlobalVar(index, value);

        [TempleDllLocation(0x1010cb10)]
        public static bool GetGlobalFlag(int index) => GameSystems.Script.GetGlobalFlag(index);

        [TempleDllLocation(0x1010cb30)]
        public static void SetGlobalFlag(int index, bool value) => GameSystems.Script.SetGlobalFlag(index, value);

        public static void WorldMapTravelByDialog(int areaId) => GameUiBridge.WorldMapTravelByDialog(areaId);

        public static void RevealTownMapMarker(int mapId, int markerId) => GameUiBridge.RevealTownMapMarker(mapId, markerId);

        public static int StoryState
        {
            set => GameSystems.Script.StoryState = value;
            get => GameSystems.Script.StoryState;
        }

        [TempleDllLocation(0x1010d1a0)]
        public static void FadeAndTeleport(int timeToAdvance, int soundId, int movieId, int destMap, int locx, int locy)
        {
            FadeAndTeleportArgs fadeArgs = new FadeAndTeleportArgs();
            fadeArgs.timeToAdvance = timeToAdvance;
            fadeArgs.soundId = soundId;
            fadeArgs.movieId = movieId;
            fadeArgs.destMap = destMap;
            fadeArgs.destLoc.locx = locx;
            fadeArgs.destLoc.locy = locy;

            fadeArgs.FadeOutArgs.transitionTime = 2.0f;
            fadeArgs.FadeOutArgs.fadeSteps = 48;

            fadeArgs.FadeInArgs.transitionTime = 2.0f;
            fadeArgs.FadeInArgs.fadeSteps = 48;
            fadeArgs.FadeInArgs.color = PackedLinearColorA.Black;
            fadeArgs.FadeInArgs.flags = FadeFlag.FadeIn;

            fadeArgs.flags = 0;

            fadeArgs.somehandle = PartyLeader;
            if (fadeArgs.timeToAdvance > 0)
            {
                fadeArgs.flags |= FadeAndTeleportFlags.advance_time;
            }

            if (fadeArgs.soundId != 0)
            {
                fadeArgs.flags |= FadeAndTeleportFlags.play_sound;
            }

            if (fadeArgs.movieId > 0)
            {
                fadeArgs.flags |= FadeAndTeleportFlags.play_movie;
            }

            GameSystems.Teleport.FadeAndTeleport(in fadeArgs);
        }

        public static void Fade(int timeToAdvance, int soundId, int movieId, int delayUntilFadeInSecs)
        {
            // TODO: This seems to be way too much logic for the glue code layer, move this to fade.cpp maybe?

            // Trigger the fade out immediately
            FadeArgs fadeArgs = new FadeArgs();
            fadeArgs.flags = 0;
            fadeArgs.field10 = 0;
            fadeArgs.color = PackedLinearColorA.Black;
            fadeArgs.transitionTime = 2.0f;
            fadeArgs.fadeSteps = 48;
            GameSystems.GFade.PerformFade(ref fadeArgs);

            GameSystems.TextBubble.RemoveAll();

            if (timeToAdvance > 0) {
                var time = TimeSpan.FromSeconds(timeToAdvance);
                GameSystems.TimeEvent.AddGameTime(time);
            }

            if (movieId != 0) {
                // Originally the soundId was passed here
                // But since no BinkW movie actually uses soundtrack ids,
                // just skip it.
                if (soundId > 0) {
                    GameSystems.Movies.PlayMovieId(movieId, soundId);
                } else {
                    GameSystems.Movies.PlayMovieId(movieId, 0);
                }
            } else if (soundId > 0) {
                GameSystems.SoundGame.Sound(soundId);
            }

            if (delayUntilFadeInSecs > 0) {
                var delay = TimeSpan.FromSeconds(delayUntilFadeInSecs);
                fadeArgs.flags = FadeFlag.FadeIn;
                GameSystems.GFade.ScheduleFade(ref fadeArgs, delay);
            } else {
                fadeArgs.flags = FadeFlag.FadeIn;
                GameSystems.GFade.PerformFade(ref fadeArgs);
            }

        }

        public static void SetProjectileParticles(GameObjectBody projectile, object particles)
        {
            throw new NotImplementedException();
        }

        public static void EndProjectileParticles(GameObjectBody projectile)
        {
            throw new NotImplementedException();
        }

        public static void MakeAreaKnown(int area) => GameSystems.Area.MakeAreaKnown(area);

        public static bool IsAreaKnown(int area) => GameSystems.Area.IsAreaKnown(area);

        public static object AttachParticles(string id, GameObjectBody obj)
        {
            return GameSystems.ParticleSys.CreateAtObj(id, obj);
        }

        public static object SpawnParticles(string id, LocAndOffsets location)
        {
            return GameSystems.ParticleSys.CreateAt(id, location.ToInches3D());
        }

        public static object SpawnParticles(string id, locXY location)
        {
            // TODO: Calls to this method should be replaced with LocAndOffsets parameters
            throw new NotImplementedException();
        }

        public static void QueueRandomEncounter(int encounterId)
        {
            GameSystems.RandomEncounter.QueueRandomEncounter(encounterId);
        }

        public static void Sound(int soundId, int loopCount = 1)
        {
            // TODO: This should usually be reserved for UI sounds and otherwise should be positional!
            GameSystems.SoundGame.Sound(soundId);
        }

        /// <summary>
        /// Proto used for AoE objects.
        /// </summary>
        public const int OBJECT_SPELL_GENERIC = 12003;

        public static TimePoint CurrentTime => GameSystems.TimeEvent.GameTime;

        public static CampaignCalendar CurrentCalendar => GameSystems.TimeEvent.CampaignCalendar;

        public static int CurrentTimeSeconds => throw new NotImplementedException();

    }
}