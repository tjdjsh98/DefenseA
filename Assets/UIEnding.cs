using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIEnding : UIBase
{

    [SerializeField] TextMeshProUGUI _clearTimeText;
    [SerializeField] TextMeshProUGUI _deathCountText;
    [SerializeField] TextMeshProUGUI _itemText;
    public override void Init()
    {
        gameObject.SetActive(false);
    }


    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Time.timeScale = 0;
        gameObject.SetActive(true);

        Refresh();
    }
    public override void Close(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Close(this);
        gameObject.SetActive(false);


        Time.timeScale = 1;
    }

    void Refresh()
    {
        float clearTime = Managers.GetManager<GameManager>().TotalTime;
        _clearTimeText.text = $"클리어 타임 : {(int)(clearTime/60)}분:{(int)(clearTime%60)}초";
        _deathCountText.text = $"죽은 횟수 : {Managers.GetManager<GameManager>().DeathCount.ToString()}";

        foreach (var item in Managers.GetManager<GameManager>().Inventory.GetItemList())
        {
            _itemText.text += $"{item.Key.ToString()}({item.Value}) ";
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
