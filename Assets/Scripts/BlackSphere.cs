using System.Collections;
using UnityEngine;

public class BlackSphere : MonoBehaviour
{
    [SerializeField] bool _debug;

    Character _owner;
    Vector3 _offset;
    bool _isMoveToDestination;

    // 공격 상태일 떄
    bool _isAttackMode;
    Vector2 _attackDirection;
    Define.Range _attackRange = new Define.Range() { center = Vector3.zero, size = Vector3.one, figureType = Define.FigureType.Circle };
    float _attackTime;
    float _attackDuration= 20;
    int _attackPower;
    [SerializeField]float _speed= 50;
    float _attackDelay;

    [SerializeField]Define.Range _explosionRange;

    bool _isExplosive = false;
    public void Init(Character owner, Vector3 offset)
    {
        _owner = owner;
        _offset = offset;
        _offset.x += Random.Range(-1f, 1f);
        _offset.y += Random.Range(-.5f, .5f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _explosionRange, Color.red);
    }

    public void Update()
    {
        if(_owner== null) return;
        if (_isMoveToDestination) return;

        if (!_isAttackMode)
        {
            Vector3 offset = _owner.transform.position;
            offset.x += _offset.x * (_owner.transform.lossyScale.x > 0 ? 1 : -1);
            offset.y += Mathf.Sin(Time.time + gameObject.GetInstanceID()) + _offset.y;
            transform.position = Vector3.Lerp(transform.position, offset, 0.01f);
        }
        else
        {
            _attackTime += Time.deltaTime;
            if (_attackDelay > _attackTime) return;

            transform.position += (Vector3)_attackDirection * Time.deltaTime*_speed;
            RaycastHit2D hit = Util.RangeCast2D(gameObject,_attackRange,Define.CharacterMask);

            if(hit.collider != null) 
            {
                Character character = hit.collider.GetComponent<Character>();

                if(character&& character.CharacterType == Define.CharacterType.Enemy)
                {
                    character.Damage(_owner, _attackPower, 10, _attackDirection, hit.point);

                    _owner = null;
                    _isMoveToDestination = false;
                    _isAttackMode = false;
                    if (_isExplosive)
                    {
                        Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
                        effect.SetProperty("Radius", _explosionRange.size.x);
                        effect.Play(transform.position);

                        Util.RangeCastAll2D(gameObject, _explosionRange, Define.CharacterMask, (hit) =>
                        {
                            Character c = hit.collider.GetComponent<Character>();
                            if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                            {
                                c.Damage(null, 10, 10, c.transform.position - transform.position,hit.point, 1f);
                            }
                            return false;
                        });

                    }
                    Destroy();
                }
            }
            if (_attackTime >= _attackDuration)
            {
                Destroy();
            }

        }
    }

    public void Destroy()
    {
        _owner = null;
        _isMoveToDestination = false;
        _isAttackMode = false;
        _attackTime = 0;
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
    public void MoveToDestination(GameObject target,float duration,bool isDestroy)
    {
        _isMoveToDestination = true;

        StartCoroutine(CorMoveToDestination(target,duration, isDestroy));
    }
    IEnumerator CorMoveToDestination(GameObject target,float duration, bool isDestroy = false)
    {
        duration *= 60f;
        for(int i = 0; i <= duration; i++)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, (float)i / (duration-i));
            if (!_isMoveToDestination) break;
            yield return new WaitForFixedUpdate();
        }

        if(_isMoveToDestination && isDestroy)
        {
            Destroy();
        }
    }
    public void MoveToDestination(Vector3 targetPosition, float duration, bool isDestroy)
    {
        _isMoveToDestination = true;

        StartCoroutine(CorMoveToDestination(targetPosition, duration, isDestroy));
    }
    IEnumerator CorMoveToDestination(Vector3 targetPosition, float duration, bool isDestroy = false)
    {
        duration *= 60f;
        for (int i = 0; i <= duration; i++)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, (float)i / (duration - i));
            if (!_isMoveToDestination) break;
            yield return new WaitForFixedUpdate();
        }

        if (_isMoveToDestination && isDestroy)
        {
            Destroy();
        }
    }

    public void CancelMoveToDestination()
    {
        _isMoveToDestination = false;

    }

    public void ChangeAttackMode(Vector3 targetPostion,int attackPower,bool explosive = false,float delay = 0)
    {
        _attackDelay = delay;
        _attackTime = 0;
        _attackPower = attackPower;
        _isAttackMode = true;
        _isExplosive = explosive;
        
        _attackDirection = (targetPostion - transform.position).normalized;

    }
}
