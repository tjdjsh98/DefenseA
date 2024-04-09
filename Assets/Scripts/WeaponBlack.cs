using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBlack : Weapon
{
    BlackSphere _reloadingBlackSphere;

    public override void FireBullet(IWeaponUsable user)
    {
        _currentAmmo--;

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        user?.ResetRebound();
        float angle = FirePosition.transform.rotation.eulerAngles.z;
        angle = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
        direction = direction.normalized;
        // 이펙트
        Effect fireFlareOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.FireFlare);
        Effect fireFlare = Managers.GetManager<ResourceManager>().Instantiate(fireFlareOrigin);
        fireFlare.Play(_firePosition.transform.position);
        fireFlare.transform.localScale = _firePosition.transform.lossyScale;
        fireFlare.transform.rotation = _firePosition.transform.rotation;
        // ---------------

        Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)_bulletName);
        if (projectile != null)
        {
            projectile.transform.position = _firePosition.transform.position;
            float damage = AttackPower * _currentAmmo;

            // 플레이어 사용자가 플레이어라면
            if (user is Player player)
            {
                if (player.GirlAbility.GetIsHaveAbility(CardName.라스트샷) && _currentAmmo == 0)
                    damage = AttackPower * Managers.GetManager<CardManager>().GetCard(CardName.라스트샷).Property;
            }

            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);


            user?.Rebound(_rebound);
            _currentAmmo = 0;
        }
    }
    public override void CompleteReload(bool isHideGauge = false)
    {
        if (_isAllReloadAmmo)
        {
            _isReload = false;
            //TODO
            //if (_player.AbilityUnlocks.ContainsKey(GirlAbility.ExtraAmmo) && !_fastReloadFailed)
            //    _currentAmmo = Mathf.CeilToInt(_maxAmmo * 1.5f);
            //else
            //  _currentAmmo = _maxAmmo;
            _reloadElapsed = 0;
            if (_reloadGauge && isHideGauge)
                _reloadGauge.gameObject.SetActive(false);

            _reloadingBlackSphere = null;
        }
        else
        {
            _currentAmmo++;
            if (_currentAmmo >= _maxAmmo)
            {
                _currentAmmo = _maxAmmo;
                _isReload = false;
                if (_reloadGauge)
                    _reloadGauge.gameObject.SetActive(false);
            }
            else
            {
                if (Managers.GetManager<CardManager>().BlackSphereList.Count <= 0)
                {
                    _isReload = false;
                    if (_reloadGauge)
                        _reloadGauge.gameObject.SetActive(false);
                }
                else
                {
                    _reloadingBlackSphere = Managers.GetManager<CardManager>().BlackSphereList[0];
                    Managers.GetManager<CardManager>().BlackSphereList.RemoveAt(0);
                    _reloadingBlackSphere.MoveToDestination(gameObject, _reloadTime,true);
                }
            }

            if (Managers.GetManager<CardManager>().BlackSphereList.Count <= 0)
            {
                _isReload = false;
                if (_reloadGauge)
                    _reloadGauge.gameObject.SetActive(false);
            }
            
          

            _reloadElapsed = 0;
            _reloadingBlackSphere = null;
        }
    }
    public override void Reload(IWeaponUsable user)
    {
        if (_maxAmmo <= _currentAmmo) return;
        if (_isReload) return;

        if (Managers.GetManager<CardManager>().BlackSphereList.Count <= 0) return;

        _reloadingBlackSphere = Managers.GetManager<CardManager>().BlackSphereList[0];
        Managers.GetManager<CardManager>().BlackSphereList.RemoveAt(0);
        _reloadingBlackSphere.MoveToDestination(gameObject, _reloadTime,true);

        _isReload = true;
        if (_reloadGauge)
        {
            _reloadGauge.SetRatio(0, 1);
            _reloadGauge.gameObject.SetActive(true);
        }
    }

    public override void CancelReload()
    {
        base.CancelReload();
        if (_reloadingBlackSphere)
        {
            _reloadingBlackSphere.CancelMoveToDestination();
            Managers.GetManager<CardManager>().BlackSphereList.Add(_reloadingBlackSphere);
        }
    }
}
