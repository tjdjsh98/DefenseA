using Codice.Client.BaseCommands.Merge.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using static Define;

public class HelperEditor : EditorWindow
{
    const string COMMON_CARD_CSV_PATH = "Data/공용카드.csv";
    const string ITEM_CSV_PATH = "Data/아이템.csv";
    const string CARD_DEFINE_PATH = "Scripts/Data/CardData.cs";
    const string ITEM_DEFINE_PATH = "Scripts/Data/ItemData.cs";

    //0 : 카드 이름
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
        "\n    [field: SerializeField] public List<float> coolTimeList {{ set; get; }}" +
        "\n" +
        "\n    // 처음 해당 카드를 선택할 때 카드 목록을 업그레이드할 목록에 넣어줌." +
        "\n    [field: SerializeField] public List<PriorCardData> PriorCards {{ set;get; }}" +
        "\n    [field: SerializeField] public int MaxUpgradeCount {{ get; set; }}" +
        "\n    [field: SerializeField] public List<float> PropertyList {{ get; set; }}" +
        "\n    [field: SerializeField] public List<float> Property2List {{ get; set; }}" +
        "\n" +
        "\n\n    public int GetEnumToInt()" +
        "\n    {{" +
        "\n        return (int)CardName;" +
        "\n    }}" +
        "\n    public static int CARD_COUNT = (int)CardName.END;" +
        "\n}}" +
        "\n    [System.Serializable]" +
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

    // {0} : 판매종류
    // {1} : 판매아이템
    const string ItemDataFormat =
        "using System.Collections;" +
        "\nusing System.Collections.Generic;" +
        "\nusing UnityEngine;" +
        "\n\n[CreateAssetMenu(fileName = \"Create Item\", menuName = \"AddData/Create ItemData\", order = 0)]" +
        "\npublic class ItemData : ScriptableObject, ITypeDefine" +
        "\n{{" +
        "\n    [field: SerializeField] public ItemName ItemName {{ set; get; }}" +
        "\n    [field: SerializeField] public ItemType ItemType {{ set; get; }}" +
        "\n    [field: SerializeField] public Sprite Image {{ set; get; }}" +
        "\n    [field:SerializeField][field:TextArea] public string Description {{ set; get; }}" +
        "\n    [field: SerializeField] public int Rank {{ set; get; }}" +
        "\n    public int Price => Rank == 0 ? 20 : Rank == 1 ? 50 : Rank == 2 ? 80 : Rank == 3 ? 150 : 300;" +
        "\n    public int GetEnumToInt()" +
        "\n    {{" +
        "\n        return (int)ItemName;" +
        "\n    }}"+
        "\n}}" +
        "\npublic enum ItemType" +
        "\n{{" +
        "\n    None = -1," +
        "      {0}" +
        "\n    END" +
        "\n}}" +
        "\n\npublic enum ItemName" +
        "\n{{" +
        "\n    None = -1," +
        "    {1}" +
        "\n    END" +
        "\n}}";

    const string CARD_FOLDER_DATA_PATH = "Resources/Datas/Card/";
    const string ITEM_FOLDER_DATA_PATH = "Resources/Datas/Item/";

