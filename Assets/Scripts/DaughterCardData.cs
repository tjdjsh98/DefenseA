using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create DaughterCardData", order = 1)]
public class DaughterCardData : CardData
{
    [field:SerializeField]public bool UnlockLastShot { set; get; }
    [field: SerializeField] public bool UnlockFastReload { set; get; }
    [field: SerializeField] public bool UnlockAutoReload { set; get; }
    [field: SerializeField] public bool UnlockExtraAmmo { set; get; }
    [field: SerializeField] public float DecreaseFireDelayPercentage { set; get; }
    [field: SerializeField] public float IncreaseReloadSpeedPercentage { set; get; }
    [field: SerializeField] public float IncreaseReboundControlPowerPercentage { set; get; }
    [field: SerializeField] public int IncreasePenerstratingPower { set; get; }
}
