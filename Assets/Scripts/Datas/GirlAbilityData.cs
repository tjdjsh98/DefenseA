using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlAbilityData
{
    public static int GIRLABILITY_COUNT = (int)GirlAbility.END;
}
public enum GirlAbility
{
    None = -1,
    BlackSphere,
    FastReload,
    ExtraAmmo,
    AutoReload,
    LastShot,
    PlentyOfBullets,
    Electric,
    LastStruggle,
    VolleyFiring,
    END
}
