using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card",menuName = "AddData/Create CardData", order = 0)]
public class CardData : ScriptableObject,ITypeDefine
{
    [field:SerializeField]public CardName CardName { set; get; }
    [field: SerializeField] public Define.CardType CardSelectionType { set; get; }
    [field: SerializeField] public string CardDescription { set; get; }
    [field: SerializeField] public bool IsStartCard { set; get; }

    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌.
    [field: SerializeField] public List<CardName> PriorCards { set;get; }
    [field: SerializeField] public int MaxUpgradeCount { get; set; }

    [field: SerializeField] public int IncreaseHp { get; set; }
    [field: SerializeField] public float IncreaseRecoverHpPower { get; set; }
    [field: SerializeField] public float IncreaseDamageReducePercentage { get; set; }
    [field:SerializeField] public int IncreaseAttackPoint { get; set; }

    public int GetEnumToInt()
    {
        return (int)CardName;
    }
    public static int CARD_COUNT = (int)CardName.END;
}
public enum CardName
{
    None = -1,
    
    라스트샷,
    자동장전,
    넉넉한총알,
    검은구체,
    마지막발악,
    전기발전,
    일제사격,
    빠른장전,
    추가장전,
    쇼크웨이브,
    땅구르기,
    생존본능,
    분노,
    방벽,
    구체생성,
    자폭,
    성장,
    공격구체,
    피격구체,
    구체증가,
    폭파구체,
    여분구체,
    END,
}