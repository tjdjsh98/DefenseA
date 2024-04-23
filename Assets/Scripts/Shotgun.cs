using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Shotgun : Weapon
{
    [Header("샷건 속성")]
    [SerializeField] int _fireCount = 5;
    [SerializeField][Range(0,1)] float _collectionRate = 1;

    public override void FireBullet(IWeaponUsable user)
    {
      

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        user?.ResetRebound();
        float angle = FirePosition.transform.rotation.eulerAngles.z;
        angle = angle * Mathf.Deg2Rad;
        // 이펙트
        Effect fireFlareOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.FireFlare);
        Effect fireFlare = Managers.GetManager<ResourceManager>().Instantiate(fireFlareOrigin);
        fireFlare.Play(_firePosition.transform.position);
        fireFlare.transform.localScale = _firePosition.transform.lossyScale;
        fireFlare.transform.rotation = _firePosition.transform.rotation;

        Managers.GetManager<GameManager>().CameraController.ShakeCamera(0.3f, 0.1f);


        for (int i = 0; i < _fireCount; i++)
        {
            float bulletAngle = angle;
            bulletAngle += Random.Range(-0.2f * (1-_collectionRate), 0.2f* (1-_collectionRate));

            Vector3 direction = new Vector3(Mathf.Cos(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
            float damage = AttackPower;

            int penerstartingPower = PenerstratingPower;
            float bulletSpeed = BulletSpeed + Random.Range(-20, 20);
            // 총을 쏘는 것이 플레이어라면 해당 스킬을 발동시킨다.
            if (user is Player player)
            {
                if (player.GirlAbility.IsActiveCanine)
                {
                    penerstartingPower = 9999;
                }
            }

            Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Bullet);
            projectile.transform.position = _firePosition.transform.position;

            projectile.Init(KnockBackPower, bulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);

        }
        user?.Rebound(_rebound);
        int addtionalAmmo = 0;

        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.재활용탄))
        {
            if (Random.Range(0, 100) < 10)
            {
                addtionalAmmo = 1;
            }
        }
        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.재활용탄_A))
        {
            if (Random.Range(0, 100) < 20)
            {
                addtionalAmmo = 1;
            }
        }
        if (Managers.GetManager<GameManager>().Inventory.GetIsHaveItem(ItemName.재활용탄_B))
        {
            if (Random.Range(0, 100) < 10)
            {
                addtionalAmmo = 2;
            }
        }
        _currentAmmo += addtionalAmmo;
        _currentAmmo--;
    }
}
