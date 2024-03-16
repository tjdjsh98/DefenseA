using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eye : MonoBehaviour
{
    [SerializeField] GameObject _innerEye;
    [SerializeField] float _constrainRadius;

    Player _player;
    public Player Player
    {
        get { 
            if(_player== null)_player= Managers.GetManager<GameManager>().Player;
            return _player; }
    }
    public void Update()
    {
        _innerEye.transform.position = Player ? Player.transform.position : Vector3.zero;
        if(_innerEye.transform.localPosition.magnitude > _constrainRadius)
        {
            float angle = Mathf.Atan2(_innerEye.transform.localPosition.y, _innerEye.transform.localPosition.x);
            _innerEye.transform.localPosition = new Vector3(Mathf.Cos(angle)*_constrainRadius, Mathf.Sin(angle) * _constrainRadius, 0);
        } 
    }
}
