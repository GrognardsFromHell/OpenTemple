using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems
{
    public class DialogSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10038470)]
        public void OnAfterTeleport(int targetMapId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100373c0)]
        public bool TryGetOkayVoiceLine(GameObjectBody obj, GameObjectBody obj2, out string text, out int soundId)
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
    }
}