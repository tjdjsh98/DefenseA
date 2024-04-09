using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Inventory
{
    CardManager _cardManager;
    CardManager CardManager
    {
        get
        {
            if(_cardManager == null)
                _cardManager = Managers.GetManager<CardManager>();  

            return _cardManager;
        }
    }
    Player _player;
    Player Player
    { 
        get 
        { 
            if(_player == null)
                _player= Managers.GetManager<GameManager>().Player;
            return _player; 
        } 
    }
    Character _girl;
    Character Girl
    {
        get
        {
            if (_girl == null)
                _girl = Managers.GetManager<GameManager>().Girl;
            return _girl;
        }
    }
    Character _creature;
    Character Creature
    {
        get
        {
            if (_creature == null)
                _creature = Managers.GetManager<GameManager>().Creature;
            return _creature;
        }
    }
    [SerializeField] List<ItemSlot> _slotList = new List<ItemSlot>();
    [SerializeField] List<ItemHistory> _itemHistroyList = new List<ItemHistory>();

    // 어둠구체의4번째 파편
    bool _IsActiveDarkSphereForthFragment = false;
    float _darkSphereForthFragmentTime = 0;

    // 피뢰침
    bool _isActiveLightingRod = false;
    float _lightingRodActiveTime;
    float _lightingRodTime;

    // 피묻은뼈목걸이
    bool _isActiveBloodyBoneNecklace = false;
    float _bloodyBoneNecklaceTime;

    // 탁한 잎
    public int CloudyLeafActiveCount = 0;
    public float _cloudyLeafDuration = 7;
    public float _cloudyLeafTime = 0;

    public void InventoryUpdate()
    {
        HandleDarkSphereForthFragment();
        HandleLightingRod();
        BloodyBoneNecklace();
        HandleCloudyLeaf();
    }

    private void HandleDarkSphereForthFragment()
    {
        if (!_IsActiveDarkSphereForthFragment) return;

        _darkSphereForthFragmentTime += Time.deltaTime;

        if(_darkSphereForthFragmentTime > 30)
        {
            _darkSphereForthFragmentTime = 0;
            CardManager.AddBlackSphere(Player.Character.GetCenter());
        }
    }
    private void HandleLightingRod()
    {
        if (!_isActiveLightingRod) return;

        if(CardManager.CurrentElectricity >= 100)
        {
            _lightingRodTime += Time.deltaTime;
            if (_lightingRodActiveTime <= _lightingRodTime)
            {
                CardManager.CurrentElectricity -= 100;
                _lightingRodActiveTime = Random.Range(2, 4);
                _lightingRodTime= 0;

                Vector3? position = Managers.GetManager<GameManager>().GetGroundTop(Player.transform.position + Vector3.right *10);
                if (position.HasValue)
                {
                    Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Lighting);
                    effect.SetAttackProperty(Player.Character, 50, 100,2, Define.CharacterType.Enemy);
                    effect.Play(position.Value);
                }
            }
        }
    }

    private void BloodyBoneNecklace()
    {
        if (!_isActiveBloodyBoneNecklace) return;

        _bloodyBoneNecklaceTime += Time.deltaTime;
        if (_bloodyBoneNecklaceTime > 5 )
        {
            _bloodyBoneNecklaceTime = 0;
            if (CardManager.Predation >= 1)
            {
                if (Girl.Hp < Girl.MaxHp || Creature.Hp < Creature.MaxHp)
                {
                    CardManager.Predation -= 1;
                    Managers.GetManager<GameManager>().Girl.Hp += 3;
                    Managers.GetManager<GameManager>().Creature.Hp += 3;
                }
            }
        }
    }

    void HandleCloudyLeaf()
    {
        _cloudyLeafTime += Time.deltaTime;
        if (_cloudyLeafDuration < _cloudyLeafTime)
        {
            CloudyLeafActiveCount = 0;
            _cloudyLeafTime = 0;
            for(int i = 0; i < GetItemCount(ItemName.탁한잎); i++)
            {
                if (CardManager.BlackSphereList.Count <= 0) return;

                BlackSphere blackSphere = CardManager.BlackSphereList[0];
                CardManager.BlackSphereList.RemoveAt(0);
                blackSphere.MoveToDestination(Girl.GetCenter(), 0.1f, true);
                CloudyLeafActiveCount++;
            }
        }
    }

    public void AddItem(ItemData itemData, int count = 1)
    {
        ItemSlot itemSlot = null;
        foreach(var slot in _slotList)
        {
            if(slot.ItemData == itemData)
            {
                itemSlot = slot;
                break;
            }
        }
        for (int i = 0; i < count; i++)
        {
            if (itemSlot == null)
            {
                itemSlot = new ItemSlot(itemData, 0);
                _slotList.Add(itemSlot);
            }
            itemSlot.Count++;
            _itemHistroyList.Add(new ItemHistory(itemData, true));
            ApplyAddItem(itemData);
        }
    }
    // 갯수가 없을 때 삭제를 취소하고 false를 반환한다.
    public bool RemoveItem(ItemData itemData, int count = 1)
    {
        ItemSlot itemSlot = null;
        foreach (var slot in _slotList)
        {
            if (slot.ItemData == itemData)
            {
                itemSlot = slot;
                break;
            }
        }
        if (itemSlot == null || itemSlot.Count < count) return false;
        for (int i = 0; i < count; i++)
        {
            itemSlot.Count--;
            
            _itemHistroyList.Add(new ItemHistory(itemData, false));
            ApplyRemoveItem(itemData);
        }
        if (itemSlot.Count == 0)
            _slotList.Remove(itemSlot);

        return true;
    }
    public bool RemoveItem(ItemName itemaName, int count = 1)
    {
        ItemSlot itemSlot = null;
        foreach (var slot in _slotList)
        {
            if (slot.ItemData.ItemName == itemaName)
            {
                itemSlot = slot;
                break;
            }
        }
        if (itemSlot == null || itemSlot.Count < count) return false;
        for (int i = 0; i < count; i++)
        {
            itemSlot.Count--;

            _itemHistroyList.Add(new ItemHistory(itemSlot.ItemData, false));
            ApplyRemoveItem(itemSlot.ItemData);
        }
        if (itemSlot.Count == 0)
            _slotList.Remove(itemSlot);

        return true;
    }
    ItemData GetPossessRandomItem(Func<ItemData, bool> condition)
    {
        List<ItemSlot> list = _slotList.Where(slot =>
        {
            if (condition.Invoke(slot.ItemData))
                return true;
            return false;
        }).ToList();

        if (list.Count <= 0) return null;

        return list.GetRandom().ItemData;

    }
    void ApplyAddItem(ItemData itemData)
    {
        if (itemData.ItemType == ItemType.Ability)
        {
            Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
            RemoveItem(itemData);
        }
        else if (itemData.ItemType == ItemType.Weapon)
        {
            WeaponItemData weaponItemData= itemData as WeaponItemData;
            if(weaponItemData != null)
            {
                Player.WeaponSwaper.ChangeNewWeapon((int)weaponItemData.weaponPosition, weaponItemData.weaponName);
            }
        }
        else if (itemData.ItemType == ItemType.StatusUp)
        {
            switch (itemData.ItemName)
            {
                case ItemName.건전지:
                    CardManager.ChargeElectricty += 0.05f;
                    break;
                case ItemName.보조배터리:
                    CardManager.ChargeElectricty += 0.1f;
                    break;
                case ItemName.네번째손:
                    Player.GirlAbility.IncreasedReloadSpeedPercentage += 20;
                    break;
                case ItemName.어둠구체의4번째파편:
                    _IsActiveDarkSphereForthFragment = true;
                    break;
                case ItemName.피뢰침:
                    _lightingRodActiveTime = Random.Range(2, 4);
                    _isActiveLightingRod = true;
                    break;
                case ItemName.어둠구체의1번째파편:
                    CardManager.BlackSphereAttackPower += 5;
                    break;
                case ItemName.망각석:
                    CardManager.RemoveRandomCard();
                    break;
                case ItemName.망각의서:
                    RemoveItem(GetPossessRandomItem(data => { return data.Rank >= 2; }));
                    break;
                case ItemName.피묻은뼈목걸이:
                    _isActiveBloodyBoneNecklace= true;
                    break;
                case ItemName.눈알케이크:
                    Managers.GetManager<GameManager>().MentalAccelerationPercentage += 100f;
                    break;
            }
            if (itemData is StatusUpItemData data)
            {
                ApplyStatus(data);
            }
        }
    }

    void ApplyRemoveItem(ItemData itemData)
    {

        if (itemData.ItemType == ItemType.StatusUp)
        {
            switch (itemData.ItemName)
            {
                case ItemName.건전지:
                    CardManager.ChargeElectricty -= 0.05f;
                    break;
                case ItemName.보조배터리:
                    CardManager.ChargeElectricty -= 0.1f;
                    break;
                case ItemName.네번째손:
                    Player.GirlAbility.IncreasedReloadSpeedPercentage -= 20;
                    break;
                case ItemName.어둠구체의4번째파편:
                    if(GetItemCount(itemData) <=0)
                        _IsActiveDarkSphereForthFragment = false;
                    break;
                case ItemName.피뢰침:
                    if (GetItemCount(itemData) <= 0)
                        _isActiveLightingRod = false;
                    break;
                case ItemName.어둠구체의1번째파편:
                    CardManager.BlackSphereAttackPower -= 5;
                    break;
                case ItemName.피묻은뼈목걸이:
                    if(GetItemCount(ItemName.피묻은뼈목걸이) <= 0)
                        _isActiveBloodyBoneNecklace = false;
                    break;
                case ItemName.눈알케이크:
                    Managers.GetManager<GameManager>().MentalAccelerationPercentage -= 100f;
                    break;
            }
            if (itemData is StatusUpItemData data)
            {
                RevertStatus(data);
            }
        }
    }

    public int GetItemCount(ItemData itemData)
    {
        foreach (var slot in _slotList)
        {
            if (slot.ItemData == itemData)
            {
                return slot.Count; 
            }
        }
        return 0;
    }
    public int GetItemCount(ItemName itemName)
    {
        foreach (var slot in _slotList)
        {
            if (slot.ItemData.ItemName.Equals(itemName))
            {
                return slot.Count;
            }
        }
        return 0;
    }
    public void ApplyStatus(StatusUpItemData statusUpItemData)
    {
        Character girl = Managers.GetManager<GameManager>().Girl;
        Character creature = Managers.GetManager<GameManager>().Creature;
        CreatureAI creatureAI = Managers.GetManager<GameManager>().CreatureAI;
        if (girl)
        {
            girl.AddMaxHp(statusUpItemData.IncreasingGirlMaxHp);
            girl.Hp += statusUpItemData.RecoverGirlHpAmount;
            girl.SetSpeed(girl.Speed + statusUpItemData.IncreasingGirlSpeed);
            Player.GirlAbility.IncreasedAttackPowerPercentage += statusUpItemData.IncreasingGirlAttackPowerPercentage;
            girl.IncreasedHpRegeneration += statusUpItemData.IncreasingGirlHpRegeneration;
        }

        if (creature)
        {
            creature.AddMaxHp(statusUpItemData.IncreasingCreatureMaxHp);
            creature.AttackPower += statusUpItemData.IncreasingCreatureAttackPower;
            creatureAI.CreatureAbility.IncreasedAttackSpeedPercentage += statusUpItemData.IncreasingCreatureAttackSpeedPercentage;
            creature.Hp += statusUpItemData.RecoverCreatureHpAmount;
            creature.IncreasedHpRegeneration += statusUpItemData.IncreasingCreatureHpRegeneration;
            creature.SetSpeed(creature.Speed + statusUpItemData.IncreasingCreatureSpeed);
        }

        Managers.GetManager<GameManager>().Mental += statusUpItemData.RecoverMentalAmount;
    }
    public void RevertStatus(StatusUpItemData statusUpItemData)
    {
        Character girl = Managers.GetManager<GameManager>().Girl;
        Character creature = Managers.GetManager<GameManager>().Creature;
        CreatureAI creatureAI = Managers.GetManager<GameManager>().CreatureAI;

        if (girl)
        {
            girl.AddMaxHp(-statusUpItemData.IncreasingGirlMaxHp);
            girl.SetSpeed(girl.Speed - statusUpItemData.IncreasingGirlSpeed);
            Player.GirlAbility.IncreasedAttackPowerPercentage -= statusUpItemData.IncreasingGirlAttackPowerPercentage;
            girl.IncreasedHpRegeneration -= statusUpItemData.IncreasingGirlHpRegeneration;
        }

        if (creature)
        {
            creature.AddMaxHp(-statusUpItemData.IncreasingCreatureMaxHp);
            creature.AttackPower -= statusUpItemData.IncreasingCreatureAttackPower;
            creatureAI.CreatureAbility.IncreasedAttackSpeedPercentage -= statusUpItemData.IncreasingCreatureAttackSpeedPercentage;
            creature.IncreasedHpRegeneration -= statusUpItemData.IncreasingCreatureHpRegeneration;
            creature.SetSpeed(creature.Speed - statusUpItemData.IncreasingCreatureSpeed);
        }
    }
}

[System.Serializable]
public class ItemSlot
{
    [field:SerializeField]public ItemData ItemData { set; get; }
    [field: SerializeField] public int Count { get; set; }
    public ItemSlot(ItemData itemData, int count)
    {
        ItemData = itemData;
        Count = count;
    }
}

[System.Serializable]
public struct ItemHistory
{ 
    public ItemHistory(ItemData itemData,bool isGet)
    {
        this.itemData = itemData;
        this.isGet= isGet;
    }
    [field: SerializeField] public ItemData itemData { set; get; }
    [field: SerializeField] public bool isGet { set; get; }
}
