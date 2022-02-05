using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.DebugUI;

public class ObjectEditor
{
    public readonly GameObject Object;

    public bool Active = true;

    public string Title;

    private readonly List<EditablePropertyGroup> Groups = new();

    public ObjectEditor(GameObject o)
    {
        Object = o;
        if (Object.IsProto())
        {
            Title = $"{GameSystems.MapObject.GetDisplayName(Object)} [{Object.type}, Prototype]";                 
        }
        else
        {
            Title = $"{GameSystems.MapObject.GetDisplayName(Object)} [{Object.type}, ${Object.ProtoId}]";
        }

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

        AddGroup(
            "3D Properties",
            TightGroup(
                SingleField($"Height", obj_f.render_height_3d),
                SingleField($"Radius", obj_f.radius)
            )
        );

        if (Object.type == ObjectType.weapon)
        {
            AddGroup(
                "Weapon Properties",
                Field("Range", obj_f.weapon_range),
                Field("Ammo Type", obj_f.weapon_ammo_type),
                Field("Ammo Consumption", obj_f.weapon_ammo_consumption),
                Field("Crit Hit Chart", obj_f.weapon_crit_hit_chart),
                Field("Attack Type", obj_f.weapon_attacktype),
                Field("Damage Dice", obj_f.weapon_damage_dice),
                Field("Anim Type", obj_f.weapon_animtype),
                Field("Type", obj_f.weapon_type),
                Field("Crit Range", obj_f.weapon_crit_range)
            );
        }
    }

    private IPropertyEditor Field(string label, obj_f field)
    {
        return (ObjectFields.GetType(field)) switch
        {
            ObjectFieldType.Int32 => Int32Field(label, field),
            ObjectFieldType.String => StringField(label, field),
            ObjectFieldType.Float32 => SingleField(label, field),
            _ => throw new ArgumentOutOfRangeException()
        };
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
            value => Object.SetInt32(field, value),
            Object.GetProtoObj()?.GetInt32(field)
        );
    }

    private IPropertyEditor SingleField(string label, obj_f field)
    {
        return new SingleEditor(
            label,
            () => Object.GetFloat(field),
            value => Object.SetFloat(field, value),
            Object.GetProtoObj()?.GetFloat(field)
        );
    }

    private IPropertyEditor BaseStatField(string label, Stat field)
    {
        return new Int32Editor(
            label,
            () => Object.GetBaseStat(field),
            value => Object.SetBaseStat(field, value),
            Object.GetProtoObj()?.GetBaseStat(field)
        );
    }

    public void Render()
    {
        if (Object.GetProtoObj() != null && ImGui.Button("Open Proto"))
        {
            ObjectEditors.Edit(Object.GetProtoObj());
        }
            
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

        // Find any particle systems attached to the object
        ImGui.Text("Particle Systems");
        ImGui.Separator();
        foreach (var partSys in GameSystems.ParticleSys.GetAttachedTo(Object))
        {
            RenderPartSys(partSys);
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

    private void RenderPartSys(PartSys partSys)
    {
        var spec = partSys.GetSpec();
        ImGui.Text(spec.GetName());
        ImGui.SameLine();
        ImGui.Text($"Alive: {(int) partSys.GetAliveInSecs()}s");
        ImGui.SameLine();

        var particles = 0;
        var ended = false;
        foreach (var emitter in partSys.GetEmitters())
        {
            particles += emitter.GetParticles().Length;
            if (emitter.IsEnded)
            {
                ended = true;
            }
        }

        ImGui.Text($"{particles} particles");

        if (ended)
        {
            ImGui.SameLine();
            ImGui.Text("[ENDED]");
        }

        if (partSys.IsDead())
        {
            ImGui.SameLine();
            ImGui.Text("[DEAD]");
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

    private string BuildClassLevels(GameObject critter)
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

    private static void RenderNameLabelPairs(params (string, string)[] pairs)
    {
        DebugUiUtils.RenderNameLabelPairs(pairs);
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

    public readonly List<IPropertyEditor> Properties = new();

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
    protected readonly string Label;

    protected readonly Func<T> Getter;

    protected readonly Action<T> Setter;

    protected AbstractPropertyEditor(string label, Func<T> getter, Action<T> setter)
    {
        Label = label;
        Getter = getter;
        Setter = setter;
    }

    public abstract void Render();

    protected virtual bool IsValueInherited => false;

    protected void RenderLabel()
    {
        Tig.DebugUI.PushSmallFont();
        ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.9f, 1.0f), Label);
        if (IsValueInherited)
        {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), " [Inherited]");
        }
        ImGui.PopFont();
        ImGui.PushItemWidth(-1);
    }
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
        if (_lastKnownObjectValue == null || _lastKnownObjectValue != Getter())
        {
            _lastKnownObjectValue = Getter();
            _currentValue = _lastKnownObjectValue;
        }

        if (ImGui.InputText(Label, ref _currentValue, 1000, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            Setter(_currentValue);
        }
    }
}

public class Int32Editor : AbstractPropertyEditor<int>
{
    private readonly int? _inheritedValue;
        
    private int _lastKnownObjectValue;

    private string _currentValue;

    public Int32Editor(string label, Func<int> getter, Action<int> setter, int? inheritedValue) : base(label, getter, setter)
    {
        UpdateValue();
        _inheritedValue = inheritedValue;
    }

    private void UpdateValue()
    {
        _lastKnownObjectValue = Getter();
        _currentValue = _lastKnownObjectValue.ToString();
    }

    public override void Render()
    {
        if (_lastKnownObjectValue != Getter())
        {
            UpdateValue();
        }

        ImGui.BeginGroup();
        RenderLabel();
        if (ImGui.InputText("##" + Label, ref _currentValue, 32, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (int.TryParse(_currentValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out int newValue))
            {
                Setter(newValue);
            }

            UpdateValue();
        }

        ImGui.PopItemWidth();
        ImGui.EndGroup();
    }

    protected override bool IsValueInherited => _inheritedValue == _lastKnownObjectValue;
}

public class SingleEditor : AbstractPropertyEditor<float>
{
    private readonly float? _inheritedValue;
        
    private float _lastKnownObjectValue;

    private string _currentValue;

    public SingleEditor(string label, Func<float> getter, Action<float> setter, float? inheritedValue) : base(label, getter, setter)
    {
        UpdateValue();
        _inheritedValue = inheritedValue;
    }

    private void UpdateValue()
    {
        _lastKnownObjectValue = Getter();
        _currentValue = _lastKnownObjectValue.ToString();
    }

    public override void Render()
    {
        if (_lastKnownObjectValue != Getter())
        {
            UpdateValue();
        }

        ImGui.BeginGroup();
        RenderLabel();
        if (ImGui.InputText("##" + Label, ref _currentValue, 32, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            if (int.TryParse(_currentValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out int newValue))
            {
                Setter(newValue);
            }

            UpdateValue();
        }

        ImGui.PopItemWidth();
        ImGui.EndGroup();
    }
        
    protected override bool IsValueInherited => _inheritedValue.HasValue 
                                                && MathF.Abs(_lastKnownObjectValue - _inheritedValue.Value) < 0.0001f;
}

internal class AbilityModifierLabel : IPropertyEditor
{
    private readonly GameObject _critter;

    private readonly Stat _stat;

    public AbilityModifierLabel(GameObject critter, Stat stat)
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