using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Startup.Discovery;

/// <summary>
/// Scans all types in an assembly for classes or fields annotated with AutoRegisterAttribute to
/// automatically pick up certain types of content.
/// </summary>
public class DefaultContentProvider : IContentProvider
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    // In debug mode we discover more fields to find mistakes
#if DEBUG
    private const BindingFlags MemberFilter =
        BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static
        | BindingFlags.Instance | BindingFlags.NonPublic;
#else
        private const BindingFlags MemberFilter =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static;
#endif

    private static readonly ISet<Type> AutoRegisteringTypes = new HashSet<Type>
    {
        typeof(RaceSpec),
        typeof(D20ClassSpec),
        typeof(ConditionSpec)
    };

    private static readonly ISet<Type> AutoRegisteringCollectionTypes = AutoRegisteringTypes.Select(t => typeof(IEnumerable<>).MakeGenericType(t)).ToHashSet();

    private static Exception Error(MemberInfo fieldInfo, string extraMessage)
    {
        return new InvalidOperationException(
            $"Cannot auto-register {fieldInfo.DeclaringType?.FullName}.{fieldInfo.Name}: {extraMessage}"
        );
    }

    public DefaultContentProvider(Assembly assembly)
    {
        var conditions = new HashSet<ConditionSpec>();
        var featConditions = new HashSet<(FeatId, ConditionSpec)>();
        var classes = new HashSet<D20ClassSpec>();
        var races = new HashSet<RaceSpec>();

        foreach (var typeInfo in assembly.GetTypes())
        {
            // If the type is annotated with AutoRegister, it means implicitly that all fields are annotated
            // as well, but it disables errors if a field's type is unrecognized.
            var registerAll = typeInfo.GetCustomAttribute<AutoRegisterAttribute>() != null;

            foreach (var fieldInfo in typeInfo.GetFields(MemberFilter))
            {
                var featCondition = fieldInfo.GetCustomAttribute<FeatConditionAttribute>();
                if (featCondition != null)
                {
                    var condition = (ConditionSpec) GetFieldValue(fieldInfo, featCondition);
                    if (condition == null)
                    {
                        throw Error(fieldInfo, "is null.");
                    }
                    featConditions.Add((featCondition.FeatId, condition));
                }

                var registerAttribute = fieldInfo.GetCustomAttribute<AutoRegisterAttribute>();
                if (registerAll || registerAttribute != null)
                {
                    object? fieldValue;
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
                    
                    switch (fieldValue)
                    {
                        case null:
                            throw Error(fieldInfo, "is null.");
                        case D20ClassSpec classSpec:
                            classes.Add(classSpec);
                            break;
                        case RaceSpec raceSpec:
                            races.Add(raceSpec);
                            break;
                        case ConditionSpec conditionSpec:
                            conditions.Add(conditionSpec);
                            break;
                        default:
                            throw Error(fieldInfo, $"unsupported value: {fieldValue}");
                    }
                }
            }

            foreach (var methodInfo in typeInfo.GetMethods(MemberFilter))
            {
                var registerAttribute = methodInfo.GetCustomAttribute<AutoRegisterAllAttribute>();
                if (registerAttribute != null)
                {
                    if (!methodInfo.IsPublic || !methodInfo.IsStatic)
                    {
                        throw Error(methodInfo, "it's not public or not static.");
                    }

                    var value = methodInfo.Invoke(null, null);

                    switch (value)
                    {
                        case IEnumerable<D20ClassSpec> classSpecs:
                            foreach (var classSpec in classSpecs)
                            {
                                classes.Add(classSpec);
                            }
                            break;
                        case IEnumerable<RaceSpec> raceSpecs:
                            foreach (var raceSpec in raceSpecs)
                            {
                                races.Add(raceSpec);
                            }
                            break;
                        case IEnumerable<ConditionSpec> conditionSpecs:
                            foreach (var conditionSpec in conditionSpecs)
                            {
                                conditions.Add(conditionSpec);                               
                            }
                            break;
                        default:
                            throw Error(methodInfo, $"unsupported value: {value}");
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

    private static object? GetFieldValue(FieldInfo fieldInfo, Attribute attribute)
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