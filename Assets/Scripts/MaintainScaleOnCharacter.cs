using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainScaleOnCharacter : MonoBehaviour
{
    [SerializeField]Character _character;


    private void Awake()
    {
        if(_character)
        {
            _character.BodyTurnHandler += OnBodyTurn;
        }
    }

    void OnBodyTurn(Vector2 direction)
    {
        Vector3 scale = transform.localScale;
        if (direction.x < 0)
        {
            scale.x = -Mathf.Abs(transform.localScale.x);
        }
        else
        {
            scale.x = Mathf.Abs(transform.localScale.x);
        }

        transform.localScale = scale;
    }
}
