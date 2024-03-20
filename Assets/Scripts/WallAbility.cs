using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor.Rendering;
using UnityEngine;

[System.Serializable]
public class WallAbility 
{
    [SerializeField]WallAI _wallAI;
  
    // 능력 해금
    Dictionary<WallAbilityName, bool> _abilityUnlocks= new Dictionary<WallAbilityName, bool>();

    // 폭파능력
    [field: Header("폭파 능력")]
    [field: SerializeField] public float ExplosionRange { set; get; } = 3;
    [field: SerializeField] public float ExplosionPower { set; get; } = 50;
    [field: SerializeField] public int ExplosionDamage { set; get; } = 50;

    // 베리어
    float _barrierCoolTime;
    float _barrierDurationTime = 10;
    float _barrierElaspedTime;
    float _barrierRange = 30;
    Barrier _barrier;

    // 잔류전기
    float _residualElectricityTime;
    float _residualElectricityCoolTime = 10;
    Define.Range _residualElectricityRange;

    // 소화
    float _digestionCoolTime = 60;
    float _digestionTime = 60;

    public void Init(WallAI wallAI)
    {
        _wallAI= wallAI;
        _wallAI.Character.CharacterDamaged += OnCharacterDamaged;
        _wallAI.Character.CharacterDeadHandler += OnCharacterDead;

        Managers.GetManager<AbilityManager>().BlackSphereAddedHandler += OnBlackSphereAdded;
        _residualElectricityRange = new Define.Range()
        {
            center = new Vector3(0, 2.5f, 0),
            size = new Vector3(10f, 5f, 0),
            figureType = Define.FigureType.Box
        };
    }

