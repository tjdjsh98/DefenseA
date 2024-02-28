using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create CreatureCardData", order = 2)]

public class CreatureCardData : CardData
{
    [field: SerializeField] public float IncreaseNormalAttackSpeedPercentage { get; set; }
    [field: SerializeField] public bool UnlockShockwave { get; set; }
    [field: SerializeField] public float IncreaseShockwaveDamagePercentage { get; set; }
    [field: SerializeField] public float IncreaseShockwaveRangePercentage { get; set; }
    [field: SerializeField] public float DecreaseShockwaveCoolTimePercentage { get; set; }
    [field: SerializeField] public int IncreaseShockwaveCount { get; set; }
    [field: SerializeField] public bool UnlockStempGround { get; set; }
    [field: SerializeField] public float IncreaseStempGroundDamagePercentage { get; set; }
    [field: SerializeField] public float IncreaseStempGroundRangePercentage { get; set; }

}
