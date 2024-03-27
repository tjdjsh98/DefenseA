using System;
using System.Collections.Generic;
using UnityEngine;

public class VendingMechine : MonoBehaviour,IInteractable
{
    Player _player;
    Player Player { get {if(_player==null) _player = Managers.GetManager<GameManager>().Player;  return _player; } }

    [SerializeField] GameObject _bubble;

    List<ShopItem> _shopItemList = new List<ShopItem>();
    private void Awake()
    {
        if (Managers.GetManager<GameManager>().ShopItemDataList != null)
        {
            List<ShopItemData> datas;
            if (Managers.GetManager<GameManager>().ShopItemDataList.Count < 4)
            {
                datas = Managers.GetManager<GameManager>().ShopItemDataList.GetRandom(Managers.GetManager<GameManager>().ShopItemDataList.Count);
                
            }
            else
            {

                datas = Managers.GetManager<GameManager>().ShopItemDataList.GetRandom(4);
            }
            foreach (var data in datas)
            {
                _shopItemList.Add(new ShopItem() { isSale = false, shopItemData = data });
            }
        }
        HideBubble();
    }
   
    public void ShowBubble()
    {
        if(_bubble== null) return;

        _bubble.SetActive(true);
    }

    public void HideBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(false);
    }

    void OpenShop()
    {
        if (_bubble.gameObject.activeSelf)
        {
            Managers.GetManager<UIManager>().GetUI<UIShop>().Open(_shopItemList);
        }

    }

    public void Interact()
    {
        OpenShop();
    }
}
