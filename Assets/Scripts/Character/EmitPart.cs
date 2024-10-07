using UnityEngine;
using Random = UnityEngine.Random;

public class EmitPart : MonoBehaviour
{
    Character _character;
    [SerializeField]GameObject[] _parts;


    Vector3 _damagedDirection;
    float _power;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _character.DamagedHandler += OnDamaged;
        _character.CharacterDeadHandler += OnDead;
    }

    private void OnDamaged(Character attacker, int dmg, float power, Vector3 direction, Vector3 point, float stun)
    {
        _damagedDirection = direction;
        _power = power;
    }

    private void OnDead()
    {
        Effect effect= Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Blooding);
        effect.SetProperty("Direction", _damagedDirection);
        effect.Play(transform.position);

        foreach(var part in _parts)
        {
            GameObject go = Instantiate(part);
            go.transform.position = transform.position;
            go.GetComponent<Rigidbody2D>().AddForce(_damagedDirection *( _power < 30? 30:_power), ForceMode2D.Impulse);
        }
    }
}
