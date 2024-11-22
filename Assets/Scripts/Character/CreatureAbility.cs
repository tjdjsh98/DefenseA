using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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

    // �߰� �ɷ�ġ
    public float IncreasedAttackPowerPercentage { get; set; }
    public float IncreasedAttackSpeedPercentage { get; set; }

    float _preHpRegeneration =0;

    /*
    * 0 = �������� ����
    * 1 = �������
    * 2 = �������
    */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();
    bool _isProhibitSkill;

    public List<GameObject> _electricReleaseTickEnemyList = new List<GameObject>();
    GameObject _electricReleaseTempEffect;


    //��������
    [SerializeField] Define.Range _survivalIntinctRange;
    float _preSurvivalIntinctValue;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    //����Ŭ��
    bool _isActiveOverclocking = false;
    SkillSlot _overclockingSlot;

    #region ������
    //�ٴð� ����
    int _needleAndLeatherHuntingCount = 0;
    int _needleAndLeatherIncreaseMaxHp = 0;


    //������
    int _preKnifeIncreasedPower;

    //���������
    float _bittenWoodCoefficient = 0.05f;
    float _prebittenWoodHpRegen;

    // ����� ������
    int _bittenStoneCoefficient = 10;
    int _prebittenStoneAttackPower;

    // �μ������͸�
    [SerializeField] Define.Range _brokenBatteryRange;
    float _brokenBatteryCoolTime = 10;
    float _brokenBatteryElaspedTime = 10;

    // ������ : �μ��� ����
    float _brokenRingFingerIncreaseAttackPowerPercentage = 50;
    float _brokenRingFingerRegenHp  = 10;

    // �Ƿ�ħ_B
    bool _lightingRod_B_Active = false;
    public bool LightingRod_B_Active
    {
        set
        {
            _lightingRod_B_ElasepdTime = 0;
            _lightingRod_B_Active = value;
        }
        get => _lightingRod_B_Active;
    }
    float _lightingRod_B_DurationTime = 5;
    float _lightingRod_B_ElasepdTime;
    float _lightingRod_B_Coefficient = 100f;

    // ������ü
    float _blackLiquidIncreasedHp = 0;

    // ����ƾ
    float _proteinTime=0;

    #endregion
    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creature = _creatureAI.GetComponent<Character>();
        _creatureAI.Character.AttackHandler += OnAttack;
        _creatureAI.Character.AddtionalAttackHandler += OnAddtionalAttack;
        _creatureAI.Character.DamagedHandler += OnDamage;
        _creature.CharacterDeadHandler += OnCreatureDaed;
        Managers.GetManager<GameManager>().Girl.CharacterDeadHandler += OnGirlDead;
        _inventory = Managers.GetManager<GameManager>().Inventory;
        _cardManager = Managers.GetManager<CardManager>();
        _electricReleaseTempEffect = _creature.transform.Find("ElectricRelease").gameObject;
        RegistSkill();
    }

    private void OnCreatureDaed()
    {
        if (_inventory.GetIsHaveItem(ItemName.��ƾ��ϴ¼���_A))
        {
            if (Random.Range(0, 100) < 30)
            {
                _creatureAI.ForceRevive();
            }
        }
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

        Util.DrawRangeOnGizmos(_creature.gameObject, _brokenBatteryRange, Color.red);
    }
    public void AbilityUpdate()
    {
        //SurvivalInstinct();

        _creatureAI.Character.AttackPower = GetAttackPower();
        _creatureAI.Character.IncreasedHpRegeneration = GetHpRegeneration();

        UpdateBrokenBattery();

        //�Ƿ�ħ_B
        if (LightingRod_B_Active)
        {
            _lightingRod_B_ElasepdTime += Time.deltaTime;
            if (_lightingRod_B_ElasepdTime > _lightingRod_B_DurationTime)
            {
                _lightingRod_B_ElasepdTime = 0;
                LightingRod_B_Active = false;
            }
        }
        // ����ƾ_B
        if (_inventory.GetIsHaveItem(ItemName.����ƾ_B))
        {
            if((_proteinTime += Time.deltaTime) > 1)
            {
                _proteinTime = 0;
                _creature.Hp += Mathf.RoundToInt((_creature.MaxHp - _creature.Hp) / 20f);
            }
            
        }
    }

    void OnAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        if (_inventory.GetIsHaveItem(ItemName.�������հ���) || _inventory.GetIsHaveItem(ItemName.�������հ���_A))
        {
            _creature.AddtionalAttack(target,totalDamage,power, direction, point + Vector3.up, stunTime);
        }
        if (_inventory.GetIsHaveItem(ItemName.�������հ���_B))
        {
            _creature.AddtionalAttack(target, totalDamage, power, direction, point + Vector3.up, stunTime);
            _creature.AddtionalAttack(target, totalDamage, power, direction, point + Vector3.up*2, stunTime);
        }

        // ������ ���� �״´ٸ�
        if (target == null || target.IsDead)
        {
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����_A))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����_B))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.Ǫī�󽺿�Ʈ_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
        }
    }
    private void OnAddtionalAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {

        // ������ ���� �״´ٸ�
        if (target == null || target.IsDead)
        {
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����_A))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.�ٴð�����_B))
                _inventory.LeatherAndNeedleHuntingCount++;
            if (_inventory.GetIsHaveItem(ItemName.Ǫī�󽺿�Ʈ_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
        }
    }

    void OnDamage(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        // ������ü
        {
            int increasedHp = 0;
            if (_inventory.GetIsHaveItem(ItemName.������ü))
            {
                increasedHp = 3;
            }
            if (_inventory.GetIsHaveItem(ItemName.������ü_A))
            {
                increasedHp = 7;
            }
            if (_inventory.GetIsHaveItem(ItemName.������ü_B))
            {
                increasedHp = 3;
                if (attacker != null && !attacker.IsDead)
                    _creature.Attack(attacker, Mathf.RoundToInt(_creature.MaxHp / 20f), 0, Vector3.zero, attacker.transform.position, 0);
            }
            
            _blackLiquidIncreasedHp += increasedHp;
            _creature.AddMaxHp(increasedHp);
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
        //  2�ʸ��� ����
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

        // ����Ŭ��
        if (_isActiveOverclocking)
        {
            percentage += _overclockingSlot.card.Property;
        }

        // ������ : �˺�����������_A
        if (_inventory.GetIsHaveItem(ItemName.�˺�����������_A))
        {
            percentage += (int)(_creature.MaxHp / 50f) * 15;
        }

        // ������ : �˺�����������, �˺�����������_B
        else if (_inventory.GetIsHaveItem(ItemName.�˺�����������) || _inventory.GetIsHaveItem(ItemName.�˺�����������_B))
        {
            percentage += (int)(_creature.MaxHp / 50f) * 10;
        }

        // ������ : �μ��� ����
        {
            if (_inventory.GetIsHaveItem(ItemName.�μ�������) || _inventory.GetIsHaveItem(ItemName.�μ�������_B))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage;
                }
            }
            if (_inventory.GetIsHaveItem(ItemName.�μ�������_A))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage * 2;
                }
            }
        }


        return percentage;
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackSpeedPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

        // ����Ŭ��
        if(_isActiveOverclocking)
        {
            percentage += _overclockingSlot.card.Property;
        }

        // ������: ���������͸�

        // ������ : �˺�����������_B
        if (_inventory.GetIsHaveItem(ItemName.�˺�����������_B))
        {
            percentage += (int)(_creature.MaxHp / 50f) * 10;
        }

        // ������ : �Ƿ�ħ_B
        if (_lightingRod_B_Active)
        {
            percentage += _lightingRod_B_Coefficient;
        }
        return percentage;
    }

    void UpdateBrokenBattery()
    {
        if (_inventory.GetIsHaveItem(ItemName.�μ���������)|| _inventory.GetIsHaveItem(ItemName.�μ���������_A)|| _inventory.GetIsHaveItem(ItemName.�μ���������_B))
        {
            if (_brokenBatteryCoolTime < _brokenBatteryElaspedTime)
            {
                _brokenBatteryElaspedTime = 0;
                if (_inventory.GetIsHaveItem(ItemName.�μ���������_B))
                {
                    _brokenBatteryElaspedTime = _brokenBatteryCoolTime/2;
                }
                float stunTime = 0.5f;
                if (_inventory.GetIsHaveItem(ItemName.�μ���������_A))
                {
                    stunTime = 1f;
                }
                Util.RangeCastAll2D(_creature.gameObject, _brokenBatteryRange, Define.CharacterMask,
                    (hit) =>
                    {
                        Character character = hit.collider.GetComponent<Character>();
                        if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                        {
                            _creature.Attack(character, 10, 0, Vector3.zero, hit.point, stunTime);
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

        regen -= _preHpRegeneration;
        _preHpRegeneration = 0;

        if (_inventory.GetIsHaveItem(ItemName.�μ�������_B) && _inventory.IsActiveBrokenRingFinger)
        {
            regen += _brokenRingFingerRegenHp;
            _preHpRegeneration += _brokenRingFingerRegenHp;
        }

        return regen;
    }
    public int GetAttackPower()
    {
        int attackPower = 0;
        attackPower = _creature.AttackPower;
   
        return attackPower;
    }
    #region ��ų����
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.���¢��, PlayRoar);
        _skillDictionary.Add(CardName.��ũ���̺�, PlayShockwave);
        _skillDictionary.Add(CardName.�������, PlayElectricRelease);
        _skillDictionary.Add(CardName.������, PlayAttraction);
        _skillDictionary.Add(CardName.����Ŭ��, PlayOverclocking);
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
                    character.SetVelocity((_creature.transform.position- character.transform.position)*3);               
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
        _creature.Hp /= 2;
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
        _creatureAI.ResetAI();

        _isProhibitSkill = true;
        _creature.IsAttack = true;
        _creature.IsEnableMove = false;
        _creature.IsEnableTurn = false;

        Character character = _creatureAI.GetCloseEnemy();
        if (character)
        {
            _creature.TurnBody(character.transform.position - _creature.transform.position);
        }
        _creature.SetAnimatorBool("Roar", true);

        yield return new WaitForSeconds(0.2f);
        Roar();

    
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

                Card card = Managers.GetManager<CardManager>().GetCard(CardName.���¢��);
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

        while(elapsedTime < slot.card.Property)
        {
            Util.RangeCastAll2D(_creature.gameObject, _attackRangeList[2], Define.CharacterMask, (hit) =>
            {
                if (_electricReleaseTickEnemyList.Contains(hit.collider.gameObject)) return false;

                _electricReleaseTickEnemyList.Add(hit.collider.gameObject);
                _creatureAI.StartCoroutine(CorRemoveElectricReleaseTickEnemy(hit.collider.gameObject, 0.2f));
                Character character = hit.collider.GetComponent<Character>();

                if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _creature.Attack(character, Mathf.RoundToInt(_creature.AttackPower/5f) , 20, Vector3.right * _creature.transform.localScale.x, hit.point, 0.2f);
                }
                return false;
            });
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
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

    // �̻�� ��ų
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