using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour,ITypeDefine
{
    [field: SerializeField] Define.ProjectileName ProjectileName { set; get; }
    protected Rigidbody2D _rigid;
    protected TrailRenderer _trailRenderer;


    protected float _knockbackPower;
    protected float _speed;
    protected int _damage;
    protected int _penerstratingPower;
    protected int _penerstrateCount;
    protected float _stunTime;
    protected Vector3 _direction;
    protected Character _attacker;

    protected bool _isAttack;
    protected  Define.CharacterType _enableAttackCharacterType;


    protected Vector3 _prePostion;
    protected List<GameObject> _attackCharacterList = new List<GameObject>();


    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();
        
    }

    public virtual void Init(float knockbackPower, float speed, int damage,Define.CharacterType enableAttackCharacterType,int penetratingPower= 0,float stunTime = 0f)
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
        CheckCollision();
        if((Camera.main.transform.position - transform.position).magnitude > 100)
        {
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }

    protected virtual void CheckCollision()
    {
        if (_isAttack) return;
        if(transform.position == _prePostion) return;

        Vector3 direction = (transform.position - _prePostion);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(_prePostion, 0.5f, direction.normalized, direction.magnitude, LayerMask.GetMask("Character"));

        _prePostion = transform.position;

        foreach (var hit in hits)
        {
            if (_attackCharacterList.Contains(hit.collider.gameObject)) continue;

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

                        _direction = _direction.normalized;
                        _attacker.Attack(characterPart? characterPart:character, _damage, _knockbackPower, _direction, hit.point, _stunTime);
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
        _rigid.velocity = (Vector2)direction.normalized * (_speed + (attacker.MySpeed.magnitude ));

        _direction = direction.normalized;
        _prePostion = transform.position;
    }
    // 중력에 영향을 받는 투사체에 사용
    public virtual void Fire(Character attacker,float power, Vector3 direction)
    {
        direction.z = 0;
        _attacker = attacker;

        Vector3 dir = direction.normalized + attacker.MySpeed.normalized;
        _rigid.AddForce(direction * power, ForceMode2D.Impulse);

        _direction = direction.normalized;
        _prePostion = transform.position;
    }
    public int GetEnumToInt()
    {
        return (int)ProjectileName;
    }
}
