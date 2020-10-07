using System;
using System.ComponentModel;
using System.Threading;
using OpenTemple.Core.Platform;

namespace OpenTemple.Core
{
    // TODO: REMOVE,  QmlNet takes care of this...
    public class UiSynchronizationContext  : SynchronizationContext
    {

        private readonly IUserInterface _userInterface;

        private WeakReference _destinationThreadRef;

        [ThreadStatic]
        private static bool _inSyncContextInstallation;

        [ThreadStatic]
        private static SynchronizationContext _previousSyncContext;

        public UiSynchronizationContext(IUserInterface userInterface)
        {
            _userInterface = userInterface;
            DestinationThread = Thread.CurrentThread;   //store the current thread to ensure its still alive during an invoke.
        }

        private UiSynchronizationContext(IUserInterface userInterface, Thread destinationThread)
        {
            _userInterface = userInterface;
            DestinationThread = destinationThread;
        }

        // Directly holding onto the Thread can prevent ThreadStatics from finalizing.
        private Thread DestinationThread
        {
            get
            {
                if ((_destinationThreadRef != null) && (_destinationThreadRef.IsAlive))
                {
                    return _destinationThreadRef.Target as Thread;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    _destinationThreadRef = new WeakReference(value);
                }
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            Thread destinationThread = DestinationThread;
            if (destinationThread == null || !destinationThread.IsAlive)
            {
                throw new InvalidAsynchronousStateException("Thread no longer valid");
            }

            _userInterface.PostTask(() => d(state)).Wait();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            _userInterface.PostTask(() => d(state));
        }

        public override SynchronizationContext CreateCopy()
        {
            return new UiSynchronizationContext(_userInterface, DestinationThread);
        }

        // Instantiate and install a WF op sync context, and save off the old one.
        public static void Install(IUserInterface userInterface)
        {
            // Exit if we shouldn't auto-install, if we've already installed and we haven't uninstalled,
            // or if we're being called recursively (creating the WF
            // async op sync context can create a parking window control).
            if (_inSyncContextInstallation)
            {
                return;
            }

            if (SynchronizationContext.Current == null)
            {
                _previousSyncContext = null;
            }

            if (_previousSyncContext != null)
            {
                return;
            }

            _inSyncContextInstallation = true;
            try
            {
                var currentContext = AsyncOperationManager.SynchronizationContext;
                //Make sure we either have no sync context or that we have one of type SynchronizationContext
                if (currentContext == null || currentContext.GetType() == typeof(SynchronizationContext))
                {
                    _previousSyncContext = currentContext;

                    AsyncOperationManager.SynchronizationContext = new UiSynchronizationContext(userInterface);
                }
            }
            finally
            {
                _inSyncContextInstallation = false;
            }
        }

    }
}