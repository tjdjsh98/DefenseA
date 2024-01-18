using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    Character _character;

    [SerializeField] bool _isRaycast;
    public GameObject _projectile;


    [SerializeField] float _power = 1;
    public float Power => _power;
    [SerializeField] float _bulletSpeed = 15;
    public float BulletSpeed => _bulletSpeed;

    [SerializeField] int _damage = 1;
    public int Damage => _damage;

    [SerializeField] int _maxAmmo;
    public int MaxAmmo => _maxAmmo;
    [SerializeField] int _currentAmmo;
    public int CurrentAmmo => _currentAmmo;

    [SerializeField] float _fireDelay;
    public float FireDelay => _fireDelay;
    [SerializeField] float _reloadDelay;
    public float ReloadDelay => _reloadDelay;

    [SerializeField] GameObject _firePosition;


    float _fireElapsed;
    float _reloadElapsed;

    bool _isReload;

    [SerializeField] HpBar _reloadGauge;

    [SerializeField] Define.EffectName _hitEffect;

    private void Awake()
    {
        _character = GetComponentInParent<Character>();
        _currentAmmo = _maxAmmo;
        if(_reloadGauge)
            _reloadGauge.gameObject.SetActive(false);

    }
    public void Fire(Character fireCharacter)
    {

        if (_currentAmmo <= 0)
        {
            Reload();
            return;
        }

        _currentAmmo--;
        _fireElapsed = 0;
        
        Vector3 direction = Managers.GetManager<InputManager>().MouseWorldPosition - _firePosition.transform.position;

        if (!_isRaycast)
        {
            GameObject go = Instantiate(_projectile);
            go.transform.position = _firePosition.transform.position;
            Projectile projectile = go.GetComponent<Projectile>();
            projectile.Init(_power, _bulletSpeed, _damage);


            projectile.Fire(fireCharacter, direction.normalized);
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(_firePosition.transform.position, direction.normalized,Mathf.Infinity,LayerMask.GetMask("Character") | LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                if (_hitEffect != Define.EffectName.None)
                {
                    Effect effect = Managers.GetManager<EffectManager>().GetEffect(_hitEffect);
                    effect.SetProperty("Direction", direction);
                    effect.Play(hit.point);
                }

                Character character = hit.collider.gameObject.GetComponent<Character>();

                if (character != null)
                {
                    character.Damage(_character, _damage, _power, direction.x > 0 ? Vector3.right: Vector3.left);
                }

            }
        }
    }
    public void Update()
    {
        if (_fireElapsed < _fireDelay)
            _fireElapsed += Time.deltaTime;

        if (_isReload && _reloadElapsed < _reloadDelay)
        {
            _reloadElapsed += Time.deltaTime;
            _reloadGauge.SetRatio(_reloadElapsed, _reloadDelay);
        }
        else if(_isReload && _reloadElapsed >= _reloadDelay)
        {
            _isReload = false;
            _currentAmmo = _maxAmmo;
            _reloadElapsed = 0;
            if (_reloadGauge)
                _reloadGauge.gameObject.SetActive(false);
        }
    }

    public void Reload()
    {
        _isReload = true;
        if (_reloadGauge)
            _reloadGauge.gameObject.SetActive(true);
    }

    public void SetPower(float power)
    {
        _power = power;
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

}