    public void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(_wallAI.gameObject, _residualElectricityRange, Color.yellow);
    }

    void OnBlackSphereAdded(BlackSphere blackSphere)
    {
        if(blackSphere == null)
            OverflowingDark();
    }

    public void AbilityUpdate()
    {
        Barrier();
        OverCharge();
        // ActivateBarrier();
        ResidualElectricity();
        Digestion();
        if (Input.GetKeyDown(KeyCode.H))
        {
            Managers.GetManager<AbilityManager>().AddElectricity(199);
        }
    }

    void ResidualElectricity()
    {
        if (GetIsHaveAbility(WallAbilityName.ResidualElectricity))
        {
            if (_residualElectricityTime < _residualElectricityCoolTime)
            {
                _residualElectricityTime += Time.deltaTime;
            }
            else
            {
                _residualElectricityTime = 0;
                Util.RangeCastAll2D(_wallAI.gameObject, _residualElectricityRange, Define.CharacterMask, (go) =>
                {
                    Character character = go.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Damage(_wallAI.Character, 1, 0, Vector3.zero, 2);
                    }
                    return false;
                });
            }
        }
    }

    private void OnCharacterDead()
    {
        SelfDestruct();
    }
    void OverflowingDark()
    {
        if(GetIsHaveAbility(WallAbilityName.OverflowingDark))
        {
            _wallAI.Character.Hp += 1;
        }
    }
    void Digestion()
    {
        if (GetIsHaveAbility(WallAbilityName.Digestion))
        {
            if(_digestionCoolTime > _digestionTime)
            {
                _digestionTime += Time.deltaTime;
            }
            else if(_wallAI.Character.Hp <= _wallAI.Character.MaxHp * 0.1f)
            {
                AbilityManager manager = Managers.GetManager<AbilityManager>();
                if (manager.Predation > 0)
                {
                    _digestionTime = 0;
                    int diff = _wallAI.Character.MaxHp - _wallAI.Character.Hp;
                    _wallAI.Character.Hp += (diff < manager.Predation? diff: manager.Predation);
                    manager.AddPredation(-(diff < manager.Predation ? diff : manager.Predation));
                }
            }
        }

    }
    private void OnCharacterDamaged(Character attacker, int damage, float power, Vector3 direction, float stunTIme)
    {
        if (GetIsHaveAbility(WallAbilityName.BlackAura))
        {
            if(Random.Range(0,100) < 3)
            {
                Managers.GetManager<AbilityManager>().AddBlackSphere(_wallAI.transform.position);
            }
        }
    }
    public bool GetIsHaveAbility(WallAbilityName wallAbility)
    {
        if (_abilityUnlocks.TryGetValue(wallAbility, out bool value) && value)
            return value;

        return false;
    }

    public void AddGirlAbility(WallAbilityName wallAbilityName)
    {
        if ((int)wallAbilityName > (int)WallAbilityName.None && (int)wallAbilityName < (int)WallAbilityName.END)
        {
            bool turnTrue = false;
            if (_abilityUnlocks.ContainsKey(wallAbilityName) && !_abilityUnlocks[wallAbilityName])
            {
                _abilityUnlocks[wallAbilityName] = true;
                turnTrue = true;
            }
            else
            {
                _abilityUnlocks.Add(wallAbilityName, true);
                turnTrue = true;
            }

            if (turnTrue)
            {
                switch(wallAbilityName)
                {
                    case WallAbilityName.OverCharge:
                        Managers.GetManager<AbilityManager>().IsUnlockOverCharge = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }
    void OverCharge()
    {
        if (GetIsHaveAbility(WallAbilityName.OverCharge))
        {
            AbilityManager manager = Managers.GetManager<AbilityManager>();

            if (manager.CurrentElectricity >= manager.MaxElectricity * 2)
            {
                Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
                effect.SetProperty("Radius", 5f);
                effect.Play(_wallAI.transform.position);

                Define.Range range = new Define.Range() { size = Vector3.one * 5, figureType = Define.FigureType.Circle };
                Util.RangeCastAll2D(_wallAI.gameObject, range, Define.CharacterMask, (go) =>
                {
                    Character character = go.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Damage(_wallAI.Character, 10, 10, character.transform.position - _wallAI.transform.position, 1);
                    }
                    return false;
                });

                manager.ResetElectricity();
            }
        }
    }

    void ActivateBarrier()
    {
        _barrier = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Barrier").GetComponent<Barrier>();
        _barrier.transform.parent = _wallAI.transform;
        _barrier.transform.position = _wallAI.transform.position;
        _barrier?.StartExpansion(_wallAI.Character, _barrierRange, _barrierDurationTime);
    }

    void Barrier()
    {
        if (_barrier)
        {
            _barrierElaspedTime += Time.deltaTime;
            if (_barrierElaspedTime > _barrierDurationTime)
            {
                _barrierElaspedTime = 0;
                Managers.GetManager<ResourceManager>().Destroy(_barrier.gameObject);
                _barrier = null;
            }
        }
    }

    void SelfDestruct()
    {
        if(GetIsHaveAbility(WallAbilityName.SelfDestruct))
        {

            Effect effectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Explosion);
            Effect effect = Managers.GetManager<ResourceManager>().Instantiate(effectOrigin);
            effect.SetProperty("Radius", ExplosionRange);
            effect.SetProperty("BubbleCount", (int)Mathf.Pow(ExplosionRange / 5, 2));
            effect.transform.position = _wallAI.transform.position;

            effect.Play(_wallAI.Character.GetCenter());

            GameObject[] gameObjects = Util.RangeCastAll2D(_wallAI.gameObject, new Define.Range()
            {
                center = _wallAI.Character.GetCenter(),
                size = new Vector3(ExplosionRange, ExplosionRange, ExplosionRange),
                figureType = Define.FigureType.Circle
            });

            foreach (var gameObject in gameObjects)
            {
                Character character = gameObject.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _wallAI.Character.Attack(character, ExplosionDamage, ExplosionPower, character.transform.position
                        - _wallAI.Character.GetCenter(), 1);
                }
            }
        }
    }
    // 부활 단축시간을 반환합니다.
    public float Salvation()
    {
        float reducedReviveTime = 0;
        if (GetIsHaveAbility(WallAbilityName.Salvation))
        {
            List<BlackSphere> blackSphereList = Managers.GetManager<AbilityManager>().BlackSphereList;
            reducedReviveTime = blackSphereList.Count;
            for (int i = 0; i < blackSphereList.Count; i++)
            {
                Managers.GetManager<ResourceManager>().Destroy(blackSphereList[i].gameObject);
            }
            blackSphereList.Clear();
        }

        return reducedReviveTime;
    }
}
