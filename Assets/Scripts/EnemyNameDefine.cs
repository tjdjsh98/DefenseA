using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNameDefine : MonoBehaviour, ITypeDefine
{
    [SerializeField] Define.EnemyName _enemyName;
    public Define.EnemyName EnemyName => _enemyName;

    [field:SerializeField] public bool IsGroup { set; get; }
    public int GetEnumToInt()
    {
        return (int)_enemyName;
    }
}
