using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartEnemyAI : EnemyAI
{
    [SerializeField] CharacterPart weakPart;
    protected override void Awake()
    {
        base.Awake();
        weakPart.CharacterPartDamagedHandler += OnCharacterPartDamaged;
    }

    private void OnCharacterPartDamaged(Character attacker, int damage, float power, Vector3 direction, float stunTime)
    {
        _attackingElasepd = 0;
        _attackRangeSprite.gameObject.SetActive(false);
        _attackElapsed = 0;
        _character.IsAttack = false;
    }
}
