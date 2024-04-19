using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgrade : UIBase
{
    WeaponUpgrader _weaponUpgrader;

    [SerializeField] GameObject _weaponFolder;
    WeaponUI _weaponUI = new WeaponUI();


    [SerializeField] TextMeshProUGUI _upgradePriceTextMesh;

    public override void Init()
    {
        _weaponUI.InitWeaponUI(_weaponFolder);

        gameObject.SetActive(false);
    }
    public void Open(WeaponUpgrader weaponUpgrader, bool except = false)
    {
        if (weaponUpgrader == null) return;

        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        _weaponUpgrader = weaponUpgrader;

        Time.timeScale = .0f;
        gameObject.SetActive(true);

        _weaponUpgrader.Open();
        Refresh();
    }

    public override void Open(bool except = false)
    {
        if (_weaponUpgrader == null) return;

        if(!except)
            Managers.GetManager<UIManager>().Open(this);


        Time.timeScale = .0f;
        gameObject.SetActive(true);

        _weaponUpgrader.Open();
        Refresh();

    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);


        if (!except)
        {
            Managers.GetManager<UIManager>().Close(this);
            _weaponUpgrader = null;
        }
    }

    public void Confirm()
    {
        _weaponUpgrader.UpgradeWeapon();

        Close();
    }
    
    public void SelectWeapon(int index)
    {
        _weaponUpgrader.SelectWeapon(index);
        Refresh();
    }
    public void IncreaseAttackPower()
    {
        _weaponUpgrader.IncreaseAttackPower();
        Refresh();
    }
    public void IncreaseKncokBackPower()
    {
        _weaponUpgrader.IncreaseKncokBackPower();
        Refresh();
    }
    public void IncreasePenerstratingPower()
    {
        _weaponUpgrader.IncreasePenerstratingPower();
        Refresh();
    }
    public void IncreaseAttackSpeed()
    {
        _weaponUpgrader.IncreaseAttackSpeed();
        Refresh();

    }
    public void IncreaseReloadSpeed()
    {
        _weaponUpgrader.IncreaseReloadSpeed();
        Refresh();
    }

    void Refresh()
    {
        if (_upgradePriceTextMesh)
        {
            _upgradePriceTextMesh.text = _weaponUpgrader.UpgradePrice.ToString();
        }
        _weaponUI.Refresh(_weaponUpgrader.Weapon);
        StartCoroutine(_weaponUI.CorFillGauge(_weaponUpgrader));

        _upgradePriceTextMesh.text = _weaponUpgrader.UpgradePrice.ToString();
    }

}

public class WeaponUI
{
    public GameObject _weaponFolder;
    public Image _weaponImage;
    public TextMeshProUGUI _weaponName;

