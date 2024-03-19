using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WallAbility 
{
    WallAI _wallAI;

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

  
    public void Init(WallAI wallAI)
    {
        _wallAI= wallAI;
        _wallAI.Character.CharacterDamaged += OnCharacterDamaged;
        _wallAI.Character.CharacterDeadHandler += OnCharacterDead;

        Managers.GetManager<AbilityManager>().BlackSphereAddedHandler += OnBlackSphereAdded;
    }

    void OnBlackSphereAdded(BlackSphere blackSphere)
    {
        if(blackSphere == null)
            OverflowingDark();
    }

    public void AbilityUpdate()
    {
        Barrier();
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
            if (_abilityUnlocks.ContainsKey(wallAbilityName))
            {
                _abilityUnlocks[wallAbilityName] = true;
            }
            else
            {
                _abilityUnlocks.Add(wallAbilityName, true);
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
