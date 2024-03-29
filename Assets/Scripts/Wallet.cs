using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    Player _player;
    Player Player
    {
        get
        {
            if (_player == null)
                _player = Managers.GetManager<GameManager>().Player;

            return _player;
        }
    }
    float _time = 0;

    private void OnEnable()
    {
        _time = 0;
    }
    private void Update()
    {
        if(_time > 2) {
            
            if (Player != null)
            {
                transform.position = Vector3.Lerp(transform.position, Player.transform.position, 0.1f);
                if((transform.position - Player.transform.position).magnitude < 0.2f)
                {
                    Interact();
                }
            }
        }else
        {
            _time += Time.deltaTime;
        }
    }

    public void Interact()
    {
        Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
