using MoreMountains.Feedbacks;
using UnityEngine;

public class WeaponUpgrader : MonoBehaviour
{
    Player _player;

    int _weaponIndex;
    public Weapon Weapon { get; set; }

    [field:SerializeField] public int UpgradePrice { get { return Weapon == null ? _upgradePrice : _upgradePrice * (Weapon.UpgradeCount+1); } }
    public int TotalPrice { private set; get; }
    int _upgradePrice = 30;

    public int IncreasingAttackPowerPercentage { get; set; } = 20;
    public float IncreasingKnockBackPowerPercentage { get; set; } = 30;
    public int IncreasingPenerstratingPower { get; set; } = 1;
    public float IncreasingAttackSpeedPercentage { get; set; } = 20;
    public float IncreasingReloadSpeedPercentage { get; set; } = 10;

    bool _isIncreaseAttackPower;
    bool _isIncreaseKnockBackPower;
    bool _isIncreasePenerstratingPower;
    bool _isIncreaseAttackSpeed;
    bool _isIncreaseReloadSpeed;


    public bool UpgradeWeapon()
    {
        if (Managers.GetManager<GameManager>().Money < UpgradePrice) return false;

        Managers.GetManager<GameManager>().Money -= UpgradePrice;
        Weapon weapon = _player.WeaponSwaper.GetWeapon(_weaponIndex);

        if (weapon == null) return false;

        if(_isIncreaseAttackPower)
            weapon.IncreasedAttackPowerPercentage += IncreasingAttackPowerPercentage;
        if(_isIncreaseAttackSpeed)
            weapon.IncreasedAttackSpeedPercentage += IncreasingAttackSpeedPercentage;
        if(_isIncreaseKnockBackPower)
            weapon.IncreasedKnockbackPowerPercentage += IncreasingKnockBackPowerPercentage;
        if(_isIncreasePenerstratingPower)
            weapon.IncreasedPenerstratingPower += IncreasingPenerstratingPower;
        if(_isIncreaseReloadSpeed)
            weapon.IncreasedReloadSpeedPercentage += IncreasingReloadSpeedPercentage;

        weapon.UpgradeCount++;

        _isIncreaseAttackPower = false;
        _isIncreaseKnockBackPower = false;
        _isIncreasePenerstratingPower = false;
        _isIncreaseAttackSpeed = false;
        _isIncreaseReloadSpeed = false;

        return true;
    }

    public void Open()
    {
        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        SelectWeapon(_weaponIndex);

        if (Weapon == null)
        {
            for (int i = 0; i < 3; i++)
            {
                Weapon = _player.WeaponSwaper.GetWeapon(i);
                if (Weapon != null)
                {
                    SelectWeapon(i);
                    return;
                }
            }
        }


    }
    public void SelectWeapon(int index)
    {
        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        if (_player == null) return;

        
        Weapon = _player.WeaponSwaper.GetWeapon(index);

        if (Weapon == null) return;

        _weaponIndex = index;
    }

    public bool IncreaseAttackPower()
    {
        _isIncreaseAttackPower = true;
        return UpgradeWeapon();
    }
    public bool IncreaseKncokBackPower()
    {
        _isIncreaseKnockBackPower = true;
        return UpgradeWeapon();
    }
    public bool IncreasePenerstratingPower()
    {
        _isIncreasePenerstratingPower= true;
        return UpgradeWeapon();
    }
    public bool IncreaseAttackSpeed()
    {
        _isIncreaseAttackSpeed= true;
        return UpgradeWeapon();
    }
    public bool IncreaseReloadSpeed()
    {
        _isIncreaseReloadSpeed = true;
        return UpgradeWeapon();
    }
}
