using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttachProjectile : Projectile
{
    bool _isAttach;
    GameObject _attachedObject;
    Vector3 _attachPosition;

    protected override void Update()
    {
        CheckCollision();
        if (!_isAttach && (Camera.main.transform.position - transform.position).magnitude > 100)
        {
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
      
    }
    private void FixedUpdate()
    {
        if (_isAttach)
        {
            transform.position = _attachedObject.transform.position + _attachPosition;
        }
    }

    public override void Init(float knockbackPower, float speed, int damage, Define.CharacterType enableAttackCharacterType, int penetratingPower = 0, float stunTime = 0.1f)
    {
        base.Init(knockbackPower,speed,damage,enableAttackCharacterType,penetratingPower);

        _isAttach = false;
    }

    protected override void CheckCollision()
    {
        if (_isAttach) return;
        if (_isAttack) return;
        if (transform.position == _prePostion) return;

        Vector3 direction = (transform.position - _prePostion);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(_prePostion, 0.5f, direction.normalized, direction.magnitude, LayerMask.GetMask("Character"));

        _prePostion = transform.position;

        foreach (var hit in hits)
        {
            if (_attackCharacterList.Contains(hit.collider.gameObject)) continue;

            _attackCharacterList.Add(hit.collider.gameObject);

            IHp hpComponent = hit.collider.GetComponent<IHp>();
            if (hpComponent != null)
            {
                Character character = hpComponent as Character;
                CharacterPart characterPart = hpComponent as CharacterPart;

                if (characterPart != null)
                    character = characterPart.Character;

                if (character)
                {
                    if (!character.IsDead && character.CharacterType == _enableAttackCharacterType)
                    {

                        _direction = _direction.normalized;
                        _attacker.Attack(characterPart ? characterPart : character, _damage, _knockbackPower, _direction, _stunTime);
                        Effect hitEffectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Hit3);
                        Effect hitEffect = Managers.GetManager<ResourceManager>().Instantiate<Effect>(hitEffectOrigin);
                        hitEffect.SetProperty("Direction", direction);
                        hitEffect.Play(transform.position);

                        _attachedObject = character.gameObject;
                        _attachPosition = hit.point - (Vector2)character.transform.position;
                        _trailRenderer.enabled = false;
                        _isAttach = true;
                        _isAttack = true;
                        return;
                    }
                }
            }
        }
    }
}
