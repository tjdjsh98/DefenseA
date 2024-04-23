using System.Collections.Generic;
using UnityEngine;

public class Thorn : MonoBehaviour
{
    Character _owner;
    [SerializeField]List<LineRenderer> _lineRendererList;
    List<Vector3> _destinationList = new List<Vector3>();
    [SerializeField] float _width = 5;

    float _time;
    bool _isEnd;
    List<GameObject> _hitList = new List<GameObject>();

    public void Init(Character owner,float width)
    {
        _owner = owner;
        _time = 0;
        _width = width;
        float interval = _width / _lineRendererList.Count;
        for(int i =0; i < _lineRendererList.Count; i++)
        {
            _lineRendererList[i].SetPosition(0, new Vector3(-(_lineRendererList.Count-1)/2f * interval + i * interval, 0, 0));
            if (_destinationList.Count <= i)
            {
                _destinationList.Add(Vector3.zero);
            }

            _destinationList[i] = new Vector3(-(_lineRendererList.Count - 1) / 2f * interval + i * interval + (i%2==0?Random.Range(1,3): Random.Range(-3, -1)), _width, 0);

        }
    }

    private void Update()
    {
        if (_owner != null)
        {
            if (!_isEnd)
            {
                if (_time + Time.deltaTime > 0.15f)
                    _time = 0.15f;
                else
                    _time += Time.deltaTime;

                float interval = _width / _lineRendererList.Count;
                for (int i = 0; i < _lineRendererList.Count; i++)
                {
                    _lineRendererList[i].SetPosition(1, Vector3.Lerp(_lineRendererList[i].GetPosition(0), _destinationList[i], _time / 0.15f));
                    Attack(_lineRendererList[i].GetPosition(1));
                }
                if(_time >= 0.15f)
                    _isEnd = true;
            }
            else
            {
                _time += Time.deltaTime;
                if (_time > 3)
                {
                    _time = 0;
                    _owner = null;
                    _isEnd = false;
                for (int i = 0; i < _lineRendererList.Count; i++)
                    {
                        _lineRendererList[i].SetPosition(0, Vector3.zero);
                    _lineRendererList[i].SetPosition(1, Vector3.zero);

                    }

                    Managers.GetManager<ResourceManager>().Destroy(gameObject);
                }
            }
        }
    }

    // 가시 끝 부분 기준으로 공격합니다.
    void Attack(Vector3 position)
    {
        Util.RangeCastAll2D(_owner.transform.position + position, new Define.Range() { center = Vector3.zero, angle = 0, figureType = Define.FigureType.Circle, size = Vector3.one },
            Define.CharacterMask, (hit) =>
            {
                if (_hitList.Contains(hit.collider.gameObject)) return false;

                _hitList.Add(hit.collider.gameObject);
                Character character = hit.collider.GetComponent<Character>();  
                if (character && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _owner.Attack(character, _owner.AttackPower, 40, position, hit.point, 0.5f);
                }
                return false;
            });
    }
}
