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
    [field: SerializeField] public List<float> coolTimeList { set; get; }

    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌.
    [field: SerializeField] public List<PriorCardData> PriorCards { set;get; }
    [field: SerializeField] public int MaxUpgradeCount { get; set; }
    [field: SerializeField] public List<float> PropertyList { get; set; }
    [field: SerializeField] public List<float> Property2List { get; set; }


    public int GetEnumToInt()
    {
        return (int)CardName;
    }
    public static int CARD_COUNT = (int)CardName.END;
}
    [System.Serializable]

    public class PriorCardData
    {
        public CardName priorCardName;
        public int priorUpgradeCount;
    }
public enum CardName
{
    None = -1,      
    일제사격,
    미끼,
    식사,
    울부짖기,
    쇼크웨이브,
    전기방출,
    부유,
    막기,
    END,
}