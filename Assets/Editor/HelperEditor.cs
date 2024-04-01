using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using static Define;

public class HelperEditor : EditorWindow
{
    const string GIRL_CARD_DATA_PATH = "Data/�ҳ�ī��.csv";
    const string GIRL_ABILITY_DATA_PATH = "Scripts/Data/GirlAbilityData.cs";
    const string CREATURE_CARD_DATA_PATH = "Data/����ī��.csv";
    const string CREATURE_ABILITY_DATA_PATH = "Scripts/Data/CreatureAbilityData.cs";
    //const string WALL_CARD_DATA_PATH = "Data/��ī��.csv";
    //const string WALL_ABILITY_DATA_PATH = "Scripts/Data/WallAbilityData.cs";
    const string COMMON_CARD_DATA_PATH = "Data/����ī��.csv";
    const string COMMON_ABILITY_DATA_PATH = "Scripts/Data/CommonAbilityData.cs";
    const string CARD_DATA_PATH = "Scripts/Data/CardData.cs";
    const string SKILL_DATA_PATH = "Scripts/Data/SkillData.cs";

    // 0 : �̸�, 1 : �빮�� �̸� , 2 : �ɷ� �̸���
    const string AbilityFormat =
        "using System.Collections;" +
        "\nusing System.Collections.Generic;" +
        "\nusing UnityEngine;" +
        "\n\npublic class {0}AbilityData" +
        "\n{{" +
        "\n    public static int {1}ABILITY_COUNT = (int){0}AbilityName.END;" +
        "\n}}" +
        "\npublic enum {0}AbilityName" +
        "\n{{" +
        "\n    None = -1," +
        "{2}"+
        "\n    END" +
        "\n}};";

    //0 : ī�� �̸�
    const string CardFormat =
        "using System.Collections;" +
        "\nusing System.Collections.Generic;" +
        "\nusing UnityEngine;" +
        "\n" +
        "\n[CreateAssetMenu(fileName = \"Create Card\",menuName = \"AddData/Create CardData\", order = 0)]" +
        "\npublic class CardData : ScriptableObject,ITypeDefine" +
        "\n{{" +
        "\n    [field:SerializeField]public CardName CardName {{ set; get; }}" +
        "\n    [field: SerializeField] public Define.CardType CardSelectionType {{ set; get; }}" +
        "\n    [field: SerializeField] public string CardDescription {{ set; get; }}" +
        "\n    [field: SerializeField] public bool IsStartCard {{ set; get; }}" +
        "\n    [field: SerializeField] public bool IsActiveAbility {{ set; get; }}"+
        "\n" +
        "\n    // ó�� �ش� ī�带 ������ �� ī�� ����� ���׷��̵��� ��Ͽ� �־���." +
        "\n    [field: SerializeField] public List<PriorCardData> PriorCards {{ set;get; }}" +
        "\n    [field: SerializeField] public int MaxUpgradeCount {{ get; set; }}" +
        "\n    [field: SerializeField] public float Property1 {{ get; set; }}" +
        "\n    [field: SerializeField] public float Property2 {{ get; set; }}" +
        "\n    [field: SerializeField] public float Property3 {{ get; set; }}" +
        "\n" +
        "\n\n    public int GetEnumToInt()" +
        "\n    {{" +
        "\n        return (int)CardName;" +
        "\n    }}" +
        "\n    public static int CARD_COUNT = (int)CardName.END;" +
        "\n}}" +
        "\n\n    public class PriorCardData" +
        "\n    {{" +
        "\n        public CardName priorCardName;" +
        "\n        public int priorUpgradeCount;" +
        "\n    }}" +
        "\npublic enum CardName" +
        "\n{{" +
        "\n    None = -1," +
        "      {0}" +
        "\n    END," +
        "\n}}";

    const string SkillDataFormat =
        "using System.Collections;" +
        "\nusing System.Collections.Generic;" +
        "\nusing UnityEngine;" +
        "\n\n[CreateAssetMenu(fileName = \"Create Skill\", menuName = \"AddData/Create SkillData\", order = 2)]" +
        "\npublic class SkillData : ScriptableObject, ITypeDefine" +
        "\n{{" +
        "\n    public SkillName skillName;" +
        "\n    public Define.MainCharacter character;" +
        "\n    " +
        "\n    public float coolTime;" +
        "\n    public int GetEnumToInt()" +
        "\n    {{" +
        "\n        return (int)skillName;" +
        "\n    }}" +
        "\n}}" +
        "\npublic enum SkillName" +
        "\n{{" +
        "\n    None = -1," +
        "      {0}" +
        "\n    END" +
        "\n}}";
        
