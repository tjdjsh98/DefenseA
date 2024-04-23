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
    골라콜라_A,
    골라콜라_B,
    핫세븐,
    핫세븐_A,
    핫세븐_B,
    푸카라스웨트,
    푸카라스웨트_A,
    푸카라스웨트_B,
    검은세포,
    검은세포_A,
    검은세포_B,
    갈라진손가락,
    갈라진손가락_A,
    갈라진손가락_B,
    부서진건전지,
    부서진건전지_A,
    부서진건전지_B,
    퀵드로우,
    퀵드로우_A,
    퀵드로우_B,
    문들어진송곳니,
    문들어진송곳니_A,
    문들어진송곳니_B,
    과적재탄창,
    과적재탄창_A,
    과적재탄창_B,
    검붉은나무파편,
    검붉은나무파편_A,
    검붉은나무파편_B,
    손트리,
    손트리_A,
    손트리_B,
    피뢰침,
    피뢰침_A,
    피뢰침_B,
    눈동자구슬,
    눈동자구슬_A,
    눈동자구슬_B,
    검은유리창파편,
    검은유리창파편_A,
    검은유리창파편_B,
    바늘과가죽,
    바늘과가죽_A,
    바늘과가죽_B,
    망각의서,
    망각의서_A,
    망각의서_B,
    닿아야하는석상,
    닿아야하는석상_A,
    닿아야하는석상_B,
    탁한잎,
    탁한잎_A,
    탁한잎_B,
    급조된소총,
    급조된권총,
    경찰권총,
    돌격소총,
    리볼버,
    더블배럭샷건,
    펌프샷건,
    아르라제코인,
    아르라제코인_A,
    아르라제코인_B,
    피묻은뼈목걸이,
    피묻은뼈목걸이_A,
    피묻은뼈목걸이_B,
    마지막탄환,
    마지막탄환_A,
    마지막탄환_B,
    보이지않는손,
    보이지않는손_A,
    보이지않는손_B,
    재활용탄,
    재활용탄_A,
    재활용탄_B,
    에너지바,
    에너지바_A,
    에너지바_B,
    부서진약지,
    부서진약지_A,
    부서진약지_B,
    과충전배터리,
    과충전배터리_A,
    과충전배터리_B,
    검은액체,
    검은액체_A,
    검은액체_B,
    작은송곳니,
    작은송곳니_A,
    작은송곳니_B,
    니트로글리세린,
    니트로글리세린_A,
    니트로글리세린_B,
    프로틴,
    프로틴_A,
    프로틴_B,
    화약,
    화약_A,
    화약_B,
    미니대포,
    미니대포_A,
    미니대포_B,
    END
}