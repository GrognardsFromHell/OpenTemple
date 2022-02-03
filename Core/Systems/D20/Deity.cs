using System;

namespace OpenTemple.Core.Systems.D20;

public enum DeityId
{
    NONE = 0,
    BOCCOB = 1,
    CORELLON_LARETHIAN = 2,
    EHLONNA = 3,
    ERYTHNUL = 4,
    FHARLANGHN = 5,
    GARL_GLITTERGOLD = 6,
    GRUUMSH = 7,
    HEIRONEOUS = 8,
    HEXTOR = 9,
    KORD = 10,
    MORADIN = 11,
    NERULL = 12,
    OBAD_HAI = 13,
    OLIDAMMARA = 14,
    PELOR = 15,
    ST_CUTHBERT = 16,
    VECNA = 17,
    WEE_JAS = 18,
    YONDALLA = 19,
    OLD_FAITH = 20,
    ZUGGTMOY = 21,
    IUZ = 22,
    LOLTH = 23,
    PROCAN = 24,
    NOREBO = 25,
    PYREMIUS = 26,
    RALISHAZ = 27
}

public static class DeityIds
{
    public static readonly DeityId[] Deities = (DeityId[]) Enum.GetValues(typeof(DeityId));
}