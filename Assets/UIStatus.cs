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
    Character _wall;
    WallAI _wallAI;

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
    Image _weaponImage;
    TextMeshProUGUI _weaponName;

    Image _attackPowerFill;
    TextMeshProUGUI _attackPowerValueText;
    Image _knockBackPowerFill;
    TextMeshProUGUI _knockBackPowersValueText;
    Image _penerstratingPowerFill;
    TextMeshProUGUI _penerstratingPowerValueText;
    Image _attackSpeedFill;
    TextMeshProUGUI _attackSpeedValueText;
    Image _reloadSpeedFill;
    TextMeshProUGUI _reloadSpeedValueText;

    TextMeshProUGUI _weaponDescription;
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
        _weaponImage = _weaponFolder.transform.Find("WeaponBackground").Find("WeaponImage").GetComponent<Image>();
        _weaponName = _weaponFolder.transform.Find("WeaponBackground").Find("WeaponName").GetComponent<TextMeshProUGUI>();

        _attackPowerFill = _weaponFolder.transform.Find("AttackPower").Find("Fill").GetComponent<Image>();
        _attackPowerValueText = _weaponFolder.transform.Find("AttackPower").Find("ValueText").GetComponent<TextMeshProUGUI>();
        _knockBackPowerFill = _weaponFolder.transform.Find("KnockBackPower").Find("Fill").GetComponent<Image>();
        _knockBackPowersValueText = _weaponFolder.transform.Find("KnockBackPower").Find("ValueText").GetComponent<TextMeshProUGUI>();
        _penerstratingPowerFill = _weaponFolder.transform.Find("PenetratingPower").Find("Fill").GetComponent<Image>();
        _penerstratingPowerValueText = _weaponFolder.transform.Find("PenetratingPower").Find("ValueText").GetComponent<TextMeshProUGUI>();
        _attackSpeedFill = _weaponFolder.transform.Find("AttackSpeed").Find("Fill").GetComponent<Image>();
        _attackSpeedValueText = _weaponFolder.transform.Find("AttackSpeed").Find("ValueText").GetComponent<TextMeshProUGUI>();
        _reloadSpeedFill = _weaponFolder.transform.Find("ReloadSpeed").Find("Fill").GetComponent<Image>();
        _reloadSpeedValueText = _weaponFolder.transform.Find("ReloadSpeed").Find("ValueText").GetComponent<TextMeshProUGUI>();

        _weaponDescription = _weaponFolder.transform.Find("DescriptionBackground").Find("WeaponDescription").GetComponent<TextMeshProUGUI>();
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
        _wall = manager.Wall;
        _wallAI = manager.WallAI;

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
        _stringBuilder.AppendLine($"체력재생력 : {_girl.IncreasedRecoverHpPower}");
        _descriptionTextMesh.text = _stringBuilder.ToString();

        GirlAbility girlAbility = _player.GirlAbility;
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"보유 능력");
        foreach (var abilityName in girlAbility.GetHaveAbilityNameList())
        {
            _stringBuilder.AppendLine($"{abilityName}");
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
        _stringBuilder.AppendLine($"체력재생력 : {_creature.IncreasedRecoverHpPower}");
        _descriptionTextMesh.text = _stringBuilder.ToString();


        CreatureAbility creatureAbility = _creatureAI.CreatureAbility;
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"보유 능력");
        foreach (var abilityName in creatureAbility.GetHaveAbilityNameList())
        {
            _stringBuilder.AppendLine($"{abilityName}");
        }
        _abilityTextMesh.text = _stringBuilder.ToString();
    }

    public void OpenWallPage()
    {

        _titleTextMesh.text = "벽";
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"체력 : {_wall.Hp}/{_wall.MaxHp}");
        _stringBuilder.AppendLine($"공격력 : {_wall.AttackPower}");
        _stringBuilder.AppendLine($"체력재생력 : {_wall.IncreasedRecoverHpPower}");
        _descriptionTextMesh.text = _stringBuilder.ToString();

        WallAbility wallAbility = _wallAI.WallAbility;
        _stringBuilder.Clear();
        _stringBuilder.AppendLine($"보유 능력");
        foreach (var abilityName in wallAbility.GetHaveAbilityNameList())
        {
            _stringBuilder.AppendLine($"{abilityName}");
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
                _weaponName.text = weapon.WeaponName.ToString();

                _attackPowerFill.fillAmount = weapon.Damage / 10f;
                _attackPowerValueText.text = weapon.Damage.ToString();

                _knockBackPowerFill.fillAmount = weapon.KnockBackPower / 100f;
                _knockBackPowersValueText.text = weapon.KnockBackPower.ToString();

                _penerstratingPowerFill.fillAmount = weapon.PenerstratingPower / 10f;
                _penerstratingPowerValueText.text = weapon.PenerstratingPower.ToString();

                _attackSpeedFill.fillAmount = 0.08f / weapon.FireDelay;
                _attackSpeedValueText.text = $"{weapon.FireDelay}s";

                _reloadSpeedFill.fillAmount = 0.1f / weapon.ReloadDelay;
                _reloadSpeedValueText.text = $"{weapon.ReloadDelay}s";

                _weaponDescription.text = weapon.WeaponDescription;

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
                _weaponName.text = weapon.WeaponName.ToString();

                _attackPowerFill.fillAmount = weapon.Damage / 10f;
                _attackPowerValueText.text = weapon.Damage.ToString();

                _knockBackPowerFill.fillAmount = weapon.KnockBackPower / 100f;
                _knockBackPowersValueText.text = weapon.KnockBackPower.ToString();

                _penerstratingPowerFill.fillAmount = weapon.PenerstratingPower / 10f;
                _penerstratingPowerValueText.text = weapon.PenerstratingPower.ToString();

                _attackSpeedFill.fillAmount = 1 / weapon.FireDelay / 10f;
                _attackSpeedValueText.text = $"{weapon.FireDelay}s";

                _reloadSpeedFill.fillAmount = 0.1f / weapon.ReloadDelay;
                _reloadSpeedValueText.text = $"{weapon.ReloadDelay}s";
                _weaponDescription.text = weapon.WeaponDescription;
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
                _weaponName.text = weapon.WeaponName.ToString();

                _attackPowerFill.fillAmount = weapon.Damage / 10f;
                _attackPowerValueText.text = weapon.Damage.ToString();

                _knockBackPowerFill.fillAmount = weapon.KnockBackPower / 100f;
                _knockBackPowersValueText.text = weapon.KnockBackPower.ToString();

                _penerstratingPowerFill.fillAmount = weapon.PenerstratingPower / 10f;
                _penerstratingPowerValueText.text = weapon.PenerstratingPower.ToString();

                _attackSpeedFill.fillAmount = 1 / weapon.FireDelay / 10f;
                _attackSpeedValueText.text = $"{weapon.FireDelay}s";

                _reloadSpeedFill.fillAmount = 0.1f / weapon.ReloadDelay;
                _reloadSpeedValueText.text = $"{weapon.ReloadDelay}s";
                _weaponDescription.text = weapon.WeaponDescription;
                _weaponFolder.gameObject.SetActive(true);
            }
        }
    }
}