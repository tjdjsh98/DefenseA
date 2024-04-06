using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Item", menuName = "AddData/Create StatusUpItemData", order = 3)]
public class StatusUpItemData : ItemData
{
    [field:SerializeField]public int IncreasingGirlMaxHp { set; get; }
    [field:SerializeField]public int IncreasingCreatureMaxHp { set; get; }
    [field: SerializeField] public float IncreasingGirlAttackPowerPercentage { set; get; }
    [field: SerializeField] public int IncreasingCreatureAttackPower { set; get; }
    [field: SerializeField] public float IncreasingGirlAttackSpeedPercentage { set; get; }
    [field: SerializeField] public float IncreasingCreatureAttackSpeedPercentage { set; get; }
    [field: SerializeField] public float IncreasingGirlHpRegeneration { set; get; }
    [field: SerializeField] public float IncreasingCreatureHpRegeneration { set; get; }
    [field: SerializeField] public int RecoverGirlHpAmount { set; get; }
    [field: SerializeField] public int RecoverCreatureHpAmount { set; get; }

    [field: SerializeField] public float IncreasingGirlSpeed { set; get; }
    [field: SerializeField] public float IncreasingCreatureSpeed { set; get; }
    [field: SerializeField] public float RecoverMentalAmount { set; get; }

    
}
