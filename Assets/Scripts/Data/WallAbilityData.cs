using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAbilityData
{
    public static int WALLABILITY_COUNT = (int)WallAbilityName.END;
}
public enum WallAbilityName
{
    None = -1,
    Barrier,
    SelfDestruct,
    Grow,
    BlackAura,
    Salvation,
    OverflowingDark,
    ResidualElectricity,
    OverCharge,
    Predation,
    LightDigestion,
    END
};