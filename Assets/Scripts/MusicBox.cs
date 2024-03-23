using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
    Character _owner;

    [SerializeField] bool _debug;
    [SerializeField] GameObject _firePoint;
    [SerializeField] Define.Range _attackRange;
    
    float _fireCoolTime = 1;
    float _fireTime;

    private void Awake()
    {
        _owner = GetComponentInParent<Character>();
    }

    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(_firePoint, _attackRange, Color.green);
    }

    private void Update()
    {
        _fireTime += Time.deltaTime;
        if (_fireCoolTime <= _fireTime)
        {
            _fireTime = 0;
            RaycastHit2D hit = Util.RangeCast2D(_firePoint, _attackRange, Define.CharacterMask);

            if(hit.collider != null)
            {
                Character character = hit.collider.GetComponent<Character>();

                if (character && character.CharacterType == Define.CharacterType.Enemy)
                {
                    Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Normal);
                    projectile.transform.position = _firePoint.transform.position;
                    projectile.Init(10, 50, 1, Define.CharacterType.Enemy);
                    projectile.Fire(_owner,character.transform.position - _firePoint.transform.position);   
                }
            }
        }
    }
}
