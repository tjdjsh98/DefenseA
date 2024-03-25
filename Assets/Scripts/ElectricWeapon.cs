using System.Collections;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;

public class ElectricWeapon : Weapon
{
    float _electric;
    public override void Fire(Character fireCharacter)
    {
        if (_fireElapsed < 1 / FireSpeed) return;

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
            int damage = Damage;
            


            // �÷��̾ ��Ʈ�� �ɷ��� �ִٸ� ������ 3��
            if (Player.GirlAbility.GetIsHaveAbility(GirlAbilityName.LastShot) && _currentAmmo == 0)
                damage = Damage * 3;
            projectile.Init(KnockBackPower, BulletSpeed, damage, Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(fireCharacter, direction.normalized);


            Player?.Rebound(_rebound);
        }
       
    }

    public override void Reload()
    {
        
    }

    public override void ReloadHold()
    {
        if (_currentAmmo < _maxAmmo)
        {
            if (Managers.GetManager<AbilityManager>().CurrentElectricity > 0.1)
            {
                Managers.GetManager<AbilityManager>().AddElectricity(-Time.deltaTime);
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
