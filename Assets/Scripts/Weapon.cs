using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour, ITypeDefine
{
    protected IWeaponUsable _user;
    protected AudioSource _audioSource;

    [SerializeField] protected bool _debug;

    [SerializeField]protected Define.WeaponName _weaponName;
    [field:TextArea][field:SerializeField] public string WeaponDescription {private set; get; }
    public Define.WeaponName WeaponName =>_weaponName;

    [SerializeField] protected Define.ProjectileName _bulletName;
    [SerializeField][Range(0,1)] float _audioLength;
    protected Coroutine _audioCoroutine;

    [Header("총기 능력치")]

    [SerializeField] protected bool _isAuto;
    public bool IsAuto => _isAuto;

    [SerializeField] protected float _knockBackPower = 1;
    public float OriginalKnockBackPower => _knockBackPower;
    public float KnockBackPower => _knockBackPower;

    [SerializeField] protected float _stunTime = 0f;
    public float StunTime => _stunTime;
    [SerializeField] protected float _bulletSpeed = 15;
    public float BulletSpeed => _bulletSpeed;

    [SerializeField] protected float _attackMultiplier = 1;
    public float AttackMutiplier => _attackMultiplier;
    [SerializeField] protected int _attackPower = 1;
    public float OriginalAttackPower => _attackPower;

    public int AttackPower
    {
        get
        {
            int result = 0;
            result += Mathf.RoundToInt(_attackPower * (1+IncreasedAttackPowerPercentage / 100) * (1 + _user.GetIncreasedAttackPowerPercentage()/100));
            return result;
        }
    }
    
    [SerializeField] int _penerstratingPower;
    public int OriginalPenerstratingPower => _penerstratingPower;
    public int PenerstratingPower => (_penerstratingPower + IncreasedPenerstratingPower);

    [SerializeField] protected int _maxAmmo;
    public int MaxAmmo => _maxAmmo;
    [SerializeField]protected int _currentAmmo;
    public int CurrentAmmo => _currentAmmo;

    [SerializeField] protected bool _isAllReloadAmmo;
    public bool IsAllReloadAmmo => _isAllReloadAmmo;
    [SerializeField] protected float _attackSpeed;
    public float OriginalAttackSpeed => _attackSpeed;
    public float AttackSpeed => (_attackSpeed * (1 + IncreasedAttackSpeedPercentage / 100) * (1+ _user.GetIncreasedAttackSpeedPercentage()/100));
    [SerializeField] protected float _reloadTime;
    public float OriginalReloadTime => _reloadTime;
    public float ReloadTime => (_reloadTime *(1- DecreasedReloadTimePercentage/ 100) /(1 + _user.GetIncreasedReloadSpeedPercentage()/100));


    // 추가 능력치
    public float IncreasedAttackPowerPercentage { set; get; }
    public float IncreasedKnockbackPowerPercentage { set; get; }
    public int IncreasedPenerstratingPower { set; get; }
    public float IncreasedAttackSpeedPercentage { set; get; }
    public float DecreasedReloadTimePercentage { set; get; }


    [SerializeField] protected GameObject _firePosition;
    public GameObject FirePosition => _firePosition;

    [SerializeField]protected float _rebound;
    public float Rebound => _rebound;

    protected float _fireElapsed;

    protected float _reloadElapsed;
    public float ReloadElapsed => _reloadElapsed;

    protected bool _isReload;
    public bool IsReload => _isReload;

    [SerializeField] protected GaugeBar _reloadGauge;

    [SerializeField] protected Define.EffectName _hitEffect;

    protected bool _fastReloadFailed;
    public GameObject HandlePosition { private set; get; }

    public int UpgradeCount { set; get; }

    public void Init(IWeaponUsable user)
    {
        _audioSource = GetComponent<AudioSource>();
        _currentAmmo = _maxAmmo;
        _user= user;
        _reloadGauge = _user.Character.transform.Find("GagueBar").GetComponent<GaugeBar>();
        _firePosition = transform.Find("FirePoint").gameObject;
        HandlePosition = transform.Find("HandlePosition").gameObject;
        
        if(_reloadGauge)
            _reloadGauge.gameObject.SetActive(false);

    }
    public virtual void Fire(IWeaponUsable user)
    {
        if (_currentAmmo <= 0)
        {
            Reload(user);
            return;
        }

        if (_fireElapsed < (1/AttackSpeed )) return;

        // 재장전 중이라면 재장전을 멈춥니다.
        if (_isReload)
        {
            CancelReload();
        }
        _fireElapsed = 0;

        FireBullet(user);
    }

    public virtual void FireBullet(IWeaponUsable user)
    {
       

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
        fireFlare.transform.localScale = _firePosition.transform.lossyScale;
        fireFlare.transform.rotation = _firePosition.transform.rotation;
        fireFlare.Play(_firePosition.transform.position);
        // ---------------

        Managers.GetManager<GameManager>().CameraController.ShakeCamera(0.2f, 0.1f);

        Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)_bulletName);
        if (projectile != null)
        {
            projectile.transform.position = _firePosition.transform.position;
            float damage = AttackPower;

            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);


            user?.Rebound(_rebound);
        }
        if (Managers.GetManager<GameManager>().Inventory.GetItemCount(ItemName.유령탄환) > 0)
        {
            if (Random.Range(0, 100) < 10)
            {
                _currentAmmo++;
            }
        }
        _currentAmmo--;
    }
    protected virtual void Update()
    {
        // 발사 딜레이
        if (_fireElapsed < 1/AttackSpeed)
            _fireElapsed += Time.deltaTime;
        Reloading();
    }

    public virtual void Reloading()
    {
        if (_isReload && _reloadElapsed < ReloadTime)
        {
            _reloadElapsed += Time.deltaTime;
            if (_reloadGauge)
                _reloadGauge.SetRatio(_reloadElapsed, ReloadTime);
        }
        else if (_isReload && _reloadElapsed >= ReloadTime)
        {
            CompleteReload();
        }
    }
    public virtual void Reload(IWeaponUsable user)
    {
        if (_maxAmmo <= _currentAmmo) return;
        if (_isReload) return;

        _fastReloadFailed = false;
        _isReload = true;
        if (_reloadGauge)
        {
            if (user is Player player)
            {
                _reloadGauge.DisablePoint();
            }
            _reloadGauge.SetRatio(0, 1);
            _reloadGauge.gameObject.SetActive(true);
        }
    }
    public virtual void ReloadHold()
    {
     
    }

    public virtual void ReloadUp()
    {

    }

    public virtual void FastReload(IWeaponUsable user)
    {
        if(!_isReload) return;
        if (_fastReloadFailed) return;

        float ratio = ReloadElapsed / ReloadTime;
        if (ratio > 0.7f && ratio < 0.9f)
        {
            CompleteFastReload(user);
        }
        else
        {
            _fastReloadFailed = true;
        }
    }
    public virtual void CompleteReload(bool isHideGauge = true)
    {
        if (_isAllReloadAmmo)
        {
            _isReload = false;
            
            _currentAmmo = _maxAmmo;
            _reloadElapsed = 0;
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
                if (_reloadGauge && isHideGauge)
                    _reloadGauge.gameObject.SetActive(false);
            }
            _reloadElapsed = 0;
        }
    }

    public virtual void CompleteFastReload(IWeaponUsable user)
    {
        if (_isAllReloadAmmo)
        {
            _isReload = false;

            if (!_fastReloadFailed && user is Player player)
            {
                _currentAmmo = _maxAmmo;
            }

            _reloadElapsed = 0;
            if (_reloadGauge)
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
            _reloadElapsed = 0;
        }
    }

    public virtual void CancelReload()
    {
        _isReload = false;
        _reloadGauge.gameObject.SetActive(false);
        _reloadGauge.SetRatio(0, 1);
        _reloadElapsed = 0;
    }

    public void SetPower(float power)
    {
        _knockBackPower = power;
    }

    public void SetDamage(int damage)
    {
        _attackPower = damage;
    }

    public void IncreaseMaxAmmo(int count)
    {
        _maxAmmo += count;
    }

    public void DecreaseReloadDelay(float delay)
    {
        _reloadTime -= delay;
    }

    public int GetEnumToInt()
    {
        return (int)WeaponName;
    }

    protected IEnumerator CorPlayAudio()
    {
        if (_audioSource) { 
        _audioSource.Stop();
        _audioSource.Play();
        yield return new WaitForSeconds(_audioLength);
        _audioSource.Stop();
            }
    }
}
