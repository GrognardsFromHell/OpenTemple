namespace SpicyTemple.Core
{
    public class GameLib
    {
        [TempleDllLocation(0x103072B8)]
        private bool _ironmanGame = false;

        [TempleDllLocation(0x10003860)]
        public bool IsIronmanGame
        {
            get => _ironmanGame;
        }

        [TempleDllLocation(0x10004870)]
        public void IronmanSave()
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10002e50)]
        public bool IsAutosaveBetweenMaps => Globals.Config.GetVanillaInt("autosave_between_maps") != 0;

        [TempleDllLocation(0x10004990)]
        public void MakeAutoSave()
        {
            Stub.TODO();
        }

    }
}