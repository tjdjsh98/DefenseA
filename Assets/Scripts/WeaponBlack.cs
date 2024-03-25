using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBlack : Weapon
{
    BlackSphere _reloadingBlackSphere;

    public override void Fire(Character fireCharacter)
    {
        if (_currentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (_fireElapsed < 1 / FireSpeed) return;

        // 재장전 중이라면 재장전을 멈춥니다.
        if (_isReload)
        {
            CancelReload();
        }


        _fireElapsed = 0;

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        _player?.ResetRebound();
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
            int damage = Damage * _currentAmmo;

            // 플레이어가 라스트샷 능력이 있다면 데미지 3배
            if (Player.GirlAbility.GetIsHaveAbility(GirlAbilityName.LastShot))
                damage = Damage * 3;
            
            projectile.Init(KnockBackPower, BulletSpeed, damage, Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(fireCharacter, direction.normalized);


            Player?.Rebound(_rebound);
        }
        _currentAmmo = 0;
    }
    public override void CompleteReload()
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
            if (_reloadGauge)
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
                if (Managers.GetManager<AbilityManager>().BlackSphereList.Count <= 0)
                {
                    _isReload = false;
                    if (_reloadGauge)
                        _reloadGauge.gameObject.SetActive(false);
                }
                else
                {
                    _reloadingBlackSphere = Managers.GetManager<AbilityManager>().BlackSphereList[0];
                    Managers.GetManager<AbilityManager>().BlackSphereList.RemoveAt(0);
                    _reloadingBlackSphere.MoveToDestinationAndDestroy(gameObject, _reloadDelay);
                }
            }

            if (Managers.GetManager<AbilityManager>().BlackSphereList.Count <= 0)
            {
                _isReload = false;
                if (_reloadGauge)
                    _reloadGauge.gameObject.SetActive(false);
            }
            
          

            _reloadElapsed = 0;
            _reloadingBlackSphere = null;
        }
    }
    public override void Reload()
    {
        if (_maxAmmo <= _currentAmmo) return;
        if (_isReload) return;

        if (Managers.GetManager<AbilityManager>().BlackSphereList.Count <= 0) return;

        _reloadingBlackSphere = Managers.GetManager<AbilityManager>().BlackSphereList[0];
        Managers.GetManager<AbilityManager>().BlackSphereList.RemoveAt(0);
        _reloadingBlackSphere.MoveToDestinationAndDestroy(gameObject, _reloadDelay);

        _isReload = true;
        if (_reloadGauge)
        {
            if (_player)
            {
                // TODO
                //if (_player.GetIsHaveAbility(GirlAbility.FastReload))
                //{
                //    _reloadGauge.Point(0.7f, 0.9f);
                //}
                //else
                //{
                //    _reloadGauge.DisablePoint();
                //}
            }
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
            Managers.GetManager<AbilityManager>().BlackSphereList.Add(_reloadingBlackSphere);
        }
    }
}
