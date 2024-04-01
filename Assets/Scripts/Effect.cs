using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.VFX;

public class Effect : MonoBehaviour, ITypeDefine
{
    [SerializeField] bool _debug;
    VisualEffect _visualEffect;

    [SerializeField] Define.EffectName _effectName;

    [SerializeField]float _offTime = 1f;
    float _eleasped;

    bool _isPlay;

    Character _attacker;
    bool _isAttack;
    int _attackPower;
    float _knockBackPower;
    float _stunTime;
    Define.CharacterType _enableAttackCharacter;
    [SerializeField]Define.Range _attackRange;


    private void Awake()
    {
        _visualEffect = GetComponent<VisualEffect>();
        _visualEffect.Stop();
    }

    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _attackRange, Color.red);
    }

    private void Update()
    {
        if (_isPlay)
        {
            if (_isAttack)
            {
                Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask, (hit) =>
                {
                    if (hit.collider == null) return false;

                    IHp hp = hit.collider.GetComponent<IHp>();
                    Character character = hp as Character; 
                    CharacterPart characterPart = hp as CharacterPart;
                    if (character == null && characterPart != null)
                    {
                        character = characterPart.Character;
                    }

                    if (character != null && (character.CharacterType & _enableAttackCharacter) != 0)
                    {
                        _attacker.Attack(character, _attackPower, _knockBackPower, character.transform.position - transform.position, hit.point, _stunTime);
                    }

                    return false;
                });
                _isAttack = false;
            }
            _eleasped += Time.deltaTime;
        }
        if(_visualEffect && _eleasped >= _offTime)
        {
            _visualEffect.Stop();
            gameObject.SetActive(false);
            _isPlay= false;
            _eleasped=0;

            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }
    public int GetEnumToInt()
    {
        return (int)_effectName;
    }

    public void SetAttackProperty(Character attacker, int attackPower,float knockBackPower,float stunTime, Define.CharacterType enableAttackCharacter)
    {
        _attacker = attacker;
        _isAttack = true;
        _attackPower = attackPower;
        _knockBackPower = knockBackPower;
        _stunTime= stunTime;
        _enableAttackCharacter= enableAttackCharacter;

    }

    public void SetProperty(string id, Vector3 vector)
    {
        _visualEffect.SetVector3(id, vector);
    }
    public void SetProperty(string id, float value)
    {
        _visualEffect.SetFloat(id, value);
    }
    public void SetProperty(string id, int value)
    {
        _visualEffect.SetInt(id, value);
    }

    public void Play(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        _visualEffect.Play();
        _isPlay = true;
    }

}
