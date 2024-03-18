using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create WallCardData", order = 3)]
public class WallCardData : CardData
{
    [field: SerializeField] public WallAbilityName UnlockAbility { set; get; }
    [field: SerializeField] public float SizeUpPercentage { set; get; }
}