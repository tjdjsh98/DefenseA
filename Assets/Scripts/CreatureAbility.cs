using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class CreatureAbility
{
    [SerializeField] bool _debug;
    [SerializeField] Character _creature;
    Inventory _inventory;
    CardManager _cardManager;

    CreatureAI _creatureAI;

    // 추가 능력치
    public float IncreasedAttackPowerPercentage { get; set; }
    public float IncreasedAttackSpeedPercentage { get; set; }

    /*
    * 0 = 땅구르기 공격
    * 1 = 울부짓기
    * 2 = 전기방출
    */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();
    bool _isProhibitSkill;

    public List<GameObject> _electricReleaseTickEnemyList = new List<GameObject>();
    float _electricReleaseDuration = 3;
    GameObject _electricReleaseTempEffect;


    //생존본능
    [SerializeField] Define.Range _survivalIntinctRange;
    float _preSurvivalIntinctValue;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    //오버클럭
    bool _isActiveOverclocking = false;
    SkillSlot _overclockingSlot;

    #region 아이템
    //바늘과 가죽
    int _needleAndLeatherHuntingCount = 0;
    int _needleAndLeatherIncreaseMaxHp = 0;
    

    //나이프
    int _preKnifeIncreasedPower;

    //베어물린나무
    float _bittenWoodCoefficient = 0.05f;
    float _prebittenWoodHpRegen;

    // 베어물린 돌맹이
    int _bittenStoneCoefficient = 10;
    int _prebittenStoneAttackPower;

    // 뜯겨진배터리
    [SerializeField]Define.Range _brokenBatteryRange;
    float _brokenBatteryCoolTime = 10;
    float _brokenBatteryElaspedTime = 10;
    #endregion
    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creature = _creatureAI.GetComponent<Character>();
        _creatureAI.Character.AttackHandler += OnAttack;
        _creatureAI.Character.DamagedHandler += OnDamage;
        Managers.GetManager<GameManager>().Girl.CharacterDeadHandler += OnGirlDead;
        _inventory = Managers.GetManager<GameManager>().Inventory;
        _cardManager = Managers.GetManager<CardManager>();
        _electricReleaseTempEffect = _creature.transform.Find("ElectricRelease").gameObject;
        RegistSkill();
    }

    private void OnGirlDead()
    {
        _creature.AddMaxHp(-_needleAndLeatherIncreaseMaxHp);
        _needleAndLeatherHuntingCount = 0;
        _needleAndLeatherIncreaseMaxHp = 0;
    }

    public void OnDrawGizmosSelected()
    {
        if (!_debug || _creature == null) return;

        if (_debugAttackRangeIndex < 0 || _attackRangeList.Count <= _debugAttackRangeIndex) return;

        Util.DrawRangeOnGizmos(_creature.gameObject, _attackRangeList[_debugAttackRangeIndex], Color.red);

        Util.DrawRangeOnGizmos(_creature.gameObject, _brokenBatteryRange,Color.red);
    }

    public void AbilityUpdate()
    {
        //SurvivalInstinct();

        _creatureAI.Character.AttackPower = GetAttackPower();
        _creatureAI.Character.IncreasedHpRegeneration = GetHpRegeneration();

        UpdateBrokenBattery();
    }

    void OnAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        
        if (target == null || target.IsDead)
        {
            int count = _inventory.GetItemCount(ItemName.바늘과가죽);
            if (count >0 )
            {
                _needleAndLeatherHuntingCount++;
                if (_needleAndLeatherHuntingCount > 10)
                {
                    _creature.AddMaxHp(count);
                    _needleAndLeatherHuntingCount= 0;
                    _needleAndLeatherIncreaseMaxHp += count;
                }
            }
        }
    }

    void OnDamage(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        if (_inventory.GetItemCount(ItemName.검은액체) > 0 )
        {
            if (Random.Range(0, 100) < 20)
            {
                Debug.Log(_creature.Hp);
                _creature.Hp += 5 * _inventory.GetItemCount(ItemName.검은액체);
                Debug.Log(_creature.Hp);
            }
        }
    }
    public void ApplyCardAbility(Card card)
    {
        switch (card.cardData.CardName)
        {

        }
    }
    public void RevertCardAbility(Card card)
    {

    }
    public bool GetIsHaveAbility(CardName cardName)
    {
         Card card = Managers.GetManager<CardManager>().GetCard(cardName);
        
        return card != null;
    }
    void SurvivalInstinct()
    {
        _survivalIntinctElapsed += Time.deltaTime;
        //  2초마다 갱신
        if (_survivalIntinctElapsed > 2)
        {
            _survivalIntinctElapsed = 0;
            List<RaycastHit2D> hits = Util.RangeCastAll2D(_creatureAI.gameObject, _survivalIntinctRange, LayerMask.GetMask("Character"));

            _survivalIntinctCount = 0;
            foreach (var hit in hits)
            {
                Character character = hit.collider.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _survivalIntinctCount++;
                }
            }
        }
    }
    public float GetIncreasedAttackPowerPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackPowerPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

        // 오버클럭
        if (_isActiveOverclocking)
        {
            Debug.Log(_overclockingSlot.card.Property);
            percentage += _overclockingSlot.card.Property;
        }
        return percentage;
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackSpeedPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

        // 오버클럭
        if(_isActiveOverclocking)
        {
            percentage += _overclockingSlot.card.Property;
        }
 
        // 아이템: 과충전배터리
        if (_inventory.GetItemCount(ItemName.과충전배터리) > 0)
        {
            percentage += _inventory.GetItemCount(ItemName.피뢰침) * 5;
            percentage += _inventory.GetItemCount(ItemName.부서진건전지) * 5;
        }

        return percentage;
    }

    void UpdateBrokenBattery()
    {
        if (_inventory.GetItemCount(ItemName.부서진건전지) > 0)
        {
            if (_brokenBatteryCoolTime < _brokenBatteryElaspedTime)
            {
                _brokenBatteryElaspedTime = 0;
                Util.RangeCastAll2D(_creature.gameObject, _brokenBatteryRange, Define.CharacterMask,
                    (hit) =>
                    {
                        Character character = hit.collider.GetComponent<Character>();
                        if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                        {
                            _creature.Attack(character, 1, 0, Vector3.zero, hit.point, 0.5f);
                        }
                        return false;
                    });
            }
            else
            {
                _brokenBatteryElaspedTime += Time.deltaTime;
            }
        }
    }
    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _creature.IncreasedHpRegeneration;

        return regen;
    }
    public int GetAttackPower()
    {
        int attackPower = 0;
        attackPower = _creature.AttackPower;
   
        return attackPower;
    }
    #region 스킬관련
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.울부짖기, PlayRoar);
        _skillDictionary.Add(CardName.쇼크웨이브, PlayShockwave);
        _skillDictionary.Add(CardName.전기방출, PlayElectricRelease);
        _skillDictionary.Add(CardName.끌어당김, PlayAttraction);
        _skillDictionary.Add(CardName.오버클럭, PlayOverclocking);
    }

    public void UseSkill(SkillSlot slot)
    {
        if (_creatureAI.Character.IsDead) return;
        if (_isProhibitSkill) return;
        if (slot.card == null || slot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(slot.card.cardData.CardName, out var func))
        {
            func?.Invoke(slot);
        }
    }

    void PlayAttraction(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        slot.isActive = true;
        _creatureAI.StartCoroutine(CorAttraction(slot));
    }

    IEnumerator CorAttraction(SkillSlot slot)
    {
        _creature.IncreasedDamageReducePercentage += 80;
        _creature.SetStanding(100);
        float time = 0;
        Define.Range range = new Define.Range() { center = Vector3.zero, size = Vector3.one * 50, figureType = Define.FigureType.Box };
        while (time < slot.card.Property) 
        {
            time += Time.deltaTime;
            Util.RangeCastAll2D(_creature.gameObject, range, Define.CharacterMask, (hit) =>
            {
               Character character = hit.collider.GetComponent<Character>();

                if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                {
                    character.AddForce((_creature.transform.position- character.transform.position));               
                }
                return false;
            });

            yield return null;
        }

        _creature.IncreasedDamageReducePercentage -= 80;

        slot.isActive = false;
        slot.skillElapsed = 0;
        _creature.SetStanding(0);
    }

    void PlayShockwave(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        _creatureAI.StartCoroutine(CorShockwaveAttack(slot));

        slot.skillElapsed = 0;
    }

    IEnumerator CorShockwaveAttack(SkillSlot slot, float later = 0, int num = 0)
    {
        if (later != 0)
            yield return new WaitForSeconds(later);
        List<GameObject> characterList = new List<GameObject>();
        characterList.Clear();
        Vector3 center = _creature.transform.position;
        float radius = 0;
        Camera.main.GetComponent<CameraController>().ShockWave(center, 30, num);
        while (radius < 30)
        {
            radius += Time.deltaTime * 30;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(center, radius, Vector2.zero, 0);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (characterList.Contains(hit.collider.gameObject)) continue;

                    characterList.Add(hit.collider.gameObject);

                    Character character = hit.collider.gameObject.GetComponent<Character>();
                    if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        _creature.Attack(character, Mathf.RoundToInt(_creature.AttackPower * slot.card.Property), 50, character.transform.position - center, hit.point);
                    }
                }
            }
            yield return null;
        }

        Camera.main.GetComponent<CameraController>().StopShockwave(num);
    }

    void PlayOverclocking(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        _creatureAI.StartCoroutine(CorOverclokcing(slot));
    }

    IEnumerator CorOverclokcing(SkillSlot slot)
    {
        slot.isActive = true;
        _isActiveOverclocking = true;
        _overclockingSlot = slot;

        yield return new WaitForSeconds(10);

        _isActiveOverclocking = false;
        _overclockingSlot = null;
        slot.isActive = false;
        slot.skillElapsed = 0;

    }
    void PlayStempGround(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        List<RaycastHit2D> hits = Util.RangeCastAll2D(_creature.gameObject, _attackRangeList[0]);
        Effect effectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.StempGround);
        Effect effect = Managers.GetManager<ResourceManager>().Instantiate(effectOrigin);
        effect.SetProperty("Range", _attackRangeList[0].size.x);
        effect.Play(_creature.transform.position);
        if (hits.Count > 0)
        {
            foreach (var hit in hits)
            {
                Character c = hit.collider.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    _creature.Attack(c, Mathf.RoundToInt(_creature.AttackPower * slot.card.Property), 100, Vector3.up, hit.point, 1);
                }
            }
        }
        slot.skillElapsed = 0;
    }

    void PlayRoar(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        _isProhibitSkill = true;
        slot.isActive = true;
        _creatureAI.StartCoroutine(CorPlayRoar(slot));
    }
    IEnumerator CorPlayRoar(SkillSlot slot)
    {
        _creatureAI.IsStopAI = true;
        // 괴물이 일반 공격 시 잠시 기다린다.
        while (_creature.IsAttack)
        {
            yield return null;
        }
        _creature.IsAttack = true;

        while (Mathf.Abs(_creature.MySpeed.x) > 0.1f)
        {
            yield return null;
        }

        Character character = _creatureAI.GetCloseEnemy();
        if (character)
        {
            _creature.TurnBody(character.transform.position - _creature.transform.position);
        

        }
        _creature.SetAnimatorBool("Roar", true);

        yield return new WaitForSeconds(0.2f);
        Roar();

        _creature.IsAttack = true;
        _creature.IsEnableMove = false;
        _creature.IsEnableTurn = false;

        yield return new WaitForSeconds(1);
        _creature.SetAnimatorBool("Roar", false);

        _creature.IsAttack = false;
        _creatureAI.IsStopAI = false;
        _creature.IsEnableMove = true;
        _creature.IsEnableTurn = true;
        _isProhibitSkill = false;
        slot.isActive = false;
        slot.skillElapsed = 0;
    }
    public void Roar()
    {
        Util.RangeCastAll2D(_creature.gameObject, _attackRangeList[1], Define.CharacterMask, (hit) =>
        {
            if (hit.collider == null) return false;

            IHp hpComponent = hit.collider.GetComponent<IHp>();

            if (hpComponent != null)
            {
                Character character = hpComponent as Character;
                CharacterPart characterPart = hpComponent as CharacterPart;

                Card card = Managers.GetManager<CardManager>().GetCard(CardName.울부짖기);
                if ((character && character.CharacterType == Define.CharacterType.Enemy) ||
                (characterPart && characterPart.Character.CharacterType == Define.CharacterType.Enemy))
                {
                    _creature.Attack(hpComponent, _creature.AttackPower, card.Property, Vector3.right * _creature.transform.localScale.x, hit.point, 2);
                }

            }
            return true;
        });
    }
    void PlayElectricRelease(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;
        
        slot.isActive = true;

        _creatureAI.StartCoroutine(CorPlayElectricRelease(slot));
    }
    IEnumerator CorPlayElectricRelease(SkillSlot slot)
    {
        _creatureAI.ResetAI();

        _isProhibitSkill = true;
        _creature.IsAttack = true;
        _creature.IsEnableMove = false;
        _creature.IsEnableTurn = false;

        _creature.SetAnimatorBool("ElectricRelease",true);

        float elapsedTime = 0;
        _electricReleaseTempEffect.gameObject.SetActive(true);
        _electricReleaseTempEffect.transform.localPosition = _attackRangeList[2].center;
        _electricReleaseTempEffect.transform.localScale = _attackRangeList[2].size;

        while(elapsedTime < _electricReleaseDuration)
        {
            Util.RangeCastAll2D(_creature.gameObject, _attackRangeList[2], Define.CharacterMask, (hit) =>
            {
                if (_electricReleaseTickEnemyList.Contains(hit.collider.gameObject)) return false;

                _electricReleaseTickEnemyList.Add(hit.collider.gameObject);
                _creatureAI.StartCoroutine(CorRemoveElectricReleaseTickEnemy(hit.collider.gameObject, 0.2f));
                Character character = hit.collider.GetComponent<Character>();

                if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _creature.Attack(character, 1, 30, Vector3.right * _creature.transform.localScale.x, hit.point, 0.2f);
                }
                return false;
            });
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _electricReleaseTempEffect.gameObject.SetActive(false);

        _creature.SetAnimatorBool("ElectricRelease", false);

        _isProhibitSkill = false;
        _creature.IsAttack = false;
        _creature.IsEnableMove = true;
        _creature.IsEnableTurn = true;

        slot.isActive = false;
        slot.skillElapsed = 0;
    }

    IEnumerator CorRemoveElectricReleaseTickEnemy(GameObject go,float time)
    {
        yield return new WaitForSeconds(time);
        _electricReleaseTickEnemyList.Remove(go);

    }

    // 미사용 스킬
    /*
    void ThrowPlayer(CreatureSkillSlot creatureSkillSlot)
    {
        if (creatureSkillSlot.creatureSkill == Define.CreatureSkill.Throw)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePosition = Managers.GetManager<InputManager>().MouseWorldPosition;
                Vector3 directionVel = mousePosition - transform.position;

                float power = directionVel.magnitude * _throwPower;
                if (power > 200)
                    power = 200;
                _player.Character.Damage(_character, 0, power, directionVel, 0);
                creatureSkillSlot.skillTime = 0;
            }
        }
    }
    */
    #endregion
}
