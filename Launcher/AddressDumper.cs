using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SpicyTemple.Core;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Utils;

namespace Launcher
{
    /// <summary>
    /// This class scans through the assembly for [TempleDllLocation] attributes and dumps them with
    /// a way to access them into a JSON file.
    /// It does this in two passes, first finding any public way of accessing something, then re-trying
    /// with inclusion of private and internal fields. If something is accessible in two ways, it's
    /// ignored (i.e. GameObjBody is accessible in many ways since it's not meant to be a singleton type).
    /// If something is only accessible once in a public way, and multiple times in a private context,
    /// it is still included. (i.e., private fields with references to a singleton may exist).
    /// </summary>
    public class AddressDumper
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private AddressDumperPass pass = AddressDumperPass.Public;
        private readonly ISet<Type> publiclyAccessibleTypes = new HashSet<Type>();
        private readonly ISet<Type> scanBlacklist = new HashSet<Type>();
        private readonly Dictionary<Type, string> alreadyScanned = new Dictionary<Type, string>();
        private Dictionary<string, ISet<string>> addresses = new Dictionary<string, ISet<string>>();
        private ISet<string> _properties = new HashSet<string>();

        private readonly Assembly _coreAssembly = typeof(GameLoop).Assembly;

        private bool IsCoreType(Type type) => type.Assembly == _coreAssembly;

        // Types that are known to NOT be singletons and thus don't need to be scanned for singleton access
        private static readonly ISet<Type> KnownInstanceTypes = new HashSet<Type>
        {
            typeof(GameObjectBody),
            typeof(Dice),
            typeof(locXY),
            typeof(LocAndOffsets)
        };

        private void ScanStaticEntryPoint(Type type)
        {
            var currentPath = type.Name;

            // Check all static methods, fields, properties
            ScanMembers(type, true, currentPath);
        }

        private void ScanEntryPoint(Type type, string currentPath, bool isPrivateRef)
        {
            if (!IsCoreType(type) || scanBlacklist.Contains(type))
            {
                return;
            }

            if (isPrivateRef && publiclyAccessibleTypes.Contains(type))
            {
                // This is a private ref, but the type is otherwise publicly accessible
                return;
            }

            if (alreadyScanned.ContainsKey(type))
            {
                throw new AlreadyScannedException(type, alreadyScanned[type], currentPath);
            }

            if (pass == AddressDumperPass.Public)
            {
                publiclyAccessibleTypes.Add(type);
            }

            alreadyScanned[type] = currentPath;

            // Check all static methods, fields, properties
            ScanMembers(type, false, currentPath);
        }

        private void ScanMembers(Type type, bool staticMembers, string currentPath)
        {

            if (type.GetCustomAttribute<DontUseForAutoTranslationAttribute>() != null)
            {
                return;
            }

            var flags = staticMembers ? BindingFlags.Static : BindingFlags.Instance;
            if (pass == AddressDumperPass.Public)
            {
                flags |= BindingFlags.Public;
            }
            else
            {
                flags |= BindingFlags.Public | BindingFlags.NonPublic;
            }
                       
            var members = type.GetMembers(flags);
            foreach (var member in members)
            {
                if (member.GetCustomAttribute<DontUseForAutoTranslationAttribute>() != null)
                {
                    continue;
                }

                var path = currentPath + "." + member.Name;

                var attrs = member.GetCustomAttributes<TempleDllLocationAttribute>();
                foreach (var attr in attrs)
                {
                    if (attr.Secondary)
                    {
                        continue;
                    }

                    var address = "0x" + attr.Location.ToString("x");
                    if (!addresses.ContainsKey(address))
                    {
                        addresses[address] = new HashSet<string>();
                    }

                    addresses[address].Add(path);
                    if (member is PropertyInfo)
                    {
                        _properties.Add(path);
                    }
                }

                // Recurse into the type of the member
                if (!TryGetMemberInfo(member, out var memberType, out var privateMember))
                {
                    continue;
                }

                if (KnownInstanceTypes.Contains(memberType))
                {
                    continue; // Skip known object types
                }
                ScanEntryPoint(memberType, path, privateMember);
            }
        }

