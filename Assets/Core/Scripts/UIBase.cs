using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected bool _isInitDone;

    public abstract void Init();

    public abstract void Open();
    public abstract void Close();
}
