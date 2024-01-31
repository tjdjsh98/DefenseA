using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class WeaponSwaper : MonoBehaviour
{
    Character _character;
    Weapon _currentWeapon;

    [SerializeField] List<Define.WeaponName> _weaponNameList;
    [SerializeField] List<GameObject> _weaponSlotList;
    public List<GameObject> WeaponSlotList => _weaponSlotList;
    List<Weapon> _weaponList;

    public Weapon CurrentWeapon=> _currentWeapon;
    public Action<Weapon> WeaponSwaped;

    int _weaponIndex;
    public int WeaponIndex => _weaponIndex;

    private void Awake()
    {
        _character = GetComponent<Character>();
        SelectWeapon(-1);
        _weaponList= new List<Weapon>();
        

        for (int i = 0; i < _weaponNameList.Count; i++)
        {
            _weaponList.Add(null);
            ChangeNewWeapon(i, _weaponNameList[i]);

        }
       

    }

    public void ChangeNewWeapon(int index, Define.WeaponName weaponName)
    {
        if (index < 0 || index >= _weaponSlotList.Count) return;

        if (_weaponList[index] != null)
        {
            Managers.GetManager<ResourceManager>().Destroy(_weaponList[index].gameObject);
        }
        _weaponList[index] = Managers.GetManager<ResourceManager>().Instantiate(Managers.GetManager<DataManager>().GetData<Weapon>((int)weaponName));
        if (_weaponList[index] != null)
        {
            _weaponList[index].transform.SetParent(_weaponSlotList[index].transform);
            _weaponList[index].transform.localPosition = Vector3.zero;
            _weaponList[index].Init(_character);
        }
    }

    public void SelectWeapon(int index)
    {
        if(index == -1)
        {
            _currentWeapon = null;
            _weaponIndex = -1;
            return;
        }
        if (index < 0 || index >= _weaponSlotList.Count) return;

        _currentWeapon = _weaponList[index];

        for(int i =0; i < _weaponList.Count;i++)
        {
            _weaponList[i].gameObject.SetActive(index == i);
        }

        _weaponIndex = index;

        WeaponSwaped?.Invoke(_currentWeapon);
    }

    public Weapon GetWeapon(int index)
    {
        if (_weaponList.Count <= index || index < 0) return null;
        return _weaponList[index];
    }
}
