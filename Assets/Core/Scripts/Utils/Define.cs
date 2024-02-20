using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public static class Define 
{
    public enum CoreManagers
    {
        None = -1,
        Data,
        Input,
        Resource,
        END,
    }

    public static int CORE_MANAGER_COUNT = (int)CoreManagers.END;

    public enum ContentManagers
    {
        None = -1,
        Game,
        UI,
        Effect,
        Text,
        END,
    }
    public static int CONTENT_MANAGER_COUNT = (int)ContentManagers.END;


    public enum CharacterType
    {
        Player,
        Enemy,
        Building
    }

    public enum EnemyName
    {
        None = -1,
        Walker1,
        Walker2,
        Filier1,
        Filier2,
        FlyingFireEnemy,
        OctoHead,
        END,
    }

    public static int ENEMY_COUNT = (int)EnemyName.END;
    public enum EffectName
    {
        None,
        Damage,
        Effect,
        Flare,
        FireFlare,
    }
    public enum FatherSkill
    {
        FindingRange = 0,
        NormalAttackRange,
        PenerstrateRange,
        SpearAttackRange,
        END
    }
    public static int FatherSkillCount = (int)FatherSkill.END;

    public enum CardType
    {
        Status,
        Weapon
    }
    public enum CardName
    {
        None = - 1,
        딸최대체력증가,
        발사간격감소,
        재장전속도증가,
        반동제어력증가,
        반동회복력증가,
        반동삭제,
        빠른재장전,
        추가탄창,
        관통력증가,
        라스트샷,
        반동제어력감소데미지증가,
        딸체력재생력증가,
        눈어시스트,
        눈데미지증가,
        엑스트라웨폰,
        웨폰마스터,
        자동장전,
        아빠최대체력증가,
        아빠일반공격력증가,
        아빠공격속도증가,
        아빠일반공격주기감소,
        쇼크웨이브언락,
        쇼크웨이브반경증가,
        쇼크웨이브공격주기감소,
        쇼크웨이브히트휫수증가,
        아빠체력재생력증가,
        꿰뚫기언락,
        꿰뚫기공격주기감소,
        꿰뚫기거리증가,
        꿰뚫기확장,
        꿰뚫기확장거리증가,
        강아지체력증가,
        END,
    }

    public static int CARD_COUNT = (int)CardName.END;

    public enum WeaponName
    {
        None = -1,
        AK47,
        Mk2,
        DoubleBarreledShotgun,
        PolicePistal,
        DesertEagle,
        Revolver,
        Bat,
        END
    }
    public enum Passive
    {
        None = 1,
        탄약소비무시,
        빠른재장전,
    }

    [System.Serializable]
    public struct Range
    {
        public Vector3 center;
        public Vector3 size;
        public FigureType figureType;
    }

    public enum FigureType
    {
        Box,
        Circle,
        Raycast
    }
}