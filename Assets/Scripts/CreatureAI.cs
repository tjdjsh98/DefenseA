using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class CreatureAI : MonoBehaviour
{
    Character _character;
    public Character Character => _character;
    Player _player;
    BoxCollider2D _boxCollider;
    GameObject _model;
    GameObject _soulModel;

    [SerializeField] CreatureAbility _creatureAbility = new CreatureAbility();
    public CreatureAbility CreatureAbility=>_creatureAbility;
    /*
     * 0 = 일반 공격 적 인식 범위
     * 1 = 일반공격
     * 2 = 관통공격
     * 3 = 바닥에서 치솟는 스피어 공격
     * 4 = 땅구르기 공격
     */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();

    float _girlToCreatureDistance = 5f;
    Character _enemyToAttack;
    bool _isMove = false;
    bool _isSoulForm = false;

    // 스킬
    Dictionary<SkillName, Action<SkillSlot>> _skillDictionary = new Dictionary<SkillName, Action<SkillSlot>>();
    public int AttackDamage
    {
        get
        {
            int ap = _character.AttackPower;
            if (_creatureAbility.GetIsHaveAbility(CreatureAbilityName.Rage))
                ap = Mathf.RoundToInt(ap * 1.5f);
            return ap;
        }
    }
    public float AttackSpeed
    {
        get
        {
            float attackSpeed = 1;
            if (_creatureAbility.GetIsHaveAbility(CreatureAbilityName.Rage))
                attackSpeed *= 1.5f;

            return attackSpeed;
        }
    }

    // 관통공격
    [Header("관통공격")]
    [SerializeField] PenetrateAttack _penetrateAttack;
    [SerializeField] GameObject _fArmIK;
    Character _closeOne;


    // 일반공격 변수
    Vector3 _normalAttackPosition;
    float _normalAttackElapsed = 0;
    float _normalAttackCoolTime = 2f;
    float NormalAttackCoolTime => DecreasedNormalAttackCoolTimePercentage > 0 ? _normalAttackCoolTime / (1 + (DecreasedNormalAttackCoolTimePercentage / 100)) : _normalAttackCoolTime * (1 - (DecreasedNormalAttackCoolTimePercentage / 100));
    public int NormalAttackDamage => IncreasedNormalAttackPercentage > 0 ? AttackDamage * (1 + IncreasedNormalAttackPercentage / 100) : AttackDamage / (1 - IncreasedNormalAttackPercentage / 100);
    public int IncreasedNormalAttackPercentage { set; get; }
    public float IncreasedNormalAttackSpeedPercentage { set; get; }
    public float DecreasedNormalAttackCoolTimePercentage { set; get; }


    // 스피어 공격 변수
    float _spearAttackElapsed = 0;
    [SerializeField] float _spearAttackCoolTime = 3f;

    // 쇼크웨이브 공격 변수
    float _shockwaveElasped = 0;
    float _shockwaveCoolTime = 20;
    public float ShockwaveCoolTime => DecreasedShockwaveCoolTimePercentage > 0 ? _shockwaveCoolTime / (1 + (DecreasedShockwaveCoolTimePercentage / 100)) : _shockwaveCoolTime * (1 - (DecreasedShockwaveCoolTimePercentage / 100));
    public float DecreasedShockwaveCoolTimePercentage { set; get; }
    public int ShockwaveCount { set; get; } = 1;
    [SerializeField] float _shockwaveRange = 20;
    public float ShockwaveRange => IncreasedShockwaveRangePercentage > 0 ? _shockwaveRange * (1 + IncreasedShockwaveRangePercentage / 100) : _shockwaveRange / (1 - IncreasedShockwaveRangePercentage / 100);
    public float IncreasedShockwaveRangePercentage { set; get; } = 0;

    public float IncreasedShockwaveDamagePercentage { set; get; } = 500;
    public int ShockwaveDamage => IncreasedShockwaveDamagePercentage > 0 ? Mathf.RoundToInt(AttackDamage * (1 + IncreasedShockwaveDamagePercentage / 100)) : Mathf.RoundToInt(AttackDamage / (1 + IncreasedShockwaveDamagePercentage / 100));

    // 땅구르기
    float _stempGroundElaspsedTime;
    float _stempGroundCoolTime = 5;
    int StempGroundDamage => IncreasedStempGroundDamagePercentage > 0 ? Mathf.RoundToInt(AttackDamage * (1 + (IncreasedStempGroundDamagePercentage / 100))) : Mathf.RoundToInt(AttackDamage / (1 - (IncreasedStempGroundDamagePercentage / 100)));
    public float IncreasedStempGroundDamagePercentage { set; get; } = 200;
    float _stempGroundPower = 50;
    public float StempGroundPower => IncreasedStempGroundPowerPercentage > 0 ? _stempGroundPower * (1 + (IncreasedStempGroundPowerPercentage / 100)) : _stempGroundPower / (1 - (IncreasedStempGroundPowerPercentage / 100));
    public float IncreasedStempGroundPowerPercentage { set; get; } = 200;
    float _stempGroundRange = 10;
    public float StempGroundRange => IncreasedStempGroundRangePercentage > 0 ? _stempGroundRange * (1 + IncreasedStempGroundRangePercentage / 100) : _stempGroundRange / (1 - IncreasedStempGroundRangePercentage / 100);
    public float IncreasedStempGroundRangePercentage { set; get; }

    // 반경 테스트 변수
    Vector3 _tc;
    float _tr;


    // 던지기
    [SerializeField] float _throwPower;

    // 스킬 슬롯
    [SerializeField] int _creatureSkillCount;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _model = transform.Find("Model").gameObject;
        _soulModel = transform.Find("SoulModel").gameObject;
        _creatureAbility.Init(this);

        Managers.GetManager<GameManager>().CreatureAI = this;
        _character.CharacterDeadHandler += OnCharacterDead;

        RegistSkill();
    }

    void RegistSkill()
    {
        _skillDictionary.Add(SkillName.Shockwave, PlayShockwave);
        _skillDictionary.Add(SkillName.Smash, PlaySmash);
        _skillDictionary.Add(SkillName.StempGround, PlayStempGround);
    }

    private void OnCharacterDead()
    {
        _model.gameObject.SetActive(false);
        _character.Move(Vector2.zero);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if (_attackRangeList.Count <= _debugAttackRangeIndex) return;

        _creatureAbility.OnDrawGizmosSelected();

        Define.Range range = _attackRangeList[_debugAttackRangeIndex];
        range.center.x = transform.lossyScale.x > 0 ? range.center.x : -range.center.x;

        if (range.figureType == Define.FigureType.Box)
            Gizmos.DrawWireCube(transform.position + range.center, range.size);
        else if (range.figureType == Define.FigureType.Circle)
            Gizmos.DrawWireSphere(transform.position + range.center, range.size.x);
        Gizmos.DrawWireSphere(_tc, _tr);
    }

    private void Update()
    {
        if (_isSoulForm)
        {
            Player player = Managers.GetManager<GameManager>().Player;

            _character.TurnBody(player.transform.position - transform.position);
            Vector3 offset = new Vector3(-3, 5, 0);
            offset.y += Mathf.Sin(Time.time * 0.5f + gameObject.GetInstanceID());
            if (player.transform.localScale.x < 0)
                offset.x = -offset.x;

            transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, 0.01f);

            return;
        }
        FollowPlayer();
        _creatureAbility.AbilityUpdate();

    }

    void FollowPlayer()
    {
        if (_character.IsAttack) return;
        if (_player == null)
        {
            _player = Managers.GetManager<GameManager>().Player;
            return;
        }

        Vector3 distacne = _player.transform.position - transform.position;
        if (Mathf.Abs(distacne.y) < 3 && Mathf.Abs(distacne.x) > _girlToCreatureDistance)
        {
            _character.Move(Vector3.right * (distacne.x + (distacne.x > 0 ? -_girlToCreatureDistance : _girlToCreatureDistance)) / (_girlToCreatureDistance));
        }
    }

    public void Transform()
    {
        if (!_isSoulForm)
        {
            _boxCollider.enabled = false;
            _isSoulForm = true;
            _model.gameObject.SetActive(false);
            _soulModel.gameObject.SetActive(true);
            _character.ChangeEnableFly(true);
        }
        else
        {
            Vector3? position = Util.GetGroundPosition(transform.position);
            if (position != null)
                transform.position = Util.GetGroundPosition(transform.position).Value;

            _boxCollider.enabled = true;
            _isSoulForm = false;
            _model.gameObject.SetActive(true);
            _soulModel.gameObject.SetActive(false);
            _character.ChangeEnableFly(false);
        }
    }

   

    public void AimFrontArmToEnemy()
    {
        Animation anim = GetComponent<Animation>();
        AnimationCurve curve;

        // create a new AnimationClip
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;

        // create a curve to move the GameObject and assign to the clip
        Keyframe[] keys;
        keys = new Keyframe[3];
        keys[0] = new Keyframe(0.0f, 0.0f);
        keys[1] = new Keyframe(1.0f, 1.5f);
        keys[2] = new Keyframe(2.0f, 0.0f);
        curve = new AnimationCurve(keys);
        clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

        // update the clip to a change the red color
        curve = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 0.0f);
        clip.SetCurve("", typeof(Material), "_Color.r", curve);
        _fArmIK.transform.position = _closeOne.transform.position;
    }

    public void StartPenerstrateAttack()
    {
        _penetrateAttack.StartAttack(_character, _closeOne.GetCenter() - _penetrateAttack.transform.position, 20);

    }
   

    public void NormalAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.CreatureSkillRange.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(c, AttackDamage, 100, c.transform.position - transform.position);
                }
            }
        }
    }
  

    Character GetCloseEnemy()
    {
        Character close = null;
        Character player = Managers.GetManager<GameManager>().Girl;

        if (player != null)
            Util.RangeCastAll2D(player.gameObject, _attackRangeList[0], Define.CharacterMask, (go) =>
            {
                if (go != null)
                {
                    Character character = go.GetComponent<Character>();
                    if (character == null || character.CharacterType != Define.CharacterType.Enemy) return false;
                    if (close == null || (close.transform.position - player.transform.position).magnitude > (go.transform.position - player.transform.position).magnitude)
                        close = character;
                }
                return true;
            });

        return close;
    }
    public void AirBorneAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.CreatureSkillRange.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {

                    _character.Attack(c, AttackDamage, 100, Vector3.up, 0.4f);
                }
            }
        }
    }

    public void FinishNormalAttack()
    {
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _character.SetAnimationSpeed(1);
    }


    public void UseSkill(SkillSlot slot)
    {
        if(slot.skillData == null) return;

        if (_skillDictionary.TryGetValue(slot.skillData.skillName, out var func))
        {
            func?.Invoke(slot);
        }
    }

    void PlayShockwave(SkillSlot slot)
    {
        if (ShockwaveCount >= 1)
            StartCoroutine(CorShockwaveAttack());
        if (ShockwaveCount >= 2)
            StartCoroutine(CorShockwaveAttack(0.2f, 1));
        _shockwaveElasped = 0;

        slot.skillTime = 0;
    }

    IEnumerator CorShockwaveAttack(float later = 0, int num = 0)
    {
        if (later != 0)
            yield return new WaitForSeconds(later);
        List<GameObject> characterList = new List<GameObject>();
        characterList.Clear();
        Vector3 center = transform.position;
        _tc = center;
        float radius = 0;
        Camera.main.GetComponent<CameraController>().ShockWave(center, 30, num);
        while (radius < ShockwaveRange)
        {
            radius += Time.deltaTime * 30;
            _tr = radius;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(center, radius, Vector2.zero, 0);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (characterList.Contains(hit.collider.gameObject)) continue;

                    characterList.Add(hit.collider.gameObject);

                    Character character = hit.collider.gameObject.GetComponent<Character>();
                    if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        _character.Attack(character, ShockwaveDamage, 50, character.transform.position - center);
                    }
                }
            }
            yield return null;
        }

        Camera.main.GetComponent<CameraController>().StopShockwave(num);
    }
   

    void PlaySmash(SkillSlot skillSlot)
    {
        if (skillSlot.isActive) return;

        skillSlot.isActive = true;

        StartCoroutine(CorPlaySmash(skillSlot));
    }
    IEnumerator CorPlaySmash(SkillSlot slot)
    {
        Character enemy = GetCloseEnemy();
        if (enemy != null)
        {
            Vector3 initPosition = transform.position;
            _character.IsAttack = true;
            Define.Range attackRange = _attackRangeList[1];
            attackRange.size = new Vector3(attackRange.size.x / 4 * 3, attackRange.size.y / 4 * 3, 0);
            while (true)
            {
                if (enemy == null) break;
                _character.Move(enemy.transform.position - transform.position);

                if (Util.RangeCastAll2D(gameObject, attackRange, Define.CharacterMask, (go) =>
                {
                    Character character = go.GetComponent<Character>();
                    if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        return true;
                    }

                    return false;
                }).Length > 0) break;
                yield return null;
            }
            _character.Move(Vector2.zero);

            _character.SetAnimationSpeed(AttackSpeed);
            _character.AnimatorSetTrigger("NormalAttack");
            _character.IsAttack = true;
            _character.IsEnableMove = false;
            _character.IsEnableTurn = false;
            _enemyToAttack = null;

            while (_character.IsAttack)
            {
                yield return null;
            }
        }
        slot.isActive = false;
        slot.skillTime = 0;
    }
   
    void PlayStempGround(SkillSlot slot)
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[4]);
        Effect effectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.StempGround);
        Effect effect = Managers.GetManager<ResourceManager>().Instantiate(effectOrigin);
        effect.SetProperty("Range", _attackRangeList[4].size.x);
        effect.Play(transform.position);
        if (gos.Length > 0)
        {
            _spearAttackElapsed = 0;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(c, StempGroundDamage, StempGroundPower, Vector3.up, 1);
                }
            }
        }
        slot.skillTime = 0;
    }

    // 미사용 스킬
    /*
    void ThrowPlayer(CreatureSkillSlot creatureSkillSlot)
    {
        if (creatureSkillSlot.creatureSkill == Define.CreatureSkill.Throw)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Managers.GetManager<InputManager>().MouseWorldPosition;
                Vector3 directionVel = mousePosition - transform.position;

                float power = directionVel.magnitude * _throwPower;
                if (power > 200)
                    power = 200;
                _player.Character.Damage(_character, 0, power, directionVel, 0);
                creatureSkillSlot.skillTime = 0;
            }
        }
    }
    */
}