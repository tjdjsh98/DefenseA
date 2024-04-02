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
    const string COMMON_CARD_DATA_PATH = "Data/공용카드.csv";
    const string CARD_DATA_PATH = "Scripts/Data/CardData.cs";
    const string SKILL_DATA_PATH = "Scripts/Data/SkillData.cs";

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
        
    const string CARD_FOLDER_DATA_PATH = "Resources/Datas/Card/";
    const string SKILL_FOLDER_DATA_PATH = "Resources/Datas/Skill/";
    [MenuItem("CustomWindow/HelperWindow", false, 0)]
    static void Init()
    {
        // 생성되어있는 윈도우를 가져온다. 없으면 새로 생성한다. 싱글턴 구조인듯하다.
        HelperEditor window = (HelperEditor)EditorWindow.GetWindow(typeof(HelperEditor));
        window.Show();
    }
    void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("데이터 정의 재생성")))
        {
            CreateDefineFile();
        }
        if (GUILayout.Button(new GUIContent("카드데이터 생성")))
        {
            CreateCardData(COMMON_CARD_DATA_PATH);
            AssetDatabase.Refresh();
        }
    }
    void CreateDefineFile()
    {
        // 공용 정의 로드
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + COMMON_CARD_DATA_PATH);
        if (textAsset == null) return;

        string cardName = "";
    
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
          
        }


        StreamWriter writer = new StreamWriter("Assets/" + CARD_DATA_PATH);
        writer.Write(string.Format(CardFormat, cardName));
        writer.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void CreateCardData(string cardDataPath)
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/" + cardDataPath);
        if (textAsset == null) return;

        string[] lines = textAsset.text.Split('\n');
        string[] preHeadWords = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] words = lines[i].Split(',');
            if (preHeadWords.Length != words.Length) continue;

            words[words.Length - 1] = words[words.Length - 1].Remove(words[words.Length - 1].Length - 1, 1);

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

                    data.PriorCards.Add(new PriorCardData() { priorCardName = cardName, priorUpgradeCount = ParseInt(priorUpgrades[j]) });
                }
            }

            // 프로퍼티 추가
            string[] properties = words[8].Split("|");
            data.PropertyList = new List<float>();

            for (int j = 0;j < properties.Length; j++)
            {
                data.PropertyList.Add(ParseFloat(properties[j]));
            }


            AssetDatabase.CreateAsset(data, "Assets/" + CARD_FOLDER_DATA_PATH + data.name + ".asset");
            AssetDatabase.SaveAssets();
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
