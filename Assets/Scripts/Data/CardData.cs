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
    [field: SerializeField] public bool IsActiveAbility { set; get; }

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
    마지막발악,
    전기발전,
    빠른장전,
    추가장전,
    검은총알,
    식사준비,
    굶주림,
    쇼크웨이브,
    땅구르기,
    생존본능,
    분노,
    검게흐르는,
    전류통과,
    충전,
    전기방출,
    강타,
    방벽,
    자폭,
    성장,
    검은기운,
    구원,
    흘러넘치는어둠,
    잔류전기,
    과충전,
    포식,
    옅은소화,
    검은구체,
    구체통제,
    일제사격,
    폭발성구체,
    미끼,
    미세전력,
    추가배터리,
    자가발전,
    식욕,
    END,
}