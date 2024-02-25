using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create DogCardData", order = 3)]
public class DogCardData : CardData
{
    [field: SerializeField] public int IncreaseReflectionDamage { get; set; }
    [field: SerializeField] public float DecreaseReviveTimePercentage { get; set; }
    [field: SerializeField] public bool UnlockExplosionWhenDead { get;set; }
    [field: SerializeField] public int IncreaseExplosionDamage { get; set; }
    [field: SerializeField] public float IncreaseExplosionRange { get; set; }
    [field: SerializeField] public bool UnlockReviveWhereDaughterPosition { get; set; }
}
