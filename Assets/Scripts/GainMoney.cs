using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainMoney : MonoBehaviour
{
    Character _character;
    [SerializeField] int _gainMoney;

    private void Awake()
    {
        _character= GetComponent<Character>();
        _character.CharacterDead += OnCharacterDead;
    }

    private void OnCharacterDead()
    {
        Managers.GetManager<GameManager>().Money += _gainMoney;
    }
}
