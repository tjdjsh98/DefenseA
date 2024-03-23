using System;
using UnityEngine;

public class CharacterPart : MonoBehaviour,IHp
{
    [SerializeField]Character _character;
    public Character Character => _character;

    public Action<Character, int, float, Vector3, float> CharacterPartDamagedHandler;

    private void Awake()
    {
        if(_character == null)
            _character = GetComponentInParent<Character>();
    }
    public int Attack(IHp target, int damage, float power, Vector3 direction,Vector3 attackPoint, float stunTime = 0.1F)
    {
        return _character.Attack(target,damage, power, direction,attackPoint, stunTime);
    }

    public int Damage(IHp attacker, int damage, float power, Vector3 direction,Vector3 damagePoint, float stunTime = 0.1F)
    {
        int resultDamage = _character.Damage(attacker,damage, power, direction,damagePoint, stunTime);

        CharacterPartDamagedHandler?.Invoke(attacker as Character,resultDamage,power,direction,stunTime);

        return resultDamage;
    }

    
}
