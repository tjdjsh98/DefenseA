using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
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
    */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();
    bool _isProhibitSkill;

    //생존본능
    [SerializeField] Define.Range _survivalIntinctRange;
    float _preSurvivalIntinctValue;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    

    #region 아이템
    //바늘과 가죽
    int _preNeedleAndLeatherIncreasedPower;
    float _needleAndLeatherIncreasedPower;

    //나이프
    int _preKnifeIncreasedPower;

    //베어물린나무
    float _bittenWoodCoefficient = 0.05f;
    float _prebittenWoodHpRegen;

    // 베어물린 돌맹이
    int _bittenStoneCoefficient = 10;
    int _prebittenStoneAttackPower;

    // 과충전배터리
    [SerializeField]Define.Range _overchargeBatteryRange;

    #endregion
    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creature = _creatureAI.GetComponent<Character>();
        _creatureAI.Character.AttackHandler += OnAttack;
        _creatureAI.Character.DamagedHandler += OnDamage;
        _inventory = Managers.GetManager<GameManager>().Inventory;
        _cardManager = Managers.GetManager<CardManager>();
        _cardManager.ElectricChargedHandler += OnElectricCharged;
        RegistSkill();
    }

    private void OnElectricCharged(int value)
    {
        // 과충전배터리
        {
            int count = _inventory.GetItemCount(ItemName.과충전배터리);

            if (count > 0)
            {
                if (value > _cardManager.MaxElectricity)
                {
                    Util.RangeCastAll2D(_creature.gameObject, _overchargeBatteryRange, Define.CharacterMask, (hit) =>
                    {
                        Character character = hit.collider.GetComponent<Character>();

                        if (character && character.CharacterType == Define.CharacterType.Enemy)
                        {
                            _creature.Attack(character, value - _cardManager.MaxElectricity, 0, Vector3.zero, hit.point);
                        }
                        return false;
                    });
                }
            }

        }
    }

    public void OnDrawGizmosSelected()
    {
        if (!_debug || _creature == null) return;

        if (_debugAttackRangeIndex < 0 || _attackRangeList.Count <= _debugAttackRangeIndex) return;

        Util.DrawRangeOnGizmos(_creature.gameObject, _attackRangeList[_debugAttackRangeIndex], Color.red);

        Util.DrawRangeOnGizmos(_creature.gameObject, _overchargeBatteryRange,Color.red);
    }

    public void AbilityUpdate()
    {
        SurvivalInstinct();

        _creatureAI.Character.AttackPower = GetAttackPower();
        _creatureAI.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    
    void OnAttack(Character target, int damage)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        if (GetIsHaveAbility(CardName.충전))
        {
            Card card = manager.GetCard(CardName.충전);
            if (card != null)
            {
                Managers.GetManager<CardManager>().CurrentElectricity += (int)card.Property;
            }
        }
        if (target == null || target.IsDead)
        {
            Managers.GetManager<CardManager>().Predation += Managers.GetManager<CardManager>().HuntingPredation;

            int count = _inventory.GetItemCount(ItemName.바늘과가죽);
            if (count >0 )
            {
                _needleAndLeatherIncreasedPower += 0.1f * count;
            }
        }
    }

    void OnDamage(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();
      
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
        if (GetIsHaveAbility(CardName.생존본능))
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
    }
    public float GetIncreasedAttackPowerPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackPowerPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

       

        return percentage;
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackSpeedPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

 

        return percentage;
    }
    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _creature.IncreasedHpRegeneration;
        if (GetIsHaveAbility(CardName.생존본능))
        {
            regen -= _preSurvivalIntinctValue;
            _preSurvivalIntinctValue = _survivalIntinctCount * Managers.GetManager<CardManager>().GetCard(CardName.생존본능).Property;
            regen += _preSurvivalIntinctValue;
        }
        // 아이템 : 베어물린 나무
        regen -= _prebittenWoodHpRegen;
        _prebittenWoodHpRegen = _inventory.GetItemCount(ItemName.베어물린나무) * _bittenWoodCoefficient * _cardManager.Predation;
        if (_prebittenWoodHpRegen > 2) _prebittenWoodHpRegen = 2;
        regen += _prebittenWoodHpRegen; 

        return regen;
    }
    public int GetAttackPower()
    {
        int attackPower = 0;
        attackPower = _creature.AttackPower;

        // 아이템 : 바늘과 가죽
        if (_inventory.GetItemCount(ItemName.바늘과가죽) > 0)
        {
            attackPower -= _preNeedleAndLeatherIncreasedPower;
            _preNeedleAndLeatherIncreasedPower = Mathf.FloorToInt(_needleAndLeatherIncreasedPower); ;
            attackPower += _preNeedleAndLeatherIncreasedPower;
        }
        else
        {
            attackPower -= _preNeedleAndLeatherIncreasedPower;
            _needleAndLeatherIncreasedPower = 0;
            _preNeedleAndLeatherIncreasedPower = 0;
        }

        // 아이템 : 나이프
        attackPower -= _preKnifeIncreasedPower;
        _preKnifeIncreasedPower =Mathf.Clamp((int)(_cardManager.Predation / 5),0,8) * _inventory.GetItemCount(ItemName.나이프);
        attackPower += _preKnifeIncreasedPower;

        // 아이템 : 베어물린 돌맹이
        attackPower -= _prebittenStoneAttackPower;
        _prebittenStoneAttackPower = (int)(_inventory.GetItemCount(ItemName.베어물린돌맹이)  * (int)_cardManager.Predation/ _bittenStoneCoefficient);
        attackPower += _prebittenStoneAttackPower;
        return attackPower;
    }
    #region 스킬관련
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.울부짖기, PlayRoar);
        _skillDictionary.Add(CardName.쇼크웨이브, PlayShockwave);
        _skillDictionary.Add(CardName.전기방출, PlayElectricRelease);
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

        _creature.SetAnimatorTrigger("Roar");

        _creature.IsAttack = true;
        _creature.IsEnableMove = false;
        _creature.IsEnableTurn = false;

        while (_creature.IsAttack)
        {
            yield return null;
        }

        slot.isActive = false;
        slot.skillElapsed = 0;
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
