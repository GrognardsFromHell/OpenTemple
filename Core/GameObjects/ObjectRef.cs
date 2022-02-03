namespace OpenTemple.Core.GameObjects;

/// <summary>
/// Used for internal storage of an object reference that is stored in
/// a <see cref="GameObject"/>. It contains both the persistent
/// ID of the object, as well as the optional live-reference.
/// </summary>
public struct ObjectRef
{

    public ObjectId Id;

    public GameObject Object;

}