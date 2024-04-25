
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIItemDescription : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _itemNameText;
    [SerializeField] TextMeshProUGUI _itemDescriptionText;

    bool _initDone  = false;
    private void Init()
    {
        _itemNameText = transform.Find("ItemNameText").GetComponent<TextMeshProUGUI>();
        _itemDescriptionText = transform.Find("DescriptionText").GetComponent<TextMeshProUGUI>();
        Hide();
        _initDone = true;
    }
    public void Show(Vector3 position, ItemData itemData)
    {
        if (!_initDone) Init();
        if (itemData == null) return;
        transform.position = position;
        _itemNameText.text = itemData.name;
        _itemDescriptionText.text = itemData.Description;
        gameObject.SetActive(true);
    }
    public void Show(Vector3 position, ItemName itemName)
    {
        if (!_initDone) Init();
        if (itemName == ItemName.None) return;
        ItemData itemData = Managers.GetManager<DataManager>().GetData<ItemData>((int)itemName);
        if (itemData == null) return;
        transform.position = position;
        _itemNameText.text = itemData.name;
        _itemDescriptionText.text = itemData.Description;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
