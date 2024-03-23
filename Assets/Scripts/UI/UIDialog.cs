using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDialog : UIBase
{
    public override void Init()
    {
        gameObject.SetActive(false);
    }

    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Time.timeScale = 0;

    }
    public override void Close(bool except = false)
    {

        Time.timeScale = 1;

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }
}
