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
    public int Price => Rank == 0 ? 20 : Rank == 1 ? 50 : Rank == 2 ? 100 : Rank == 3 ? 200 : 400;
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
    손트리,
    뜯어진얼굴가죽,
    피뢰침,
    구름솜사탕,
    어둠구체의1번째파편,
    망각석,
    강렬한별빛,
    까마귀깃털,
    눈동자구슬,
    검은유리창파편,
    바늘과가죽,
    문들어진어금니,
    망각의서,
    포크,
    나이프,
    알사탕,
    초콜릿,
    아르라제코인,
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
    에너지바,
    부서진약지,
    곁눈증표,
    과충전배터리,
    두부통조림,
    바나나스테이크,
    김치파스타,
    민트초코,
    베어물린나무,
    베어물린돌맹이,
    검은액체,
    검은가루,
    작은송곳니,
    END
}