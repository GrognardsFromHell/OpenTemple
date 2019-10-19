using System;
using System.Collections.Generic;
using System.Reflection;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.ObjScript;

namespace SpicyTemple.Core.Systems.Script
{
    /// <summary>
    /// Loads a script assembly and maintains an index of the annotated script classes within.
    /// </summary>
    public class ScriptAssembly
    {
        private readonly Assembly _scriptAssembly;

        private readonly Dictionary<int, ConstructorInfo> _objectScripts = new Dictionary<int, ConstructorInfo>();

        private readonly Dictionary<int, ConstructorInfo> _dialogScripts = new Dictionary<int, ConstructorInfo>();

        private readonly Dictionary<int, ConstructorInfo> _spellScripts = new Dictionary<int, ConstructorInfo>();

        internal ScriptAssembly(string name)
        {
            _scriptAssembly = Assembly.Load(name);

            var exportedTypes = _scriptAssembly.GetExportedTypes();
            foreach (var exportedType in exportedTypes)
            {
                IndexObjectScript(exportedType);
                IndexSpellScript(exportedType);
                IndexDialogScript(exportedType);
            }
        }

        public bool TryCreateObjectScript(int scriptId, out BaseObjectScript scriptObject)
        {
            if (!_objectScripts.TryGetValue(scriptId, out var constructor))
            {
                scriptObject = null;
                return false;
            }

            scriptObject = (BaseObjectScript) constructor.Invoke(null);
            return true;
        }

        public bool TryCreateDialogScript(int scriptId, out IDialogScript scriptObject)
        {
            if (!_dialogScripts.TryGetValue(scriptId, out var constructor))
            {
                scriptObject = null;
                return false;
            }

            scriptObject = (IDialogScript) constructor.Invoke(null);
            return true;
        }

        public bool TryCreateSpellScript(int scriptId, out BaseSpellScript scriptObject)
        {
            if (!_spellScripts.TryGetValue(scriptId, out var constructor))
            {
                scriptObject = null;
                return false;
            }

            scriptObject = (BaseSpellScript) constructor.Invoke(null);
            return true;
        }

        private void IndexDialogScript(Type exportedType)
        {
            var dialogScriptAttributes = exportedType.GetCustomAttributes<DialogScriptAttribute>();
            foreach (var dialogScriptAttribute in dialogScriptAttributes)
            {
                var scriptId = dialogScriptAttribute.Id;

                var constructor = exportedType.GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for dialog script {scriptId}, but does not have a public default constructor"
                    );
                }

                if (!_dialogScripts.TryAdd(scriptId, constructor))
                {
                    var otherClassName = _dialogScripts[scriptId]?.DeclaringType?.FullName;
                    throw new ArgumentException(
                        $"Duplicate dialog script ID: {scriptId} used by both {exportedType.FullName} and {otherClassName}"
                    );
                }

                if (!typeof(BaseObjectScript).IsAssignableFrom(exportedType))
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for dialog script {scriptId}, but does not extend from {nameof(BaseObjectScript)}"
                    );
                }

                if (!typeof(IDialogScript).IsAssignableFrom(exportedType))
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for dialog script {scriptId}, but does not implement {nameof(IDialogScript)}"
                    );
                }
            }
        }

        private void IndexSpellScript(Type exportedType)
        {
            var spellScriptAttributes = exportedType.GetCustomAttributes<SpellScriptAttribute>();
            foreach (var spellScriptAttribute in spellScriptAttributes)
            {
                var spellId = spellScriptAttribute.Id;

                var constructor = exportedType.GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for spell {spellId}, but does not have a public default constructor"
                    );
                }

                if (!_spellScripts.TryAdd(spellId, constructor))
                {
                    var otherClassName = _spellScripts[spellId]?.DeclaringType?.FullName;
                    throw new ArgumentException(
                        $"Duplicate script ID: {spellId} used by both {exportedType.FullName} and {otherClassName}"
                    );
                }

                if (!typeof(BaseSpellScript).IsAssignableFrom(exportedType))
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for spell {spellId}, but does not extend from {nameof(BaseSpellScript)}"
                    );
                }
            }
        }

        private void IndexObjectScript(Type exportedType)
        {
            var objScriptAttributes = exportedType.GetCustomAttributes<ObjectScriptAttribute>();
            foreach (var objScript in objScriptAttributes)
            {
                var scriptId = objScript.Id;

                var constructor = exportedType.GetConstructor(Array.Empty<Type>());
                if (constructor == null)
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for object script {scriptId}, but does not have a public default constructor"
                    );
                }

                if (!_objectScripts.TryAdd(scriptId, constructor))
                {
                    var otherClassName = _objectScripts[scriptId]?.DeclaringType?.FullName;
                    throw new ArgumentException(
                        $"Duplicate script ID: {scriptId} used by both {exportedType.FullName} and {otherClassName}"
                    );
                }

                if (!typeof(BaseObjectScript).IsAssignableFrom(exportedType))
                {
                    throw new ArgumentException(
                        $"Class {exportedType} is used for object script {scriptId}, but does not extend from {nameof(BaseObjectScript)}"
                    );
                }
            }
        }
    }
}