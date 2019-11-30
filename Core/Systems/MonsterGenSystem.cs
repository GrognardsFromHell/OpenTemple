using System;
using System.Drawing;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class MonsterGenSystem : IGameSystem, ISaveGameAwareGameSystem, IBufferResettingSystem, IResetAwareSystem
    {

        [TempleDllLocation(0x10aa3288)]
        private byte[] monsterGenSthg;

        [TempleDllLocation(0x10aa328c)]
        private Rectangle screenRect => new Rectangle(Point.Empty, Tig.RenderingDevice.GetCamera().ScreenSize);

        [TempleDllLocation(0x100500c0)]
        public MonsterGenSystem()
        {
            monsterGenSthg = new byte[0x100];
        }

        [TempleDllLocation(0x10050160)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10050140)]
        public void Reset()
        {
            Array.Fill(monsterGenSthg, (byte) 0);
        }

        [TempleDllLocation(0x100501d0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            savedGameState.MonsterGenState.State = monsterGenSthg.ToArray();
        }

        [TempleDllLocation(0x100501a0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            monsterGenSthg = savedGameState.MonsterGenState.State.ToArray();
        }

        [TempleDllLocation(0x10050170)]
        public void ResetBuffers()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100508c0)]
        public void CritterKilled(GameObjectBody critter)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10050740)]
        public bool GetNextEventTime(GameObjectBody generator, out TimeSpan delay)
        {
            throw new NotImplementedException();
        }
    }
}