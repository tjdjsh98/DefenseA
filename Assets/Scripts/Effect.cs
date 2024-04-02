using UnityEngine;
using UnityEngine.VFX;

public class Effect : MonoBehaviour, ITypeDefine
{
    [SerializeField] bool _debug;
    VisualEffect _visualEffect;

    [SerializeField] Define.EffectName _effectName;

    [SerializeField]float _offTime = 1f;
    float _eleasped;

    bool _isPlay;

    Character _attacker;
    bool _isAttack;
    int _attackPower;
    float _knockBackPower;
    float _stunTime;

    float _multiflyAttackSIze = 1;

    Define.CharacterType _enableAttackCharacter;
    [SerializeField]Define.Range _attackRange;

    [Header("애니메이션")]
    Animator _animator;
    [SerializeField] string _animatorTriggerName;
    [SerializeField]Vector2 _attackDirection;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _visualEffect = GetComponent<VisualEffect>();
        if(_visualEffect)
            _visualEffect.Stop();
        if(_animator)
            _animator.StopPlayback();
    }

    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _attackRange, Color.red);
    }

    private void Update()
    {
        if (_isPlay)
        {
            _eleasped += Time.deltaTime;
        }
        if(_eleasped >= _offTime)
        {
            if(_visualEffect)
                _visualEffect.Stop();
            if(_animator)
                _animator.StopPlayback();
            gameObject.SetActive(false);
            _isPlay= false;
            _eleasped=0;
            _multiflyAttackSIze = 1;
            transform.localScale = Vector3.one;
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }
    public int GetEnumToInt()
    {
        return (int)_effectName;
    }

    public void SetAttackProperty(Character attacker, int attackPower,float knockBackPower,float stunTime, Define.CharacterType enableAttackCharacter)
    {
        _attacker = attacker;
        _isAttack = true;
        _attackPower = attackPower;
        _knockBackPower = knockBackPower;
        _stunTime= stunTime;
        _enableAttackCharacter= enableAttackCharacter;

    }

    public void SetProperty(string id, Vector3 vector)
    {
        _visualEffect.SetVector3(id, vector);
    }
    public void SetProperty(string id, float value)
    {
        _visualEffect.SetFloat(id, value);
    }
    public void SetProperty(string id, int value)
    {
        _visualEffect.SetInt(id, value);
    }

    public void Play(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        _visualEffect.Play();
        _isPlay = true;
    }

    public void PlayeAnimation(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        _animator.SetTrigger(_animatorTriggerName);
        _isPlay = true;
    }

    public void SetMultiflySize(float size)
    {
        _multiflyAttackSIze= size;
        transform.localScale *= size;
    }

    public void Attack()
    {
        Define.Range range = _attackRange;
        range.size = _attackRange.size * +_multiflyAttackSIze;
        range.center = _attackRange.center * +_multiflyAttackSIze;

        Util.RangeCastAll2D(gameObject, range, Define.CharacterMask, (hit) =>
        {
            if (hit.collider == null) return false;

            IHp hp = hit.collider.GetComponent<IHp>();
            Character character = hp as Character;
            CharacterPart characterPart = hp as CharacterPart;
            if (character == null && characterPart != null)
            {
                character = characterPart.Character;
            }

            if (character != null && (character.CharacterType & _enableAttackCharacter) != 0)
            {
                _attacker.Attack(character, _attackPower, _knockBackPower, _attackDirection, hit.point, _stunTime);
            }

            return false;
        });
    }
}
