using UnityEngine;
using UnityEditor;
using Codice.Client.Common;

[CustomEditor(typeof(MapData))]
[CanEditMultipleObjects]
public class MapDataEditor : Editor
{
    SerializedProperty sceneNameProperty;
    SerializedProperty timeWaveProperty;
    SerializedProperty distanceWaveProperty;
    SerializedProperty mentalWaveProperty;
    SerializedProperty presetWaveProperty;

    SerializedProperty mapSizeProperty;
    SerializedProperty nextMapDataProperty;
    SerializedProperty shopItemDataList;
    SerializedProperty randomEvent;
    SerializedProperty randomEventInterval;

    bool distanceWaveFoldout;
    bool timeWaveFoldout;
    bool mentalWaveFoldout;
    bool presetWaveFoldout;

    Vector2 timeScrollPos;
    Vector2 distanceScrollPos;
    Vector2 mentalScrollPos;
    Vector2 presetScrollPos;

    public virtual void OnEnable()
    {
        sceneNameProperty = serializedObject.FindProperty("sceneName");
        timeWaveProperty = serializedObject.FindProperty("timeWave");
        distanceWaveProperty = serializedObject.FindProperty("distanceWave");
        mentalWaveProperty = serializedObject.FindProperty("mentalWave");
        presetWaveProperty = serializedObject.FindProperty("presetWave");

        mapSizeProperty = serializedObject.FindProperty("mapSize");
        nextMapDataProperty = serializedObject.FindProperty("nextMapData");
        shopItemDataList = serializedObject.FindProperty("shopItemDataList");
        randomEvent = serializedObject.FindProperty("randomEvent");
        randomEventInterval = serializedObject.FindProperty("randomEventInterval");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sceneNameProperty);

        TimeWaveGUI();
        DistanceWaveGUI();
        MentalWaveGUI();
        PresetWaveGUI();

        EditorGUILayout.PropertyField(mapSizeProperty);
        EditorGUILayout.PropertyField(nextMapDataProperty);
        EditorGUILayout.PropertyField(shopItemDataList);
        EditorGUILayout.PropertyField(randomEvent);
        EditorGUILayout.PropertyField(randomEventInterval);


        serializedObject.ApplyModifiedProperties();
    }

    void DistanceWaveGUI()
    {
        
        distanceWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(distanceWaveFoldout, new GUIContent("�Ÿ� ���̺�"));
        GUILayout.FlexibleSpace();
           

        if (distanceWaveFoldout)
        {
            if (distanceWaveProperty.arraySize > 0)
            {
                int height = distanceWaveProperty.arraySize * 70 > 500 ? 500 : distanceWaveProperty.arraySize * 70;
                distanceScrollPos = EditorGUILayout.BeginScrollView(distanceScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25),GUILayout.MinHeight(height));

                for (int i = 0; i < distanceWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("��");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("�Ÿ�");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty distance = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("distance");
                            distance.floatValue = EditorGUILayout.FloatField(distance.floatValue);
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
                if (GUILayout.Button("�߰�", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    distanceWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("����", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
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
    void TimeWaveGUI()
    {

        timeWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(timeWaveFoldout, new GUIContent("�ð� ���̺�"));
        GUILayout.FlexibleSpace();

        if (timeWaveFoldout)
        {
            if (timeWaveProperty.arraySize > 0)
            {
                int height = timeWaveProperty.arraySize * 70 > 500 ? 500 : timeWaveProperty.arraySize * 70;

                timeScrollPos = EditorGUILayout.BeginScrollView(timeScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < timeWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("������");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("�����ֱ�");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty genTime = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genTime");
                            genTime.floatValue = EditorGUILayout.FloatField(genTime.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty startTime = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("startTime");
                            startTime.intValue = EditorGUILayout.IntField(startTime.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty endTime = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("endTime");
                            endTime.intValue = EditorGUILayout.IntField(endTime.intValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();


                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndScrollView();
            }
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("�߰�", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    timeWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("����", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    if (timeWaveProperty.arraySize > 0)
                        timeWaveProperty.arraySize -= 1;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    void MentalWaveGUI()
    {
        mentalWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mentalWaveFoldout, new GUIContent("���� ���̺�"));
        GUILayout.FlexibleSpace();

        if (mentalWaveFoldout)
        {
            if (mentalWaveProperty.arraySize > 0)
            {
                int height = mentalWaveProperty.arraySize * 70 > 500 ? 500 : mentalWaveProperty.arraySize * 70;
                mentalScrollPos = EditorGUILayout.BeginScrollView(mentalScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < mentalWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("������");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("�����ֱ�");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("���ŷ�");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyName");
                            enemy.intValue = (int)(Define.EnemyName)EditorGUILayout.EnumPopup((Define.EnemyName)enemy.intValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty genTime = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genTime");
                            genTime.floatValue = EditorGUILayout.FloatField(genTime.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty genMentalLevelOrMore = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genMentalLevelOrMore");
                            genMentalLevelOrMore.intValue = EditorGUILayout.IntField(genMentalLevelOrMore.intValue);
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
                if (GUILayout.Button("�߰�", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    mentalWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("����", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
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
    void PresetWaveGUI()
    {

        presetWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(presetWaveFoldout, new GUIContent("������ ���̺�"));
        GUILayout.FlexibleSpace();


        if (presetWaveFoldout)
        {
            if (presetWaveProperty.arraySize > 0)
            {
                int height = presetWaveProperty.arraySize * 70 > 500 ? 500 : presetWaveProperty.arraySize * 70;
                presetScrollPos = EditorGUILayout.BeginScrollView(presetScrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.MinHeight(height));

                for (int i = 0; i < presetWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("������");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("�ð�");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("����");
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            SerializedProperty enemy = presetWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("enemyPreset");
                            enemy.objectReferenceValue = EditorGUILayout.ObjectField(enemy.objectReferenceValue,typeof(GameObject), true) as GameObject;
                            GUILayout.FlexibleSpace();

                            SerializedProperty time = presetWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genTime");
                            time.floatValue = EditorGUILayout.FloatField(time.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty multiply = presetWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("multiply");
                            multiply.floatValue = EditorGUILayout.FloatField(multiply.floatValue);
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();
                        SerializedProperty property = presetWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genLocalPosition");
                        EditorGUILayout.PropertyField(property);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("�߰�", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    presetWaveProperty.arraySize += 1;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("����", GUILayout.Width(EditorGUIUtility.currentViewWidth / 2)))
                {
                    if (presetWaveProperty.arraySize > 0)
                        presetWaveProperty.arraySize -= 1;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}