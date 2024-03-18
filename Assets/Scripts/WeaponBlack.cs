using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBlack : Weapon
{
    BlackSphere _reloadingBlackSphere;

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
                if (_player.GirlAbility.BlackSphereList.Count <= 0)
                {
                    _isReload = false;
                    if (_reloadGauge)
                        _reloadGauge.gameObject.SetActive(false);
                }
                else
                {
                    _reloadingBlackSphere = _player.GirlAbility.BlackSphereList[0];
                    _player.GirlAbility.BlackSphereList.RemoveAt(0);
                    _reloadingBlackSphere.MoveToDestinationAndDestroy(gameObject, _reloadDelay);
                }
            }

            if (_player.GirlAbility.BlackSphereList.Count <= 0)
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

        if (_player.GirlAbility.BlackSphereList.Count <= 0) return;

        _reloadingBlackSphere = _player.GirlAbility.BlackSphereList[0];
        _player.GirlAbility.BlackSphereList.RemoveAt(0);
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
            _player.GirlAbility.BlackSphereList.Add(_reloadingBlackSphere);
        }
    }
}
