using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour, ITypeDefine
{
    protected Character _character;
    protected Player _player;
    protected AudioSource _audioSource;
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

    [SerializeField][Range(0,1)] float _audioLength;
    Coroutine _audioCoroutine;

    [SerializeField]protected bool _isRaycast;
    [SerializeField] protected bool _isAuto;
    public bool IsAuto => _isAuto;

    [SerializeField] protected float _power = 1;
    public float Power => _power;
    [SerializeField] protected float _bulletSpeed = 15;
    public float BulletSpeed => _bulletSpeed;

    [SerializeField] protected int _damage = 1;
    public int Damage => _damage;

    [SerializeField] int _penerstratingPower;
    public int PenerstratingPower => (_penerstratingPower+ (_player? _player.PenerstratingPower:0));

    [SerializeField] protected int _maxAmmo;
    public int MaxAmmo => _maxAmmo;
    [SerializeField]protected int _currentAmmo;
    public int CurrentAmmo => _currentAmmo;

    [SerializeField] protected bool _isAllReloadAmmo;
    public bool IsAllReloadAmmo => _isAllReloadAmmo;
    [SerializeField] protected float _fireDelay;
    public float FireDelay => _fireDelay;
    [SerializeField] protected float _reloadDelay;
    public float ReloadDelay => _reloadDelay - (_player? (_player.ReduceReloadTime /100f)* _reloadDelay:0);

    [SerializeField] protected GameObject _firePosition;
    public GameObject FirePosition => _firePosition;

    [SerializeField]protected float _rebound;
    public float Rebound => _rebound;

    protected float _fireElapsed;
    protected float _reloadElapsed;

    protected bool _isReload;
    public bool IsReload => _isReload;

    [SerializeField] protected GaugeBar _reloadGauge;

    [SerializeField] protected Define.EffectName _hitEffect;

    public void Init(Character character)
    {
        _audioSource = GetComponent<AudioSource>();
        _currentAmmo = _maxAmmo;
        _character = character;
        _reloadGauge = _character.transform.Find("GagueBar").GetComponent<GaugeBar>();
        
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

        // 재장전 중이라면 재장전을 멈춥니다.
        if (_isReload)
        {
            _isReload = false;
            if(_reloadGauge)
                _reloadGauge.gameObject.SetActive(false);
            _reloadElapsed = 0;
        }


        _currentAmmo--;
        _fireElapsed = 0;

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        float angle = FirePosition.transform.rotation.eulerAngles.z;
        angle = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x/ Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
        direction = direction.normalized;
        if (!_isRaycast)
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Projectile");
            go.transform.position = _firePosition.transform.position;
            Projectile projectile = go.GetComponent<Projectile>();
            projectile.Init(Power, BulletSpeed, Damage,Define.CharacterType.Enemy,PenerstratingPower);
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
                    character.Damage(Character, _damage, _power, direction);
                }

            }
        }

        Player?.Rebound(_rebound);
    }
    public void Update()
    {
        if (_fireElapsed < _fireDelay)
            _fireElapsed += Time.deltaTime;

        if (_isReload && _reloadElapsed < _reloadDelay)
        {
            _reloadElapsed += Time.deltaTime;
            if(_reloadGauge)
                _reloadGauge.SetRatio(_reloadElapsed, _reloadDelay);
        }
        else if(_isReload && _reloadElapsed >= _reloadDelay)
        {
            if (_isAllReloadAmmo)
            {
                _isReload = false;
                _currentAmmo = _maxAmmo;
                _reloadElapsed = 0;
                if (_reloadGauge)
                    _reloadGauge.gameObject.SetActive(false);
            }
            else
            {
                _currentAmmo++;
                if(_currentAmmo >= _maxAmmo)
                {
                    _currentAmmo = _maxAmmo;
                    _isReload = false;
                    if (_reloadGauge)
                        _reloadGauge.gameObject.SetActive(false);
                }
                _reloadElapsed = 0;
            }
        }
    }

    public void Reload()
    {
        _isReload = true;
        if (_reloadGauge)
        {
            _reloadGauge.SetRatio(0, 1);
            _reloadGauge.gameObject.SetActive(true);
        }
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

    IEnumerator CorPlayAudio()
    {
        if (_audioSource) { 
        _audioSource.Stop();
        _audioSource.Play();
        yield return new WaitForSeconds(_audioLength);
        _audioSource.Stop();
            }
    }
}
