using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create FatherCardData", order = 2)]

public class FatherCardData : CardData
{
    [field: SerializeField] public float IncreaseNormalAttackSpeedPercentage;
    [field: SerializeField] public bool UnlockShockwave;
    [field: SerializeField] public float IncreaseShockwaveDamagePercentage;
    [field: SerializeField] public float IncreaseShockwaveRangePercentage;
    [field: SerializeField] public float IncreaseShockwaveAttackSpeedPercentage;
    [field: SerializeField] public int IncreaseShockwaveCount;
    [field: SerializeField] public bool UnlockStempGround;
    [field: SerializeField] public float IncreaseStempGroundDamagePercentage;
    [field: SerializeField] public float IncreaseStempGroundRangePercentage;

}
