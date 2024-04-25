using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMechine : MonoBehaviour,IInteractable,IShop
{
    Player _player;
    Player Player { get {if(_player==null) _player = Managers.GetManager<GameManager>().Player;  return _player; } }

    public List<ShopItem> ShopItemList { get; set; } = new List<ShopItem>();
    public int RestockCost { get; set; } = 5;
    public int EnableRestockCount { get; set; }
    public int RestockCount { get; set; }

    [SerializeField] GameObject _bubble;


    [SerializeField]float rank0Probability = 80f;
    [SerializeField] float rank1Probability = 15f;
    [SerializeField] float rank2Probability = 5f;

    int _freeBuyCount = 0;


    private void Awake()
    {
        RestockShopItems();
        HideBubble();

        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.아르라제코인))
        {
            _freeBuyCount = 1;
        }
        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.아르라제코인_B))
        {
            _freeBuyCount = 2;
        }
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

    // 확률 0 : 80, 1 : 15, 2 : 5 ,3 :0
    public void RestockShopItems()
    {
        List<ItemData> itemList = null;
        List<ItemData> itemList0 = Managers.GetManager<GameManager>().RankItemDataList[0].ToList();
        List<ItemData> itemList1 = Managers.GetManager<GameManager>().RankItemDataList[1].ToList();
        List<ItemData> itemList2 = Managers.GetManager<GameManager>().RankItemDataList[2].ToList();
        List<ItemData> itemList3 = Managers.GetManager<GameManager>().RankItemDataList[3].ToList();

        List<ItemInfo> itemInfos = Managers.GetManager<GameManager>().Inventory.GetItemInfoList();

        foreach (var item in itemInfos)
        {
            itemList0.Remove(item.ItemData);
            itemList1.Remove(item.ItemData);
            itemList2.Remove(item.ItemData);
            itemList3.Remove(item.ItemData);
        }

        for (int i = 0; i < 5; i++)
        {
            if (ShopItemList.Count > i && ShopItemList[i].isSaled) continue;

            ItemData itemData = null;

            float randomValue = Random.Range(0, 100);
            if (randomValue < rank0Probability)
            {
                itemList = itemList0;
            }
            else if (randomValue < rank0Probability + rank1Probability)
            {
                itemList = itemList1;
            }
            else if (randomValue < rank0Probability + rank1Probability + rank2Probability)
            {
                itemList = itemList2;
            }
            else
            {
                itemList = itemList3;
            }

            if (itemList.Count == 0)
            {
                itemData = Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.푸카라스웨트);
            }
            else
            {
                // 첫 칸은 무조건 무기
                if (i == 0)
                {
                    itemData = itemList.Where(data => { return data.ItemType == ItemType.Weapon; }).ToList().GetRandom();
                    if (itemData != null)
                        itemList.Remove(itemData);
                    else
                        itemData = Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.푸카라스웨트);

                }
                // 나머지는 무기가 아닌 것으로
                else
                {

                    itemData = itemList.Where(data => { return data.ItemType != ItemType.Weapon; }).ToList().GetRandom();
                    if (itemData != null)
                    {
                        itemList.Remove(itemData);
                    }
                    else
                        itemData = Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.푸카라스웨트);
                }
            }
            if (ShopItemList.Count <= i)
            {
                ShopItemList.Add(null);
            }
            ShopItemList[i] = new ShopItem() { isSaled = false, shopItemData = itemData };
        }
        Refresh();

    }

    public void Refresh()
    {
        foreach(var item in ShopItemList)
        {
            if (_freeBuyCount > 0)
            {
                item.Price = 0;
            }
            else
            {
                Card card = Managers.GetManager<CardManager>().GetCard(CardName.할인판매);
                if (card != null)
                    item.Price = Mathf.RoundToInt(item.shopItemData.Price * (100 - (card.rank + 1) * 5) / 100f);
                else
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

            if (_freeBuyCount > 0)
            {
                _freeBuyCount--;
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