using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTemple.Core.Ui.DOM
{
    public delegate void MutationObserverCallback(IReadOnlyList<MutationRecord> records, MutationObserver observer);

    public class MutationObserverImpl : MutationObserver
    {
        // If we were to implement the MutationObserver by spec, the MutationObservers will not be collected by the GC because
        // all the MO are kept in a mutation observer list (https://github.com/jsdom/jsdom/pull/2398/files#r238123889). The
        // mutation observer list is primarily used to invoke the mutation observer callback in the same order than the
        // mutation observer creation.
        // In order to get around this issue, we will assign an increasing id for each mutation observer, this way we would be
        // able to invoke the callback in the creation order without having to keep a list of all the mutation observers.
        private static int _mutationObserverId = 0;

        // Non-spec compliant: List of all the mutation observers with mutation records enqueued. It's a replacement for
        // mutation observer list (https://dom.spec.whatwg.org/#mutation-observer-list) but without leaking since it's empty
        // before notifying the mutation observers.
        private static readonly HashSet<MutationObserverImpl> ActiveMutationObservers = new HashSet<MutationObserverImpl>();

        // https://dom.spec.whatwg.org/#mutation-observer-compound-microtask-queued-flag
        private static bool mutationObserverMicrotaskQueueFlag;

        internal List<MutationRecord> _recordQueue = new List<MutationRecord>();

        private readonly MutationObserverCallback _callback;

        private readonly List<Node> _nodeList = new List<Node>();

        private int _id;

        public MutationObserverImpl(MutationObserverCallback callback)
        {
            this._callback = callback;
            this._id = ++_mutationObserverId;
        }

        // https://dom.spec.whatwg.org/#queue-a-tree-mutation-record
        public static void QueueTreeMutationRecord(Node target, IReadOnlyList<Node> addedNodes, IReadOnlyList<Node> removedNodes, Node previousSibling, Node nextSibling) {
            QueueMutationRecord(
                MutationType.CHILD_LIST,
                target,
                null,
                null,
                null,
                addedNodes,
                removedNodes,
                previousSibling,
                nextSibling
            );
        }

        // https://dom.spec.whatwg.org/#queue-an-attribute-mutation-record
        public static void QueueAttributeMutationRecord(Node target, string name, string ns, string oldValue)
        {
            QueueMutationRecord(
                MutationType.ATTRIBUTES,
                target,
                name,
                ns,
                oldValue,
                new List<Node>(),
                new List<Node>(),
                null,
                null
            );
        }

        // https://dom.spec.whatwg.org/#queue-a-mutation-record
        public static void QueueMutationRecord(
            MutationType type,
            Node target,
            string name,
            string ns,
            string oldValue,
            IReadOnlyList<Node> addedNodes,
            IReadOnlyList<Node> removedNodes,
            Node previousSibling,
            Node nextSibling
        ) {
            var interestedObservers = new Dictionary<MutationObserverImpl, string>();

            foreach (var node in target.AncestorsIterator()) {
                foreach (var registered in node._registeredObserverList) {
                    var options = registered.options;
                    var mo = (MutationObserverImpl) registered.observer;

                    if (
                        !(node != target && options.subtree == false) &&
                        !(type == MutationType.ATTRIBUTES && options.attributes != true) &&
                        !(type == MutationType.ATTRIBUTES && options.attributeFilter != null &&
                          !options.attributeFilter.Any(value => value == name || value == ns)) &&
                        !(type == MutationType.CHARACTER_DATA && options.characterData != true) &&
                        !(type == MutationType.CHILD_LIST && options.childList == false)
                    ) {
                        interestedObservers.TryAdd(mo, null);

                        if (
                            (type == MutationType.ATTRIBUTES && options.attributeOldValue == true) ||
                            (type == MutationType.CHARACTER_DATA && options.characterDataOldValue == true)
                        )
                        {
                            interestedObservers[mo] = oldValue;
                        }
                    }
                }
            }

            foreach (var (observer, mappedOldValue) in interestedObservers) {
                var record = new MutationRecord(
                    type,
                    target,
                    attributeName: name,
                    attributeNamespace: ns,
                oldValue: mappedOldValue,
                addedNodes,
                removedNodes,
                previousSibling,
                nextSibling
                    );

                observer._recordQueue.Add(record);
                ActiveMutationObservers.Add(observer);
            }

            QueueMutationObserverMicrotask();
        }

        // https://dom.spec.whatwg.org/#queue-a-mutation-observer-compound-microtask
        private static void QueueMutationObserverMicrotask()
        {
            if (mutationObserverMicrotaskQueueFlag) {
                return;
            }

            mutationObserverMicrotaskQueueFlag = true;

            // TODO: This will likely run asynchronously, which we do NOT want
            Task.Run(NotifyMutationObservers);
        }

        // https://dom.spec.whatwg.org/#notify-mutation-observers
        private static void NotifyMutationObservers() {
          mutationObserverMicrotaskQueueFlag = false;

          var notifyList = ActiveMutationObservers.ToList();
          notifyList.Sort((a, b) => a._id - b._id);
          ActiveMutationObservers.Clear();

          foreach (var mo in notifyList)
          {
              // TODO OPTIMIZE (i.e. swap)
              var records = mo._recordQueue.ToArray();
              mo._recordQueue.Clear();

            foreach (var node in mo._nodeList)
            {
                // TODO: Is this correct???
              node._registeredObserverList.RemoveAll(ro => ro.observer == mo);

              if (records.Length > 0) {
                try {
                  mo._callback(
                    records,
                    mo
                  );
                } catch (Exception e) {
                  var target = records[0].Target;
                  ((Document)target.OwnerDocument).ReportException(e);
                }
              }
            }
          }
        }
    }

    public enum MutationType
    {
        ATTRIBUTES,
        CHARACTER_DATA,
        CHILD_LIST
    }

    public class MutationRecord
    {
        public MutationType Type {get;}
        public Node Target {get;}
        public string AttributeName {get;}
        public string AttributeNamespace {get;}
        public string OldValue {get;}
        public IReadOnlyList<Node> AddedNodes {get;}
        public IReadOnlyList<Node> RemovedNodes {get;}
        public Node PreviousSibling {get;}
        public Node NextSibling {get;}

        public MutationRecord(MutationType type, Node target, string attributeName, string attributeNamespace, string oldValue, IReadOnlyList<Node> addedNodes, IReadOnlyList<Node> removedNodes, Node previousSibling, Node nextSibling)
        {
            Type = type;
            Target = target;
            AttributeName = attributeName;
            AttributeNamespace = attributeNamespace;
            OldValue = oldValue;
            AddedNodes = addedNodes;
            RemovedNodes = removedNodes;
            PreviousSibling = previousSibling;
            NextSibling = nextSibling;
        }
    }
}