using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Shotgun : Weapon
{
    [Header("º¶∞« º”º∫")]
    [SerializeField] int _fireCount = 5;
    [SerializeField][Range(0,1)] float _collectionRate = 1;

    public override void FireBullet(IWeaponUsable user)
    {
        _currentAmmo--;

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        user?.ResetRebound();
        float angle = FirePosition.transform.rotation.eulerAngles.z;
        angle = angle * Mathf.Deg2Rad;
        // ¿Ã∆Â∆Æ
        Effect fireFlareOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.FireFlare);
        Effect fireFlare = Managers.GetManager<ResourceManager>().Instantiate(fireFlareOrigin);
        fireFlare.Play(_firePosition.transform.position);
        fireFlare.transform.localScale = _firePosition.transform.lossyScale;
        fireFlare.transform.rotation = _firePosition.transform.rotation;


        
        for (int i = 0; i < _fireCount; i++)
        {
            float bulletAngle = angle;
            bulletAngle += Random.Range(-0.2f * (1-_collectionRate), 0.2f* (1-_collectionRate));

            Vector3 direction = new Vector3(Mathf.Cos(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
            float damage = AttackPower;

          
            Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Bullet);
            projectile.transform.position = _firePosition.transform.position;

            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);

        }
        user?.Rebound(_rebound);
    }
}
