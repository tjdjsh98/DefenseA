using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Item", menuName = "AddData/Create ItemData", order = 0)]
public class ItemData : ScriptableObject, ITypeDefine
{
    [field: SerializeField] public ItemName ItemName { set; get; }
    [field: SerializeField] public ItemType ItemType { set; get; }
    [field: SerializeField] public Sprite Image { set; get; }
    [field:SerializeField][field:TextArea] public string Description { set; get; }
    [field: SerializeField] public int Rank { set; get; }
    public int Price => Rank == 0 ? 10 : Rank == 1 ? 30 : Rank == 2 ? 100 : Rank == 3 ? 200 : 500;
    public int GetEnumToInt()
    {
        return (int)ItemName;
    }
}
public enum ItemType
{
    None = -1,      
    Ability,
    StatusUp,
    CreatureWeapon,
    Weapon,
    END
}

public enum ItemName
{
    None = -1,    
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
    니트로글리세린,
    깜짝박스,
    손전등,
    망각석,
    강렬한별빛,
    이쁜가게전단지,
    까마귀깃털,
    눈동자구슬,
    검은유리창파편,
    바늘과가죽,
    문들어진어금니,
    닿아가는고서,
    전봇대,
    망각의서,
    포크,
    나이프,
    살점상자,
    알사탕,
    초콜릿,
    아르라제코인,
    빛나는상자,
    유리건틀릿,
    피묻은뼈목걸이,
    눈알케이크,
    닿아야하는석상,
    탁한잎,
    급조된소총,
    급조된권총,
    경찰권총,
    돌격소총,
    리볼버,
    블랙,
    더블배럭샷건,
    펌프샷건,
    호출기,
    END
}