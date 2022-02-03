namespace OpenTemple.Core.Systems.RadialMenus;

public enum RadialMenuEntryType
{
	Action = 0,
	Slider = 1,
	Toggle = 2, // Toggle button
	Choice = 3, // One of N (broken in vanilla, i.e. Guidance)
	Parent = 4
}