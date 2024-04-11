using UnityEngine;

public class WeaponUpgrader : MonoBehaviour
{
    Player _player;

    int _weaponIndex;
    public Weapon Weapon { get; set; }

    [field:SerializeField] public int UpgradePrice { get { return Weapon == null ? _upgradePrice : _upgradePrice * (int)Mathf.Pow((Weapon.UpgradeCount+1),2); } }

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

    }

    public void Refresh()
    {
        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        _weaponIndex = Random.Range(0, 3);

        Weapon = _player.WeaponSwaper.GetWeapon(_weaponIndex);

        if (Weapon == null)
        {
            for (int i = 0; i < 2; i++)
            {
                _weaponIndex++;
                if (_weaponIndex >= 3)
                {
                    _weaponIndex = 0;
                }
                Weapon = _player.WeaponSwaper.GetWeapon(_weaponIndex);
                if (Weapon != null) break;
            }
        }

        if (Random.Range(0, 2) == 0)
            IncreasingAttackPowerPercentage = 50;
        if (Random.Range(0, 2) == 0)
            IncreasingKnockBackPowerPercentage = Random.Range(20, 30);
        if (Random.Range(0, 5) == 0)
            IncreasingPenerstratingPower = Random.Range(0, 2);
        if (Random.Range(0, 2) == 0)
            IncreasingAttackSpeedPercentage = Random.Range(0, 20);
        if (Random.Range(0, 2) == 0)
            DecreasingReloadTimePercentage = Random.Range(0, 20);

    }
}
