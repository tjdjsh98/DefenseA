using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAbilityData
{
    public static int CREATUREABILITY_COUNT = (int)CreatureAbility.END;
}
public enum CreatureAbility
{
    None = -1,
    Shockwave,
    StempGround,
    SurvialIntinct,
    Rage,
    END
};