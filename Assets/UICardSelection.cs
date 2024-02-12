using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UICardSelection : UIBase
{
    [SerializeField] List<GameObject> _cardList;

    List<CardSelectionData> _cashDatas;

    int _cardCount;

    List <TextMeshProUGUI> _cardNameTextList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _cardDescriptionTextList = new List<TextMeshProUGUI>();
    List<CardSelectionData> _cardSelectionList = new List<CardSelectionData>();

    public override void Init()
    {
        Close();

        foreach (var card in _cardList)
        {
            _cardNameTextList.Add(card.transform.Find("Model").Find("Front").Find("CardName").GetComponent<TextMeshProUGUI>());
            _cardDescriptionTextList.Add(card.transform.Find("Model").Find("Front").Find("CardDescription").GetComponent<TextMeshProUGUI>());
        }

        _cardCount = _cardList.Count;

        _isInitDone = true;
    }
    public override void Open()
    {
        Time.timeScale = 0;
        Managers.GetManager<GameManager>().IsStopWave = true;
        _cardSelectionList.Clear();


        _cashDatas = Managers.GetManager<GameManager>().GetRemainCardSelection().ToList();

        for(int i =0; i < _cardCount; i++)
        {
            int random = Random.Range(0, _cashDatas.Count);
            _cardSelectionList.Add(_cashDatas[random]);
            _cashDatas.RemoveAt(random);
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

        Debug.Log(cardIndex + " " + _cardSelectionList[cardIndex].CardSelection);
        Close();
    }

    public void Reroll()
    {
        if (_cashDatas.Count < _cardCount) return;

        _cardSelectionList.Clear();
          for (int i = 0; i < _cardCount; i++)
        {
            int random = Random.Range(0, _cashDatas.Count);
            _cardSelectionList.Add(_cashDatas[random]);
            _cashDatas.RemoveAt(random);
        }
        Refresh();
    }
}
