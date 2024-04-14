using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLittleBlack : Weapon
{
    BlackSphere _reloadingBlackSphere;

    protected override void Update()
    {
        base.Update();
        _attackPower = Mathf.RoundToInt(CurrentAmmo * 2f);
    }
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
        // ¿Ã∆Â∆Æ
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
            float damage = AttackPower ;

            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);


            user?.Rebound(_rebound);
        }
    }

    public override void Reload(IWeaponUsable user)
    {
        if (_maxAmmo <= _currentAmmo) return;
        if (_isReload) return;

        if (Managers.GetManager<CardManager>().BlackSphereList.Count <= 0) return;

        _reloadingBlackSphere = Managers.GetManager<CardManager>().BlackSphereList[0];
        Managers.GetManager<CardManager>().BlackSphereList.RemoveAt(0);
        _reloadingBlackSphere.MoveToDestination(gameObject, _reloadTime, true);

        _isReload = true;
        if (_reloadGauge)
        {
            _reloadGauge.SetRatio(0, 1);
            _reloadGauge.gameObject.SetActive(true);
        }
    }
    public override void CompleteReload(bool isHideGauge = false)
    {
        if (_isAllReloadAmmo)
        {
            _isReload = false;
            _reloadElapsed = 0;
            _reloadingBlackSphere = null;
            if (_reloadGauge && isHideGauge)
                _reloadGauge.gameObject.SetActive(false);

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
                    _reloadingBlackSphere.MoveToDestination(gameObject, _reloadTime, true);
                }
            }

            _reloadElapsed = 0;
            _reloadingBlackSphere = null;
        }
    }
}
