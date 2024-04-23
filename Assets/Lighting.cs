using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighting : MonoBehaviour
{
    Effect _effect;

    public void Awake()
    {
        _effect = GetComponent<Effect>();
        _effect.ContactAttackRangeHandler += OnContactAttackRange;
    }

    private void OnContactAttackRange(Character character)
    {
        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.ÇÇ·ÚÄ§_B))
        {
            if (character == Managers.GetManager<GameManager>().Creature)
            {
                Managers.GetManager<GameManager>().CreatureAI.CreatureAbility.LightingRod_B_Active= true;
            }
            if(character == Managers.GetManager<GameManager>().Girl)
            {
                Managers.GetManager<GameManager>().Player.GirlAbility.LightingRod_B_Active = true;
            }
        }
    }
}
