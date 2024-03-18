using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create CommonCardData", order = 4)]
public class CommonCardData : CardData
{
    [field: SerializeField] public CommonAbilityName UnlockAbility { get; set; }
}
