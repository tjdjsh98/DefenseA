using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public GameObject _projectile;

    [SerializeField] float _power = 1;
    public float Power => _power;
    [SerializeField] float _bulletSpeed = 15;
    public float BulletSpeed => _bulletSpeed;

    [SerializeField] int _damage = 1;
    public int Damage => _damage;


    [SerializeField] GameObject _firePosition;
    public void Fire(Character fireCharacter)
    {
        GameObject go = Instantiate(_projectile);
        go.transform.position = _firePosition.transform.position;

        Projectile projectile = go.GetComponent<Projectile>();
        projectile.Init(_power, _bulletSpeed, _damage);

        Vector3 direction = Managers.GetManager<InputManager>().MouseWorldPosition - _firePosition.transform.position;

        projectile.Fire(fireCharacter, direction.normalized);
    }

    public void SetPower(float power)
    {
        _power = power;
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

}
