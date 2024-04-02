using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create ShopItem", menuName = "AddData/Create ShopItemData", order = 0)]
public class ShopItemData : ScriptableObject
{
    [field: SerializeField] public ShopItemName ShopItemName { set; get; }
    [field: SerializeField] public SellType SellType { set; get; }
    [field: SerializeField] public Sprite Image { set; get; }
    [field:SerializeField][field:TextArea] public string Description { set; get; }
    [field: SerializeField] public int Rank { set; get; }
    [field: SerializeField] public int Price => Rank == 0 ? 10 : Rank == 1 ? 30 : Rank == 2 ? 100 : Rank == 3 ? 200 : 500;
}
public enum SellType
{
    Weapon,
    UpgradeWeapon,
    Ability,
    StatusUp,
}

public enum ShopItemName
{
    None = -1,
    Ak47,
    Black,
    ExplosionGun,
    ImprovisedRifle,
    PolicePistal,
    �Ϲݴɷ�ī��,
    ��޴ɷ�ī��,
    ����ݶ�,
    �ּ���,
    Ǫī�󽺿�Ʈ,
    ��������ũ,
    ��������,
    ������Ű,
    ������,
    �������͸�,
    ��������۰���,
    �׹�°��,
    ��ұ�ü��4��°����,
    �˺�����������,
    ���Ժ���������,
    ���Ժ���������,
    ��ο�����,
    ��Ʈ��,
    ������󱼰���,
    �Ƿ�ħ,
    �����ػ���,
    ��ұ�ü��1��°����,
    PumpShotgun,
    END
}