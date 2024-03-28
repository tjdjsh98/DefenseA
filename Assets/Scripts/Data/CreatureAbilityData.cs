using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAbilityData
{
    public static int CREATUREABILITY_COUNT = (int)CreatureAbilityName.END;
}
public enum CreatureAbilityName
{
    None = -1,
    Shockwave,
    StempGround,
    SurvialIntinct,
    Rage,
    FlowingBlack,
    CurrentPassing,
    Charge,
    ElectricDischarge,
    DiningEtiquette,
    Roar,
    END
};