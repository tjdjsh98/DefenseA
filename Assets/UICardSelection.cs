using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UICardSelection : UIBase
{
    [SerializeField] List<GameObject> _cardList;
    List<TextMeshProUGUI> _cardNameTextList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _cardDescriptionTextList = new List<TextMeshProUGUI>();

    List<CardSelectionData> _cardSelectionList = new List<CardSelectionData>();

    public override void Init()
    {
        Close();

        foreach (var card in _cardList)
        {
            _cardNameTextList.Add(card.transform.Find("Model").Find("CardName").GetComponent<TextMeshProUGUI>());
            _cardDescriptionTextList.Add(card.transform.Find("Model").Find("CardDescription").GetComponent<TextMeshProUGUI>());
        }



        _isInitDone = true;
    }
    public override void Open()
    {
        Time.timeScale = 0;
        Managers.GetManager<GameManager>().IsStopWave = true;
        _cardSelectionList.Clear();


        List<CardSelectionData> datas = Managers.GetManager<GameManager>().GetRandomCardSelectionData(3);
        foreach (var data in datas)
        {
            _cardSelectionList.Add(data);
        }
        Refresh();

        gameObject.SetActive(true);
    }

    public override void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Managers.GetManager<GameManager>().IsStopWave = false;
    }

    void Refresh()
    {
        for(int i =0; i < _cardNameTextList.Count; i++)
        {
            _cardNameTextList[i].text = _cardSelectionList[i].CardSelection.ToString(); 
            _cardDescriptionTextList[i].text = _cardSelectionList[i].CardDescription; ;
        }
    }

    public void SelectCard(int cardIndex)
    {
        Managers.GetManager<GameManager>().SelectCardData(_cardSelectionList[cardIndex]);
        Close();
    }
}
