using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }
    private void Update()
    {
        MoveForward();
    }

    protected virtual void MoveForward()
    {
        _character.Move(Vector2.right);
    }
}
