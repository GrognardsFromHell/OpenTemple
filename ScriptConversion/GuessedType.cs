namespace ScriptConversion
{
    enum GuessedType
    {
        /// <summary>
        /// An object handle, essentially. Anything that's callable on a game object is available.
        /// </summary>
        Object,
        /// <summary>
        /// Event object for a triggered trap.
        /// </summary>
        TrapSprungEvent,
        /// <summary>
        /// An active spell (SpellPacketBody).
        /// </summary>
        Spell,
        /// <summary>
        /// A list of objects, such as returned by obj_list_vicinity.
        /// </summary>
        ObjectList,
        IntList,
        TrapDamageList,
        TrapDamage,
        SpellTargets,
        /// <summary>
        /// A primitive (i.e. string, number)
        /// </summary>
        Bool,
        Integer,
        ParticleSystem,
        Float,
        String,
        Unknown,
        UnknownList,
        FeatId,
        Quest,
        QuestState,
        ObjectType,
        Location,
        LocationFull,
        Time,
        Stat,
        EncounterQueue,
        Dice,
        SpellTarget,
        Alignment,
        Gender,
        SpellDescriptor,
        Race,
        D20CAF,
        Material,
        Void,
        MonsterCategory,
        MonsterSubtype,
        EquipSlot,
        LootSharingType,
        MapTerrain,
        TutorialTopic,
        DispatcherKey,
        CritterFlag,
        SizeCategory,
        Domain,
        Deity,
        ObjScriptEvent,
        ContainerFlag,
        PortalFlag,
        ItemFlag,
        NpcFlag,
        ObjectField,
        Skill,
        SchoolOfMagic,
        FrogGrapplePhase,
        ResurrectionType,
        D20ActionType,
        D20AttackPower,
        DamageType,
        D20SavingThrowFlag,
        SavingThrowType,
        D20SavingThrowReduction,
        FadeOutResult,
        ObjectFlag,
        ObjectListFilter,
        WeaponFlag,
        RadialMenuParam,
        SleepStatus,
        TargetListOrder,
        TargetListOrderDirection,
        StandPointType,
        RandomEncounterQuery,
        RandomEncounterType,
        RandomEncounterEnemies,
        RandomEncounterEnemy,
        RandomEncounter,
        TextFloaterColor,
        Co8SpellFlag
    }
}