using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMechine : MonoBehaviour,IInteractable,IShop
{
    Player _player;
    Player Player { get {if(_player==null) _player = Managers.GetManager<GameManager>().Player;  return _player; } }

    public List<ShopItem> ShopItemList { get; set; } = new List<ShopItem>();
    public int RestockCost { get; set; } = 5;
    [SerializeField] GameObject _bubble;

    public float RestockTime { get; set; } = 180;
    public float MaxRestockTime { get; set; } = 180;

    float rank0Probability = 80f;
    float rank1Probability = 15f;
    float rank2Probability = 5f;

    int _restockCount = 0;

    private void Awake()
    {
        RestockShopItems();
        HideBubble();
    }

    private void Update()
    {
        RestockTime -= Time.deltaTime;
        if(MaxRestockTime < RestockTime)
        {
            RestockTime = MaxRestockTime;
            RestockShopItems();
            RestockCost = 0;

            if (_restockCount == 0)     // 0 ��
            {
                rank0Probability = 85f;
                rank1Probability = 14f;
                rank2Probability = 1f;
            }
            if (_restockCount == 1)     // 3��
            {
                rank0Probability = 70f;
                rank1Probability = 25f;
                rank2Probability = 5f;
            }
            if (_restockCount == 2)     // 6��
            {
                rank0Probability = 50f;
                rank1Probability = 35f;
                rank2Probability = 10f;
            }
            if (_restockCount == 3)     // 9��
            {
                rank0Probability = 35f;
                rank1Probability = 35f;
                rank2Probability = 20f;
            }
            if(_restockCount == 4)      // 12��
            {
                rank0Probability = 10f;
                rank1Probability = 40f;
                rank2Probability = 30f;
            }
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

    // Ȯ�� 0 : 80, 1 : 15, 2 : 5 ,3 :0
    public void RestockShopItems()
    {
        List<ItemData> datas = new List<ItemData>();
       
        for(int i = 0; i < 8; i++)
        {
            int rank = 0;
            float randomValue = Random.Range(0, 100);
            if (randomValue < rank0Probability)
            {
                rank = 0;
            }
            else if (randomValue < rank0Probability + rank1Probability)
            {
                rank = 1;
            }
            else if (randomValue < rank0Probability + rank1Probability + rank2Probability)
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
                datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.��������ũ));
            }
            else
            {
                // ù ĭ�� ������ ����
                if (i == 0)
                {
                    ItemData weaponData = itemList.Where(data => { return data.ItemType == ItemType.Weapon; }).ToList().GetRandom();
                    if(weaponData != null)
                        datas.Add(weaponData);
                    else
                        datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.��������ũ));
                }
                // �������� ���Ⱑ �ƴ� ������
                else
                {

                    ItemData weaponData = itemList.Where(data => { return data.ItemType != ItemType.Weapon; }).ToList().GetRandom();
                    if (weaponData != null)
                        datas.Add(weaponData);
                    else
                        datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.��������ũ));
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
            if(Managers.GetManager<GameManager>().Inventory.GetItemCount(ItemName.�Ƹ���������) > 0)
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

            if (Managers.GetManager<GameManager>().Inventory.GetItemCount(ItemName.�Ƹ���������) > 0)
            {
                Managers.GetManager<GameManager>().Inventory.RemoveItem(ItemName.�Ƹ���������);
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