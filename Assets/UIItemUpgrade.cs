using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemUpgrade : UIBase
{
    List<GameObject> _itemSlot = new List<GameObject>();

    public override void Init()
    {
        gameObject.SetActive(false);
    }

    public override void Open(bool except = false)
    {
        if(!except)
            Managers.GetManager<UIManager>().Open(this);

        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown
            ;
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);


        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    void Refresh()
    {
        //Dictionary<ItemName,int> itemDic = Managers.GetManager<GameManager>().Inventory.GetItemList();
    }

    void OnUIMouseDown(List<GameObject> list)
    {

    }
}
