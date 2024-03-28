using System;
using System.Collections.Generic;
using UnityEngine;

public class VendingMechine : MonoBehaviour,IInteractable,IShop
{
    Player _player;
    Player Player { get {if(_player==null) _player = Managers.GetManager<GameManager>().Player;  return _player; } }

    public List<ShopItem> ShopItemList { get; set; } = new List<ShopItem>();
    public int RestockCost { get; set; } = 10;

    [SerializeField] GameObject _bubble;

    private void Awake()
    {
        RestockShopItems();
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
            Managers.GetManager<UIManager>().GetUI<UIShop>().Open(this);
        }

    }

    public void Interact()
    {
        OpenShop();
    }

    public void RestockShopItems()
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
            ShopItemList.Clear();
            foreach (var data in datas)
            {
                ShopItemList.Add(new ShopItem() { isSale = false, shopItemData = data });
            }
        }
    }
}
