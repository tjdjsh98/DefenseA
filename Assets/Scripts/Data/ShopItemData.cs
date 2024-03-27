using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create ShopItem", menuName = "AddData/Create ShopItemData", order = 0)]
public class ShopItemData : ScriptableObject
{
    [field: SerializeField] public SellType SellType { set; get; }
    [field: SerializeField] public Sprite Image { set; get; }
    [field:SerializeField][field:TextArea] public string Description { set; get; }
    public int price;
}
public enum SellType
{
    Weapon,
    UpgradeWeapon,
    Ability,
    StatusUp,
}