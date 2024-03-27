using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Create ShopItem", menuName = "AddData/Create StatusUpShopItemData", order = 3)]
public class StatusUpShopItemData : ShopItemData
{
    [field:SerializeField]public int IncreasingGirlMaxHp { set; get; }
    [field:SerializeField]public int IncreasingCreatureMaxHp { set; get; }
    [field:SerializeField]public int IncreasingWallMaxHp { set; get; }
    [field: SerializeField] public int IncreasingCreatureAttackPower { set; get; }
    [field: SerializeField] public int IncreasingWallAttackPower { set; get; }
    [field: SerializeField] public int RecoverGirlHpAmount { set; get; }
    [field: SerializeField] public int RecoverCreatureHpAmount { set; get; }
    [field: SerializeField] public int RecoverWallHpAmount { set; get; }

    [field: SerializeField] public float IncreasingGirlSpeed { set; get; }
    [field: SerializeField] public float IncreasingCreatureSpeed { set; get; }
    [field: SerializeField] public float IncreasingWallSpeed { set; get; }

    public void Apply()
    {
        Character girl = Managers.GetManager<GameManager>().Girl;
        Character creature = Managers.GetManager<GameManager>().Creature;
        Character wall = Managers.GetManager<GameManager>().Wall;

        if (girl)
        {
            girl.AddMaxHp(IncreasingGirlMaxHp);
            girl.Hp += RecoverGirlHpAmount;
            girl.SetSpeed(girl.Speed + IncreasingGirlSpeed);
        }

        if(creature)
        {
            creature.AddMaxHp(IncreasingCreatureMaxHp);
            creature.AttackPower += IncreasingCreatureAttackPower;
            creature.Hp  += RecoverCreatureHpAmount;
            creature.SetSpeed(creature.Speed + IncreasingCreatureSpeed);
        }

        if (wall)
        {
            wall.AddMaxHp(IncreasingWallMaxHp);
            wall.AttackPower += IncreasingWallAttackPower;
            wall.Hp += RecoverWallHpAmount;
            wall.SetSpeed(wall.Speed + IncreasingWallSpeed);
        }

    }
}
