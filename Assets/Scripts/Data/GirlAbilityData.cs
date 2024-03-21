using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirlAbilityData
{
    public static int GIRLABILITY_COUNT = (int)GirlAbilityName.END;
}
public enum GirlAbilityName
{
    None = -1,
    LastShot,
    AutoReload,
    PlentyOfBullets,
    LastStruggle,
    FastReload,
    ExtraAmmo,
    BlackBullet,
    MealPreparation,
    Hunger,
    END
};