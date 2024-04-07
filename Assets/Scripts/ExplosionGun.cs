using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ExplosionGun : Weapon
{
    [SerializeField] Define.Range _explosionRange;
    [SerializeField] int _explosionDamage;


    List<Projectile> _projectileList = new List<Projectile>();

    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;
        Util.DrawRangeOnGizmos(gameObject, _explosionRange,Color.red);
    }

    public override void Fire(IWeaponUsable user)
    {
        if (_currentAmmo <= 0)
        {
            return;
        }

        if (_fireElapsed < (1/AttackSpeed )) return;

        // 재장전 중이라면 재장전을 멈춥니다.
        if (_isReload)
        {
            CancelReload();
        }


        _currentAmmo--;
        _fireElapsed = 0;

        if (_audioCoroutine != null)
            StopCoroutine(_audioCoroutine);
        _audioCoroutine = StartCoroutine(CorPlayAudio());

        user?.ResetRebound();
        float angle = FirePosition.transform.rotation.eulerAngles.z;
        angle = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), Mathf.Sin(angle) * transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x), 0);
        direction = direction.normalized;

        // 이펙트
        Effect fireFlareOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.FireFlare);
        Effect fireFlare = Managers.GetManager<ResourceManager>().Instantiate(fireFlareOrigin);
        fireFlare.Play(_firePosition.transform.position);
        fireFlare.transform.localScale = _firePosition.transform.lossyScale;
        fireFlare.transform.rotation = _firePosition.transform.rotation;
        // ---------------

        Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)_bulletName);
        if (projectile != null)
        {
            projectile.transform.position = _firePosition.transform.position;
            _projectileList.Add(projectile);
            float damage = AttackPower;

            damage += damage * user.GetIncreasedDamagePercentage() / 100f;
            // 플레이어 사용자가 플레이어라면
            if (user is Player player)
            {
                if (player.GirlAbility.GetIsHaveAbility(CardName.라스트샷) && _currentAmmo == 0)
                    damage = AttackPower * Managers.GetManager<CardManager>().GetCard(CardName.라스트샷).property;
            }
            projectile.Init(KnockBackPower, BulletSpeed, Mathf.RoundToInt(damage), Define.CharacterType.Enemy, PenerstratingPower, StunTime);
            projectile.Fire(user.Character, direction.normalized);

            user?.Rebound(_rebound);
        }
    }

    public override void Reload(IWeaponUsable user)
    {
        if (_maxAmmo <= _currentAmmo) return;
        if (_isReload) return;

        ExcuteExplosion();

        _fastReloadFailed = false;
        _isReload = true;
        if (_reloadGauge)
        {
            if (user is Player player)
            {
                if (player.GirlAbility.GetIsHaveAbility(CardName.빠른장전))
                {
                    _reloadGauge.Point(0.7f, 0.9f);
                }
                else
                {
                    _reloadGauge.DisablePoint();
                }
            }
            _reloadGauge.SetRatio(0, 1);
            _reloadGauge.gameObject.SetActive(true);
        }
    }
    void ExcuteExplosion()
    {
        foreach (var bullet in _projectileList)
        {
            if (bullet == null) continue;
            Effect explosion = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
            float radius = _explosionRange.size.x;
            explosion.SetMultiflySize(radius/6);
            explosion.Play(bullet.transform.position);
            Util.RangeCastAll2D(bullet.gameObject, _explosionRange, Define.CharacterMask, (hit) =>
            {
                Character character = hit.collider.GetComponent<Character>();
                if(character && character.CharacterType == Define.CharacterType.Enemy)
                {
                    character.Damage(_user.Character, _explosionDamage, 20, character.transform.position - transform.position, hit.point, 1);
                }
                return false;
            }
            );
        }

        for(int i = _projectileList.Count- 1; i >= 0; i--)
        {
            if (_projectileList[i] == null) continue;

            Managers.GetManager<ResourceManager>().Destroy(_projectileList[i].gameObject);
        }
        _projectileList.Clear();
            
    }
}