using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

public class ElectricWeapon : Weapon
{
    float _electric;
    public override void Fire(IWeaponUsable user)
    {
        if (_fireElapsed < 1 / AttackSpeed) return;

        if (_currentAmmo == _maxAmmo)
        {
            _reloadGauge.gameObject.SetActive(false);

            _currentAmmo = 0;
            _fireElapsed = 0;
            if (_audioCoroutine != null)
                StopCoroutine(_audioCoroutine);
            _audioCoroutine = StartCoroutine(CorPlayAudio());

            float angle = FirePosition.transform.rotation.eulerAngles.z;
            angle = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
            direction = direction.normalized;
            Effect fireFlareOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.FireFlare);
            Effect fireFlare = Managers.GetManager<ResourceManager>().Instantiate(fireFlareOrigin);
            fireFlare.Play(_firePosition.transform.position);
            fireFlare.transform.localScale = _firePosition.transform.lossyScale;
            fireFlare.transform.rotation = _firePosition.transform.rotation;

            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Projectile");
            go.transform.position = _firePosition.transform.position;
            Projectile projectile = go.GetComponent<Projectile>();
            float damage = AttackPower;

            // 플레이어 사용자가 플레이어라면
            if (user is Player player)
            {
                if (player.GirlAbility.GetIsHaveAbility(CardName.라스트샷) && _currentAmmo == 0)
                    damage = AttackPower * Managers.GetManager<CardManager>().GetCard(CardName.라스트샷).property;
            }

            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);


            user?.Rebound(_rebound);
        }
       
    }

    public override void Reload(IWeaponUsable user)
    {
        
    }

    public override void ReloadHold()
    {
        if (_currentAmmo < _maxAmmo)
        {
            if (Managers.GetManager<CardManager>().CurrentElectricity > 0.1)
            {
                Managers.GetManager<CardManager>().CurrentElectricity += -Time.deltaTime;
                _electric += Time.deltaTime;
                if (_electric >= 1)
                {
                    _currentAmmo += 1;
                    _electric -= 1;
                }
                if (_reloadGauge)
                {
                    _reloadGauge.gameObject.SetActive(true);
                    _reloadGauge.SetRatio(_currentAmmo + _electric, _maxAmmo);
                }

            }
            else
            {
                _reloadGauge.gameObject.SetActive(false);
                _electric = 0;
                _currentAmmo = 0;
            }
        }
        else
        {
            _reloadGauge.gameObject.SetActive(false);
            _electric = 0;
            _currentAmmo = 0;
        }
    }

    public override void ReloadUp()
    {
        _reloadGauge.gameObject.SetActive(false);

        if(_currentAmmo < _maxAmmo)
        {
            _currentAmmo = 0;
        }
    }
}
