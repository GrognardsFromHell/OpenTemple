using System;

namespace OpenTemple.Core.AAS
{
    public enum AnimEventType
    {
        Script,
        End,
        Action
    }

    internal class EventHandler
    {
        private readonly Action<AasEvent> _animEventHandler;

        private readonly LegacyScriptConverter _scriptConverter = new LegacyScriptConverter();

        private AnimEvents _flagsOut;

        public EventHandler(Action<AasEvent> animEventHandler)
        {
            _animEventHandler = animEventHandler;
        }

        public void SetFlagsOut(AnimEvents flagsOut)
        {
            _flagsOut = flagsOut;
        }

        public void ClearFlagsOut()
        {
            _flagsOut = null;
        }

        public void HandleEvent(int frame, float frameTime, AnimEventType type, string args)
        {
            switch (type)
            {
                case AnimEventType.Action:
                    if (_flagsOut != null)
                    {
                        _flagsOut.action = true;
                    }

                    break;
                case AnimEventType.End:
                    if (_flagsOut != null)
                    {
                        _flagsOut.end = true;
                    }

                    break;
                case AnimEventType.Script:
                    if (_animEventHandler != null && _scriptConverter.TryConvert(args, out var scriptEvt))
                    {
                        _animEventHandler(scriptEvt);
                    }

                    break;
            }
        }
    }
}