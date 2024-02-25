using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card",menuName = "AddData/Create CardData", order = 0)]
public class CardData : ScriptableObject,ITypeDefine
{
    [field:SerializeField]public Define.CardName CardName { set; get; }
    [field: SerializeField] public Define.CardType CardSelectionType { set; get; }
    [field: SerializeField] public string CardDescription { set; get; }
    [field: SerializeField] public bool IsStartCard { set; get; }

    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌.
    [field: SerializeField] public List<Define.CardName> PriorCards {set;get; }
    [field: SerializeField] public int MaxUpgradeCount { get; set; }

    [field: SerializeField] public int IncreaseHp { get; set; }
    [field: SerializeField] public float IncreaseRecoverHpPower { get; set; }
    [field: SerializeField] public float IncreaseDamageReducePercentage { get; set; }
    [field:SerializeField] public int IncreaseAttackPoint { get; set; }

    public int GetEnumToInt()
    {
        return (int)CardName;
    }
}
