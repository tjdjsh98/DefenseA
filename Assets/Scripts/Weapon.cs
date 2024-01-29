using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour, ITypeDefine
{
    protected Character _character;
    protected Player _player;

    protected Character Character
    {
        get { if (_character == null) _character = GetComponentInParent<Character>(); return _character; }
    }
    protected Player Player
    {
        get { if(_player == null) _player = GetComponentInParent<Player>(); return _player; }
    }


    [SerializeField]protected Define.WeaponName _weaponName;
    public Define.WeaponName WeaponName =>_weaponName;

    [SerializeField]protected bool _isRaycast;
    public GameObject _projectile;


    [SerializeField] protected float _power = 1;
    public float Power => _power;
    [SerializeField] protected float _bulletSpeed = 15;
    public float BulletSpeed => _bulletSpeed;

    [SerializeField] protected int _damage = 1;
    public int Damage => _damage;

    [SerializeField] protected int _maxAmmo;
    public int MaxAmmo => _maxAmmo;
    [SerializeField]protected int _currentAmmo;
    public int CurrentAmmo => _currentAmmo;

    [SerializeField] protected float _fireDelay;
    public float FireDelay => _fireDelay;
    [SerializeField] protected float _reloadDelay;
    public float ReloadDelay => _reloadDelay;

    [SerializeField] protected GameObject _firePosition;

    [SerializeField]protected float _rebound;
    public float Rebound => _rebound;

    protected float _fireElapsed;
    protected float _reloadElapsed;

    protected bool _isReload;

    [SerializeField] protected HpBar _reloadGauge;

    [SerializeField] protected Define.EffectName _hitEffect;

    private void Awake()
    {
        _currentAmmo = _maxAmmo;
        if(_reloadGauge)
            _reloadGauge.gameObject.SetActive(false);

    }
    public virtual void Fire(Character fireCharacter)
    {
        if (_currentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (_fireElapsed < _fireDelay) return;

        _currentAmmo--;
        _fireElapsed = 0;


        float angle = transform.rotation.eulerAngles.z;

        angle = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x/ Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
        direction = direction.normalized;
        if (!_isRaycast)
        {
            GameObject go = Instantiate(_projectile);
            go.transform.position = _firePosition.transform.position;
            Projectile projectile = go.GetComponent<Projectile>();
            projectile.Init(_power, _bulletSpeed, _damage,Define.CharacterType.Enemy);


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
                    character.Damage(Character, _damage, _power, direction.x > 0 ? Vector3.right: Vector3.left);
                }

            }
        }

        Player?.OutAngle(_rebound);
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

    public void IncreaseMaxAmmo(int count)
    {
        _maxAmmo += count;
    }

    public void DecreaseReloadDelay(float delay)
    {
        _reloadDelay -= delay;
    }

    public int GetEnumToInt()
    {
        return (int)WeaponName;
    }
}
