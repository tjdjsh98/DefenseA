using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatherAI : MonoBehaviour
{
    Character _character;
    Player _player;
    [SerializeField]PenetrateAttack _penetrateAttack;
    float _playerDistance = 5f;

    [SerializeField] Define.Range _spearAttackRange;

    bool _isMove = false;

    float _attackElapsed = 0;
    float _attackDuration = 3f;

    public bool IsUnlockSpear { set; get; } = false;
    public bool IsUnlockShockwave { set; get; } = false;

    Vector3 _tc;
    float _tr;

    private void Awake()
    {
        _character = GetComponent<Character>();
        Managers.GetManager<GameManager>().FatherAI = this;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position + _spearAttackRange.center, _spearAttackRange.size);

        Gizmos.DrawWireSphere(_tc, _tr);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {

            GameObject[] gos = Util.BoxcastAll2D(gameObject, _spearAttackRange);

            Character closeOne = null;
            float distance = 100;
            if (gos.Length > 0)
            {
                foreach (var go in gos)
                {
                    Character c = go.GetComponent<Character>();
                    if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                    {
                        if((transform.position - c.GetCenter()).magnitude < distance)
                        {
                            closeOne = c;
                            distance = (transform.position - c.GetCenter()).magnitude;
                        }
                    }
                }
            }

            if (closeOne != null) {
                _penetrateAttack.StartAttack(_character, closeOne.GetCenter()-transform.position ,20);
            }
        }

        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        FollwerPlayer();
        _attackElapsed += Time.deltaTime;
        if(_attackElapsed > _attackDuration)
        {
            int random = Random.Range(0, 2);

            if(random == 0)
                SpearAttack();
            if (random == 1)
            {
                if(IsUnlockShockwave)
                    StartCoroutine(CorShockwaveAttack());
                _attackElapsed = 0;
            }
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

    IEnumerator CorShockwaveAttack()
    {
        List<GameObject> characterList = new List<GameObject>();
        characterList.Clear();
        Vector3 center = transform.position;
        _tc = center;
        float radius = 0;
        Camera.main.GetComponent<CameraController>().ShockWave(center);
        while (radius < 30) {
            radius += Time.deltaTime*30;
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
                        character.Damage(_character, 1, 10, character.transform.position - center);
                    }
                }
            }

            yield return null;
        }
    }
}
