using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonAbilityData
{
    public static int COMMONABILITY_COUNT = (int)CommonAbilityName.END;
}
public enum CommonAbilityName
{
    None = -1,
    BlackSphere,
    ControlSphere,
    VolleyFire,
    ExplosiveSphere,
    Bait,
    MicroPower,
    ExtraBattery,
    SelfGeneration,
    Appetite,
    END
};