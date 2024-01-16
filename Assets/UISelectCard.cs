using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISelectCard : UIBase
{
    [SerializeField] List<TextMeshProUGUI> _textList;
    public override void Init()
    {
        _isInitDone = true;
    }
    public override void Open()
    {
    }

    public override void Close()
    {
    }


    public void SelectCard(int index)
    {

    }
}
