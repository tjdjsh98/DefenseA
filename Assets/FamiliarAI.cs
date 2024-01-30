using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FamiliarAI : MonoBehaviour
{
    Character _character;
    Player _player;
    float _playerDistance = 5f;

    [SerializeField] Define.Range _spearAttackRange;

    bool _isMove = false;

    float _attackElapsed = 0;
    float _attackDuration = 3f;

    public bool IsUnlockSpear { set; get; } = false;

    private void Awake()
    {
        _character = GetComponent<Character>();
        Managers.GetManager<GameManager>().Familiar = this;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position + _spearAttackRange.center, _spearAttackRange.size);
    }

    private void Update()
    {
        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        FollwerPlayer();
        _attackElapsed += Time.deltaTime;
        if(_attackElapsed > _attackDuration)
        {
            SpearAttack();
        }
    }

    void FollwerPlayer()
    {
        if(_player ==null) return;

        if(Mathf.Abs(_player.transform.position.x - transform.position.x) > _playerDistance+1)
        {
            _isMove = true;
        }

        if(_player.transform.position.x < transform.position.x)
            _character.Move(Vector3.right * (_player.transform.position.x+2.5f - transform.position.x));
        else
            _character.Move(Vector3.right * (_player.transform.position.x-2.5f - transform.position.x));


    }

    void SpearAttack()
    {
        if (!IsUnlockSpear) return;

        GameObject[] gos = Util.BoxcastAll2D(gameObject, _spearAttackRange);

        if(gos.Length > 0) 
        {
            _attackElapsed = 0;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    GameObject spear = Managers.GetManager<ResourceManager>().Instantiate("Spear");
                    spear.transform.position = c.transform.position;
                    spear.GetComponent<AttackObject>().StartAttack(_character, 10);
                    return;
                }
            }
        }
    }
}
