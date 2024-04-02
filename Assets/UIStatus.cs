using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor.Hardware;
using UnityEngine;
using UnityEngine.UI;

public class UIStatus : UIBase
{
    Character _girl;
    Player _player;
    Character _creature;
    CreatureAI _creatureAI;

    StringBuilder _stringBuilder;

    [SerializeField] TextMeshProUGUI _titleTextMesh;
    [SerializeField] TextMeshProUGUI _descriptionTextMesh;
    [SerializeField] TextMeshProUGUI _abilityTextMesh;

    [SerializeField] TextMeshProUGUI _equipment1ButtonTextMesh;
    [SerializeField] TextMeshProUGUI _equipment2ButtonTextMesh;
    [SerializeField] TextMeshProUGUI _equipment3ButtonTextMesh;

    [SerializeField] TextMeshProUGUI _equipmentTitleTextMesh;
    [SerializeField] TextMeshProUGUI _equipmentDescriptionTextMesh;

    [SerializeField] Button _equipment1Button;
    [SerializeField] Button _equipment2Button;
    [SerializeField] Button _equipment3Button;

    #region 무기 장비 변수
    GameObject _weaponFolder;
    WeaponUI _weaponUI = new WeaponUI();
    
    #endregion

    public override void Init()
    {
        _stringBuilder = new StringBuilder();

        InitWeaponEquipment();
        gameObject.SetActive(false);
    }


    void InitWeaponEquipment()
    {
        _weaponFolder = transform.Find("Equipment").Find("WeaponEquipment").gameObject;
        _weaponUI.InitWeaponUI(_weaponFolder);
    }

    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);


        Time.timeScale = 0;

        GameManager manager = Managers.GetManager<GameManager>();
        _girl = manager.Girl;
        _player = manager.Player;
        _creature = manager.Creature;
        _creatureAI = manager.CreatureAI;

        OpenGirlPage();

        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        gameObject.SetActive(false);

        Time.timeScale = 1;

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    public void OpenGirlPage()
    {
        _titleTextMesh.text = "소녀";
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"체력 : {_girl.Hp}/{_girl.MaxHp}");
        _stringBuilder.AppendLine($"공격력 : {_girl.AttackPower}");
        _stringBuilder.AppendLine($"체력재생력 : {_girl.IncreasedHpRegeneration}");
        _descriptionTextMesh.text = _stringBuilder.ToString();

        GirlAbility girlAbility = _player.GirlAbility;
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"보유 능력");
        foreach (var card in Managers.GetManager<CardManager>().GetPossessCardList())
        {
            _stringBuilder.AppendLine($"{card.cardData.CardName}");
        }
        _abilityTextMesh.text = _stringBuilder.ToString();

        _equipment1ButtonTextMesh.text = "메인무기";
        _equipment2ButtonTextMesh.text = "보조무기";
        _equipment3ButtonTextMesh.text = "특수무기";

        _equipment1Button.onClick.RemoveAllListeners();
        _equipment1Button.onClick.AddListener(OpenMainWeaponPage);
        _equipment2Button.onClick.RemoveAllListeners();
        _equipment2Button.onClick.AddListener(OpenSubWeaponPage);
        _equipment3Button.onClick.RemoveAllListeners();
        _equipment3Button.onClick.AddListener(OpenSpecialWeaponPage);
    }

    public void OpenCreaturePage()
    {
        _titleTextMesh.text = "괴물";
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"체력 : {_creature.Hp}/{_creature.MaxHp}");
        _stringBuilder.AppendLine($"공격력 : {_creature.AttackPower}");
        _stringBuilder.AppendLine($"체력재생력 : {_creature.IncreasedHpRegeneration}");
        _descriptionTextMesh.text = _stringBuilder.ToString();


        CreatureAbility creatureAbility = _creatureAI.CreatureAbility;
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"보유 능력");
        foreach (var card in Managers.GetManager<CardManager>().GetPossessCardList())
        {
            _stringBuilder.AppendLine($"{card.cardData.CardName}");
        }
        _abilityTextMesh.text = _stringBuilder.ToString();
    }

    public void OpenMainWeaponPage()
    {
        Weapon weapon = _player.WeaponSwaper.GetWeapon(0);
        {
            _weaponFolder.gameObject.SetActive(false);
            if (weapon != null)
            {
                _weaponUI.Refresh(weapon);
                StartCoroutine(_weaponUI.CorFillGauge(weapon));

                _weaponFolder.gameObject.SetActive(true);
            }
        }
    }
    public void OpenSubWeaponPage()
    {
        Weapon weapon = _player.WeaponSwaper.GetWeapon(1);
        {
            _weaponFolder.gameObject.SetActive(false);
            if (weapon != null)
            {
                _weaponUI.Refresh(weapon);
                StartCoroutine(_weaponUI.CorFillGauge(weapon));

                _weaponFolder.gameObject.SetActive(true);
            }
        }
    }
    public void OpenSpecialWeaponPage()
    {
        Weapon weapon = _player.WeaponSwaper.GetWeapon(2);
        {
            _weaponFolder.gameObject.SetActive(false);
            if (weapon != null)
            {
                _weaponUI.Refresh(weapon);
                StartCoroutine(_weaponUI.CorFillGauge(weapon));

                _weaponFolder.gameObject.SetActive(true);
            }
        }
    }
}