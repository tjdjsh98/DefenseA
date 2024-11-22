using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwaper : MonoBehaviour
{
    Character _character;
    IWeaponUsable _user;
    Weapon _currentWeapon;
    [SerializeField] protected GaugeBar _reloadGauge;

    [SerializeField] List<Define.WeaponName> _weaponNameList;
    [SerializeField] List<GameObject> _weaponSlotList;
    public List<GameObject> WeaponSlotList => _weaponSlotList;
    List<Weapon> _weaponList;

    public Weapon CurrentWeapon=> _currentWeapon;
    public Action<Weapon> WeaponSwaped;

    int _weaponIndex;
    public int WeaponIndex => _weaponIndex;

    public void Init()
    {
        _character = GetComponent<Character>();
        _user = GetComponent<IWeaponUsable>();
        SelectWeapon(-1);
        _weaponList= new List<Weapon>();
        _reloadGauge = _character.transform.Find("GagueBar")?.GetComponent<GaugeBar>();

        for (int i = 0; i < _weaponSlotList.Count; i++)
        {
            _weaponList.Add(null);
            if(_weaponNameList.Count > i && _weaponNameList[i] != Define.WeaponName.None)
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
            _weaponList[index].transform.localScale = Vector3.one;
            _weaponList[index].transform.localRotation = new Quaternion(0, 0, 0,0);
            _weaponList[index].Init(GetComponent<IWeaponUsable>());
            _weaponList[index].gameObject.SetActive(false);
        }

        if(_weaponIndex == index)
        {
            SelectWeapon(index);
        }
        if (_reloadGauge)
            _reloadGauge.gameObject.SetActive(false);
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


        _currentWeapon?.CancelReload();
        _currentWeapon = _weaponList[index];

        for(int i =0; i < _weaponList.Count;i++)
        {
            _weaponList[i]?.gameObject.SetActive(index == i);
        }
        _weaponIndex = index;

        WeaponSwaped?.Invoke(_currentWeapon);
    }

    public Weapon GetWeapon(int index)
    {
        if (_weaponList.Count <= index || index < 0) return null;
        return _weaponList[index];
    }
    public int GetWeaponCount()
    {
        return _weaponList.Count;
    }
}
