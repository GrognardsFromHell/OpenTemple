using OpenTemple.Core.Time;

for (int i = 0; i < 100; i++) {
    UiSystems.Logbook.Keys.KeyAcquired(i, GameSystems.TimeEvent.GameTime);
}
