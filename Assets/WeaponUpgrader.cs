using UnityEngine;

public class WeaponUpgrader : MonoBehaviour
{
    Player _player;

    int _weaponIndex;
    public Weapon Weapon { get; set; }

    [field:SerializeField] public int UpgradePrice { get { return Weapon == null ? _upgradePrice : _upgradePrice * Weapon.UpgradeCount; } }
    public int TotalPrice { private set; get; }
    int _upgradePrice = 30;

    public int IncreasingAttackPowerPercentage { get; set; }
    public float IncreasingKnockBackPowerPercentage { get; set; }
    public int IncreasingPenerstratingPower { get; set; }
    public float IncreasingAttackSpeedPercentage { get; set; }
    public float DecreasingReloadTimePercentage { get; set; }


    public void UpgradeWeapon()
    {
        if (Managers.GetManager<GameManager>().Money < UpgradePrice) return;


        Weapon weapon = _player.WeaponSwaper.GetWeapon(_weaponIndex);

        if (weapon == null) return;

        weapon.IncreasedAttackPowerPercentage += IncreasingAttackPowerPercentage;
        weapon.IncreasedAttackSpeedPercentage += IncreasingAttackSpeedPercentage;
        weapon.IncreasedKnockbackPowerPercentage += IncreasingKnockBackPowerPercentage;
        weapon.IncreasedPenerstratingPower += IncreasingPenerstratingPower;
        weapon.DecreasedReloadTimePercentage += DecreasingReloadTimePercentage;

        weapon.UpgradeCount++;

        IncreasingAttackPowerPercentage = 0;
        IncreasingAttackSpeedPercentage = 0;
        IncreasingKnockBackPowerPercentage = 0;
        IncreasingPenerstratingPower = 0;
        DecreasingReloadTimePercentage = 0;
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

    public void IncreaseAttackPower()
    {
        IncreasingAttackPowerPercentage = 20;
        UpgradeWeapon();
    }
    public void IncreaseKncokBackPower()
    {
        IncreasingKnockBackPowerPercentage = 20;
        UpgradeWeapon();
    }
    public void IncreasePenerstratingPower()
    {
        IncreasingPenerstratingPower = 1;
        UpgradeWeapon();
    }
    public void IncreaseAttackSpeed()
    {
        IncreasingAttackSpeedPercentage = 20;

    }
    public void IncreaseReloadSpeed()
    {
        DecreasingReloadTimePercentage = 20;
    }
}
