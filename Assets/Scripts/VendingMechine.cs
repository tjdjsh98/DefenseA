using System;
using System.Collections.Generic;
using UnityEngine;

public class VendingMechine : MonoBehaviour
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
            if (Managers.GetManager<GameManager>().ShopItemDataList.Count < 5)
            {
                datas = Managers.GetManager<GameManager>().ShopItemDataList.GetRandom(Managers.GetManager<GameManager>().ShopItemDataList.Count);
                
            }
            else
            {

                datas = Managers.GetManager<GameManager>().ShopItemDataList.GetRandom(5);
            }
            foreach (var data in datas)
            {
                _shopItemList.Add(new ShopItem() { isSale = false, shopItemData = data });
            }
        }
    }
    private void Update()
    {
        CheckPlayer();
    }

    private void OnEnable()
    {
        Managers.GetManager<InputManager>().InteractKeyDownHandler += OpenShop;

    }
    private void OnDisable()
    {
        if(Managers.GetManager<InputManager>())
            Managers.GetManager<InputManager>().InteractKeyDownHandler -= OpenShop;
    }

    public void CheckPlayer()
    {
        if(Player == null) return;

        if ((Player.transform.position - transform.position).magnitude < 5)
        {
            ShowBubble();
        }
        else
        {
            HideBubble();
        }

    }
     void ShowBubble()
    {
        if(_bubble== null) return;

        _bubble.SetActive(true);
    }

    void HideBubble()
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
}
