namespace SpicyTemple.Core.GameObject
{

    /// <summary>
    /// Used for internal storage of an object reference that is stored in
    /// a <see cref="GameObjectBody"/>. It contains both the persistent
    /// ID of the object, as well as the optional live-reference.
    /// </summary>
    public struct ObjectRef
    {

        public ObjectId Id;

        public GameObjectBody Object;

    }

}