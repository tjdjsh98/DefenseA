using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class UICardSelection : UIBase
{
    [SerializeField] List<GameObject> _cardList;
    [SerializeField] List<MMF_Player> _playerList;
    [SerializeField] GameObject _rerollButton;

    List<Card> _cashDatas;

    int _cardCount;

    List<TextMeshProUGUI> _cardNameTextList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _cardDescriptionTextList = new List<TextMeshProUGUI>();
    List<Image> _cardBloodImageList = new List<Image>();
    List<float> _cardBloodDryValueList = new List<float>();
    List<bool> _cardBloodIsHoverList = new List<bool>();
    List<Card> _cardSelectionList = new List<Card>();

    static int _dryValueID = Shader.PropertyToID("_DryValue");

    bool _isSelectingReplaceSkill;
    int _replaceSkillCardIndex;
    [SerializeField] TextMeshProUGUI _skillChangeText = new TextMeshProUGUI();
    [SerializeField] List<GameObject> _skillSlotList = new List<GameObject>();
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
            _cardBloodImageList[_cardBloodImageList.Count - 1].material = new Material(_cardBloodImageList[_cardBloodImageList.Count - 1].material);
        }
        _cardCount = _cardList.Count;

        _isInitDone = true;
    }

    private void Update()
    {
        for (int i = 0; i < _cardBloodDryValueList.Count; i++)
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
                    if (color.a > 0)
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
        for (int i = 0; i < _cardBloodIsHoverList.Count; i++)
        {
            _cardBloodIsHoverList[i] = false;
        }
    }
    public override void Open(bool except = false)
    {
        if(!except)
            Managers.GetManager<UIManager>().Open(this);
        Time.timeScale = 0;
        _cardSelectionList.Clear();


        _cashDatas = Managers.GetManager<CardManager>().GetRemainCardSelection().ToList();

        for (int i = 0; i < _cardCount; i++)
        {
            if (_cashDatas.Count > 0)
            {
                int random = Random.Range(0, _cashDatas.Count);
                _cardSelectionList.Add(_cashDatas[random]);
                _cashDatas.RemoveAt(random);
            }
        }
        Refresh();
        if (_cashDatas.Count <= 0)
        {
            Close();
            return;
        }
        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;

        gameObject.SetActive(true);
        StartCoroutine(CorOpenCards());
    }

    public void OpenSkillCardSelection()
    {
        Time.timeScale = 0;
        _cardSelectionList.Clear();


        _cashDatas = Managers.GetManager<CardManager>().GetRemainCardSelection().Where(data =>
        {
            return data.cardData.IsActiveAbility;
        }).ToList();



        for (int i = 0; i < _cardCount; i++)
        {
            if (_cashDatas.Count > 0)
            {
                int random = Random.Range(0, _cashDatas.Count);
                _cardSelectionList.Add(_cashDatas[random]);
                _cashDatas.RemoveAt(random);
            }
        }
        Refresh();
        if (_cashDatas.Count <= 0)
        {
            Close();
            return;
        }

        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;

        gameObject.SetActive(true);
        StartCoroutine(CorOpenCards());
    }
    public void OpenNpcSelection()
    {
        Time.timeScale = 0;
        _cardSelectionList.Clear();


        _cashDatas = Managers.GetManager<CardManager>().GetRemainCardSelection().Where(data =>
        {
            return !data.cardData.IsActiveAbility;
        }).ToList();
            
        for (int i = 0; i < _cardCount; i++)
        {
            if (_cashDatas.Count > 0)
            {
                int random = Random.Range(0, _cashDatas.Count);
                _cardSelectionList.Add(_cashDatas[random]);
                _cashDatas.RemoveAt(random);
            }
        }

        if (_cashDatas.Count <= 0)
        {
            Close();
            return;
        }
        Refresh();


        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;
        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown;

        gameObject.SetActive(true);
        StartCoroutine(CorOpenCards());
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Managers.GetManager<InputManager>().UIMouseDownHandler -= OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler -= OnUIMouseHover;
        _skillChangeText.gameObject.SetActive(false);

        foreach (var card in _cardList)
            card.transform.Find("Model").localScale = new Vector3(-1, 1, 1);

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    void Refresh()
    {
        for (int i = 0; i < _cardNameTextList.Count; i++)
        {
            _cardNameTextList[i].text = _cardSelectionList[i].cardData.CardName.ToString(); 

            int cardRank = _cardSelectionList[i].rank;

            float value = 0;
            if(_cardSelectionList[i].cardData.PropertyList.Count == 0)
            {
                value = 0;
            }
            else if (_cardSelectionList[i].cardData.PropertyList.Count > cardRank + 1)
            {
                value = _cardSelectionList[i].cardData.PropertyList[cardRank + 1];
            }
            else 
            {
                value = _cardSelectionList[i].cardData.PropertyList[_cardSelectionList[i].cardData.PropertyList.Count-1];
            }

            float value2 = 0;
            if (_cardSelectionList[i].cardData.Property2List.Count == 0)
            {
                value2 = 0;
            }
            else if (_cardSelectionList[i].cardData.Property2List.Count > cardRank + 1)
            {
                value2 = _cardSelectionList[i].cardData.Property2List[cardRank + 1];
            }
            else
            {
                value2 = _cardSelectionList[i].cardData.Property2List[_cardSelectionList[i].cardData.Property2List.Count - 1];
            }
            float coolTime = 0;
            if (_cardSelectionList[i].cardData.Property2List.Count == 0)
            {
                coolTime = 0;
            }
            else if (_cardSelectionList[i].cardData.coolTimeList.Count > cardRank + 1)
            {
                coolTime = _cardSelectionList[i].cardData.coolTimeList[cardRank + 1];
            }
            else
            {
                coolTime = _cardSelectionList[i].cardData.coolTimeList[_cardSelectionList[i].cardData.coolTimeList.Count - 1];
            }
            string cardDescription = $"랭크 : {cardRank + 1}\n\n";
            cardDescription += string.Format(_cardSelectionList[i].cardData.CardDescription,value, value2);
            cardDescription += $"\n\n쿨타임 : {coolTime}";
            _cardDescriptionTextList[i].text = cardDescription;
        }
    }

    // 카드와 리롤 버튼을 숨깁니다.
    void HideCards()
    {
        foreach(var card in _cardList)
        {
            card.gameObject.SetActive(false);
        }
        _rerollButton.SetActive(false);
    }
    void ShowCards()
    {
        foreach (var card in _cardList)
        {
            card.gameObject.SetActive(true);
        }
        _rerollButton.SetActive(true);
    }
    public void SelectCard(int cardIndex)
    {
        if (_isSelectingReplaceSkill) return;

        // 카드가 액티브 카드이고
        // 이미 존재한 카드가 아니고
        // 카드 슬롯이 모두 차 있다면 카드 슬롯 중 하나를 교체
        if (_cardSelectionList[cardIndex].cardData.IsActiveAbility)
        {
            bool _isExist = false;
            for (int i = 0; i < 4; i++)
            {
                if (Managers.GetManager<CardManager>().GetSkillSlot(i).card == _cardSelectionList[cardIndex])
                {
                    _isExist = true;
                    break;
                }
            }
            if (!_isExist && Managers.GetManager<CardManager>().GetSkillSlot(3).card != null)
            {
                _isSelectingReplaceSkill = true;
                _skillChangeText.gameObject.SetActive(true);
                _replaceSkillCardIndex = cardIndex;
                HideCards();
                return;
            }
            else
            {
                Managers.GetManager<CardManager>().AddCard(_cardSelectionList[cardIndex]);
            }
        }
        // 카드 슬롯이 비어 있다면 즉시 채운다.
        else
        {
            Managers.GetManager<CardManager>().AddCard(_cardSelectionList[cardIndex]);
        }

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

    private void OnUIMouseDown(List<GameObject> list)
    {
        if (!_isSelectingReplaceSkill) return;

        bool isReplace = false;
        for(int i = 0; i < _skillSlotList.Count; i++)
        {
            if (list.Contains(_skillSlotList[i]))
            {
                Managers.GetManager<CardManager>().RemoveSkill(i);
                Managers.GetManager<CardManager>().AddCard(_cardSelectionList[_replaceSkillCardIndex]);
                isReplace = true;
                break;
            }
        }

        _isSelectingReplaceSkill = false;
        _skillChangeText.gameObject.SetActive(false);
        _replaceSkillCardIndex = -1;
        ShowCards();
        if (isReplace)
        {
            Close();
        }
    }
    private void OnUIMouseHover(List<GameObject> list)
    {
        foreach (GameObject go in list)
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

    IEnumerator CorOpenCards()
    {
        for (int i = 0; i < _playerList.Count; i++)
        {
            _playerList[i].PlayFeedbacks();

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    public void TranslateCardDescription(CardData cardData, ref string description)
    {
        if (description != null)
        {
           
        }
    }
}