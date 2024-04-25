using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, ITypeDefine
{
    [field: SerializeField] Define.ProjectileName ProjectileName { set; get; }
    protected Rigidbody2D _rigid;
    protected TrailRenderer _trailRenderer;

    [SerializeField] protected bool _isCheckBox;
    [SerializeField] protected Define.Range _boxSize;

    protected float _knockbackPower;
    protected float _speed;
    protected int _damage;
    protected int _penerstratingPower;
    protected int _penerstrateCount;
    protected float _stunTime;
    protected Vector3 _direction;
    protected Character _attacker;

    protected bool _isAttack;
    protected bool _isTouchGround;
    protected Define.CharacterType _enableAttackCharacterType;


    protected Vector3 _prePostion;
    protected List<GameObject> _attackCharacterList = new List<GameObject>();


    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();

    }

    private void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(gameObject,_boxSize,Color.red);
    }

    public virtual void Init(float knockbackPower, float speed, int damage, Define.CharacterType enableAttackCharacterType, int penetratingPower = 0, float stunTime = 0f)
    {
        _trailRenderer.enabled = true;
        _trailRenderer.Clear();
        _knockbackPower = knockbackPower;
        _speed = speed;
        _damage = damage;
        _stunTime = stunTime;
        _enableAttackCharacterType = enableAttackCharacterType;
        _penerstratingPower = penetratingPower;
        _isAttack = false;
        _penerstrateCount = 0;
        _attackCharacterList.Clear();
    }

    protected virtual void Update()
    {
        if (_isTouchGround||(Camera.main.transform.position - transform.position).magnitude > Managers.GetManager<GameManager>().CameraController.GetCameraWidth()/2 + 10)
        {
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
            _isAttack = false;
        }
        else
        {
            RotateBody();
            CheckCollision();
        }
    }

    protected virtual void RotateBody()
    {
        float angle = Mathf.Atan2((_prePostion - transform.position).y, (_prePostion - transform.position).x);
        transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        _boxSize.angle = angle * Mathf.Rad2Deg;
    }
    protected virtual void CheckCollision()
    {
        if (!_isAttack) return;
        if (transform.position == _prePostion) return;

        Vector3 direction = (transform.position - _prePostion);

        RaycastHit2D[] hits = null;
        if (!_isCheckBox)
        {
            hits = Physics2D.CircleCastAll(_prePostion, 0.5f, direction.normalized, direction.magnitude, LayerMask.GetMask("Character") | LayerMask.GetMask("Ground"));
        }
        else
        {
            hits = Util.RangeCastAll2D(gameObject, _boxSize, Define.CharacterMask | LayerMask.GetMask("Ground")).ToArray();
        }

        _prePostion = transform.position;

        foreach (var hit in hits)
        {
            if (_attackCharacterList.Contains(hit.collider.gameObject)) continue;
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _isTouchGround = true;
                return;
            }
            _attackCharacterList.Add(hit.collider.gameObject);

            IHp hpComponent = hit.collider.GetComponent<IHp>();
            if (hpComponent != null)
            {
                Character character = hpComponent as Character;
                CharacterPart characterPart = hpComponent as CharacterPart;

                if (characterPart != null)
                    character = characterPart.Character;

                if (character)
                {
                    if (!character.IsDead && character.CharacterType == _enableAttackCharacterType)
                    {
                        _attacker.Attack(characterPart ? characterPart : character, _damage, _knockbackPower, _rigid.velocity.normalized, hit.point, _stunTime);
                        Effect hitEffectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Hit3);
                        Effect hitEffect = Managers.GetManager<ResourceManager>().Instantiate<Effect>(hitEffectOrigin);
                        hitEffect.SetProperty("Direction", direction);
                        hitEffect.Play(transform.position);

                        _penerstrateCount++;

                        if (_penerstrateCount > _penerstratingPower)
                        {
                            Managers.GetManager<ResourceManager>().Destroy(gameObject);
                            _isAttack = true;
                            return;
                        }
                    }
                }
            }
        }
    }



    // 중력을 안 받는 투사체에 사용
    public virtual void Fire(Character attacker, Vector3 direction)
    {
        direction.z = 0;
        _attacker = attacker;

        Vector3 dir = direction.normalized + attacker.MySpeed.normalized;
        _rigid.velocity = (Vector2)direction.normalized * (_speed + (attacker.MySpeed.magnitude));

        _direction = direction.normalized;
        _prePostion = transform.position;
        _isAttack = true;
        _isTouchGround= false;
    }
    // 중력에 영향을 받는 투사체에 사용
    public virtual void Fire(Character attacker, float power, Vector3 direction)
    {
        direction.z = 0;
        _attacker = attacker;

        Vector3 dir = direction.normalized + attacker.MySpeed.normalized;
        _rigid.AddForce(direction * power, ForceMode2D.Impulse);

        _direction = direction.normalized;
        _prePostion = transform.position;
        _isAttack = true;
        _isTouchGround= false;
    }
    public int GetEnumToInt()
    {
        return (int)ProjectileName;
    }
}
