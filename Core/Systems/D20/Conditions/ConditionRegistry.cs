using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class ConditionRegistry
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x11868F60)]
        private readonly Dictionary<string, ConditionSpec> _conditionsByName;

        [TempleDllLocation(0x11868F60)]
        private readonly Dictionary<int, ConditionSpec> _conditionsByHash;

        private readonly List<ConditionAttachment> _globalAttachments;

        [TempleDllLocation(0x100e19a0)]
        public ConditionRegistry()
        {
            _conditionsByName = new Dictionary<string, ConditionSpec>();
            _conditionsByHash = new Dictionary<int, ConditionSpec>();
            _globalAttachments = new List<ConditionAttachment>();
        }

        [TempleDllLocation(0x100e19c0)]
        public void Register(ConditionSpec spec, bool allowOverwrite = false)
        {
            if (!allowOverwrite && _conditionsByName.ContainsKey(spec.condName))
            {
                throw new ArgumentException($"Condition {spec.condName} is already registered.");
            }

            // Index by both name and hash
            _conditionsByName[spec.condName] = spec;
            var nameHash = ElfHash.Hash(spec.condName);
            _conditionsByHash[nameHash] = spec;
        }

        public ConditionSpec this[string name] => _conditionsByName.GetValueOrDefault(name, null);

        public ConditionSpec GetByHash(int nameElfHash) => _conditionsByHash.GetValueOrDefault(nameElfHash, null);

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
    }
}