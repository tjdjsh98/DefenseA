using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card", menuName = "AddData/Create CreatureCardData", order = 2)]

public class CreatureCardData : CardData
{
    [field: SerializeField] public Define.CreatureAbility UnlockAbility { get; set; }
}