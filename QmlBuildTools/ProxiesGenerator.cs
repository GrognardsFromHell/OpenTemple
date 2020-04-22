using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace QmlBuildTools
{
    public static class ProxiesGenerator
    {
        private static TypeSyntax InferredType => IdentifierName("var");

        public static string Generate(IEnumerable<TypeInfo> types)
        {
            return CompilationUnit()
                .WithUsings(List(new[]
                    {
                        UsingDirective(IdentifierName("System")),
                        UsingDirective(IdentifierName("System.Runtime.InteropServices"))
                    }
                ))
                .WithMembers(
                    List<MemberDeclarationSyntax>(
                        types.GroupBy(t => GetNamespace(t).ToFullString())
                            .Select(CreateNamespace)
                    )
                )
                .NormalizeWhitespace()
                .ToFullString();
        }

        private static NamespaceDeclarationSyntax CreateNamespace(IEnumerable<TypeInfo> types)
        {
            return NamespaceDeclaration(GetNamespace(types.First()))
                .WithMembers(
                    List<MemberDeclarationSyntax>(types.Select(CreateProxyClass))
                );
        }

        private static ClassDeclarationSyntax CreateProxyClass(TypeInfo type)
        {
            return ClassDeclaration(GetTypeName(type).Identifier)
                    .WithBaseList(GetBaseList(type))
                    .WithPublicAccess()
                    .WithMembers(List(CreateProxyMembers(type)))
                ;
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateProxyMembers(TypeInfo type)
        {
            foreach (var part in CreateTypeInitializer(type))
            {
                yield return part;
            }

            // Create constructor
            if (true || type.Kind == TypeInfoKind.CppQObject || type.Kind == TypeInfoKind.QmlQObject)
            {
                yield return ConstructorDeclaration(GetTypeName(type).Identifier)
                    .WithPublicAccess()
                    .WithParameterList(
                        ParameterList(
                            SingletonSeparatedList(
                                Parameter(
                                        Identifier("handle"))
                                    .WithType(
                                        IdentifierName("IntPtr")))))
                    .WithInitializer(
                        ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName("handle"))))))
                    .WithBody(Block(ParseStatement("LazyInitializeMetaObject(handle);")));
            }

            foreach (var prop in type.Props)
            {
                foreach (var syntax in CreateProperty(prop))
                {
                    yield return syntax;
                }
            }

            foreach (var method in type.Methods)
            {
                foreach (var syntax in CreateMethod(method))
                {
                    yield return syntax;
                }
            }

            // We do not support signal overloading, which Qt _does_ support,
            // so instead, we'll have to use the signal with the longest unambiguous
            // parameter list
            var signals = type.Signals.ToList();
            for (var i = signals.Count - 1; i >= 0; i--)
            {
                var name = signals[i].Name;
                var paramCount = signals[i].Params.Count();
                for (var j = i - 1; j >= 0; j--)
                {
                    if (signals[j].Name == name)
                    {
                        // Conflicting definition found
                        var otherLen = signals[j].Params.Count();
                        if (otherLen == paramCount)
                        {
                            Console.WriteLine("Ambiguous signal found: " + name);
                        }
                        else if (otherLen < paramCount)
                        {
                            // the other one has less signifiance, swap and remove
                            signals[j] = signals[i];
                            signals.RemoveAt(i);
                            break;
                        }
                        else if (otherLen > paramCount)
                        {
                            signals.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            foreach (var signal in signals)
            {
                foreach (var syntax in CreateEvent(signal))
                {
                    yield return syntax;
                }
            }
        }

        private static TypeSyntax GetTypeName(TypeRef typeRef)
        {
            return typeRef.Kind switch
            {
                TypeRefKind.Void => PredefinedType(Token(SyntaxKind.VoidKeyword)),
                TypeRefKind.TypeInfo => GetQualifiedName(typeRef.TypeInfo),
                TypeRefKind.BuiltIn => PredefinedType(Token(typeRef.BuiltIn switch
                {
                    BuiltInType.Bool => SyntaxKind.BoolKeyword,
                    BuiltInType.Int32 => SyntaxKind.IntKeyword,
                    BuiltInType.UInt32 => SyntaxKind.UIntKeyword,
                    BuiltInType.Int64 => SyntaxKind.LongKeyword,
                    BuiltInType.UInt64 => SyntaxKind.ULongKeyword,
                    BuiltInType.Double => SyntaxKind.DoubleKeyword,
                    BuiltInType.Char => SyntaxKind.CharKeyword,
                    BuiltInType.String => SyntaxKind.StringKeyword,
                    _ => throw new ArgumentOutOfRangeException(typeRef.BuiltIn.ToString())
                })),
                _ => throw new ArgumentOutOfRangeException(typeRef.Kind.ToString())
            };
        }

        // Generates a private static method InitializeMetaObject, which will be called exactly once
        // to initialize the type properties (it's unsafe, btw.) and relies on a property _metaObject;
        // NOTE: We CANNOT do this in the static constructor, because in case of QML types and even
        // CPP types that come from a QML plugin may be loaded in response to loading a .QML file.
        // An alternative approach may be remembering the int's and sanity checking only, allowing
        // the integers to be inlined.
        private static IEnumerable<MemberDeclarationSyntax> CreateTypeInitializer(TypeInfo type)
        {
            yield return ParseMemberDeclaration("private static IntPtr _metaObject;");

            yield return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("LazyInitializeMetaObject")
                )
                .WithParameterList(
                    ParameterList(SingletonSeparatedList(
                        Parameter(Identifier("exampleInstance"))
                            .WithType(IdentifierName("IntPtr"))
                    ))
                )
                .WithPrivateStatic()
                .WithBody(
                    Block(CreateTypeInitializerBody(type))
                );
        }

        private static IEnumerable<StatementSyntax> CreateTypeInitializerBody(TypeInfo type)
        {
            // Do nothing if the meta object is already known
            yield return ParseStatement(@"if (_metaObject != IntPtr.Zero) return;");

            // Create a local variable for the discovered meta object
            yield return LocalDeclarationStatement(
                VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator("metaObject")
                            .WithInitializer(EqualsValueClause(CreateMetaObjectDiscovery(type)))
                    ))
            );

            // Now write code that will lookup every property's index
            // in the discovered meta object for faster setting/getting
            foreach (var prop in type.Props)
            {
                var propertyIndexField = GetPropertyIndexField(prop);
                yield return ParseStatement(
                    $"FindMetaObjectProperty(metaObject, \"{prop.Name}\", out {propertyIndexField});");
            }

            // Same for signals & methods
            foreach (var signal in type.Signals)
            {
                var indexField = GetSignalIndexField(signal);
                yield return ParseStatement(
                    $"FindMetaObjectMethod(metaObject, \"{signal.Signature}\", out {indexField});");
            }

            foreach (var method in type.Methods)
            {
                var indexField = GetMethodIndexField(method);
                yield return ParseStatement(
                    $"FindMetaObjectMethod(metaObject, \"{method.Signature}\", out {indexField});");
            }

            // Save the discovered meta object
            yield return ParseStatement(@"_metaObject = metaObject;");
        }

        // How we can detect the meta object depends on which kind of type we're generating
        // the binding for. For C++ types, we will do it in advance using the C++ type name,
        // while for QML documents, we need an active Engine (sucks), so we'll just use
        // the meta object of the actual object passed in.
        private static ExpressionSyntax CreateMetaObjectDiscovery(TypeInfo type)
        {
            // Class names of QML files are dynamic at runtime (depend on load order)
            if (type.Kind == TypeInfoKind.QmlQObject)
            {
                return InvocationExpression(IdentifierName("GetMetaObjectByQmlSourceUrl"))
                    .WithArgumentList(ArgumentList(
                        SeparatedList(new[]
                        {
                            // We need to pass the example instance, because _really_ the
                            // QML file's are loaded just into the QML engine and their
                            // global registration is temporary
                            Argument(IdentifierName("exampleInstance")),
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(type.QmlSourceUrl)))
                        }))
                    );
            }

            return InvocationExpression(IdentifierName("GetMetaObjectByClassName"))
                .WithArgumentList(ArgumentList(
                    SingletonSeparatedList(Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(type.MetaClassName)))))
                );
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateProperty(PropInfo prop)
        {
            var accessors = new List<AccessorDeclarationSyntax>();

            var propertyIndexField = GetPropertyIndexField(prop);

            yield return CreateMetaObjectIndexField(propertyIndexField);

            if (prop.IsReadable)
            {
                if (prop.Type.Kind == TypeRefKind.BuiltIn)
                {
                    if (prop.Type.BuiltIn == BuiltInType.String)
                    {
                        accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    InvocationExpression(IdentifierName("GetPropertyQString"))
                                        .WithArgumentList(ArgumentList(
                                            SingletonSeparatedList(Argument(IdentifierName(propertyIndexField)))))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        );
                    }
                    else
                    {
                        accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        );
                    }
                }
                else
                {
                    accessors.Add(AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    );
                }
            }

            if (prop.IsWritable)
            {
                if (prop.Type.Kind == TypeRefKind.BuiltIn)
                {
                    if (prop.Type.BuiltIn == BuiltInType.String)
                    {
                        accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithExpressionBody(
                                ArrowExpressionClause(
                                    InvocationExpression(IdentifierName("SetPropertyQString"))
                                        .WithArgumentList(ArgumentList(
                                            SeparatedList(new[]
                                            {
                                                Argument(IdentifierName(propertyIndexField)),
                                                Argument(IdentifierName("value"))
                                            })))))
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        );
                    }
                    else
                    {
                        accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        );
                    }
                }
                else
                {
                    accessors.Add(AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    );
                }
            }

            yield return PropertyDeclaration(GetTypeName(prop.Type), SanitizeIdentifier(prop.Name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(AccessorList(List(accessors)));
        }

        private static string Capitalize(string text)
        {
            if (text.Length == 0 || char.IsUpper(text[0]))
            {
                return text;
            }

            return text.Substring(0, 1).ToUpperInvariant() + text.Substring(1);
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateEvent(MethodInfo signal)
        {
            var eventName = SanitizeIdentifier("On" + Capitalize(signal.Name));

            var signalParams = signal.Params.ToList();

            // Declare a static field to save the index of the signal
            var indexFieldName = GetSignalIndexField(signal);
            yield return CreateMetaObjectIndexField(indexFieldName);

            // Declare a static field that holds the delegate instance for marshalling the Qt signal parameters
            // back to the parameters expected by the actual user-facing delegate type
            var thunkDelegateName = eventName + "ThunkDelegate";
            var thunkMethodName = eventName + "Thunk";
            yield return ParseMemberDeclaration(
                $"private static readonly unsafe DelegateSlotCallback {thunkDelegateName} = {thunkMethodName};"
            );

            // Determine the type of the user-facing delegate. We will not support return values. Hence 'Action'
            TypeSyntax delegateType;
            if (signalParams.Count > 0)
            {
                delegateType = GenericName(Identifier("System.Action")).WithTypeArgumentList(TypeArgumentList(
                    SeparatedList(
                        signalParams.Select(p => GetTypeName(p.Type))
                    )));
            }
            else
            {
                delegateType = IdentifierName("System.Action");
            }

            // Generate a method that will serve as the thunk for native calls
            yield return CreateSignalThunkMethod(thunkMethodName, signalParams, delegateType);

            var accessors = new List<AccessorDeclarationSyntax>
            {
                AccessorDeclaration(SyntaxKind.AddAccessorDeclaration)
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            ParseExpression($"AddSignalHandler({indexFieldName}, value, {thunkDelegateName})")))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration)
                    .WithExpressionBody(
                        ArrowExpressionClause(
                            ParseExpression($"RemoveSignalHandler({indexFieldName}, value)")))
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            };

            yield return EventDeclaration(delegateType, eventName)
                .WithPublicAccess()
                .WithAccessorList(AccessorList(List(accessors)));
        }

        private static MemberDeclarationSyntax CreateSignalThunkMethod(string thunkMethodName,
            List<MethodParamInfo> signalParams,
            TypeSyntax delegateType)
        {
            var method = (MethodDeclarationSyntax)
                ParseMemberDeclaration(
                    $"private static unsafe void {thunkMethodName}(GCHandle delegateHandle, void** args) {{}}");

            return method.WithBody(Block(CreateSignalThunkMethodBody(signalParams, delegateType)));
        }

        private static IEnumerable<StatementSyntax> CreateSignalThunkMethodBody(
            List<MethodParamInfo> signalParams,
            TypeSyntax delegateType)
        {
            // Cast the GCHandle's target to our action
            yield return LocalDeclarationStatement(
                VariableDeclaration(InferredType, SingletonSeparatedList(
                    VariableDeclarator("dlgt")
                        .WithInitializer(EqualsValueClause(CastExpression(delegateType,
                            ParseExpression("delegateHandle.Target"))))
                ))
            );

            var methodParams = signalParams.Select((signalParam, index) =>
            {
                // index +1 because first array entry is the return value
                var ptrValue = ParseExpression("args[" + (index + 1) + "]");
                return TransformExpressionNativeToManaged(ptrValue, signalParam.Type);
            }).Select(Argument).ToArray();

            // Call the delegate
            yield return ExpressionStatement(InvocationExpression(
                IdentifierName("dlgt"), ArgumentList(SeparatedList(methodParams))
            ));
        }

        private static ExpressionSyntax CastVoidPtrToIntPtr(ExpressionSyntax voidPtr) =>
            CastExpression(IdentifierName("System.IntPtr"), voidPtr);

        private static ExpressionSyntax DerefPointerToPrimitive<T>(ExpressionSyntax voidPtr) =>
            PrefixUnaryExpression(SyntaxKind.PointerIndirectionExpression,
                CastExpression(IdentifierName(typeof(T).Name + "*"), voidPtr)
            );

        private static ExpressionSyntax TransformExpressionNativeToManaged(ExpressionSyntax expression, TypeRef type)
        {
            // Transform ptrValue (a void*) to the target type
            switch (type.Kind)
            {
                case TypeRefKind.TypeInfo:
                    return ObjectCreationExpression(GetTypeName(type))
                        .WithArgumentList(
                            ArgumentList(SeparatedList(new[] {Argument(CastVoidPtrToIntPtr(expression))})));
                case TypeRefKind.BuiltIn:
                    switch (type.BuiltIn)
                    {
                        case BuiltInType.Bool:
                            return DerefPointerToPrimitive<bool>(expression);
                        case BuiltInType.Int32:
                            return DerefPointerToPrimitive<int>(expression);
                        case BuiltInType.UInt32:
                            return DerefPointerToPrimitive<uint>(expression);
                        case BuiltInType.Int64:
                            return DerefPointerToPrimitive<long>(expression);
                        case BuiltInType.UInt64:
                            return DerefPointerToPrimitive<ulong>(expression);
                        case BuiltInType.Double:
                            return DerefPointerToPrimitive<double>(expression);
                        case BuiltInType.Char:
                            return DerefPointerToPrimitive<char>(expression);
                        case BuiltInType.String:
                            // We need special deserialization for String, sadly
                            return InvocationExpression(IdentifierName("QString_read"))
                                .WithArgumentList(ArgumentList(SeparatedList(new[] {Argument(expression)})));
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMethod(MethodInfo method)
        {
            // Declare a static field to save the index of the method
            var indexFieldName = GetMethodIndexField(method);
            yield return CreateMetaObjectIndexField(indexFieldName);

            yield return MethodDeclaration(GetTypeName(method.ReturnType), method.Name)
                .WithPublicAccess()
                .WithParameterList(CreateParameterList(method))
                .WithBody(Block(ParseStatement("throw new Exception();")));
        }

        private static ParameterListSyntax CreateParameterList(MethodInfo method)
        {
            return ParameterList(SeparatedList(
                method.Params.Select((p, pIdx) => Parameter(Identifier(
                        SanitizeIdentifier(p.Name ?? $"arg{pIdx + 1}")))
                    .WithType(GetTypeName(p.Type)))
            ));
        }

        private static string GetPropertyIndexField(PropInfo prop)
        {
            return "_" + prop.Name + "Index";
        }

        private static string GetMethodIndexField(MethodInfo method)
        {
            var fieldName = "_" + method.Name + "Index";
            if (method.OverloadIndex != -1)
            {
                fieldName += method.OverloadIndex;
            }

            return fieldName;
        }

        private static string GetSignalIndexField(MethodInfo signal)
        {
            return "_" + signal.Name + "SignalIndex";
        }

        // Creates a private static field holding the actual index to somoe property/method/signal from a metaobject
        // can only be determined safely at runtime...
        private static FieldDeclarationSyntax CreateMetaObjectIndexField(string propertyIndexField)
        {
            return FieldDeclaration(
                    VariableDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.IntKeyword)))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(Identifier(propertyIndexField))
                                    .WithInitializer(
                                        EqualsValueClause(
                                            PrefixUnaryExpression(
                                                SyntaxKind.UnaryMinusExpression,
                                                LiteralExpression(
                                                    SyntaxKind.NumericLiteralExpression,
                                                    Literal(1))))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)));
        }

        private static string SanitizeIdentifier(string name)
        {
            // Prefix with @ if it's a keyword
            var isKeyword = SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None
                            || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None;
            return isKeyword ? "@" + name : name;
        }

        private static BaseListSyntax GetBaseList(in TypeInfo type)
        {
            var parent = type.Parent;
            if (!parent.HasValue)
            {
                if (type.Kind == TypeInfoKind.CppGadget)
                {
                    return BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(QualifiedName(IdentifierName("QtInterop"), IdentifierName("QGadgetBase")))
                    ));
                }

                return BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(QualifiedName(IdentifierName("QtInterop"), IdentifierName("QObjectBase")))
                ));
            }

            return BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(GetQualifiedName(parent.Value))
            ));
        }

        private static IdentifierNameSyntax GetTypeName(TypeInfo typeInfo)
        {
            return IdentifierName(typeInfo.Name);
        }

        private static NameSyntax GetQualifiedName(TypeInfo typeInfo)
        {
            return QualifiedName(GetNamespace(typeInfo), GetTypeName(typeInfo));
        }

        private static NameSyntax GetNamespace(TypeInfo typeInfo)
        {
            var ns = typeInfo.QmlModule;
            // Namespace for types that come from direct .QML files
            if (ns == null)
            {
                ns = "QmlFiles";
            }

            return IdentifierName(ns);
        }
    }

    internal static class SyntaxExtensions
    {
        internal static T WithPublicAccess<T>(this T syntax) where T : MemberDeclarationSyntax
        {
            return (T) syntax.WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
        }

        internal static T WithPrivateStatic<T>(this T syntax) where T : MemberDeclarationSyntax
        {
            return (T) syntax.WithModifiers(
                TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword)));
        }
    }
}