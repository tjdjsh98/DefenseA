using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public static class Define 
{
    public static int CharacterMask = LayerMask.GetMask("Character");
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
        Slime,
        Spider,
        Bat,
        FlyOcto,
        WhiteBat,
        OctoHead,
        BatGroup,
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
        Hit2,
        Explosion,
        StempGround,
    }
    public enum CreatureSkill
    {
        None = -1,
        Shockwave,
        StempGround,
        END,
    }
    public static int CreatureSkillCount = (int)CreatureSkill.END;
    public enum CreatureSkillRange
    {
        FindingRange = 0,
        NormalAttackRange,
        PenerstrateRange,
        SpearAttackRange,
        END
    }

    public enum CardType
    {
        Status,
        Weapon
    }
    public enum CardName
    {
        None = - 1,
        라스트샷,
        연사광,
        빠른재장전,
        추가탄창,
        넉넉한총알,
        검은구체_소녀,
        마지막발악,
        검은구체,
        전기발전,

        쇼크웨이브,
        땅구르기,
        생존본능,

        구체생성,
        방벽,
        자폭,
        검은구체_벽,
        END,
    }
    public static int CARD_COUNT = (int)CardName.END;

    public enum GirlAbility
    {
        None = -1,
        BlackSphere,
        FastReload,
        ExtraAmmo,
        AutoReload,
        LastShot,
        PlentyOfBullets,
        Electric,
        LastStruggle,
        END
    }
    public static int GIRLABILITY_COUNT = (int)GirlAbility.END;
    public enum WallAbility
    {
        None = -1,
        GenerateSphere,
        Barrier,
        SelfDestruct,
        END
    }
    public static int WALLABILITY_COUNT = (int)WallAbility.END;
     public enum CreatureAbility
    {
        None = -1,
        Shockwave,
        StempGround,
        SurvialIntinct,
        Spear,
        END
    }
    public static int CREATUREABILITY_COUNT = (int)CreatureAbility.END;

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
        Stungun,
        Railgun,
        END
    }

    public enum WeaponPosition
    {
        Nonoe = -1,
        Main,
        Sub,
        Melee,
        END
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