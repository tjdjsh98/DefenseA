using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachObject : MonoBehaviour
{
    [SerializeField] GameObject _target;

    // Update is called once per frame
    void Update()
    {
        if (_target)
            transform.position = _target.transform.position;
    }
}
