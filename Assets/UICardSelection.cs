using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UICardSelection : UIBase
{
    [SerializeField] List<GameObject> _cardList;

    List<CardSelectionData> _cashDatas;

    int _cardCount;

    List <TextMeshProUGUI> _cardNameTextList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _cardDescriptionTextList = new List<TextMeshProUGUI>();
    List<Image> _cardBloodImageList = new List<Image>();
    List<float> _cardBloodDryValueList = new List<float>();
    List<bool> _cardBloodIsHoverList = new List<bool>();
    List<CardSelectionData> _cardSelectionList = new List<CardSelectionData>();

    static int _dryValueID = Shader.PropertyToID("_DryValue");

    public override void Init()
    {
        Close();

        foreach (var card in _cardList)
        {
            _cardNameTextList.Add(card.transform.Find("Model").Find("Front").Find("CardName").GetComponent<TextMeshProUGUI>());
            _cardDescriptionTextList.Add(card.transform.Find("Model").Find("Front").Find("CardDescription").GetComponent<TextMeshProUGUI>());
            _cardBloodDryValueList.Add(1.5f);
            _cardBloodIsHoverList.Add(false);
            _cardBloodImageList.Add(card.transform.Find("Model").Find("Front").Find("Blood").GetComponent<Image>());
            _cardBloodImageList[_cardBloodImageList.Count-1].material = new Material(_cardBloodImageList[_cardBloodImageList.Count-1].material);
        }
        _cardCount = _cardList.Count;

        _isInitDone = true;
    }

    private void Update()
    {
        for(int i =0; i < _cardBloodDryValueList.Count; i++)
        {
            if (_cardBloodIsHoverList[i] == true)
            {
                if (_cardBloodDryValueList[i] > -1)
                {
                    if (_cardBloodDryValueList[i] - Time.unscaledDeltaTime * 3 < -1)
                        _cardBloodDryValueList[i] = -1;
                    else
                        _cardBloodDryValueList[i] -= Time.unscaledDeltaTime * 3;

                    Color color = _cardNameTextList[i].color;
                    if(color.a > 0)
                    {
                        color.a -= Time.unscaledDeltaTime * 3;
                        _cardNameTextList[i].color = color;
                    }
                }
                _cardBloodImageList[i].material.SetFloat(_dryValueID, _cardBloodDryValueList[i]);
            }
            else
            {
                if (_cardBloodDryValueList[i] < 1.5f)
                {
                    if (_cardBloodDryValueList[i] + Time.unscaledDeltaTime * 3 > 1.5f)
                        _cardBloodDryValueList[i] = 1.5f;
                    else
                        _cardBloodDryValueList[i] += Time.unscaledDeltaTime * 3;

                    Color color = _cardNameTextList[i].color;
                    if (color.a < 1)
                    {
                        color.a += Time.unscaledDeltaTime * 3;
                        _cardNameTextList[i].color = color;
                    }
                }
                _cardBloodImageList[i].material.SetFloat(_dryValueID, _cardBloodDryValueList[i]);
            }
        }
        for(int i =0; i < _cardBloodIsHoverList.Count; i++)
        {
            _cardBloodIsHoverList[i] = false;
        }
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

        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;

        gameObject.SetActive(true);
    }



    public override void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Managers.GetManager<InputManager>().UIMouseHoverHandler -= OnUIMouseHover;
        Managers.GetManager<GameManager>().IsStopWave = false;

        foreach (var card in _cardList)
        {
            card.transform.Find("Model").localScale = new Vector3(-1, 1, 1);
        }
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

    private void OnUIMouseHover(List<GameObject> list)
    {
        foreach(GameObject go in list)
        {
            for (int i = 0; i < _cardBloodImageList.Count; i++)
            {
                if (_cardBloodImageList[i].gameObject == go)
                {
                    _cardBloodIsHoverList[i] = true;

                }
            }
        }
    }
}