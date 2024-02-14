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

    public enum CardSelectionType
    {
        Status,
        Weapon
    }
    public enum CardSelection
    {
        None = - 1,
        반동제어,
        체력회복,
        최대체력증가,
        AK47,
        Mk2,
        더블배럭,
        경찰권총,
        리볼버,
        데저트이글,
        방망이,
        스피어능력해제,
        패밀리어공격력증가,
        방벽최대체력증가,
        방벽크기증가,
        방벽속도증가,
        총알관통력증가,
        총알데미지증가,
        쇼크웨이브능력해제,    
        재장전시간감소,
    }

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