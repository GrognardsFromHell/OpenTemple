using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Startup.Discovery
{
    public class ContentDiscovery
    {
        private static bool _initialized;

        private static List<IContentProvider> _contentProviders;

        private static void ProcessAssembly(Assembly assembly)
        {
            var markerAttribute = assembly.GetCustomAttribute<GameContentAttribute>();
            if (markerAttribute == null)
            {
                return;
            }

            _contentProviders.Add(new DefaultContentProvider(assembly));
        }

        private static void EnsureInitialized()
        {
            lock (typeof(ContentDiscovery))
            {
                if (_initialized)
                {
                    return;
                }

                _contentProviders = new List<IContentProvider>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    ProcessAssembly(assembly);
                }

                AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => { ProcessAssembly(args.LoadedAssembly); };

                _initialized = true;
            }
        }

        public static IEnumerable<D20ClassSpec> Classes
        {
            get
            {
                EnsureInitialized();
                return _contentProviders.SelectMany(contentProvider => contentProvider.Classes);
            }
        }

        public static IEnumerable<ConditionSpec> Conditions
        {
            get
            {
                EnsureInitialized();
                return _contentProviders.SelectMany(contentProvider => contentProvider.Conditions);
            }
        }

        public static IEnumerable<(FeatId, ConditionSpec)> FeatConditions
        {
            get
            {
                EnsureInitialized();
                return _contentProviders.SelectMany(contentProvider => contentProvider.FeatConditions);
            }
        }
    }
}