    public Image _attackPowerFill;
    public TextMeshProUGUI _attackPowerValueText;
    public Image _knockBackPowerFill;
    public TextMeshProUGUI _knockBackPowersValueText;
    public Image _penerstratingPowerFill;
    public TextMeshProUGUI _penerstratingPowerValueText;
    public Image _attackSpeedFill;
    public TextMeshProUGUI _attackSpeedValueText;
    public Image _reloadSpeedFill;
    public TextMeshProUGUI _reloadTimeValueText;

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
        _reloadTimeValueText = _weaponFolder.transform.Find("ReloadSpeed").Find("ValueText").GetComponent<TextMeshProUGUI>();

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
        else
        {
            _weaponName.text = string.Empty;
            _weaponDescription.text = string.Empty;
        }
    }
    public IEnumerator CorFillGauge(WeaponUpgrader weaponUpgrader)
    {
        float duration = 30;
        Weapon weapon = weaponUpgrader.Weapon;
        if (weapon)
        {
            _attackPowerValueText.text = $"{weapon.AttackPower} + {weapon.OriginalAttackPower * (weaponUpgrader.IncreasingAttackPowerPercentage) / 100f}";

            _knockBackPowersValueText.text = $"{weapon.KnockBackPower} + {weapon.OriginalKnockBackPower * (weaponUpgrader.IncreasingKnockBackPowerPercentage) / 100f}";

            _penerstratingPowerValueText.text = $"{weapon.PenerstratingPower} + { weaponUpgrader.IncreasingPenerstratingPower}";

            _attackSpeedValueText.text = $"{weapon.AttackSpeed} + {weapon.OriginalAttackSpeed* ( weaponUpgrader.IncreasingAttackSpeedPercentage) / 100f}";

            _reloadTimeValueText.text = $"{weapon.ReloadTime} - {weapon.OriginalReloadTime * (weaponUpgrader.IncreasingReloadSpeedPercentage) / 100f}";

            for (int i = 0; i <= duration; i++)
            {
                _attackPowerFill.fillAmount = Mathf.Lerp(0, (weapon.AttackPower + weaponUpgrader.IncreasingAttackPowerPercentage / 100f * weapon.OriginalAttackPower) / 10f, i / (duration));
                _knockBackPowerFill.fillAmount = Mathf.Lerp(0, (weapon.KnockBackPower + weaponUpgrader.IncreasingKnockBackPowerPercentage / 100f * weapon.OriginalKnockBackPower) / 100f, i / (duration));
                _penerstratingPowerFill.fillAmount = Mathf.Lerp(0, (weapon.PenerstratingPower + weaponUpgrader.IncreasingPenerstratingPower) / 10f, 1f / (duration - i));
                _attackSpeedFill.fillAmount = Mathf.Lerp(0, (weapon.AttackSpeed + weapon.OriginalAttackSpeed * weaponUpgrader.IncreasingAttackSpeedPercentage / 100f) / 10f, i / (duration));
                _reloadSpeedFill.fillAmount = Mathf.Lerp(0, 0.1f / (weapon.ReloadTime + weapon.OriginalReloadTime * weaponUpgrader.IncreasingReloadSpeedPercentage / 100f), i / (duration));

                yield return null;
            }
        }
        else
        {
            _attackPowerValueText.text = string.Empty;
            _knockBackPowersValueText.text = string.Empty;
            _penerstratingPowerValueText.text = string.Empty;
            _attackSpeedValueText.text = string.Empty;
            _reloadTimeValueText.text = string.Empty;
            _attackPowerFill.fillAmount = 0;
            _knockBackPowerFill.fillAmount = 0;
            _penerstratingPowerFill.fillAmount = 0;
            _attackSpeedFill.fillAmount = 0;
            _reloadSpeedFill.fillAmount = 0;

        }
    }
    public IEnumerator CorFillGauge(Weapon weapon)
    {
        float duration = 30;
        if (weapon)
        {
            _attackPowerValueText.text = $"{weapon.AttackPower}";

            _knockBackPowersValueText.text = $"{weapon.KnockBackPower}";

            _penerstratingPowerValueText.text = $"{weapon.PenerstratingPower}";

            _attackSpeedValueText.text = $"{weapon.AttackSpeed}";

            _reloadTimeValueText.text = $"{weapon.ReloadTime}";

            for (int i = 0; i <= duration; i++)
            {
                _attackPowerFill.fillAmount = Mathf.Lerp(0, weapon.AttackPower / 10f, i / (duration));
                _knockBackPowerFill.fillAmount = Mathf.Lerp(0, weapon.KnockBackPower / 100f, i / (duration));
                _penerstratingPowerFill.fillAmount = Mathf.Lerp(0, weapon.PenerstratingPower / 10f, i / (duration));
                _attackSpeedFill.fillAmount = Mathf.Lerp(0, weapon.AttackSpeed / 10f, i / (duration));
                _reloadSpeedFill.fillAmount = Mathf.Lerp(0, 0.1f / weapon.ReloadTime, i / (duration));

                yield return null;
            }
        }
        else
        {
            _attackPowerValueText.text = string.Empty;
            _knockBackPowersValueText.text = string.Empty;
            _penerstratingPowerValueText.text = string.Empty;
            _attackSpeedValueText.text = string.Empty;
            _reloadTimeValueText.text = string.Empty;
            _attackPowerFill.fillAmount = 0;
            _knockBackPowerFill.fillAmount = 0;
            _penerstratingPowerFill.fillAmount = 0;
            _attackSpeedFill.fillAmount = 0;
            _reloadSpeedFill.fillAmount = 0;

            yield return null;
        }
    }
}
