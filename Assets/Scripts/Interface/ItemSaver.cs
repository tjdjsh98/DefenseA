using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSaver : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject _bubble;
    private void Awake()
    {
        HideBubble();
    }
    public void HideBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(false);
    }

    public void Interact()
    {
        if (_bubble.gameObject.activeSelf)
        {
            Managers.GetManager<UIManager>().GetUI<UIInventorySaver>().Open(this);
        }
    }

    public void ShowBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(true);
    }
}