using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
            Managers.GetManager<UIManager>().GetUI<UIShop>().Open(this);
        }

    }

    public void Interact()
    {
        OpenShop();
    }

    // È®·ü 0 : 60, 1 : 25, 2 : 13 ,3 :2
    public void RestockShopItems()
    {
        List<ItemData> datas = new List<ItemData>();
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

        for(int i = 0; i < 4; i++)
        {
            List<ItemData> itemList = Managers.GetManager<GameManager>().RankItemDataList[rank];
            if (itemList.Count == 0)
            {
                datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.µþ±âÄÉÀÌÅ©));
            }
            else
            {
                // Ã¹ Ä­Àº ¹«Á¶°Ç ¹«±â
                if (i == 0)
                {
                    ItemData weaponData = itemList.Where(data => { return data.ItemType == ItemType.Weapon; }).ToList().GetRandom();
                    if(weaponData != null)
                        datas.Add(weaponData);
                    else
                        datas.Add(Managers.GetManager<DataManager>().GetData<ItemData>((int)ItemName.µþ±âÄÉÀÌÅ©));
                }
                else
                {

                    datas.Add(itemList.GetRandom());
                }
            }
        }
        foreach (var data in datas)
        {
            ShopItemList.Add(new ShopItem() { isSale = false, shopItemData = data });
        }

    }
}
