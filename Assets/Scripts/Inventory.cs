using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
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
            if (_cardManager == null)
                _cardManager = Managers.GetManager<CardManager>();

            return _cardManager;
        }
    }
    Player _player;
    Player Player
    {
        get
        {
            if (_player == null)
                _player = Managers.GetManager<GameManager>().Player;
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

    [SerializeField] List<ItemInfo> _itemInfoList = new List<ItemInfo>();
    [SerializeField] List<ItemHistory> _itemHistroyList = new List<ItemHistory>();
    Dictionary<ItemName, bool> _itemExistDictionary = new Dictionary<ItemName, bool>();
    // 피뢰침
    float _lightingRodCoolTime;
    float _lightingRodTime;

    // 탁한 잎
    public int CloudyLeafActiveCount = 0;
    public float _cloudyLeafDuration = 10;
    public float _cloudyLeafTime = 0;

    // 부서진 약지
    public bool IsActiveBrokenRingFinger { get; set; }

    // 검은 세포
    private float _blackCellElapsedTime;
    private float _blackCellCoolTime;

    // 화약
    private float _explosionDamagePercentage;
    float _explosionDamage = 200;
    public int ExplosionDamage =>  Mathf.RoundToInt(_explosionDamage*(1 + _explosionDamagePercentage / 100f));
    public void InventoryUpdate()
    {
        HandleLightingRod();
        BloodyBoneNecklace();
        HandleCloudyLeaf();
        HandleBlackCell();
    }

    private void HandleBlackCell()
    {
        if (GetIsHaveItem(ItemName.검은세포) || GetIsHaveItem(ItemName.검은세포_A) || GetIsHaveItem(ItemName.검은세포_B))
        {
            _blackCellElapsedTime += Time.deltaTime;
            if (_blackCellElapsedTime > _blackCellCoolTime)
            {
                _blackCellElapsedTime = 0;
                if (GetIsHaveItem(ItemName.검은세포_A))
                {
                    _blackCellCoolTime = Random.Range(7, 10);
                }
                else
                {
                    _blackCellCoolTime = Random.Range(15, 20);
                }
                float width = 10;
                if (GetIsHaveItem(ItemName.검은세포_B))
                    width = 20;
                Thorn thorn = Managers.GetManager<ResourceManager>().Instantiate<Thorn>("Prefabs/Thorn");
                thorn.transform.position = Creature.transform.position;
                thorn.Init(Creature,width);
            }
        }
    }

    private void HandleLightingRod()
    {
        if (GetIsHaveItem(ItemName.피뢰침) || GetIsHaveItem(ItemName.피뢰침_A) || GetIsHaveItem(ItemName.피뢰침_B))
        {
            _lightingRodTime += Time.deltaTime;
            if (_lightingRodCoolTime <= _lightingRodTime)
            {
                if (GetIsHaveItem(ItemName.피뢰침_A))
                    _lightingRodCoolTime = Random.Range(7, 10);
                else
                    _lightingRodCoolTime = Random.Range(15, 20);

                _lightingRodTime = 0;

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
                else
                {
                    Vector3? position = Managers.GetManager<GameManager>().GetGroundTop(Random.Range(0, 2) == 0 ? Girl.transform.position : Creature.transform.position);
                    if (position.HasValue)
                    {
                        Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Lighting);
                        effect.SetAttackProperty(Player.Character, 50, 100, 2, Define.CharacterType.Enemy);
                        effect.Play(position.Value);
                    }
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

    public void Explosion(Character attacker,Vector3 point, float radius)
    {
        Define.Range range = new Define.Range() { center = Vector3.zero, size = Vector3.one * radius, angle = 0, figureType = Define.FigureType.Circle };
        Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
  
        Util.RangeCastAll2D(point, range, Define.CharacterMask, (hit) =>
        {
            Character character = hit.collider.GetComponent<Character>();
            if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
            {
                attacker.Attack(character, ExplosionDamage, 150, character.transform.position - point, hit.point, 0.3f);
            }
            return false;
        });
        effect.SetProperty("Radius", radius);
        effect.Play(point);

        Gunpowder(attacker, point, radius);
    }

    void Gunpowder(Character attacker, Vector3 point, float radius)
    {
        if (GetIsHaveItem(ItemName.화약_B))
        {
            for (int i = 0; i < Random.Range(3, 5); i++)
            {
                Vector3 random = Random.insideUnitCircle * radius / 2f;
                Define.Range range = new Define.Range() { center = Vector3.zero, size = Vector3.one * radius/2, angle = 0, figureType = Define.FigureType.Circle };
                Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
                Util.RangeCastAll2D(point + random, range, Define.CharacterMask, (hit) =>
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        attacker.Attack(character, ExplosionDamage, 50, character.transform.position - point, hit.point, 0.3f);
                    }
                    return false;
                });
                effect.SetProperty("Radius", radius/2);
                effect.Play(point+random);
            }
        }
    }
    public void AddItem(ItemData itemData)
    {
        ItemInfo itemInfo = null;
      
        itemInfo = new ItemInfo(itemData);
        _itemInfoList.Add(itemInfo);
        _itemHistroyList.Add(new ItemHistory(itemInfo, true));

        if (!_itemExistDictionary.ContainsKey(itemData.ItemName))
            _itemExistDictionary.Add(itemData.ItemName, true);
        _itemExistDictionary[itemData.ItemName] = true;

        ApplyAddItem(itemInfo);
    }
    // 갯수가 없을 때 삭제를 취소하고 false를 반환한다.
    public bool RemoveItem(ItemInfo itemInfo)
    {
        foreach (var slot in _itemInfoList)
        {
            if (slot == itemInfo)
            {
                itemInfo = slot;
                break;
            }
        }
        if (itemInfo == null) return false;
            
        _itemHistroyList.Add(new ItemHistory(itemInfo, false));
        ApplyRemoveItem(itemInfo);


        if (_itemExistDictionary.ContainsKey(itemInfo.ItemData.ItemName))
            _itemExistDictionary[itemInfo.ItemData.ItemName] = false;

        _itemInfoList.Remove(itemInfo);

        return true;
    }
    public bool RemoveItem(ItemName itemName)
    {
        ItemInfo itemInfo = null;
        foreach (var slot in _itemInfoList)
        {
            if (slot.ItemData.ItemName == itemName)
            {
                itemInfo = slot;
                break;
            }
        }
        if (itemInfo == null) return false;

        _itemHistroyList.Add(new ItemHistory(itemInfo, false));
        ApplyRemoveItem(itemInfo);


        if (_itemExistDictionary.ContainsKey(itemInfo.ItemData.ItemName))
            _itemExistDictionary[itemInfo.ItemData.ItemName] = false;

        _itemInfoList.Remove(itemInfo);

        return true;
    }
    public bool RemoveItem(int index)
    {
        if (_itemInfoList.Count <= index) return false;
        ItemInfo itemInfo = _itemInfoList[index];
        _itemInfoList.RemoveAt(index);
        _itemHistroyList.Add(new ItemHistory(itemInfo, false));

        if (_itemExistDictionary.ContainsKey(itemInfo.ItemData.ItemName))
            _itemExistDictionary[itemInfo.ItemData.ItemName] = false;


        return true;
    }

    public List<ItemInfo> GetItemInfoList()
    {
        return _itemInfoList;
    }

    ItemData GetPossessRandomItem(Func<ItemData, bool> condition)
    {
        if (condition != null)
        {
            List<ItemInfo> list = _itemInfoList.Where(slot =>
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
            return _itemInfoList.GetRandom().ItemData;
        }

    }
    void ApplyAddItem(ItemInfo itemInfo)
    {
        ItemData itemData = itemInfo.ItemData;
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
                case ItemName.피뢰침_B:
                    _lightingRodCoolTime = Random.Range(15, 20);
                    break;
                case ItemName.피뢰침_A:
                    _lightingRodCoolTime = Random.Range(7, 10);
                    break;
                case ItemName.망각의서:
                    break;
                case ItemName.아르라제코인:
                    Managers.GetManager<GameManager>().EnableRestockCount++;
                    break;
                case ItemName.부서진약지:
                case ItemName.부서진약지_A:
                case ItemName.부서진약지_B:
                    IsActiveBrokenRingFinger = true;
                    break;
                case ItemName.에너지바:
                    Girl.IgnoreDamageCount += 3;
                    break;
                case ItemName.에너지바_A:
                    Girl.IgnoreDamageCount += 5;
                    break;
                case ItemName.화약:
                    _explosionDamagePercentage += 100f;
                    break;
                case ItemName.화약_A:
                    _explosionDamagePercentage += 200f;
                    break;
                case ItemName.화약_B:
                    _explosionDamagePercentage += 50f;
                    break;
            }
            if (itemData is StatusUpItemData data)
            {
                ApplyStatus(data);
            }
        }
    }

    void ApplyRemoveItem(ItemInfo itemInfo)
    {
        ItemData itemData = itemInfo.ItemData;
        if (itemData.ItemType == ItemType.StatusUp)
        {
            switch (itemData.ItemName)
            {
                case ItemName.아르라제코인:
                    Managers.GetManager<GameManager>().EnableRestockCount--;
                    break;
                case ItemName.부서진약지:
                case ItemName.부서진약지_A:
                case ItemName.부서진약지_B:
                    IsActiveBrokenRingFinger = false;
                    break;
                case ItemName.화약:
                    _explosionDamagePercentage -= 100f;
                    break;
                case ItemName.화약_A:
                    _explosionDamagePercentage -= 200f;
                    break;
                case ItemName.화약_B:
                    _explosionDamagePercentage -= 50f;
                    break;

            }
            if (itemData is StatusUpItemData data)
            {
                RevertStatus(data);
            }
        }
    }

    public void RemoveAllItem()
    {
        foreach (var item in _itemInfoList)
        {
            RemoveItem(item);
        }
    }
  
    public bool GetIsHaveItem(ItemName itemName)
    {
        if (!_itemExistDictionary.ContainsKey(itemName))
            return false;
        return _itemExistDictionary[itemName];
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
            creatureAI.CreatureAbility.IncreasedAttackPowerPercentage += statusUpItemData.IncreasingCreatureAttackPowerPercentage;
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
            creatureAI.CreatureAbility.IncreasedAttackPowerPercentage -= statusUpItemData.IncreasingCreatureAttackPowerPercentage;
        }
        Managers.GetManager<GameManager>().MentalAccelerationPercentage -= statusUpItemData.AccelMentalDownPercentage;
    }
}

[System.Serializable]
public class ItemInfo
{
    [field:SerializeField]public ItemData ItemData { set; get; }
    public ItemInfo(ItemData itemData)
    {
        ItemData = itemData;
    }
}

[System.Serializable]
public struct ItemHistory
{ 
    public ItemHistory(ItemInfo itemInfo,bool isGet)
    {
        this.itemInfo = itemInfo;
        this.isGet= isGet;
    }
    [field: SerializeField] public ItemInfo itemInfo { set; get; }
    [field: SerializeField] public bool isGet { set; get; }
}
