using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    Character _character;

    [SerializeField] WeaponSwaper _weaponSwaper;

    [SerializeField] Define.Range _attackRange;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + _attackRange.center, _attackRange.size);
    }
    private void Update()
    {
        MoveForward();
        Targeting();
    }

    protected virtual void MoveForward()
    {
        _character.Move(Vector2.right);
    }

    public void Targeting()
    {
        List<GameObject> hands = _weaponSwaper.WeaponSlotList;

        for(int i =0; i < hands.Count; i++) 
        {
            if(_weaponSwaper.GetWeapon(i) == null) continue;

            GameObject[] gos = Util.BoxcastAll2D(hands[i], _attackRange);

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
