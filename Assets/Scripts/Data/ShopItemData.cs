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
    일반능력카드,
    상급능력카드,
    골라콜라,
    핫세븐,
    푸카라스웨트,
    딸기케이크,
    검은세포,
    초코쿠키,
    건전지,
    보조배터리,
    문들어진송곳니,
    네번째손,
    어둠구체의4번째파편,
    검붉은나무파편,
    낮게보는페이지,
    높게보는페이지,
    어두울전구,
    손트리,
    뜯어진얼굴가죽,
    피뢰침,
    구름솜사탕,
    어둠구체의1번째파편,
    PumpShotgun,
    END
}