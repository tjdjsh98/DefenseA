using System;
using System.Collections.Generic;
using System.Linq;
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
    Dictionary<ItemName, int> _itemCount = new Dictionary<ItemName, int>(); 
    [SerializeField] List<ItemSlot> _slotList = new List<ItemSlot>();
    [SerializeField] List<ItemHistory> _itemHistroyList = new List<ItemHistory>();

    // 피뢰침
    bool _isActiveLightingRod = false;
    float _lightingRodActiveTime;
    float _lightingRodTime;

    // 탁한 잎
    public int CloudyLeafActiveCount = 0;
    public float _cloudyLeafDuration = 10;
    public float _cloudyLeafTime = 0;

    // 부서진 약지
    public bool IsActiveBrokenRingFinger { get; set; }
    public void InventoryUpdate()
    {
        HandleLightingRod();
        BloodyBoneNecklace();
        HandleCloudyLeaf();
    }

 
    private void HandleLightingRod()
    {
        if (!_isActiveLightingRod) return;

       
        _lightingRodTime += Time.deltaTime;
        if (_lightingRodActiveTime <= _lightingRodTime)
        {
            _lightingRodActiveTime = Random.Range(10, 20);
            _lightingRodTime= 0;

            GameObject go = Managers.GetManager<GameManager>().GetCloseEnemyFromGirl();

            if (go != null)
            {
                Vector3? position = Managers.GetManager<GameManager>().GetGroundTop(go.transform.position);
                if (position.HasValue)
                {
                    Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Lighting);
                    effect.SetAttackProperty(Player.Character, 50, 100, 2, Define.CharacterType.Enemy);
                    effect.Play(position.Value);
                }
            }
        }
    }

    private void BloodyBoneNecklace()
    {
       
    }

    void HandleCloudyLeaf()
    {
        _cloudyLeafTime += Time.deltaTime;
        if (_cloudyLeafDuration < _cloudyLeafTime)
        {
            CloudyLeafActiveCount = 0;
            _cloudyLeafTime = 0;
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
            if(!_itemCount.ContainsKey(itemData.ItemName))
                _itemCount.Add(itemData.ItemName, 0);
            _itemCount[itemData.ItemName]++;
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

            if (!_itemCount.ContainsKey(itemData.ItemName))
                _itemCount.Add(itemData.ItemName, 0);
            if (_itemCount[itemData.ItemName] > 0)
                _itemCount[itemData.ItemName]--;
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
        if (condition != null)
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
        else
        {
            return _slotList.GetRandom().ItemData;
        }

    }
    void ApplyAddItem(ItemData itemData)
    {
        if (itemData.ItemType == ItemType.Weapon)
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
             
           
                case ItemName.피뢰침:
                    _lightingRodActiveTime = Random.Range(10, 20);
                    _isActiveLightingRod = true;
                    break;
                case ItemName.망각의서:
                    RemoveItem(GetPossessRandomItem((data) => { return data.ItemName != ItemName.망각의서; }));
                    break;
                case ItemName.아르라제코인:
                    Managers.GetManager<GameManager>().EnableRestockCount++;
                    break;
                case ItemName.부서진약지:
                    IsActiveBrokenRingFinger = true;
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
                case ItemName.피뢰침:
                    if (GetItemCount(itemData) <= 0)
                        _isActiveLightingRod = false;
                    break;
                case ItemName.아르라제코인:
                    Managers.GetManager<GameManager>().EnableRestockCount--;
                    break;
                case ItemName.부서진약지:
                    IsActiveBrokenRingFinger= false;
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
        if (!_itemCount.ContainsKey(itemData.ItemName)) return 0;
        return _itemCount[itemData.ItemName];
    }
    public int GetItemCount(ItemName itemName)
    {
        if (!_itemCount.ContainsKey(itemName)) return 0;
        return _itemCount[itemName];
    }

    public Dictionary<ItemName,int> GetItemList()
    {
        return _itemCount;
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
            Player.GirlAbility.IncreasedAttackSpeedPercentage += statusUpItemData.IncreasingGirlAttackSpeedPercentage;
            girl.IncreasedHpRegeneration += statusUpItemData.IncreasingGirlHpRegeneration;
            Player.GirlAbility.IncreasedReloadSpeedPercentage += statusUpItemData.IncreasingReloadSpeedPercentage;

        }

        if (creature)
        {
            creature.AddMaxHp(statusUpItemData.IncreasingCreatureMaxHp);
            creature.AttackPower += statusUpItemData.IncreasingCreatureAttackPower;
            creatureAI.CreatureAbility.IncreasedAttackSpeedPercentage += statusUpItemData.IncreasingCreatureAttackSpeedPercentage;
            creature.Hp += statusUpItemData.RecoverCreatureHpAmount;
            creature.IncreasedHpRegeneration += statusUpItemData.IncreasingCreatureHpRegeneration;
            creature.SetSpeed(creature.Speed + statusUpItemData.IncreasingCreatureSpeed);
            creatureAI.ReviveTime -= statusUpItemData.ReviveTimeDown;
        }

        Managers.GetManager<GameManager>().MentalAccelerationPercentage += statusUpItemData.AccelMentalDownPercentage;
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
            Player.GirlAbility.IncreasedAttackSpeedPercentage -= statusUpItemData.IncreasingGirlAttackSpeedPercentage;
            girl.IncreasedHpRegeneration -= statusUpItemData.IncreasingGirlHpRegeneration;
            Player.GirlAbility.IncreasedReloadSpeedPercentage -= statusUpItemData.IncreasingReloadSpeedPercentage;
        }

        if (creature)
        {
            creature.AddMaxHp(-statusUpItemData.IncreasingCreatureMaxHp);
            creature.AttackPower -= statusUpItemData.IncreasingCreatureAttackPower;
            creatureAI.CreatureAbility.IncreasedAttackSpeedPercentage -= statusUpItemData.IncreasingCreatureAttackSpeedPercentage;
            creature.IncreasedHpRegeneration -= statusUpItemData.IncreasingCreatureHpRegeneration;
            creature.SetSpeed(creature.Speed - statusUpItemData.IncreasingCreatureSpeed);
            creatureAI.ReviveTime += statusUpItemData.ReviveTimeDown;
        }
        Managers.GetManager<GameManager>().MentalAccelerationPercentage -= statusUpItemData.AccelMentalDownPercentage;
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
