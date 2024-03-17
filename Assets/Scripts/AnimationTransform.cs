using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

[ExecuteInEditMode]
public class AnimationTransform : MonoBehaviour
{
    [SerializeField] bool _debug;
    [SerializeField] bool _cloneTransform;

    [SerializeField] Vector3 _localPosition;
    [SerializeField] Vector3 _localRotation;
    [SerializeField] Vector3 _localScale;

    Vector3 _lastLocalPosition;
    Vector3 _lastLocalRotation;
    Vector3 _lastLocalScale;
    bool _isSaved;

    private void OnDisable()
    {
        if (_isSaved)
        {
            transform.localPosition = _lastLocalPosition;
            transform.localRotation = Quaternion.Euler(_lastLocalRotation);
            transform.localScale = _lastLocalScale;
            _isSaved = false;
        }
    }

    void Update()
    {
        if (_debug || Application.isPlaying)
        {
            if (!_isSaved)
            {
                _lastLocalPosition = transform.localPosition;
                _lastLocalRotation = transform.localRotation.eulerAngles;
                _lastLocalScale = transform.localScale;
                _isSaved = true;
            }
            transform.localPosition = _localPosition;
            transform.localRotation = Quaternion.Euler(_localRotation);
            transform.localScale = _localScale;
        }
        else
        {
            if(_isSaved)
            {
                transform.localPosition = _lastLocalPosition;
                transform.localRotation = Quaternion.Euler(_lastLocalRotation);
                transform.localScale = _lastLocalScale;
                _isSaved = false;
            }
        }

        if(_cloneTransform)
        {
            _localPosition = transform.localPosition;
            _localRotation =  transform.localRotation.eulerAngles;
            _localScale = transform.localScale;

            _cloneTransform = false;
        }
    }
}
