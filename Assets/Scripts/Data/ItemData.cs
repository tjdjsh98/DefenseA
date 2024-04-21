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
    [field: SerializeField] public bool IsUnique { set; get; }
    public int Price => Rank == 0 ? 20 : Rank == 1 ? 50 : Rank == 2 ? 80 : Rank == 3 ? 150 : 300;
    public int GetEnumToInt()
    {
        return (int)ItemName;
    }
}
public enum ItemType
{
    None = -1,      
    StatusUp,
    Weapon,
    END
}

public enum ItemName
{
    None = -1,    
    골라콜라,
    핫세븐,
    푸카라스웨트,
    검은세포,
    갈라진손가락,
    부서진건전지,
    퀵드로우,
    문들어진송곳니,
    과적재탄창,
    검붉은나무파편,
    손트리,
    피뢰침,
    눈동자구슬,
    검은유리창파편,
    바늘과가죽,
    비형성체,
    망각의서,
    닿아야하는석상,
    탁한잎,
    급조된소총,
    급조된권총,
    경찰권총,
    돌격소총,
    리볼버,
    더블배럭샷건,
    펌프샷건,
    아르라제코인,
    피묻은뼈목걸이,
    마지막탄환,
    보이지않는손,
    재활용탄,
    에너지바,
    부서진약지,
    과충전배터리,
    검은액체,
    작은송곳니,
    니트로글리세린,
    프로틴,
    END
}