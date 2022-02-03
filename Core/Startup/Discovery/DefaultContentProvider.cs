using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Startup.Discovery;

public class DefaultContentProvider : IContentProvider
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly Assembly _assembly;

    // In debug mode we discover more fields to find mistakes
#if DEBUG
    private const BindingFlags FieldFilter =
        BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static
        | BindingFlags.Instance | BindingFlags.NonPublic;
#else
        private const BindingFlags FieldFilter =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static;
#endif

    private static readonly ISet<Type> AutoRegisteringTypes = new HashSet<Type>
    {
        typeof(RaceSpec),
        typeof(D20ClassSpec),
        typeof(ConditionSpec)
    };

    private static Exception Error(MemberInfo fieldInfo, string extraMessage)
    {
        return new InvalidOperationException(
            $"Cannot auto-register {fieldInfo.DeclaringType?.FullName}.{fieldInfo.Name}: {extraMessage}"
        );
    }

    public DefaultContentProvider(Assembly assembly)
    {
        _assembly = assembly;

        var conditions = new List<ConditionSpec>();
        var featConditions = new List<(FeatId, ConditionSpec)>();
        var classes = new List<D20ClassSpec>();
        var races = new List<RaceSpec>();

        foreach (var typeInfo in assembly.GetTypes())
        {
            // If the type is annotated with AutoRegister, it means implicitly that all fields are annotated
            // as well, but it disables errors if a field's type is unrecognized.
            var registerAll = typeInfo.GetCustomAttribute<AutoRegisterAttribute>() != null;

            foreach (var fieldInfo in typeInfo.GetFields(FieldFilter))
            {
                var featCondition = fieldInfo.GetCustomAttribute<FeatConditionAttribute>();
                if (featCondition != null)
                {
                    var condition = (ConditionSpec) GetFieldValue(fieldInfo, featCondition);
                    featConditions.Add((featCondition.FeatId, condition));
                }

                var registerAttribute = fieldInfo.GetCustomAttribute<AutoRegisterAttribute>();
                if (registerAll || registerAttribute != null)
                {
                    object fieldValue;
                    if (registerAttribute == null)
                    {
                        if (!AutoRegisteringTypes.Contains(fieldInfo.FieldType))
                        {
                            continue;
                        }

                        if (!fieldInfo.IsPublic || !fieldInfo.IsStatic)
                        {
                            throw Error(fieldInfo, "it's not public or not static.");
                        }

                        fieldValue = fieldInfo.GetValue(null);
                    }
                    else
                    {
                        fieldValue = GetFieldValue(fieldInfo, registerAttribute);
                    }

                    if (fieldValue is D20ClassSpec classSpec)
                    {
                        classes.Add(classSpec);
                    }
                    else if (fieldValue is RaceSpec raceSpec)
                    {
                        races.Add(raceSpec);
                    }
                    else if (fieldValue is ConditionSpec conditionSpec)
                    {
                        conditions.Add(conditionSpec);
                    }
                    else
                    {
                        throw Error(fieldInfo, $"unsupported value: {fieldValue}");
                    }
                }
            }
        }

        Logger.Info("Discovered content in {0}:", assembly.GetName().Name);
        Logger.Info(" - Conditions: {0}", conditions.Count);
        Logger.Info(" - Feat Conditions: {0}", featConditions.Count);
        Logger.Info(" - Classes: {0}", classes.Count);
        Logger.Info(" - Races: {0}", races.Count);

        Conditions = conditions;
        Classes = classes;
        Races = races;
        FeatConditions = featConditions;
    }

    private static object GetFieldValue(FieldInfo fieldInfo, Attribute attribute)
    {
        var attributeName = attribute.GetType().Name;
        if (!fieldInfo.IsStatic)
        {
            throw Error(fieldInfo, $"Annotated with {attributeName}, but not static.");
        }

        if (!fieldInfo.IsPublic)
        {
            throw Error(fieldInfo, $"Annotated with {attributeName}, but not public.");
        }

        return fieldInfo.GetValue(null);
    }

    public IEnumerable<ConditionSpec> Conditions { get; }

    public IEnumerable<D20ClassSpec> Classes { get; }

    public IEnumerable<RaceSpec> Races { get; }

    public IEnumerable<(FeatId, ConditionSpec)> FeatConditions { get; }
}