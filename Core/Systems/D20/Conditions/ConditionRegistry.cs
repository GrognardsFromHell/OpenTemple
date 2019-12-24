using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class ConditionRegistry
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly Dictionary<string, ConditionSpec> _conditionsByName;

        [TempleDllLocation(0x11868F60)]
        private readonly Dictionary<int, ConditionSpec> _conditionsByHash;

        private readonly List<ConditionAttachment> _globalAttachments;

        private readonly List<ConditionSpec> _pendingExtensions = new List<ConditionSpec>();

        [TempleDllLocation(0x100e19a0)]
        public ConditionRegistry()
        {
            _conditionsByName = new Dictionary<string, ConditionSpec>();
            _conditionsByHash = new Dictionary<int, ConditionSpec>();
            _globalAttachments = new List<ConditionAttachment>();
        }

        public void Register(IEnumerable<ConditionSpec> specs)
        {
            foreach (var spec in specs)
            {
                Register(spec);
            }
        }

        [TempleDllLocation(0x100e19c0)]
        public void Register(ConditionSpec spec, bool allowOverwrite = false)
        {
            if (spec.IsExtension)
            {
                RegisterExtension(spec);
                return;
            }

            if (!allowOverwrite && _conditionsByName.ContainsKey(spec.condName.ToUpperInvariant()))
            {
                throw new ArgumentException($"Condition {spec.condName} is already registered.");
            }

            // Index by both name and hash
            _conditionsByName[spec.condName.ToUpperInvariant()] = spec;
            var nameHash = ElfHash.Hash(spec.condName);
            _conditionsByHash[nameHash] = spec;

            // Process pending extensions to this spec
            for (var i = _pendingExtensions.Count - 1; i >= 0; i--)
            {
                var pendingExtension = _pendingExtensions[i];
                if (string.Equals(pendingExtension.condName, spec.condName, StringComparison.InvariantCultureIgnoreCase))
                {
                    spec.subDispDefs = _pendingExtensions[i].subDispDefs.Concat(spec.subDispDefs).ToImmutableArray();
                    _pendingExtensions.RemoveAt(i);
                }
            }
        }

        private void RegisterExtension(ConditionSpec spec)
        {
            if (_conditionsByName.TryGetValue(spec.condName.ToUpperInvariant(), out var baseSpec))
            {
                // Extend with the callbacks from the extension
                baseSpec.subDispDefs = baseSpec.subDispDefs.Concat(spec.subDispDefs).ToImmutableArray();
            }
            else
            {
                // Extension came before the base spec, so register it later, when the base spec is registered
                _pendingExtensions.Add(spec);
            }
        }

        public ConditionSpec this[string name] => _conditionsByName.GetValueOrDefault(name.ToUpperInvariant(), null);

        public ConditionSpec GetByHash(int nameElfHash) => _conditionsByHash.GetValueOrDefault(nameElfHash, null);

        public int Count => _conditionsByName.Count;

        [TempleDllLocation(0x100e1ee0)]
        public void AttachGlobally(ConditionSpec condition)
        {
            Trace.Assert(_conditionsByName.Values.Contains(condition));
            Trace.Assert(_globalAttachments.All(a => a.condStruct != condition));

            var attachment = new ConditionAttachment(condition);
            _globalAttachments.Add(attachment);
            Logger.Debug("Attaching condition '{0}' globally.", attachment.condStruct.condName);
        }

        public IEnumerable<ConditionAttachment> GlobalAttachments => _globalAttachments;

        public void WarnAboutPendingExtensions()
        {
            foreach (var pendingExtension in _pendingExtensions)
            {
                Logger.Info(" Base condition {0} for extension was not found.", pendingExtension.condName);
            }
        }
    }
}