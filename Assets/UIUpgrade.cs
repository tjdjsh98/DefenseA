using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class UIUpgrade : UIBase
{
    Player _player;

    [SerializeField] GameObject _fromWeaponFolder;
    WeaponUI _fromWeaponUI = new WeaponUI();

    [SerializeField] GameObject _toWeaponFolder;
    WeaponUI _toWeaponUI = new WeaponUI();

    int _randomWeaponIndex;

    public override void Init()
    {
        _fromWeaponUI.InitWeaponUI(_fromWeaponFolder);
        _toWeaponUI.InitWeaponUI(_toWeaponFolder);

        gameObject.SetActive(false);
    }
    public override void Open(bool except = false)
    {
        if(!except)
            Managers.GetManager<UIManager>().Open(this);

        if(_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        Time.timeScale = .0f;
        gameObject.SetActive(true);

        Reroll();

    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    public void Confime()
    {
        Weapon weapon = _player.WeaponSwaper.GetWeapon(_randomWeaponIndex);
        if(weapon != null)
        {
            weapon.IncreasedAttackPowerPercentage += _toWeaponUI.increasingAttackPowerPercentage;
            weapon.IncreasedAttackSpeedPercentage += _toWeaponUI.increasingAttackSpeedPercentage;
            weapon.IncreasedKnockbackPowerPercentage += _toWeaponUI.increasingKnockBackPowerPercentage;
            weapon.IncreasedPenerstratingPower += _toWeaponUI.increasingPenerstratingPower;
            weapon.IncreasedReloadSpeedPercentage += _toWeaponUI.increasingReloadSpeedPercentage;
        }

        Close();
    }

    public void Reroll()
    {
        _randomWeaponIndex = Random.Range(0, 3);

        Weapon weapon = _player.WeaponSwaper.GetWeapon(_randomWeaponIndex);

        if (weapon == null)
        {
            for (int i = 0; i < 2; i++)
            {
                _randomWeaponIndex++;
                if (_randomWeaponIndex >= 3)
                {
                    _randomWeaponIndex = 0;
                }
                weapon = _player.WeaponSwaper.GetWeapon(_randomWeaponIndex);
                if (weapon != null) break;
            }
        }

        if(weapon != null)
        {
            _fromWeaponUI.Refresh(weapon);
            StartCoroutine(_fromWeaponUI.CorFillGauge(weapon));

            if(Random.Range(0,2) == 0)
                _toWeaponUI.increasingAttackPowerPercentage = Random.Range(20, 50);
            if (Random.Range(0, 2) == 0)
                _toWeaponUI.increasingKnockBackPowerPercentage= Random.Range(20, 30);
            if (Random.Range(0, 5) == 0)
                _toWeaponUI.increasingPenerstratingPower = Random.Range(0, 2);
            if (Random.Range(0, 2) == 0)
                _toWeaponUI.increasingAttackSpeedPercentage= Random.Range(0, 20);
            if (Random.Range(0, 2) == 0)
                _toWeaponUI.increasingReloadSpeedPercentage = Random.Range(0, 20);

            _toWeaponUI.Refresh(weapon);
            StartCoroutine(_toWeaponUI.CorFillGauge(weapon));
        }
    }
}

public class WeaponUI
{
    public GameObject _weaponFolder;
    public Image _weaponImage;
    public TextMeshProUGUI _weaponName;

    string weaponName;
    string weaponDescription;

    public int increasingAttackPowerPercentage;
    public float increasingKnockBackPowerPercentage;
    public int increasingPenerstratingPower;
    public float increasingAttackSpeedPercentage;
    public float increasingReloadSpeedPercentage;

    public Image _attackPowerFill;
    public TextMeshProUGUI _attackPowerValueText;
    public Image _knockBackPowerFill;
    public TextMeshProUGUI _knockBackPowersValueText;
    public Image _penerstratingPowerFill;
    public TextMeshProUGUI _penerstratingPowerValueText;
    public Image _attackSpeedFill;
    public TextMeshProUGUI _attackSpeedValueText;
    public Image _reloadSpeedFill;
    public TextMeshProUGUI _reloadSpeedValueText;

    public TextMeshProUGUI _weaponDescription;

    public void InitWeaponUI(GameObject weaponFolder)
    {
        _weaponFolder = weaponFolder;
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

    public void Refresh(Weapon weapon)
    {
        if (weapon != null)
        {
            _weaponName.text = weapon.WeaponName.ToString();

            _weaponDescription.text = weapon.WeaponDescription;

            _weaponFolder.gameObject.SetActive(true);
        }
    }
    public IEnumerator CorFillGauge(Weapon weapon)
    {
        int duration = 30;
        if (increasingAttackPowerPercentage != 0)
            _attackPowerValueText.text = $"{weapon.OriginalAttackPower} + {weapon.OriginalAttackPower * (weapon.IncreasedAttackPowerPercentage + increasingAttackPowerPercentage)/100f}";
        else
            _attackPowerValueText.text = $"{weapon.OriginalAttackPower} + {weapon.OriginalAttackPower * (weapon.IncreasedAttackPowerPercentage) / 100f}";

        if (increasingKnockBackPowerPercentage != 0)
            _knockBackPowersValueText.text = $"{weapon.OriginalKnockBackPower} + {weapon.OriginalKnockBackPower *(weapon.IncreasedKnockbackPowerPercentage +increasingKnockBackPowerPercentage)/ 100f}";
        else
            _knockBackPowersValueText.text = $"{weapon.KnockBackPower}  + {weapon.OriginalKnockBackPower *(weapon.IncreasedKnockbackPowerPercentage)/ 100f}";

        if (increasingPenerstratingPower != 0)
            _penerstratingPowerValueText.text = $"{weapon.OriginalPenerstratingPower} + {weapon.IncreasedPenerstratingPower+increasingPenerstratingPower}";
        else
            _penerstratingPowerValueText.text = $"{weapon.OriginalPenerstratingPower} + {weapon.IncreasedPenerstratingPower }";

        if (increasingAttackSpeedPercentage != 0)
            _attackSpeedValueText.text = $"{weapon.OriginalAttackSpeed} + {weapon.OriginalAttackSpeed * (weapon.IncreasedAttackSpeedPercentage + increasingAttackSpeedPercentage) / 100f}";
        else
            _attackSpeedValueText.text = $"{weapon.OriginalAttackSpeed} + {weapon.OriginalAttackSpeed * (weapon.IncreasedAttackSpeedPercentage) / 100f}";

        if (increasingReloadSpeedPercentage != 0)
            _reloadSpeedValueText.text = $"{weapon.OriginalReloadSpeed} + {weapon.OriginalReloadSpeed * (weapon.IncreasedAttackSpeedPercentage + increasingReloadSpeedPercentage )/ 100f}";
        else
            _reloadSpeedValueText.text = $"{weapon.OriginalReloadSpeed} + {weapon.OriginalReloadSpeed * (weapon.IncreasedAttackSpeedPercentage )/ 100f}";

        for (int i = 0; i < duration; i++)
        {
            _attackPowerFill.fillAmount = Mathf.Lerp(0, (weapon.AttackPower + increasingAttackPowerPercentage * weapon.OriginalAttackPower ) / 10f, 1f/(duration - i));

            _knockBackPowerFill.fillAmount = Mathf.Lerp(0, (weapon.KnockBackPower+ increasingKnockBackPowerPercentage * weapon.OriginalKnockBackPower) / 100f, 1f / (duration - i));

            _penerstratingPowerFill.fillAmount = Mathf.Lerp(0, (weapon.PenerstratingPower+ increasingPenerstratingPower) /10f, 1f / (duration - i));

            _attackSpeedFill.fillAmount = Mathf.Lerp(0, (weapon.AttackSpeed + weapon.OriginalAttackSpeed * increasingAttackSpeedPercentage) /10f, 1f / (duration - i));

            _reloadSpeedFill.fillAmount = Mathf.Lerp(0, (weapon.ReloadSpeed + weapon.OriginalReloadSpeed * increasingReloadSpeedPercentage) /10f, 1f / (duration - i));

            yield return null;
        }
    }
}
