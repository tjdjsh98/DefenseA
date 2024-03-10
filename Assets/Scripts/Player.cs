using MoreMountains.FeedbacksForThirdParty;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    Animator _animator;

    Character _character;
    public Character Character => _character;
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;

    [SerializeField] bool _debug;

    [SerializeField] Define.Range _meleeAttackRange;


    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _backArmIK;
    [SerializeField] GameObject _head;
    [SerializeField] GameObject _body;

    public int ReduceReloadTime { set; get; } = 0;

    [SerializeField] float _initHeadAngle;
    [SerializeField] float _initBodyAngle;
    [SerializeField]float _initFrontArmAngle;

    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    [SerializeField] float _rightPadding;
    [SerializeField] float _upPadding;

    CameraController _cameraController;

    float _rebound;
    float _reboundControlPower = 50f;
    public float ReboundControlPower => IncreasedReboundControlPowerPercent > 0 ? _reboundControlPower * (1 + IncreasedReboundControlPowerPercent / 100) : _reboundControlPower / (1 - IncreasedReboundControlPowerPercent / 100);

    [SerializeField]float _reboundRecoverPower = 20f;
    public float ReboundRecoverPower => IncreasedReboundRecoverPercent > 0 ? _reboundRecoverPower * (1 + IncreasedReboundRecoverPercent / 100) : _reboundRecoverPower / (1 - IncreasedReboundRecoverPercent / 100);
    bool _isRiding;

    // 추가적인 능력
    public int IncreasedPenerstratingPower { set; get; } = 0;
    public float DecreasedFireDelayPercent { set; get; }
    public float IncreasedAttackSpeedPercentage { set; get; }
    public float IncreasedReloadSpeedPercent { set; get; }
    public float IncreasedReboundControlPowerPercent { set; get; }
    [field:SerializeField]public float IncreasedReboundRecoverPercent { set; get; }

    float _plentyOfBulletsIncreasedAttackPointPercentage;
    float _maxElectric = 10;
    public float MaxElectric => _maxElectric;
    public float CurrentElectric { set;get; }
    

    public float IncreaseAttackPowerPercentage => _plentyOfBulletsIncreasedAttackPointPercentage;
    // 능력 언락부분
    public Dictionary<Define.GirlAbility, bool> AbilityUnlocks { set; get; } = new Dictionary<Define.GirlAbility, bool>();
    
    // 자동장전
    List<float> _autoReloadElaspedTimeList = new List<float>();

    // 검은구체
    List<BlackSphere> _blackSphereList = new List<BlackSphere>();
    int _maxBlackSphereCount = 10;
    public List<BlackSphere> BlackSphereList => _blackSphereList;


    [SerializeField] bool _bounce;
    [SerializeField] float _power;

    [SerializeField] PositionLimit _eyeClose;
    float _eyeCloseElasepdTime;
    float _eyeCloseTime = 4;

    bool _isSliding;
    bool _isRun;
    bool _isFire;

    float _runToFireCoolTime = 0.2f;
    float _runToFireElaspedTime = 0f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _character = GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _cameraController = Camera.main.GetComponent<CameraController>();

        _character.AttackHandler += OnAttack;

        _initHeadAngle = _head.transform.rotation.eulerAngles.z;
        _initBodyAngle = _body.transform.rotation.eulerAngles.z;
        _initFrontArmAngle = _frontArm.transform.rotation.eulerAngles.z;

        _character.CharacterDead += OnCharacterDead;

        Managers.GetManager<GameManager>().Player = this;
        Managers.GetManager<InputManager>().MouseButtonDownHandler += UseWeapon;
        Managers.GetManager<InputManager>().MouseButtonHoldHandler += AutoUseWeapon;
        Managers.GetManager<InputManager>().ReloadKeyDownHandler += OnReloadKeyDown;
        Managers.GetManager<InputManager>().ReloadKeyHoldHandler += OnReloadKeyHold;
        Managers.GetManager<InputManager>().ReloadKeyUpHandler += OnReloadKeyUp;
        HandleMove();

        Managers.GetManager<InputManager>().Num1KeyDownHandler += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDownHandler += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDownHandler += () => _weaponSwaper.SelectWeapon(2);
        Managers.GetManager<InputManager>().JumpKeyDownHandler += _character.Jump;

    }
    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _meleeAttackRange, Color.red);
    }
    private void OnCharacterDead()
    {
        SceneManager.LoadScene("MainMenu");

    }

    private void OnReloadKeyDown()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;


        if (AbilityUnlocks.TryGetValue(Define.GirlAbility.FastReload,out bool value)&& value)
        {
            _weaponSwaper.CurrentWeapon.FastReload();
        }
        _weaponSwaper.CurrentWeapon.Reload();
    }

    private void OnReloadKeyHold()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

        _weaponSwaper.CurrentWeapon.ReloadHold();
    }
    private void OnReloadKeyUp()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

        _weaponSwaper.CurrentWeapon.ReloadUp();
    }
    public void Update()
    {
        if(Managers.GetManager<GameManager>().IsPlayTimeline) return;
       
        if (_bounce)
        {
            _rigidbody.AddForce(Vector2.up * _power,ForceMode2D.Impulse);
            _bounce = false;
        }
        // 임시 공격

        if (Input.GetMouseButtonDown(1) && !_isSliding)
        {
            _character.IsAttack = true;
            _animator.SetTrigger("MeleeAttack");
        }

        HandleSliding();
        TurnBody();
        RotateArm();
        RotateBody();
        Riding();
        Roll();
        AutoReload();
        PlentyOfBullets();
        Electric();
        _eyeCloseElasepdTime += Time.deltaTime;
        if(_eyeCloseElasepdTime > _eyeCloseTime)
        {
            _eyeCloseElasepdTime = 0;
            _eyeCloseTime = UnityEngine.Random.Range(6f,7f);
            _eyeClose.PlayRound(3
                );
        }


        for(int i = _blackSphereList.Count-1; i >= 0 ; i--) 
        {
            if (_blackSphereList[i] == null)
                _blackSphereList.RemoveAt(i);
        }

        // 달리는 중에는 방향에 따라 이동, 걷는 중에는 총방향에 따라서
        if (!_isRun)
        {
            _character.IsTurnBodyAlongVelocity= false;
        }
        // 달리기 -> 총 전환 시 약간의 텀
        _runToFireElaspedTime += Time.deltaTime;
        _isRun = false;
        _isFire = false;

    }

    void HandleSliding()
    {
        if (!_isSliding)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                bool isRight = _character.MySpeed.x > 0;
                if(isRight)
                    _character.TurnBody(isRight ? Vector3.right : Vector3.left);
                _rigidbody.AddForce(Vector2.right * 150 * (isRight ? 1 : -1), ForceMode2D.Impulse);
                _isSliding = true;
                _character.IsEnableMove = false;
                _character.AnimatorSetBool("Sliding", _isSliding);
            }
        }
        else
        {
            if (Mathf.Abs(_rigidbody.velocity.x) < 0.1f)
            {
                _isSliding = false;
                _character.IsEnableMove = true;
                _character.AnimatorSetBool("Sliding", _isSliding);
            }
        }

    }
    public float GetIncreasedDamagePercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;
        percentage += _plentyOfBulletsIncreasedAttackPointPercentage;

        if (AbilityUnlocks.TryGetValue(Define.GirlAbility.LastStruggle, out bool value) && value)
        {
            if ((wall == null || wall.IsDead) && (creature == null || creature.IsDead))
                percentage += 100f;
        }
             
        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        if (AbilityUnlocks.TryGetValue(Define.GirlAbility.LastStruggle, out bool value) && value)
        {
            if ((wall == null || wall.IsDead) && (creature == null || creature.IsDead))
                percentage += 100f;
        }

        return percentage;
    }

    private void Electric()
    {
        if (AbilityUnlocks.TryGetValue(Define.GirlAbility.Electric, out bool value) && value)
        {
            if (_maxElectric > CurrentElectric)
                CurrentElectric += Time.deltaTime * 0.1f;
            else
                CurrentElectric = _maxElectric;
        }
    }

    public void MeleeAttack()
    {
        Util.RangeCastAll2D(gameObject, _meleeAttackRange, Define.CharacterMask,
            (go) =>
            {
                Character characrer = go.GetComponent<Character>();
                if (characrer && characrer.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(characrer, _character.AttackPower, 100, characrer.transform.position - transform.position,0.2f);
                }
                return true;
            });
    }

    void PlentyOfBullets()
    {
        if (_weaponSwaper.CurrentWeapon)
        {
            if (GetIsHaveAbility(Define.GirlAbility.PlentyOfBullets))
            {
                int amount = _weaponSwaper.CurrentWeapon.MaxAmmo / 10;
                _plentyOfBulletsIncreasedAttackPointPercentage = amount * 10f;
            }
        }
    }

    void OnAttack(Character target, int damage)
    {
        if (GetIsHaveAbility(Define.GirlAbility.BlackSphere))
        {
            if (_maxBlackSphereCount > _blackSphereList.Count)
            {
                // 20%의 확률로
                if (Random.Range(0, 5) == 0)
                {
                    GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/BlackSphere");
                    go.transform.position = target.transform.position;
                    BlackSphere blackSphere = go.GetComponent<BlackSphere>();
                    blackSphere.Init(_character, new Vector3(-3, 5));
                    _blackSphereList.Add(blackSphere);
                }
            }
        }
        if (GetIsHaveAbility(Define.GirlAbility.VolleyFiring))
        {
            if(_maxBlackSphereCount <= _blackSphereList.Count)
            {
                float delay = 0;
                foreach(BlackSphere blackSphere in _blackSphereList)
                {
                    blackSphere.ChangeAttackMode(target,delay);
                    delay += 0.1f;
                }
                _blackSphereList.Clear();
            }
        }


    }
    private void AutoReload()
    {
        if (AbilityUnlocks.TryGetValue(Define.GirlAbility.AutoReload, out bool value) && value)
        {

            for (int i = 0; i < _weaponSwaper.GetWeaponCount(); i++)
            {
                if (_autoReloadElaspedTimeList.Count <= i)
                    _autoReloadElaspedTimeList.Add(0);
                if (_weaponSwaper.WeaponIndex == i)
                {
                    _autoReloadElaspedTimeList[i] = 0;
                    continue;
                }

                Weapon weapon = _weaponSwaper.GetWeapon(i);
                if (weapon == null) continue;

                if (weapon.CurrentAmmo < weapon.MaxAmmo)
                {
                    if (_autoReloadElaspedTimeList[i] > weapon.ReloadDelay)
                    {
                        weapon.CompleteReload();
                        _autoReloadElaspedTimeList[i] = 0;
                    }
                    else
                    {
                        _autoReloadElaspedTimeList[i] += Time.deltaTime;
                    }
                }
            }
        }
    }

        public void SetReboundControlPower(float power)
    {
        _reboundControlPower = power;
    }

    void Roll()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!_character.IsStun)
            {
                _character.AnimatorSetTrigger("Roll");
            }
        }
    }
    public void Rebound(float angle)
    {
        _rebound = angle;

        if (_rebound > 45)
            _rebound = 45;
        
    }

    private void TurnBody()
    {
        if (!_isRun && !_isSliding)
        {
            if (Managers.GetManager<InputManager>().MouseWorldPosition.x < transform.position.x)
                _character.TurnBody(Vector2.left);
            else
                _character.TurnBody(Vector2.right);
        }
    }

    private void RotateBody()
    {
        if(_isRun || _isSliding) return;

        float bodyAngle = 0;
        float headAngle = 0;

        Vector3 distance = Vector3.zero;
        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
        mousePos.z = 0;

        distance = mousePos - _frontArm.transform.position;
        float angle = _frontArm.transform.rotation.eulerAngles.z - (transform.lossyScale.x > 0? 1 : -1) *_initFrontArmAngle;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;

        bodyAngle = angle / 4;
        headAngle = angle / 2;
        _body.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? bodyAngle + _initBodyAngle : bodyAngle - _initBodyAngle));
        _head.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? headAngle + _initHeadAngle : headAngle - _initHeadAngle));
    }

    private void RotateArm()
    {
        if (Time.timeScale == 0) return;
        if(_isRun) return;

        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
     
        if (_rebound > 0)
        {
            if ((_rebound - ReboundRecoverPower * Time.deltaTime) < 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound -= ReboundRecoverPower  * Time.deltaTime;
            }
        }
        if(_rebound < 0)
        {
            if ((_rebound + ReboundRecoverPower  * Time.deltaTime) > 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound += ReboundRecoverPower * Time.deltaTime;
            }
        }
      

        Vector3 distance = Vector3.zero;
        mousePos.z = 0;
        distance = mousePos - _frontArm.transform.position;
        float angle = Mathf.Atan2(distance.y, Mathf.Abs(distance.x)) * Mathf.Rad2Deg + _initFrontArmAngle * (transform.lossyScale.x > 0 ? 1 : 1);
        float weaponAngle = 0;

        if (_weaponSwaper.CurrentWeapon)
        {
            angle = 0;
            float armAngle = _frontArm.transform.eulerAngles.z * (transform.lossyScale.x > 0 ? 1 : -1);
            weaponAngle = _weaponSwaper.CurrentWeapon.FirePosition.transform.eulerAngles.z * (transform.lossyScale.x > 0 ? 1 : -1);

            Vector3 firePointToTarget = mousePos - _weaponSwaper.CurrentWeapon.FirePosition.transform.position;
            float firePointToTargetAngle = Mathf.Atan2(firePointToTarget.y, Mathf.Abs(firePointToTarget.x)) * Mathf.Rad2Deg;

            firePointToTargetAngle += _rebound;

            weaponAngle += 90;
            firePointToTargetAngle += 90;

            weaponAngle %= 360;
            firePointToTargetAngle %= 360;

            if (weaponAngle < -180) weaponAngle = weaponAngle + 360;

            if (firePointToTargetAngle > 180) firePointToTargetAngle = 179f;

            if (Mathf.Abs(Mathf.Abs(firePointToTargetAngle) - Mathf.Abs(weaponAngle) )> 0.1f)
            {
                if (weaponAngle < firePointToTargetAngle)
                {
                    if (firePointToTarget.magnitude > 5)
                    {
                        armAngle += (firePointToTargetAngle - weaponAngle);
                    }
                    else
                    {

                        armAngle += (firePointToTargetAngle - weaponAngle) * Time.deltaTime * 10f;
                    }
                }
                else
                {
                    if (firePointToTarget.magnitude > 5)
                        armAngle -= (weaponAngle - firePointToTargetAngle);
                    else
                        armAngle -= (weaponAngle - firePointToTargetAngle) * Time.deltaTime*10f;
                }
            }
            angle = armAngle ;

        }

        _frontArm.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle  : -angle ));
        if (_weaponSwaper.CurrentWeapon && _weaponSwaper.CurrentWeapon.HandlePosition)
        {
            _backArmIK.transform.position = _weaponSwaper.CurrentWeapon.HandlePosition.transform.position;
        }
        float screenWidth = Screen.width;
        Vector3 mousePosition = Input.mousePosition;

        if (mousePosition.x > screenWidth / 4 * 3)
        {
            _cameraController.ExpandsionView(Vector3.right);
        }
        else if (mousePosition.x < screenWidth / 4)
        {
            _cameraController.ExpandsionView(Vector3.left);
        }
    }

    private void HandleMove()
    {
        if (_isRiding) return;
        Managers.GetManager<InputManager>().RightArrowPressedHandler += OnRightArrowPressed;
        Managers.GetManager<InputManager>().LeftArrowPressedHandler += OnLeftArrowPressed;

    }

    void OnRightArrowPressed()
    {
        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }


        float run = 0.4f;
        if (!_isFire && Input.GetKey(KeyCode.LeftShift))
        {
            run = 1f;
            _character.IsTurnBodyAlongVelocity= true;
            _isRun= true;
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.right * run);
    }
    void OnLeftArrowPressed()
    {
        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }

        float run = 0.4f;
        if (!_isFire && Input.GetKey(KeyCode.LeftShift))
        {
            run = 1f;
            _character.IsTurnBodyAlongVelocity= true;
            _isRun = true;
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.left * run);
    }

    void UseWeapon()
    {
        if (Time.timeScale == 0) return;

        _isFire= true;
        if (_runToFireElaspedTime < _runToFireCoolTime) return;

        if (_weaponSwaper.CurrentWeapon != null  && !_weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);
       
    }
    void AutoUseWeapon()
    {
        if (Time.timeScale == 0) return;

        _isFire= true;
        if (_runToFireElaspedTime < _runToFireCoolTime) return;

        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);
    }

    bool CheckIsCloseEnemy()
    {
        Define.Range range = new Define.Range()
        {
            center = new Vector3(0, 0.5f, 0),
            size = new Vector3(1, 1, 0),
            figureType = Define.FigureType.Box
        };

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, range, Define.CharacterMask, (go)=>
        {
            Character character = go.GetComponent<Character>();
            if (character == null) return false;

            return character.CharacterType == Define.CharacterType.Enemy;
        });

        if (gameObjects.Length <= 0) return false;

        return true;
    }

    void Riding()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_isRiding == false)
            {
                GameObject[] gos = Util.RangeCastAll2D(gameObject, new Define.Range { center = new Vector3(0, 0), size = Vector3.one });

                if (gos.Length > 0)
                {
                    foreach (var go in gos)
                    {
                        if (go.gameObject.name.Equals("Dog"))
                        {
                            transform.SetParent(go.transform);
                            transform.position = go.GetComponent<WallAI>().SitPosition.transform.position;
                            _isRiding = true;
                            _character.AnimatorSetBool("Sit", true);
                            _ridingCharacter = go.GetComponent<Character>();
                            _character.IsEnableMove = false;
                            _rigidbody.isKinematic = true;
                        }
                    }
                }
            }
            else
            {
                _isRiding = false;
                _character.IsEnableMove = true;
                transform.SetParent(transform.parent.parent);
                _rigidbody.isKinematic = false;
                _character.AnimatorSetBool("Sit", false);
            }
        }
    }

    public bool GetIsHaveAbility(Define.GirlAbility girlAbility)
    {
        if (AbilityUnlocks.TryGetValue(girlAbility, out bool value) && value)
            return true;

        return false;
    }
    public void ResetRebound()
    {
        _rebound = 0;
        RotateArm();
    }
}