        private static bool TryGetMemberInfo(MemberInfo member, out Type memberType, out bool privateMember)
        {
            memberType = null;
            privateMember = false;

            if (member is FieldInfo fieldInfo)
            {
                // Bad heuristic to skip backing fields
                if (member.Name.EndsWith("_BackingField"))
                {
                    return false;
                }

                memberType = fieldInfo.FieldType;
                privateMember = !fieldInfo.IsPublic;
            }
            else if (member is PropertyInfo propertyInfo)
            {
                memberType = propertyInfo.PropertyType;
                privateMember = !propertyInfo.GetMethod.IsPublic;
            }
            else if (member is MethodInfo methodInfo)
            {
                // Bad heuristic to skip setters/getters (since we'll process the property instead)
                if (member.Name.StartsWith("set_") || member.Name.StartsWith("get_"))
                {
                    return false;
                }

                // Only recurse into methods that have no argument
                if (methodInfo.GetParameters().Length != 0)
                {
                    return false;
                }

                memberType = methodInfo.ReturnType;
                privateMember = !methodInfo.IsPublic;
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds members on the given type that represent functions in temple.dll and return a map from
        /// temple.dll address to member name for thiscall translations.
        /// </summary>
        private Dictionary<string, string> ScanInstanceMethods(Type type)
        {
            var result = new Dictionary<string, ISet<string>>();

            var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                if (member.GetCustomAttribute<DontUseForAutoTranslationAttribute>() != null)
                {
                    continue;
                }

                var attrs = member.GetCustomAttributes<TempleDllLocationAttribute>();
                foreach (var attr in attrs)
                {
                    if (attr.Secondary)
                    {
                        continue;
                    }

                    var address = "0x" + attr.Location.ToString("x");
                    if (!result.ContainsKey(address))
                    {
                        result[address] = new HashSet<string>();
                    }

                    result[address].Add(member.Name);
                }
            }

            var flatResult = new Dictionary<string, string>();
            foreach (var kvp in result)
            {
                if (kvp.Value.Count > 1)
                {
                    Logger.Info("Address {1} maps to multiple members on {0}: {2}",
                        type.Name, kvp.Key, string.Join(", ", kvp.Value));
                }
                else
                {
                    flatResult[kvp.Key] = kvp.Value.First();
                }
            }
            return flatResult;
        }

        private void ScanStaticEntyPoints()
        {
            foreach (var entryPoint in _coreAssembly.DefinedTypes.Where(t => t.IsClass))
            {
                ScanStaticEntryPoint(entryPoint);
            }
        }

        public void DumpAddresses()
        {
            pass = AddressDumperPass.Public;
            while (true)
            {
                alreadyScanned.Clear();
                addresses = new Dictionary<string, ISet<string>>();
                try
                {
                    ScanStaticEntyPoints();
                    break;
                }
                catch (AlreadyScannedException e)
                {
                    Logger.Info($"Blacklisting type {e.Type}: {e.FirstPath} and {e.SecondPath}");
                    scanBlacklist.Add(e.Type);
                }
            }

            var publicAddresses = addresses;
            pass = AddressDumperPass.Private;
            while (true)
            {
                alreadyScanned.Clear();
                addresses = new Dictionary<string, ISet<string>>();
                try
                {
                    ScanStaticEntyPoints();
                    break;
                }
                catch (AlreadyScannedException e)
                {
                    Logger.Info($"Blacklisting type {e.Type}: {e.FirstPath} and {e.SecondPath}");
                    scanBlacklist.Add(e.Type);
                }
            }

            var privateAddresses = addresses;

            var addressMap = new Dictionary<string, string>();

            foreach (var kvp in publicAddresses)
            {
                if (kvp.Value.Count > 1)
                {
                    Logger.Info("Ambiguous Address: {0}: {1}", kvp.Key, string.Join(", ", kvp.Value));
                }
                else
                {
                    addressMap[kvp.Key] = kvp.Value.First();
                }

                privateAddresses.Remove(kvp.Key); // Remove anything private for which we have equivalent public access
            }

            foreach (var kvp in privateAddresses)
            {
                if (kvp.Value.Count > 1)
                {
                    Logger.Info("Ambiguous private Address: {0}: {1}", kvp.Key, string.Join(", ", kvp.Value));
                }
                else
                {
                    addressMap[kvp.Key] = kvp.Value.First();
                }
            }

            // Try finding the instance methods on any type used multiple times
            var instanceTypes = new Dictionary<string, Dictionary<string, string>>();
            foreach (var possibleInstanceType in scanBlacklist.Concat(KnownInstanceTypes).Distinct())
            {
                var instanceMethods = ScanInstanceMethods(possibleInstanceType);
                if (instanceMethods.Count != 0)
                {
                    instanceTypes[possibleInstanceType.Name] = instanceMethods;
                }
            }

            var model = new AddressMapModel();
            model.InstanceAddresses = instanceTypes;
            model.GlobalAddresses = addressMap;
            model.Properties = _properties;

            var addressMapJson = JsonSerializer.SerializeToUtf8Bytes(model, JsonSerializerOptions);
            File.WriteAllBytes("addresses.json", addressMapJson);
        }

        public class AddressMapModel
        {
            public Dictionary<string, Dictionary<string, string>> InstanceAddresses { get; set; }

            public Dictionary<string, string> GlobalAddresses { get; set; }

            public ISet<string> Properties { get; set; }
        }

        private enum AddressDumperPass
        {
            Public,
            Private
        }

        private class AlreadyScannedException : Exception
        {
            public Type Type { get; }

            public string FirstPath { get; }

            public string SecondPath { get; }

            public AlreadyScannedException(Type type, string firstPath, string secondPath)
            {
                Type = type;
                FirstPath = firstPath;
                SecondPath = secondPath;
            }
        }
    }


}