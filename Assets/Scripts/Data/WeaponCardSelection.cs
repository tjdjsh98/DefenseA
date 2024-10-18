using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create CardSelection",menuName ="AddData/Create WeaponCardSelection", order = 1)]
public class WeaponCardSelection : CardData
{
    [SerializeField] Define.WeaponName _weaponName;
    public Define.WeaponName WeaponName => _weaponName;
    [SerializeField] int _weaponSlotIndex;
    public int WeaponSlotIndex => _weaponSlotIndex;
}
