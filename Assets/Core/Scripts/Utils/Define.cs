
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
        Text,
        Save,
        Ability,
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
        FatGuy,
        Boomer,
        Boar,
        UpEnemy,
        PartTestEnemy,
        Spore,
        Thrower,
        UniqueSlime,
        StandingSlime,
        Blob,
        SporeEgg,
        END,
    }


    public static int ENEMY_COUNT = (int)EnemyName.END;
    public enum MainCharacter
    {
        None = -1,
        Girl,
        Creture,
        Wall,
        Common,
        END,
    }
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
        Hit3,
        Barrier,
        Blooding,
        Slash,
        Lighting,
        BlackWhale,
        NormalAttack,
    }
   
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
    public enum WeaponName
    {
        None = -1,
        AssaultRifle,
        Mk2,
        DoubleBarreledShotgun,
        PolicePistal,
        DesertEagle,
        Revolver,
        Bat,
        Stungun,
        Railgun,
        Black,
        ExplosionGun,
        ImprovisedRifle,
        ImprovisedPistol,
        PumpShotgun,
        LittleBlack,
        END
    }

    public enum WeaponPosition
    {
        None = -1,
        Main,
        Sub,
        Special,
        END
    }

    public enum ProjectileName
    {
        None = -1,
        Normal,
        Parabola,
        Attach,
        Bullet,
        Bezier,
        BoarBone,
        END
    }

    public static int ItemRank0Price = 30;
    public static int ItemRank1Price = 70;
    public static int ItemRank2Price = 150;
    public static int ItemRank3Price = 300;

    [System.Serializable]
    public struct Range
    {
        public Vector3 center;
        public Vector3 size;
        public float angle;
        public FigureType figureType;
    }

    public enum FigureType
    {
        Box,
        Circle,
        Raycast
    }
}