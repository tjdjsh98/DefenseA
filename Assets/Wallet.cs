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

    void Update()
    {
        if((Player.transform.position - transform.position).magnitude < 1)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
            }
        }
    }
}
