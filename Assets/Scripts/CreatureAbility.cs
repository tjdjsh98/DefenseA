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

    CreatureAI _creatureAI;

    // �߰� �ɷ�ġ
    public float IncreasedAttackPowerPercentage { get; set; }
    public float IncreasedAttackSpeedPercentage { get; set; }

    /*
    * 0 = �������� ����
    * 1 = �������
    */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();

    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    //��������
    [SerializeField] Define.Range _survivalIntinctRange;
    float _preSurvivalIntinctValue;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;


    #region ������
    //�ٴð� ����
    int _preNeedleAndLeatherIncreasedPower;
    float _needleAndLeatherIncreasedPower;

    //������
    int _preKnifeIncreasedPower;

    #endregion
    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creature = _creatureAI.GetComponent<Character>();
        _creatureAI.Character.AttackHandler += OnAttack;
        _creatureAI.Character.DamagedHandler += OnDamage;
        _inventory = Managers.GetManager<GameManager>().Inventory;

        RegistSkill();
    }

    public void OnDrawGizmosSelected()
    {
        if (!_debug || _creature == null) return;

        if (_debugAttackRangeIndex < 0 || _attackRangeList.Count <= _debugAttackRangeIndex) return;

        Util.DrawRangeOnGizmos(_creature.gameObject, _attackRangeList[_debugAttackRangeIndex], Color.red);
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

        if (GetIsHaveAbility(CardName.����))
        {
            Card card = manager.GetCard(CardName.����);
            if (card != null)
            {
                Managers.GetManager<CardManager>().CurrentElectricity += card.property;
            }
        }
        if (target == null || target.IsDead)
        {
            if (GetIsHaveAbility(CardName.�Ŀ�))
            {
                Managers.GetManager<CardManager>().Predation+=1;
            }
            if (GetIsHaveAbility(CardName.�Ļ翹��))
            {
                Managers.GetManager<CardManager>().Predation += 3;
            }
            int count = _inventory.GetItemCount(ItemName.�ٴð�����);
            if (count >0 )
            {
                _needleAndLeatherIncreasedPower += 0.1f * count;
            }
        }
    }

    void OnDamage(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();
        if (GetIsHaveAbility(CardName.�˰��帣��))
        {
            Card card = manager.GetCard(CardName.�˰��帣��);
            if (card != null)
            {
                if (Random.Range(0, 100) < card.property)
                {
                    manager.AddBlackSphere(_creatureAI.Character.GetCenter());
                }

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
        if (GetIsHaveAbility(CardName.��������))
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
    }
    public float GetIncreasedAttackPowerPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackPowerPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

        if (GetIsHaveAbility(CardName.�г�))
        {
            percentage += 50;
        }


        return percentage;
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        percentage += IncreasedAttackSpeedPercentage;
        CardManager cardManager = Managers.GetManager<CardManager>();

        if (GetIsHaveAbility(CardName.�г�))
        {
            percentage += 50;
        }


        return percentage;
    }
    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _creature.IncreasedHpRegeneration;
        if (GetIsHaveAbility(CardName.��������))
        {
            regen -= _preSurvivalIntinctValue;
            _preSurvivalIntinctValue = _survivalIntinctCount * Managers.GetManager<CardManager>().GetCard(CardName.��������).property;
            regen += _preSurvivalIntinctValue;
        }

        return regen;
    }
    public int GetAttackPower()
    {
        int attackPower = 0;
        attackPower = _creature.AttackPower;

        // ������ : �ٴð� ����
        if (_inventory.GetItemCount(ItemName.�ٴð�����) > 0)
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

        // ������ : ������
        attackPower -= _preKnifeIncreasedPower;
        _preKnifeIncreasedPower = (int)(Managers.GetManager<CardManager>().Predation / 5) * _inventory.GetItemCount(ItemName.������);
        attackPower += _preKnifeIncreasedPower;

        return attackPower;
    }
    #region ��ų����
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.���¢��, PlayRoar);
        _skillDictionary.Add(CardName.��ũ���̺�, PlayShockwave);
        _skillDictionary.Add(CardName.��������, PlayStempGround);
        _skillDictionary.Add(CardName.�������, PlayElectricRelease);
    }

    public void UseSkill(SkillSlot slot)
    {
        if (_creatureAI.Character.IsDead) return;
        if (_creature.IsAttack) return;
        if (slot.card == null || slot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(slot.card.cardData.CardName, out var func))
        {
            func?.Invoke(slot);
        }
    }

    void PlayShockwave(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;

        _creatureAI.StartCoroutine(CorShockwaveAttack(slot));

        slot.skillTime = 0;
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
                        _creature.Attack(character, Mathf.RoundToInt(_creature.AttackPower * slot.card.property), 50, character.transform.position - center, hit.point);
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
        if (slot.skillCoolTime > slot.skillTime) return;

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
                    _creature.Attack(c, Mathf.RoundToInt(_creature.AttackPower * slot.card.property), 100, Vector3.up, hit.point, 1);
                }
            }
        }
        slot.skillTime = 0;
    }

    void PlayRoar(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;

        slot.isActive = true;

        _creatureAI.StartCoroutine(CorPlayRoar(slot));
    }
    IEnumerator CorPlayRoar(SkillSlot slot)
    {
        _creature.IsAttack = true;

        while (Mathf.Abs(_creature.MySpeed.x) > 0.1f)
        {
            yield return null;
        }

        Character character = _creatureAI.GetCloseEnemy();
        if (character)
        {
            _creature.TurnBody(character.transform.position - _creature.transform.position);
            _creature.AnimatorSetBool("Roar", true);

            yield return new WaitForSeconds(0.2f);
            Roar();

            _creature.IsAttack = true;
            _creature.IsEnableMove = false;
            _creature.IsEnableTurn = false;

            yield return new WaitForSeconds(1);
            _creature.AnimatorSetBool("Roar", false);

            _creature.IsAttack = false;
            _creature.IsEnableMove = true;
            _creature.IsEnableTurn = true;

        }
        slot.isActive = false;
        slot.skillTime = 0;
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
                    _creature.Attack(hpComponent, _creature.AttackPower, card.property, Vector3.right * _creature.transform.localScale.x, hit.point, 2);
                }

            }
            return true;
        });
    }
    void PlayElectricRelease(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;

        slot.isActive = true;

        _creatureAI.StartCoroutine(CorPlayElectricRelease(slot));
    }
    IEnumerator CorPlayElectricRelease(SkillSlot slot)
    {

        _creature.AnimatorSetTrigger("Roar");

        _creature.IsAttack = true;
        _creature.IsEnableMove = false;
        _creature.IsEnableTurn = false;

        while (_creature.IsAttack)
        {
            yield return null;
        }

        slot.isActive = false;
        slot.skillTime = 0;
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
