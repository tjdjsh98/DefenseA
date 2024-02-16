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

        if (_fireElapsed < _fireDelay) return;

        _currentAmmo--;
        _fireElapsed = 0;



        float angle = transform.rotation.eulerAngles.z;

        angle = angle * Mathf.Deg2Rad;
        
        if (!_isRaycast)
        {
            for (int i = 0; i < _fireCount; i++)
            {
                float bulletAngle = angle;
                bulletAngle += Random.Range(-0.2f * (1-_collectionRate), 0.2f* (1-_collectionRate));

                Vector3 direction = new Vector3(Mathf.Cos(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(bulletAngle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Projectile");
                go.transform.position = _firePosition.transform.position;
                Projectile projectile = go.GetComponent<Projectile>();
                projectile.Init(_power, _bulletSpeed, _damage, Define.CharacterType.Enemy);

                projectile.Fire(fireCharacter, direction.normalized);
            }
        }
        else
        {
            Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);

            RaycastHit2D hit = Physics2D.Raycast(_firePosition.transform.position, direction.normalized, Mathf.Infinity, LayerMask.GetMask("Character") | LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                if (_hitEffect != Define.EffectName.None)
                {
                    Effect effect = Managers.GetManager<EffectManager>().GetEffect(_hitEffect);
                    effect.SetProperty("Direction", direction);
                    effect.Play(hit.point);
                }

                Character character = hit.collider.gameObject.GetComponent<Character>();

                if (character != null)
                {
                    character.Damage(_character, _damage, _power, direction.x > 0 ? Vector3.right : Vector3.left);
                }

            }
        }
        Player?.Rebound(_rebound);
    }
}
