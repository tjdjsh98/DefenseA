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
    [SerializeField] protected int _maxUpgradeCount;
    public int MaxUpgradeCount => _maxUpgradeCount;

    [TextArea][SerializeField] protected string _cardDescription;
    public string CardDescription => _cardDescription;

    public int GetEnumToInt()
    {
        return (int)_cardSelection;
    }
}
