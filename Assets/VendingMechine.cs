using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class VendingMechine : MonoBehaviour
{
    Player _player;
    Player Player { get {if(_player==null) _player = Managers.GetManager<GameManager>().Player;  return _player; } }

    [SerializeField] GameObject _bubble;

    List<ShopItem> _shopItemList = new List<ShopItem>();
    private void Awake()
    {
        Managers.GetManager<InputManager>().InteractKeyDownHandler += OpenShop;
        _shopItemList = Managers.GetManager<GameManager>().ShopItemList.GetRandom(5);

    }
    private void Update()
    {
        CheckPlayer();
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
