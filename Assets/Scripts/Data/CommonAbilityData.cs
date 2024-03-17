using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonAbilityData
{
    public static int COMMONABILITY_COUNT = (int)CommonAbility.END;
}
public enum CommonAbility
{
    None = -1,
    GenerateSphere,
    AttackSphere,
    HitSphere,
    IncreaseSphere,
    VolleyFiring,
    ExplosionSphere,
    SpareSphere,
    END
};