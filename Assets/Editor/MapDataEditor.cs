using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapData))]
[CanEditMultipleObjects]
public class MapDataEditor : Editor
{
    SerializedProperty sceneNameProperty;
    SerializedProperty timeWaveProperty;
    SerializedProperty distanceWaveProperty;
    SerializedProperty mentalWaveProperty;

    bool distanceWaveFoldout;
    bool timeWaveFoldout;
    bool mentalWaveFoldout;

    Vector2 scrollPos;

    public virtual void OnEnable()
    {
        sceneNameProperty = serializedObject.FindProperty("sceneName");
        timeWaveProperty = serializedObject.FindProperty("timeWave");
        distanceWaveProperty = serializedObject.FindProperty("distanceWave");
        mentalWaveProperty = serializedObject.FindProperty("mentalWave");

    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sceneNameProperty);

        TimeWaveGUI();
        DistanceWaveGUI();
        MentalWaveGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void DistanceWaveGUI()
    {
        Rect rect = EditorGUILayout.BeginHorizontal();
        {
            distanceWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(distanceWaveFoldout, new GUIContent("�Ÿ� ���̺�"));
            GUILayout.FlexibleSpace();
            
            distanceWaveProperty.arraySize = EditorGUI.IntField(new Rect(rect.x +  rect.width -50,rect.y,50,rect.height),distanceWaveProperty.arraySize);
            GUILayout.FlexibleSpace();

        }
        EditorGUILayout.EndHorizontal();

        if (distanceWaveFoldout)
        {
            if (distanceWaveProperty.arraySize > 0)
            {
                int height = distanceWaveProperty.arraySize * 70 > 500 ? 500 : distanceWaveProperty.arraySize * 70;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25),GUILayout.MinHeight(height));

                for (int i = 0; i < distanceWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("��");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("ü�¹���");
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

                            SerializedProperty hpMul = distanceWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("hpMultiply");
                            hpMul.floatValue = EditorGUILayout.FloatField(hpMul.floatValue);
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
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
    void TimeWaveGUI()
    {

        Rect rect = EditorGUILayout.BeginHorizontal();
        {
            timeWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(timeWaveFoldout, new GUIContent("�ð� ���̺�"));
            GUILayout.FlexibleSpace();

            timeWaveProperty.arraySize = EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), timeWaveProperty.arraySize);
            GUILayout.FlexibleSpace();

        }
        EditorGUILayout.EndHorizontal();

        if (timeWaveFoldout)
        {
            if (timeWaveProperty.arraySize > 0)
            {
                int height = distanceWaveProperty.arraySize * 70 > 500 ? 500 : distanceWaveProperty.arraySize * 70;

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < timeWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("������");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("ü�¹���");
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

                            SerializedProperty hpMul = timeWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("hpMultiply");
                            hpMul.floatValue = EditorGUILayout.FloatField(hpMul.floatValue);
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
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    void MentalWaveGUI()
    {
        Rect rect = EditorGUILayout.BeginHorizontal();
        {
            mentalWaveFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mentalWaveFoldout, new GUIContent("���� ���̺�"));
            GUILayout.FlexibleSpace();

            mentalWaveProperty.arraySize = EditorGUI.IntField(new Rect(rect.x + rect.width - 50, rect.y, 50, rect.height), mentalWaveProperty.arraySize);
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();

        if (mentalWaveFoldout)
        {
            if (mentalWaveProperty.arraySize > 0)
            {
                int height = distanceWaveProperty.arraySize * 70 > 500 ? 500 : distanceWaveProperty.arraySize * 70;
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25), GUILayout.Height(height));

                for (int i = 0; i < mentalWaveProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("������");
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("ü�¹���");
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

                            SerializedProperty hpMul = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("hpMultiply");
                            hpMul.floatValue = EditorGUILayout.FloatField(hpMul.floatValue);
                            GUILayout.FlexibleSpace();

                            SerializedProperty genMentalLevelOrMore = mentalWaveProperty.GetArrayElementAtIndex(i).FindPropertyRelative("genMentalLevelOrMore");
                            genMentalLevelOrMore.intValue = EditorGUILayout.IntField(genMentalLevelOrMore.intValue);
                            GUILayout.FlexibleSpace();

                        }
                        EditorGUILayout.EndHorizontal();


                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndScrollView();
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }
}