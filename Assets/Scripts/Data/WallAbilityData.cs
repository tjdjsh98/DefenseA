using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAbilityData
{
    public static int WALLABILITY_COUNT = (int)WallAbility.END;
}
public enum WallAbility
{
    None = -1,
    Barrier,
    GenerateSphere,
    SelfDestruct,
    END
};