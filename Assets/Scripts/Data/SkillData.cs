using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Skill", menuName = "AddData/Create SkillData", order = 2)]
public class SkillData : ScriptableObject, ITypeDefine
{
    public SkillName skillName;
    public Define.MainCharacter character;
    
    public float coolTime;
    public int GetEnumToInt()
    {
        return (int)skillName;
    }
}
public enum SkillName
{
    None = -1,      
    Shockwave,
    StempGround,
    ElectricDischarge,
    Roar,
    VolleyFire,
    Bait,
    END
}