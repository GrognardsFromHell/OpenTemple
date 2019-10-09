using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.DebugUI
{
    public class ObjectEditor
    {
        public readonly GameObjectBody Object;

        public bool Active = true;

        public string Title;

        private readonly List<EditablePropertyGroup> Groups = new List<EditablePropertyGroup>();

        public ObjectEditor(GameObjectBody o)
        {
            Object = o;
            Title = $"{GameSystems.MapObject.GetDisplayName(Object)} [{Object.type}, {Object.ProtoId}]";

            if (Object.IsPC())
            {
                AddGroup(
                    "PC",
                    StringField("Name", obj_f.pc_player_name)
                );
            }

            AddGroup(
                "Health",
                TightGroup(GetHpFields().ToArray())
            );

            if (Object.IsCritter())
            {
                AddGroup(
                    "Ability Scores",
                    TightGroup(
                        BaseStatField("Str", Stat.strength),
                        BaseStatField("Dex", Stat.dexterity),
                        BaseStatField("Con", Stat.constitution),
                        BaseStatField("Int", Stat.intelligence),
                        BaseStatField("Wis", Stat.wisdom),
                        BaseStatField("Cha", Stat.charisma)
                    )
                );
            }
        }

        private IEnumerable<IPropertyEditor> GetHpFields()
        {
            yield return Int32Field("Max HP", obj_f.hp_pts);
            yield return Int32Field("HP Damage", obj_f.hp_damage);
            if (Object.IsCritter())
            {
                yield return Int32Field("Subdual Damage", obj_f.critter_subdual_damage);
            }
        }

        private EditablePropertyGroup AddGroup(string title, params IPropertyEditor[] properties)
        {
            var group = new EditablePropertyGroup(title);
            group.Properties.AddRange(properties);
            Groups.Add(group);
            return group;
        }

        private IPropertyEditor TightGroup(params IPropertyEditor[] editors)
        {
            return new TightGroup(editors);
        }

        private IPropertyEditor StringField(string label, obj_f field)
        {
            return new OneLineStringEditor(
                label,
                () => Object.GetString(field),
                value => Object.SetString(field, value)
            );
        }

        private IPropertyEditor Int32Field(string label, obj_f field)
        {
            return new Int32Editor(
                label,
                () => Object.GetInt32(field),
                value => Object.SetInt32(field, value)
            );
        }

        private IPropertyEditor BaseStatField(string label, Stat field)
        {
            return new Int32Editor(
                label,
                () => Object.GetBaseStat(field),
                value => Object.SetBaseStat(field, value)
            );
        }

        public void Render()
        {
            if (Object.IsCritter())
            {
                RenderNameLabelPairs(
                    ("Type", $"{Object.type} [{(int) Object.type}]"),
                    ("Name", GameSystems.MapObject.GetDisplayName(Object))
                );

                RenderNameLabelPairs(
                    ("Class-Levels", BuildClassLevels(Object)),
                    ("ECL", Object.GetStat(Stat.level).ToString()),
                    ("Race", Object.GetRace().ToString()),
                    ("Size", ((SizeCategory) Object.GetStat(Stat.size)).ToString()),
                    ("Gender", Object.GetGender().ToString())
                );

                RenderNameLabelPairs(
                    ("Alignment", GetShortAlignment()),
                    ("Deity", Object.GetDeity().ToString()),
                    ("Radius (in)", Math.Round(Object.GetRadius()).ToString()),
                    ("Height (in)", Math.Round(Object.GetRenderHeight()).ToString())
                );
            }

            foreach (var group in Groups)
            {
                ImGui.Text(group.Title);
                ImGui.Separator();
                foreach (var property in group.Properties)
                {
                    property.Render();
                    ImGui.Separator();
                }
            }

            if (Object.IsCritter())
            {
                if (GameSystems.Spell.GetSchoolSpecialization(Object,
                    out var specializedSchool,
                    out var forbiddenSchool1,
                    out var forbiddenSchool2))
                {
                    ImGui.Text("Wizard Specialization");
                    ImGui.Text("Specialized: " + specializedSchool);
                    ImGui.Text("Forbidden: " + forbiddenSchool1 + ", " + forbiddenSchool2);
                    ImGui.Separator();
                }

                var spellsPerDay = GameSystems.Spell.GetSpellsPerDay(Object);
                foreach (var classSpellsPerDay in spellsPerDay)
                {
                    ImGui.Text(classSpellsPerDay.Name + " (" + classSpellsPerDay.ShortName + ")");

                    foreach (var level in classSpellsPerDay.Levels)
                    {
                        if (level.Slots.Length == 0)
                        {
                            continue;
                        }

                        ImGui.Text("Level " + level.Level);
                        foreach (var slot in level.Slots)
                        {
                            if (slot.HasSpell)
                            {
                                var spellName = GameSystems.Spell.GetSpellName(slot.SpellEnum);
                                ImGui.Text("Memorized: " + spellName);
                            }
                            else
                            {
                                ImGui.Text("[  empty slot  ]");
                            }

                            ImGui.SameLine();
                            ImGui.Text(slot.Source.ToString());
                        }
                    }
                }
            }
        }

        private string GetShortAlignment()
        {
            switch (Object.GetAlignment())
            {
                case Alignment.NEUTRAL:
                    return "N";
                case Alignment.LAWFUL:
                    return "LN";
                case Alignment.CHAOTIC:
                    return "CN";
                case Alignment.GOOD:
                    return "NG";
                case Alignment.EVIL:
                    return "NE";
                case Alignment.LAWFUL_GOOD:
                    return "LG";
                case Alignment.CHAOTIC_GOOD:
                    return "CG";
                case Alignment.LAWFUL_EVIL:
                    return "LE";
                case Alignment.CHAOTIC_EVIL:
                    return "CE";
                default:
                    return "UNKNOWN";
            }
        }

        private string BuildClassLevels(GameObjectBody critter)
        {
            var levelList = new List<(string, int)>();

            foreach (var classEnum in D20ClassSystem.AllClasses)
            {
                var classLevels = critter.GetStat(classEnum);
                if (classLevels > 0)
                {
                    levelList.Add((GameSystems.Stat.GetStatName(classEnum), classLevels));
                }
            }

            levelList.Sort((t1, t2) => t1.Item2 - t2.Item2);
            var result = new StringBuilder();
            foreach (var (className, levels) in levelList)
            {
                if (result.Length > 0)
                {
                    result.Append(" / ");
                }

                result.Append($"{className} {levels}");
            }

            return result.ToString();
        }

        private void RenderNameLabelPairs(params (string, string)[] pairs)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
            ImGui.Columns(pairs.Length, null, false);

            foreach (var (_, value) in pairs)
            {
                ImGui.Text(value);
                ImGui.NextColumn();
            }

            for (var index = 0; index < pairs.Length; index++)
            {
                var drawList = ImGui.GetWindowDrawList();
                var pos = ImGui.GetCursorScreenPos();
                drawList.AddLine(new Vector2(pos.X - 9999, pos.Y), new Vector2(pos.X + 9999, pos.Y), 0xFF999999);
                ImGui.NextColumn();
            }

            ImGui.SetWindowFontScale(0.8f);
            foreach (var (label, _) in pairs)
            {
                ImGui.TextColored(new Vector4(0.8f, 0.8f, 0.8f, 1), label);
                ImGui.NextColumn();
            }

            ImGui.SetWindowFontScale(1.0f);
            ImGui.Columns(1);
            ImGui.PopStyleVar();
        }
    }

    internal class TightGroup : IPropertyEditor
    {
        private readonly IPropertyEditor[] _editors;

        public TightGroup(IPropertyEditor[] editors)
        {
            _editors = editors;
        }

        public void Render()
        {
            ImGui.Columns(_editors.Length);
            foreach (var editor in _editors)
            {
                editor.Render();
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }
    }

    internal class EditablePropertyGroup
    {
        public readonly string Title;

        public readonly List<IPropertyEditor> Properties = new List<IPropertyEditor>();

        public EditablePropertyGroup(string title)
        {
            Title = title;
        }
    }

    internal interface IPropertyEditor
    {
        void Render();
    }

    public abstract class AbstractPropertyEditor<T> : IPropertyEditor
    {
        protected readonly string _label;

        protected readonly Func<T> _getter;

        protected readonly Action<T> _setter;

        protected AbstractPropertyEditor(string label, Func<T> getter, Action<T> setter)
        {
            _label = label;
            _getter = getter;
            _setter = setter;
        }

        public abstract void Render();
    }

    public class OneLineStringEditor : AbstractPropertyEditor<string>
    {
        private string _lastKnownObjectValue;

        private string _currentValue;

        public OneLineStringEditor(string label, Func<string> getter, Action<string> setter) : base(label, getter,
            setter)
        {
        }

        public override void Render()
        {
            if (_lastKnownObjectValue == null || _lastKnownObjectValue != _getter())
            {
                _lastKnownObjectValue = _getter();
                _currentValue = _lastKnownObjectValue;
            }

            if (ImGui.InputText(_label, ref _currentValue, 1000, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                _setter(_currentValue);
            }
        }
    }

    public class Int32Editor : AbstractPropertyEditor<int>
    {
        private int _lastKnownObjectValue;

        private string _currentValue;

        public Int32Editor(string label, Func<int> getter, Action<int> setter) : base(label, getter, setter)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            _lastKnownObjectValue = _getter();
            _currentValue = _lastKnownObjectValue.ToString();
        }

        public override void Render()
        {
            if (_lastKnownObjectValue != _getter())
            {
                UpdateValue();
            }

            ImGui.BeginGroup();
            Tig.DebugUI.PushSmallFont();
            ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.9f, 1.0f), _label);
            ImGui.PopFont();
            ImGui.PushItemWidth(-1);
            if (ImGui.InputText("##" + _label, ref _currentValue, 32, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (int.TryParse(_currentValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out int newValue))
                {
                    _setter(newValue);
                }

                UpdateValue();
            }

            ImGui.PopItemWidth();
            ImGui.EndGroup();
        }
    }

    internal class AbilityModifierLabel : IPropertyEditor
    {
        private readonly GameObjectBody _critter;

        private readonly Stat _stat;

        public AbilityModifierLabel(GameObjectBody critter, Stat stat)
        {
            _critter = critter;
            _stat = stat;
        }

        public void Render()
        {
            var value = _critter.GetBaseStat(_stat);
            if (value > 0)
            {
                ImGui.Text("+" + value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                ImGui.Text(value.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}