using System;

namespace SpicyTemple.Core.AAS
{
    public enum AnimEventType
    {
        Script,
        End,
        Action
    };

    public interface IAnimEventHandler
    {
        void HandleEvent(int frame, float frameTime, AnimEventType type, string args);
    }
}