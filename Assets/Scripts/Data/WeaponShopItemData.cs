using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create ShopItem", menuName = "AddData/Create WeaponShopItemData", order = 1)]
public class WeaponShopItemData : ShopItemData
{
    public Define.WeaponName weaponName;
    public Define.WeaponPosition weaponPosition;
}