    [MenuItem("CustomWindow/HelperWindow", false, 0)]
    static void Init()
    {
        // 생성되어있는 윈도우를 가져온다. 없으면 새로 생성한다. 싱글턴 구조인듯하다.
        HelperEditor window = (HelperEditor)EditorWindow.GetWindow(typeof(HelperEditor));
        window.Show();
    }
    void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("카드 데이터 정의 재생성")))
        {
            CreateCardDefineFile();
        }
        if (GUILayout.Button(new GUIContent("아이템 데이터 정의 재생성")))
        {
            CreateItemDefineFile();
        }
        if (GUILayout.Button(new GUIContent("카드데이터 생성")))
        {
            CreateCardData(COMMON_CARD_CSV_PATH);
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button(new GUIContent("아이템데이터 생성")))
        {
            CreateItemData(ITEM_CSV_PATH);
            AssetDatabase.Refresh();
        }
    }
    void CreateCardDefineFile()
    {
        // 공용 정의 로드
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + COMMON_CARD_CSV_PATH);
        if (textAsset == null) return;

        string cardName = "";

        string split = $"{(char)13}\n";
        string[] lines = textAsset.text.Split(split, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');

            if (!string.IsNullOrEmpty(words[0]))
            {
                cardName += $"\n    {words[0]},";
            }
          
        }


        StreamWriter writer = new StreamWriter("Assets/" + CARD_DEFINE_PATH);
        writer.Write(string.Format(CardFormat, cardName));
        writer.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateItemDefineFile()
    {
        // 공용 정의 로드
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + ITEM_CSV_PATH);
        if (textAsset == null) return;

        string itemName = "";
        List<string> sellTypeList = new List<string>();

        string split = $"{(char)13}\n";
        string[] lines = textAsset.text.Split(split,System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');

            Debug.Log(words[0]);
            if (!string.IsNullOrEmpty(words[0]))
            {
                itemName += $"\n    {words[0]},";
            }
            if (!string.IsNullOrEmpty(words[1]))
            {
                if (!sellTypeList.Contains(words[1]))
                    sellTypeList.Add(words[1]);

            }

        }
        string sellType = "";
        foreach(string word in sellTypeList)
        {
            sellType += $"\n    {word},";
        }


        StreamWriter writer = new StreamWriter("Assets/" + ITEM_DEFINE_PATH);
        writer.Write(string.Format(ItemDataFormat,sellType,itemName));
        writer.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    void CreateCardData(string cardDataPath)
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + cardDataPath);
        if (textAsset == null) return;

        string split = $"{(char)13}\n";
        string[] lines = textAsset.text.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            Debug.Log(lines[i]);
            string[] words = lines[i].Split(',');
            if (words[0].Equals(string.Empty)) continue;
            CardData data = ScriptableObject.CreateInstance<CardData>();
            data.name = words[0];
            data.CardName = GetCardName(words[0]);
            data.IsActiveAbility = ParseBoolean(words[1]);

            // 쿨타임
            string[] coolTimes = words[2].Split("|");
            data.coolTimeList = new List<float>();
            foreach (var coolTime in coolTimes)
                data.coolTimeList.Add(ParseFloat(coolTime));

            data.CardDescription = words[3];
            data.IsStartCard = words[4].Equals("1") ? true : false;

            data.MaxUpgradeCount = words[5].Equals("") ? 0 : int.Parse(words[5]);
            string[] priorCards = words[6].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            string[] priorUpgrades = words[7].Split("|", options: System.StringSplitOptions.RemoveEmptyEntries);
            data.PriorCards = new List<PriorCardData>();
            for (int j = 0; j < priorCards.Length; j++)
            {
                CardName cardName = GetCardName(priorCards[j]);
                if (cardName != CardName.None)
                {
                    if (priorUpgrades.Length <= j)
                    {
                        data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = 0 });
                    }
                    else
                    {
                        data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = ParseInt(priorUpgrades[j]) });
                    }
                }
            }

            // 프로퍼티 추가
            string[] properties = words[8].Split("|");
            data.PropertyList = new List<float>();

            for (int j = 0;j < properties.Length; j++)
            {
                data.PropertyList.Add(ParseFloat(properties[j]));
            }

            string[] properties2 = words[9].Split("|");
            data.Property2List = new List<float>();

            for (int j = 0; j < properties2.Length; j++)
            {
                data.Property2List.Add(ParseFloat(properties2[j]));
            }


            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
    void CreateItemData(string itemDataPath)
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + itemDataPath);
        if (textAsset == null) return;

        string split = $"{(char)13}\n";
        string[] lines = textAsset.text.Split(split, System.StringSplitOptions.RemoveEmptyEntries);
        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {

            Debug.Log(lines[i]);
            string[] words = lines[i].Split(',');

            if (words[0].Equals(string.Empty)) continue;

            ItemType itemType = GetItemType(words[1]);

            if (itemType == ItemType.StatusUp )
            {
                StatusUpItemData data = ScriptableObject.CreateInstance<StatusUpItemData>();
                data.name = words[0];
                data.ItemName = GetItemName(words[0]);
                data.ItemType = GetItemType(words[1]);
                data.Description = words[2];
                data.Rank = ParseInt(words[3]);
                data.IncreasingGirlMaxHp = ParseInt(words[4]);
                data.RecoverGirlHpAmount = ParseInt(words[5]);
                data.IncreasingGirlHpRegeneration = ParseFloat(words[6]);
                data.IncreasingGirlAttackPowerPercentage= ParseFloat(words[7]);
                data.IncreasingGirlAttackSpeedPercentage= ParseFloat(words[8]);
                data.IncreasingGirlSpeed = ParseFloat(words[9]);
                data.IncreasingCreatureMaxHp = ParseInt(words[10]);
                data.RecoverCreatureHpAmount = ParseInt(words[11]);
                data.IncreasingCreatureHpRegeneration = ParseFloat(words[12]);
                data.IncreasingCreatureAttackPower = ParseInt(words[13]);
                data.IncreasingCreatureAttackSpeedPercentage = ParseFloat(words[14]);
                data.IncreasingCreatureSpeed = ParseFloat(words[15]);
                data.AccelMentalDownPercentage = ParseFloat(words[16]);
                data.IncreasingReloadSpeedPercentage = ParseFloat(words[19]);
                data.ReviveTimeDown = ParseFloat(words[20]);

                AssetDatabase.CreateAsset(data, "Assets/" + ITEM_FOLDER_DATA_PATH + data.name + ".asset");
                AssetDatabase.SaveAssets();
            }
            if (itemType == ItemType.Weapon)
            {
                WeaponItemData data = ScriptableObject.CreateInstance<WeaponItemData>();
                data.name = words[0];
                data.ItemName = GetItemName(words[0]);
                data.ItemType = GetItemType(words[1]);
                data.Description = words[2];
                data.Rank = ParseInt(words[3]);
                data.weaponPosition = GetWeaponPosition(words[17]);
                data.weaponName = ConvertItemToWeapon(words[18]);
               
                AssetDatabase.CreateAsset(data, "Assets/" + ITEM_FOLDER_DATA_PATH + data.name + ".asset");
                AssetDatabase.SaveAssets();
            }
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

    ItemName GetItemName(string itemName)
    {
        for (int i = 0; i < (int)ItemName.END; i++)
        {
            if (itemName.Equals(((ItemName)i).ToString()))
            {
                return (ItemName)i;
            }
        }
        return ItemName.None;
    }
    ItemType GetItemType(string itemType)
    {
        for (int i = 0; i < (int)ItemType.END; i++)
        {
            if (itemType.Equals(((ItemType)i).ToString()))
            {
                return (ItemType)i;
            }
        }
        return ItemType.StatusUp;
    }
    WeaponName ConvertItemToWeapon(string itemName)
    {
        for(int i = 0; i < (int)WeaponName.END;i++)
        {
            if (itemName.Equals(((WeaponName)i).ToString()))
            {
                return (WeaponName)i;
            }
        }
        return WeaponName.None;
    }
    WeaponPosition GetWeaponPosition(string weaponPosition)
    {
        for (int i = 0; i < (int)WeaponPosition.END; i++)
        {
            if (weaponPosition.Equals(((WeaponPosition)i).ToString()))
            {
                return (WeaponPosition)i;
            }
        }
        return WeaponPosition.None;
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
