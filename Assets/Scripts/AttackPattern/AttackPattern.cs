using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPattern : MonoBehaviour
{
    Character _owner;
    SpriteRenderer _rangeSprite;

    [SerializeField]bool _debug;

    [SerializeField]bool _isStart;
    [SerializeField] Define.Range _attackRange;
    [SerializeField] float _delay;
    [SerializeField] Define.CharacterType _attackingCharacterType;
    [SerializeField] int _damage;
    [SerializeField] float _power;
    [SerializeField] Vector3 _direction;
    [SerializeField] float _stunTime;

    MMF_Player _mmfPlayer;

    Coroutine _attackPatternCoroutine;

    private void Awake()
    {
        _rangeSprite = transform.Find("Range").GetComponent<SpriteRenderer>();
        _mmfPlayer= GetComponent<MMF_Player>();
        _rangeSprite.enabled = false;
    }
    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject,_attackRange,Color.red);
    }

    private void Update()
    {
        if (!_isStart) return;

        if(_attackPatternCoroutine == null)
            _attackPatternCoroutine = StartCoroutine(CorStartShowRange(_delay));

        _isStart = false;
    }

    public void StartPattern(Character owner, float delay, Define.CharacterType attackingCharacterType,
        int damage,float power, Vector3 direction,float stunTime)
    {
        _isStart = true;
        _owner = owner;
        _delay = delay;
        _damage = damage;
        _power = power;
        _direction = direction;
        _stunTime = stunTime;
        _attackingCharacterType= attackingCharacterType;
        _rangeSprite.transform.localScale = _attackRange.size;
        _rangeSprite.transform.localPosition = _attackRange.center;
        _rangeSprite.color = new Color(1, 0, 0, 0f);
        _rangeSprite.enabled = true;

        _mmfPlayer.PlayFeedbacks();
    }

    public void StartShowRange(float delay)
    {
        _rangeSprite.color = new Color(1, 0, 0, 0f);
        _rangeSprite.enabled = true;
        StartCoroutine(CorStartShowRange(delay));
    }

    public void Attack()
    {
        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask);

        foreach (var hit in hits)
        {
            Character character = hit.collider.GetComponent<Character>();
            if (character == null) continue;

            if (character.CharacterType == _attackingCharacterType)
            {
                if (_owner)
                    _owner.Attack(character, _damage, _power, _direction, hit.point, _stunTime);
                else
                    character.Damage(_owner, _damage, _power, _direction, hit.point, _stunTime);
            }
        }
    }
    
    IEnumerator CorStartShowRange(float delay)
    {
        float time = 0;
        while(time < delay)
        {
            time += Time.deltaTime;

            Color color = new Color(1, 0, 0, time/delay);
            _rangeSprite.color = color;
            yield return null;
        }
    }

    public void HideRange()
    {
        _rangeSprite.enabled = false;
    }

    public void EndPattern()
    {
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
