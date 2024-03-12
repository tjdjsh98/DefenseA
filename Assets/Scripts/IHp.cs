using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public interface IHp 
{
    // 최종적으로 가한 데미지를 반환합니다.
    public int Damage(IHp attacker, int damage, float power, Vector3 direction, float stunTime = 0.1f);
    public int Attack(IHp target, int damage, float power, Vector3 direction, float stunTime = 0.1f);
}