    const string CARD_FOLDER_DATA_PATH = "Resources/Datas/Card/";
    const string SKILL_FOLDER_DATA_PATH = "Resources/Datas/Skill/";
    [MenuItem("CustomWindow/HelperWindow", false, 0)]
    static void Init()
    {
        // �����Ǿ��ִ� �����츦 �����´�. ������ ���� �����Ѵ�. �̱��� �����ε��ϴ�.
        HelperEditor window = (HelperEditor)EditorWindow.GetWindow(typeof(HelperEditor));
        window.Show();
    }
    void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("������ ���� �����")))
        {
            CreateDefineFile();
        }
        if (GUILayout.Button(new GUIContent("ī�嵥���� ����")))
        {
            CreateGirlCardData();
            CreateCreatureCardData();
            //CreateWallCardData();
            CreateCommonCardData();
            CreateSkillData();
        }
    }

    void CreateDefineFile()
    {
        // �ҳ� ���� �ε�
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + GIRL_CARD_DATA_PATH);
        if (textAsset == null) return;

        string cardName = "";
        string ability = "";
        string skillName = "";

        string[] lines = textAsset.text.Split('\n');
        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;

            if (!string.IsNullOrEmpty(words[0]))
            {
                cardName += $"\n    {words[0]},";
            }

            if (ParseBoolean(words[1]))
            {
                if (!string.IsNullOrEmpty(words[7]))
                {
                    skillName += $"\n    {words[7]},";
                }
            }

            if (!string.IsNullOrEmpty(words[7]))
            {
                ability += $"\n    {words[7]},";
            }
        }
        StreamWriter writer = new StreamWriter("Assets/"+GIRL_ABILITY_DATA_PATH);
        writer.Write(string.Format(AbilityFormat,"Girl","GIRL", ability));
        writer.Close();

        // ���� ���� �ε�
        textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + CREATURE_CARD_DATA_PATH);
        if (textAsset == null) return;

        ability = "";

        lines = textAsset.text.Split('\n');
        preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;


            if (!string.IsNullOrEmpty(words[0]))
            {
                cardName += $"\n    {words[0]},";
            }

            if (ParseBoolean(words[1]))
            {
                if (!string.IsNullOrEmpty(words[7]))
                {
                    skillName += $"\n    {words[7]},";
                }
            }

            if (!string.IsNullOrEmpty(words[7]))
            {
                ability += $"\n    {words[7]},";
            }
        }
        writer = new StreamWriter("Assets/" + CREATURE_ABILITY_DATA_PATH);
        writer.Write(string.Format(AbilityFormat, "Creature", "CREATURE", ability));
        writer.Close();

        //// �� ���� �ε�
        //textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + WALL_CARD_DATA_PATH);
        //if (textAsset == null) return;

        //ability = "";

        //lines = textAsset.text.Split('\n');
        //preHeadWords = lines[0].Split(',');

        //for (int i = 1; i < lines.Length; i++)
        //{
        //    string[] words = lines[i].Split(',');
        //    if (preHeadWords.Length != words.Length) continue;

        //    if (!string.IsNullOrEmpty(words[0]))
        //    {
        //        cardName += $"\n    {words[0]},";
        //    }
        //    if (ParseBoolean(words[1]))
        //    {
        //        if (!string.IsNullOrEmpty(words[10]))
        //        {
        //            skillName += $"\n    {words[10]},";
        //        }
        //    }

        //    if (!string.IsNullOrEmpty(words[10]))
        //    {
        //        ability += $"\n    {words[10]},";
        //    }
        //}
        //writer = new StreamWriter("Assets/" + WALL_ABILITY_DATA_PATH);
        //writer.Write(string.Format(AbilityFormat, "Wall", "WALL", ability));
        //writer.Close();

        // ���� ���� �ε�
        textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + COMMON_CARD_DATA_PATH);
        if (textAsset == null) return;

        ability = "";

        lines = textAsset.text.Split('\n');
        preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;

            if (!string.IsNullOrEmpty(words[0]))
            {
                cardName += $"\n    {words[0]},";
            }
            if (ParseBoolean(words[1]))
            {
                if (!string.IsNullOrEmpty(words[7]))
                {
                    skillName += $"\n    {words[7]},";
                }
            }

            Debug.Log(words[7]);
            if (!string.IsNullOrEmpty(words[7]))
            {
                ability += $"\n    {words[7]},";
            }
        }
        writer = new StreamWriter("Assets/" + COMMON_ABILITY_DATA_PATH);
        writer.Write(string.Format(AbilityFormat, "Common", "COMMON", ability));
        writer.Close();

        writer = new StreamWriter("Assets/" + CARD_DATA_PATH);
        writer.Write(string.Format(CardFormat, cardName));
        writer.Close();

        writer = new StreamWriter("Assets/" + SKILL_DATA_PATH);
        writer.Write(string.Format(SkillDataFormat, skillName));
        writer.Close();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void CreateGirlCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + GIRL_CARD_DATA_PATH);
        if (textAsset == null) return;

        string[] lines = textAsset.text.Split('\n');
        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;

            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length-1 , 1);

            GirlCardData data = ScriptableObject.CreateInstance<GirlCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.IsActiveAbility = ParseBoolean(words[1]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;

            data.MaxUpgradeCount = words[4].Equals("") ? 0 : int.Parse(words[4]);
            string[] priorCards = words[5].Split("|",options:System.StringSplitOptions.RemoveEmptyEntries);
            string[] priorUpgrades = words[6].Split("|",options:System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<PriorCardData>();
            for(int j = 0; j < priorCards.Length; j++)
            {
                CardName cardName = GetCardName(priorCards[j]);
                if(cardName != CardName.None)
                {
                    
                    data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = ParseInt(priorUpgrades[j]) });
                }
            }
            data.UnlockAbility = GetGirlAbility(words[7]);
            data.Property1 = ParseFloat(words[8]);
            data.Property2 = ParseFloat(words[9]);
            data.Property3 = ParseFloat(words[10]);
            

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    void CreateCreatureCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + CREATURE_CARD_DATA_PATH);
        if (textAsset == null) return;  

        string[] lines = textAsset.text.Split('\n');

        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;
            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length-1 , 1);

            CreatureCardData data = ScriptableObject.CreateInstance<CreatureCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.IsActiveAbility = ParseBoolean(words[1]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;

            data.MaxUpgradeCount = words[4].Equals("") ? 0 : int.Parse(words[4]);
            string[] priorCards = words[5].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            string[] priorUpgrades = words[6].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<PriorCardData>();
            for (int j = 0; j < priorCards.Length; j++)
            {
                CardName cardName = GetCardName(priorCards[j]);
                if (cardName != CardName.None)
                {
                    data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = ParseInt(priorUpgrades[j]) });
                }
            }
            data.UnlockAbility = GetCreatureAbility(words[7]);
            data.Property1 = ParseFloat(words[8]);
            data.Property2 = ParseFloat(words[9]);
            data.Property3 = ParseFloat(words[10]);

            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
  
    void CreateCommonCardData()
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + COMMON_CARD_DATA_PATH);
        if (textAsset == null) return;

        string[] lines = textAsset.text.Split('\n');

        string[] preHeadWords = lines[0].Split(',');
        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;
            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length - 1, 1);

            CommonCardData data = ScriptableObject.CreateInstance<CommonCardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.IsActiveAbility = ParseBoolean(words[1]);
            data.CardDescription = words[2];
            data.IsStartCard = words[3].Equals("1") ? true : false;

            data.MaxUpgradeCount = words[4].Equals("") ? 0 : int.Parse(words[4]);
            string[] priorCards = words[5].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            string[] priorUpgrades = words[6].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<PriorCardData>();
            for (int j = 0; j < priorCards.Length; j++)
            {
                CardName cardName = GetCardName(priorCards[j]);
                if (cardName != CardName.None)
                {
                    data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = ParseInt(priorUpgrades[j]) });
                }
            }
            data.UnlockAbility = GetCommonAbility(words[7]);
            data.Property1 = ParseFloat(words[8]);
            data.Property2 = ParseFloat(words[9]);
            data.Property3 = ParseFloat(words[10]);
            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void CreateSkillData()
    {
        for (int i = 0; i < (int)SkillName.END; i++) 
        {
            SkillName skillName = (SkillName)i;

            SkillData data = ScriptableObject.CreateInstance<SkillData>();

            data.name = skillName.ToString();
            data.skillName = GetSkillName(data.name);
            data.character = GetWhosSkill(data.skillName);
            data.coolTime = 1;
            AssetDatabase.CreateAsset(data, "Assets/" + SKILL_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    public static T[] GetAssetsAtPath<T>(string path) where T : Object
    {
        List<T> returnList = new List<T>();

        //get the contents of the folder's full path (excluding any meta files) sorted alphabetically
        IEnumerable<string> fullpaths = Directory.GetFiles(FullPathForRelativePath(path)).Where(x => !x.EndsWith(".meta")).OrderBy(s => s);
        //loop through the folder contents
        foreach (string fullpath in fullpaths)
        {
            //determine a path starting with Assets
            string assetPath = fullpath.Replace(Application.dataPath, "Assets");
            //load the asset at this relative path
            Object obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            //and add it to the list if it is of type T
            if (obj is T) { returnList.Add(obj as T); }
        }

        return returnList.ToArray();
    }

    private static string FullPathForRelativePath(string path)
    {
        throw new System.NotImplementedException();
    }

    CardName GetCardName(string cardName)
    {
        for (int i = 0; i < CardData.CARD_COUNT; i++)
        {
            if (cardName.Equals(((CardName)i).ToString()))
            {
                return (CardName)i;
            }
        }
        return  CardName.None;
    }
    GirlAbilityName GetGirlAbility(string name)
    {
        for (int i = 0; i < GirlAbilityData.GIRLABILITY_COUNT; i++)
        {
            if (name.Equals(((GirlAbilityName)i).ToString()))
            {
                return (GirlAbilityName)i;
            }
        }
        return GirlAbilityName.None;
    }
    WallAbilityName GetWallAbility(string name)
    {
        for (int i = 0; i < WallAbilityData.WALLABILITY_COUNT; i++)
        {
            if (name.Equals(((WallAbilityName)i).ToString()))
            {
                return (WallAbilityName)i;
            }
        }
        return WallAbilityName.None;
    }
    CommonAbilityName GetCommonAbility(string name)
    {
        for (int i = 0; i < CommonAbilityData.COMMONABILITY_COUNT; i++)
        {
            if (name.Equals(((CommonAbilityName)i).ToString()))
            {
                return (CommonAbilityName)i;
            }
        }
        return CommonAbilityName.None;
    }
    CreatureAbilityName GetCreatureAbility(string name)
    {
        for (int i = 0; i < CreatureAbilityData.CREATUREABILITY_COUNT; i++)
        {
            if (name.Equals(((CreatureAbilityName)i).ToString()))
            {
                return (CreatureAbilityName)i;
            }
        }
        return CreatureAbilityName.None;
    }
    SkillName GetSkillName(string name)
    {
        for (int i = 0; i < (int)SkillName.END; i++)
        {
            if (name.Equals(((SkillName)i).ToString()))
            {
                return (SkillName)i;
            }
        }
        return SkillName.None;
    }

    Define.MainCharacter GetWhosSkill(SkillName skillName)
    {
        if(GetGirlAbility(skillName.ToString()) != GirlAbilityName.None)
        {
            return Define.MainCharacter.Girl;
        }
        if (GetCreatureAbility(skillName.ToString()) != CreatureAbilityName.None)
        {
            return Define.MainCharacter.Creture;
        }
        if (GetWallAbility(skillName.ToString()) != WallAbilityName.None)
        {
            return Define.MainCharacter.Wall;
        }
        if (GetCommonAbility(skillName.ToString()) != CommonAbilityName.None)
        {
            return Define.MainCharacter.Common;
        }

        return Define.MainCharacter.None;
    }
    int ParseInt(string value)
    {
        int result = 0;
        int.TryParse(value, out result);
        return result;
    }

    float ParseFloat(string value)
    {
        float result = 0;
        float.TryParse(value, out result);
        return result;
    }
    bool ParseBoolean(string value)
    {
        if (value.Equals("1"))
            return true;

        return false;
    }

}
