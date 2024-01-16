using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject _projectile;

    [SerializeField] float _power;

    [SerializeField] GameObject _firePosition;
    public void Fire(Character fireCharacter)
    {
        GameObject go = Instantiate(_projectile);
        go.transform.position = _firePosition.transform.position;

        Projectile projectile = go.GetComponent<Projectile>();
        projectile.Init(_power, 15, 1);

        Vector3 direction = Managers.GetManager<InputManager>().MouseWorldPosition - _firePosition.transform.position;

        projectile.Fire(fireCharacter, direction.normalized);
    }
}
