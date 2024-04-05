using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
            Refresh();
            Managers.GetManager<UIManager>().GetUI<UIShop>().Open(this);
        }

    }

    public void Interact()
    {
        OpenShop();
    }

    // 확률 0 : 60, 1 : 25, 2 : 13 ,3 :2
    public void RestockShopItems()
    {
        List<ItemData> datas = new List<ItemData>();
       
        for(int i = 0; i < 4; i++)
        {
            int rank = 0;
            float randomValue = Random.Range(0, 100);
            if (randomValue < 60)
            {
                rank = 0;
            }
            else if (randomValue < 85)
            {
                rank = 1;
            }
            else if (randomValue < 98)
            {
                rank = 2;
            }
            else
            {
                rank = 3;
            }

            List<ItemData> itemList = Managers.GetManager<GameManager>().RankItemDataList[rank];
            if (itemList.Count == 0)
            {
                datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.딸기케이크));
            }
            else
            {
                // 첫 칸은 무조건 무기
                if (i == 0)
                {
                    ItemData weaponData = itemList.Where(data => { return data.ItemType == ItemType.Weapon; }).ToList().GetRandom();
                    if(weaponData != null)
                        datas.Add(weaponData);
                    else
                        datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.딸기케이크));
                }
                else
                {

                    datas.Add(itemList.GetRandom());
                }
            }
        }
        ShopItemList.Clear();
        foreach (var data in datas)
        {
            ShopItemList.Add(new ShopItem() { isSaled = false, shopItemData = data });
        }
        Refresh();

    }

    public void Refresh()
    {
        foreach(var item in ShopItemList)
        {
            if(Managers.GetManager<GameManager>().Inventory.GetItemCount(ItemName.아르라제코인) > 0)
            {
                item.Price = 0;
            }
            else
            {
                item.Price = item.shopItemData.Price;
            }
        }
    }

    public void SellItem(ShopItem item)
    {
        if (item.isSaled) return;
        if (item.shopItemData == null) return;

        GameManager gameManager = Managers.GetManager<GameManager>();

        if (item.Price <= gameManager.Money)
        {
            gameManager.Money -= item.Price;
            item.isSaled = true; 

            Managers.GetManager<GameManager>().Inventory.AddItem(item.shopItemData);

            if (Managers.GetManager<GameManager>().Inventory.GetItemCount(ItemName.아르라제코인) > 0)
            {
                Managers.GetManager<GameManager>().Inventory.RemoveItem(ItemName.아르라제코인);
            }

            Refresh();
        }
    }
}
[System.Serializable]
public class ShopItem
{
    public ItemData shopItemData;
    public bool isSaled;
    public int Price { get; set; }
    public ItemType sellType => shopItemData.ItemType;
}