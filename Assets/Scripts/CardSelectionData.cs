using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create CardSelection",menuName = "AddData/Create CardSelection", order = 0)]
public class CardSelectionData : ScriptableObject,ITypeDefine
{
    [SerializeField]protected Define.CardSelection _cardSelection;
    public Define.CardSelection CardSelection => _cardSelection;
    [SerializeField] protected Define.CardSelectionType _cardSelectionType;
    public Define.CardSelectionType CardSelectionType => _cardSelectionType;

    [TextArea][SerializeField] protected string _cardDescription;
    public string CardDescription => _cardDescription;

    [SerializeField] bool _isStartCard;
    public bool IsStartCard => _isStartCard;
    
    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌.
   
    [SerializeField] List<CardSelectionData> _cardListToAdd;
    public List<CardSelectionData> CardListToAdd => _cardListToAdd;

    [SerializeField] protected int _maxUpgradeCount;
    public int MaxUpgradeCount => _maxUpgradeCount;
    public int GetEnumToInt()
    {
        return (int)_cardSelection;
    }
}
