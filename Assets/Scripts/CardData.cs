using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Card",menuName = "AddData/Create CardData", order = 0)]
public class CardData : ScriptableObject,ITypeDefine
{
    [SerializeField]protected Define.CardName _cardName;
    public Define.CardName CardName => _cardName;
    [SerializeField] protected Define.CardType _cardType;
    public Define.CardType CardSelectionType => _cardType;

    [TextArea][SerializeField] protected string _cardDescription;
    public string CardDescription => _cardDescription;

    [SerializeField] bool _isStartCard;
    public bool IsStartCard => _isStartCard;
    
    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌.
   
    [SerializeField] List<CardData> _cardListToAdd;
    public List<CardData> CardListToAdd => _cardListToAdd;

    [SerializeField] protected int _maxUpgradeCount;
    public int MaxUpgradeCount => _maxUpgradeCount;
    public int GetEnumToInt()
    {
        return (int)_cardName;
    }

    public CardData()
    {
        
    }
}
