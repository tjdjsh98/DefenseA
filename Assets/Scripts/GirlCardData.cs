using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create GirlCardData", order = 1)]
public class GirlCardData : CardData
{
    [field: SerializeField] public Define.GirlAbility UnlockAbility { set; get; }
    [field: SerializeField] public float DecreaseFireDelayPercentage { set; get; }
    [field: SerializeField] public float IncreaseReloadSpeedPercentage { set; get; }
    [field: SerializeField] public float IncreaseReboundControlPowerPercentage { set; get; }
    [field: SerializeField] public int IncreasePenerstratingPower { set; get; }
}
