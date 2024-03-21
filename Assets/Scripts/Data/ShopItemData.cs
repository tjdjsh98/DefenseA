using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create ShopItem", menuName = "AddData/Create ShopItemData", order = 0)]
public class ShopItemData : ScriptableObject
{
    public SellType sellType;
    [TextArea]public string description;
    public int price;
}
public enum SellType
{
    Weapon,
    Ability
}