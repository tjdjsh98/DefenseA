using System.Collections.Generic;
using UnityEngine;

public class Thorn : MonoBehaviour
{
    Character _owner;
    [SerializeField]List<LineRenderer> _lineRendererList;
    [SerializeField] float _width = 5;

    float _time;

    List<GameObject> _hitList = new List<GameObject>();

    public void Init(Character owner)
    {
        _owner = owner;
        _time = 0;

        float interval = _width / _lineRendererList.Count;
        for(int i =0; i < _lineRendererList.Count; i++)
        {
            _lineRendererList[i].SetPosition(0, new Vector3(-(_lineRendererList.Count-1)/2f * interval + i * interval, 0, 0));

        }
    }

    private void Update()
    {
        if (_owner != null)
        {
            _time += Time.deltaTime;
            if (_time < 0.15f)
            {
                float interval = _width / _lineRendererList.Count;
                for (int i = 0; i < _lineRendererList.Count; i++)
                {
                    Vector3 direction = _lineRendererList[i].GetPosition(0);
                    direction.y = _width/2 -  Mathf.Abs(direction.x);
                    _lineRendererList[i].SetPosition(1,_lineRendererList[i].GetPosition(0) + direction.normalized * _time*100f);
                    Attack(_lineRendererList[i].GetPosition(1));
                }
            }
            if(_time > 5)
            {
            
                _time = 0;
                _owner = null;

                Managers.GetManager<ResourceManager>().Destroy(gameObject);
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
