using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Item", menuName = "AddData/Create WeaponItemData", order = 1)]
public class WeaponItemData : ItemData
{
    public Define.WeaponName weaponName;
    public Define.WeaponPosition weaponPosition;
}
