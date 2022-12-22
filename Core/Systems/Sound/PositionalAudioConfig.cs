using System.Collections.Generic;

namespace OpenTemple.Core.Systems.Sound;

public class PositionalAudioConfig
{
    /// <summary>
    /// Sounds closer than this (in screen coordinates) are unattenuated.
    /// This seems to be relative to the center of the screen.
    /// </summary>
    public Dictionary<SoundSourceSize, int> AttenuationRangeStart { get; } =
        new()
        {
            {SoundSourceSize.Small, 50},
            {SoundSourceSize.Medium, 50},
            {SoundSourceSize.Large, 150},
            {SoundSourceSize.ExtraLarge, 50}
        };

    /// <summary>
    /// Sound sources further away than this (in screen coordinates) from the
    /// center of the screen play at zero volume.
    /// TODO: This should calculate based on the screen edge.
    /// </summary>
    public Dictionary<SoundSourceSize, int> AttenuationRangeEnd { get; } =
        new()
        {
            {SoundSourceSize.Small, 150},
            {SoundSourceSize.Medium, 400},
            {SoundSourceSize.Large, 800},
            {SoundSourceSize.ExtraLarge, 1500}
        };

    /// <summary>
    /// The volume for sound sources of a given size at minimum attenuation.
    /// </summary>
    public Dictionary<SoundSourceSize, int> AttenuationMaxVolume { get; } =
        new()
        {
            {SoundSourceSize.Small, 40},
            {SoundSourceSize.Medium, 70},
            {SoundSourceSize.Large, 100},
            {SoundSourceSize.ExtraLarge, 100}
        };

    /// <summary>
    /// Sounds within this range of the screen center (in screen coordinates) play
    /// dead center.
    /// </summary>
    [TempleDllLocation(0x108f2880)]
    public int PanningMinRange { get; } = 150;

    /// <summary>
    /// Sounds further away than this range relative to the screen center (in screen coordinates) play
    /// fully on that side.
    /// </summary>
    [TempleDllLocation(0x108f2860)]
    public int PanningMaxRange { get; } = 400;

    public PositionalAudioConfig()
    {
    }

    public PositionalAudioConfig(Dictionary<int, string> parameters)
    {
        AttenuationRangeStart[SoundSourceSize.Large] = int.Parse(parameters[1]);
        AttenuationRangeEnd[SoundSourceSize.Large] = int.Parse(parameters[2]);
        PanningMinRange = int.Parse(parameters[3]);
        PanningMaxRange = int.Parse(parameters[4]);

        AttenuationRangeStart[SoundSourceSize.Small] = int.Parse(parameters[10]);
        AttenuationRangeEnd[SoundSourceSize.Small] = int.Parse(parameters[11]);
        AttenuationMaxVolume[SoundSourceSize.Small] = int.Parse(parameters[12]);

        AttenuationRangeStart[SoundSourceSize.Medium] = int.Parse(parameters[20]);
        AttenuationRangeEnd[SoundSourceSize.Medium] = int.Parse(parameters[21]);
        AttenuationMaxVolume[SoundSourceSize.Medium] = int.Parse(parameters[22]);

        AttenuationRangeStart[SoundSourceSize.ExtraLarge] = int.Parse(parameters[30]);
        AttenuationRangeEnd[SoundSourceSize.ExtraLarge] = int.Parse(parameters[31]);
        AttenuationMaxVolume[SoundSourceSize.ExtraLarge] = int.Parse(parameters[32]);
    }
}