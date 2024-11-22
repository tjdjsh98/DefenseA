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

    GameObject _model;


    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
    }

    private void Update()
    {
        _model.transform.eulerAngles += new Vector3(0, 0, 360) * Time.deltaTime;
        if ((transform.position - Player.transform.position).magnitude < 0.2f)
        {
            Interact();
        }

    }

    public void Interact()
    {
        Managers.GetManager<UIManager>().GetUI<UICardSelection>().OpenSkillCardSelection();
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
