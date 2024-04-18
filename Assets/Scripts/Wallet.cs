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
    float _reachTime = 0;
    private void OnEnable()
    {
        _time = 0;
        _reachTime = 0;
    }
    private void Update()
    {
        if(_time > 2) {
            
            if (Player != null)
            {
                _reachTime += Time.deltaTime;
                if (_reachTime >= 2) _reachTime = 1.99f;
                transform.position = Vector3.Lerp(transform.position, Player.transform.position, Time.deltaTime/(2-_reachTime));
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
        Managers.GetManager<UIManager>().GetUI<UICardSelection>().OpenSkillCardSelection();
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
