using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour, IInteractable
{
    Player _player;
    Player Player
    {
        get
        {
            if (_player == null)
                _player = Managers.GetManager<GameManager>().Player;

            return _player;
        }
    }
    [SerializeField] GameObject _bubble;

    private void Awake()
    {
        HideBubble();
    }
    public void ShowBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(true);
    }

    public void HideBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(false);
    }
    public void Interact()
    {
        Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
