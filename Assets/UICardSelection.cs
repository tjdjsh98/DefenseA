using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UICardSelection : UIBase
{
    [SerializeField] List<TextMeshProUGUI> _cardText;
    List<CardSelectionData> _cardSelectionList = new List<CardSelectionData>();

    public override void Init()
    {
        Close();
        _isInitDone = true;
    }
    public override void Open()
    {
        Time.timeScale = 0;
        Managers.GetManager<GameManager>().IsStopWave = true;
        _cardSelectionList.Clear();


        _cardSelectionList.Add(Managers.GetManager<GameManager>().GetRandomCardSelectionData());
        _cardSelectionList.Add(Managers.GetManager<GameManager>().GetRandomCardSelectionData());
        _cardSelectionList.Add(Managers.GetManager<GameManager>().GetRandomCardSelectionData());
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
        for(int i =0; i < _cardText.Count; i++)
        {
            _cardText[i].text = _cardSelectionList[i].CardSelection.ToString();
        }
    }

    public void SelectCard(int cardIndex)
    {
        Managers.GetManager<GameManager>().SelectCardData(_cardSelectionList[cardIndex]);
        Close();
    }
}
