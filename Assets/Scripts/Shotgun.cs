using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Shotgun : Weapon
{
    [Header("¼¦°Ç ¼Ó¼º")]
    [SerializeField] int _fireCount = 5;
    [SerializeField][Range(0,1)] float _collectionRate = 1;


    public override void Fire(Character fireCharacter)
    {
        if (_currentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (_fireElapsed < 1 / FireSpeed) return;

        _currentAmmo--;
        _fireElapsed = 0;



        float angle = transform.rotation.eulerAngles.z;

        angle = angle * Mathf.Deg2Rad;
        
        for (int i = 0; i < _fireCount; i++)
        {
            float bulletAngle = angle;
            bulletAngle += Random.Range(-0.2f * (1-_collectionRate), 0.2f* (1-_collectionRate));

            Vector3 direction = new Vector3(Mathf.Cos(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
            Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Bullet);
            projectile.transform.position = _firePosition.transform.position;
            projectile.Init(_knockBackPower, Random.Range(_bulletSpeed -20, BulletSpeed +20), _damage, Define.CharacterType.Enemy);

            projectile.Fire(fireCharacter, direction.normalized);
       
        }
        Player?.Rebound(_rebound);
    }
}
