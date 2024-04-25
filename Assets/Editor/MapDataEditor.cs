using UnityEngine;
using UnityEditor;
using Codice.Client.Common;

[CustomEditor(typeof(MapData))]
[CanEditMultipleObjects]
public class MapDataEditor : Editor
{
    SerializedProperty sceneNameProperty;
    SerializedProperty distanceWaveProperty;
    SerializedProperty bossWaveProperty;
    SerializedProperty mentalWaveProperty;

    SerializedProperty mapSizeProperty;
    SerializedProperty nextMapDataProperty;
    SerializedProperty shopItemDataList;
    SerializedProperty randomEvent;
    SerializedProperty randomEventInterval;

    SerializedProperty initMutiflyProperty;
    SerializedProperty mutiflyIntervalProperty;
    SerializedProperty addMutiflyProperty;

    bool bossWaveFoldout;
    bool distanceWaveFoldout;
    bool mentalWaveFoldout;

    Vector2 distanceScrollPos;
    Vector2 bossScrollPos;
    Vector2 mentalScrollPos;

    public virtual void OnEnable()
    {
        sceneNameProperty = serializedObject.FindProperty("sceneName");
        distanceWaveProperty = serializedObject.FindProperty("distanceWave");
        bossWaveProperty = serializedObject.FindProperty("bossWave");
        mentalWaveProperty = serializedObject.FindProperty("mentalWave");

        mapSizeProperty = serializedObject.FindProperty("mapSize");
        nextMapDataProperty = serializedObject.FindProperty("nextMapData");
        shopItemDataList = serializedObject.FindProperty("shopItemDataList");
        randomEvent = serializedObject.FindProperty("randomEvent");
        randomEventInterval = serializedObject.FindProperty("randomEventInterval");

        initMutiflyProperty = serializedObject.FindProperty("initMutifly");
        mutiflyIntervalProperty = serializedObject.FindProperty("multiflyInterval");
        addMutiflyProperty = serializedObject.FindProperty("addMultifly");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sceneNameProperty);

        EditorGUILayout.BeginHorizontal();
        {

            GUILayout.Label("초기 배율");
            GUILayout.FlexibleSpace();
            GUILayout.Label("추가 배율");
            GUILayout.FlexibleSpace();
            GUILayout.Label("배율 간역");
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {

            
            initMutiflyProperty.floatValue = EditorGUILayout.FloatField(initMutiflyProperty.floatValue);
            GUILayout.FlexibleSpace();
            addMutiflyProperty.floatValue = EditorGUILayout.FloatField(addMutiflyProperty.floatValue);
            GUILayout.FlexibleSpace();
            mutiflyIntervalProperty.floatValue = EditorGUILayout.FloatField(mutiflyIntervalProperty.floatValue);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        BossWaveGUI();
        DistanceWaveGUI();
        MentalWaveGUI();

        EditorGUILayout.PropertyField(mapSizeProperty);
        EditorGUILayout.PropertyField(nextMapDataProperty);
        EditorGUILayout.PropertyField(shopItemDataList);
        EditorGUILayout.PropertyField(randomEvent);
        EditorGUILayout.PropertyField(randomEventInterval);


        serializedObject.ApplyModifiedProperties();
    }

    void BossWaveGUI()
    {
        
        bossWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(bossWaveFoldout, new GUIContent("보스 웨이브"));
        GUILayout.FlexibleSpace();
           

        if (bossWaveFoldout)
        {
            if (bossWaveProperty.arraySize > 0)
            {
                int height = bossWaveProperty.arraySize * 70 > 500 ? 500 : bossWaveProperty.arraySize * 70;
                bossScrollPos = EditorGUILayout.BeginScrollView(bossScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25),GUILayout.MinHeight(height));

                for (int i = 0; i < bossWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("적");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("배율");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("거리");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = bossWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = bossWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty distance = bossWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("distance");
                            distance.floatValue = EditorGUILayout.FloatField(distance.floatValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        SerializedProperty property = bossWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genLocalPosition");
                        EditorGUILayout.PropertyField(property);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("추가", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    bossWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("제거", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    if (bossWaveProperty.arraySize > 0)
                        bossWaveProperty.arraySize -= 1;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    void DistanceWaveGUI()
    {

        distanceWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(distanceWaveFoldout, new GUIContent("거리 웨이브"));
        GUILayout.FlexibleSpace();

        if (distanceWaveFoldout)
        {
            if (distanceWaveProperty.arraySize > 0)
            {
                int height = distanceWaveProperty.arraySize * 150 > 500 ? 500 : distanceWaveProperty.arraySize * 150;

                distanceScrollPos = EditorGUILayout.BeginScrollView(distanceScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < distanceWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("생성적");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("프리셋");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("배율");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty preset = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyPreset");
                            preset.objectReferenceValue = EditorGUILayout.ObjectField(preset.objectReferenceValue, typeof(GameObject), true) as GameObject;
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("생성주기");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("시작거리");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("종료거리");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {

                            SerializedProperty genTime = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genTime");
                            genTime.floatValue = EditorGUILayout.FloatField(genTime.floatValue);
                            GUILayout.FlexibleSpace();


                            SerializedProperty startDistance = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("startDistance");
                            startDistance.floatValue = EditorGUILayout.FloatField(startDistance.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty endDistance = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("endDistance");
                            endDistance.floatValue = EditorGUILayout.FloatField(endDistance.floatValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        SerializedProperty property = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genLocalPosition");
                        EditorGUILayout.PropertyField(property);
                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("추가", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    distanceWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("제거", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    if (distanceWaveProperty.arraySize > 0)
                        distanceWaveProperty.arraySize -= 1;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    void MentalWaveGUI()
    {
        mentalWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mentalWaveFoldout, new GUIContent("정신 웨이브"));
        GUILayout.FlexibleSpace();

        if (mentalWaveFoldout)
        {
            if (mentalWaveProperty.arraySize > 0)
            {
                int height = mentalWaveProperty.arraySize * 120 > 500 ? 500 : mentalWaveProperty.arraySize * 120;
                mentalScrollPos = EditorGUILayout.BeginScrollView(mentalScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < mentalWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("생성적");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("생성적프리셋");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("배율");
                            GUILayout.FlexibleSpace();
                            
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty preset = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyPreset");
                            preset.objectReferenceValue = EditorGUILayout.ObjectField(preset.objectReferenceValue, typeof(GameObject), true) as GameObject;
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("생성주기");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("정신력이상");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("정신력이하");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty genTime = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genTime");
                            genTime.floatValue = EditorGUILayout.FloatField(genTime.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty genMentalLevelMore = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genMentalLevelMore");
                            genMentalLevelMore.intValue = EditorGUILayout.IntField(genMentalLevelMore.intValue);
                            GUILayout.FlexibleSpace();
                            SerializedProperty genMentalLevelLess = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genMentalLevelLess");
                            genMentalLevelLess.intValue = EditorGUILayout.IntField(genMentalLevelLess.intValue);
                            GUILayout.FlexibleSpace();

                        }
                        EditorGUILayout.EndHorizontal();

                        SerializedProperty property = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genLocalPosition");
                        EditorGUILayout.PropertyField(property);
                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("추가", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    mentalWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("제거", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    if (mentalWaveProperty.arraySize > 0)
                        mentalWaveProperty.arraySize -= 1;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    
}