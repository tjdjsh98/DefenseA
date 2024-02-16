using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    Character _character;

    [SerializeField] WeaponSwaper _weaponSwaper;

    [SerializeField] Define.Range _attackRange;
    [SerializeField] GameObject _sitPosition;
    public GameObject SitPosition => _sitPosition;


    [Header("이동을 하기 위핸 조건 변수")]
    [SerializeField] Define.Range _detecctRange;
    [SerializeField] float _playerDistance;
    bool _isDetectEnemy;
    Coroutine _detectEnemyCoroutine;

    private void Awake()
    {
        _character = GetComponent<Character>();
        Managers.GetManager<GameManager>().DogAI = this;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + _attackRange.center, _attackRange.size);
        Util.DrawRangeOnGizmos(gameObject, _detecctRange, Color.green);
    }
    private void Update()
    {
        MoveForward();
        Targeting();
    }

    protected virtual void MoveForward()
    {
        if (_detectEnemyCoroutine == null)
            _detectEnemyCoroutine = StartCoroutine(CorDetectEnemy());

        if(!_isDetectEnemy && ((Vector2)(Managers.GetManager<GameManager>().Player.transform.position - transform.position)).magnitude < _playerDistance)
            _character.Move(Vector2.right);
    }

    IEnumerator CorDetectEnemy()
    {
        while (true)
        {
            GameObject[] gos = Util.RangeCastAll2D(gameObject, _detecctRange, LayerMask.GetMask("Character"));

            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    _isDetectEnemy = true;
                    yield return new WaitForSeconds(0.2f);
                    continue;
                }
            }
            _isDetectEnemy = false;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void Targeting()
    {
        List<GameObject> hands = _weaponSwaper.WeaponSlotList;

        for(int i =0; i < hands.Count; i++) 
        {
            if(_weaponSwaper.GetWeapon(i) == null) continue;

            GameObject[] gos = Util.RangeCastAll2D(hands[i], _attackRange);

            Character mostClose = null;
            float closeDistance = 100;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();

                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    if((c.transform.position - hands[i].transform.position).magnitude < closeDistance)
                    {
                        closeDistance = (c.transform.position - hands[i].transform.position).magnitude;
                        mostClose= c;
                    }
                }
            }

            if(mostClose != null)
            {
                Vector3 distance = mostClose.transform.position - hands[i].transform.position;

                float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
                hands[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                _weaponSwaper.GetWeapon(i).Fire(_character);
            }
        }
    }
}
