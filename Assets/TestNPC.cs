using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNPC : MonoBehaviour
{
    Player _player;
    Player Player { get { if (_player == null) _player = Managers.GetManager<GameManager>().Player; return _player; } }

    [SerializeField] GameObject _bubble;
    private void Update()
    {
        CheckPlayer();
    }

    private void OnEnable()
    {
        Managers.GetManager<InputManager>().InteractKeyDownHandler += OpenDialog;

    }
    private void OnDisable()
    {
        if (Managers.GetManager<InputManager>())
            Managers.GetManager<InputManager>().InteractKeyDownHandler -= OpenDialog;
    }
    public void CheckPlayer()
    {
        if (Player == null) return;

        if ((Player.transform.position - transform.position).magnitude < 5)
        {
            ShowBubble();
        }
        else
        {
            HideBubble();
        }

    }
    void ShowBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(true);
    }

    void HideBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(false);
    }

    void OpenDialog()
    {
        Managers.GetManager<UIManager>().GetUI<UIDialog>().AssginDialog("테스트 다이어로그입니다.");
        Managers.GetManager<UIManager>().GetUI<UIDialog>().AssginSelection1("테스트 선택지 1.", () =>
        {
            Managers.GetManager<UIManager>().GetUI<UIUpgrade>().Open();
        });
        Managers.GetManager<UIManager>().GetUI<UIDialog>().AssginSelection2("테스트 선택지 2.", () =>
        {
            Managers.GetManager<UIManager>().GetUI<UIDialog>().Close();
        });

        Managers.GetManager<UIManager>().GetUI<UIDialog>().Open();
    }
}
