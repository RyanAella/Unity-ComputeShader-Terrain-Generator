// using _Scripts.Manager;
// using _Scripts.ScriptableObjects;
// using UnityEditor;
// using UnityEngine;
//
// namespace _Scripts.Editor
// {
//     [CustomEditor(typeof(GameManager))]
//     public class GameManagerEditor : UnityEditor.Editor
//     {
//         private SerializedProperty generalSettingsProp;
//         private SerializedProperty shaderSettingsProp;
//         private SerializedProperty groundNoiseSettingsProp;
//         private SerializedProperty waterNoiseSettingsProp;
//
//         private bool showGeneralSettings = true;
//         private bool showShaderSettings = true;
//         private bool showGroundGeneration = true;
//         private bool showWaterGeneration = true;
//
//         private void OnEnable()
//         {
//             generalSettingsProp = serializedObject.FindProperty("generalSettings");
//             shaderSettingsProp = serializedObject.FindProperty("shaderSettings");
//             groundNoiseSettingsProp = serializedObject.FindProperty("groundNoiseSettings");
//             waterNoiseSettingsProp = serializedObject.FindProperty("waterNoiseSettings");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();
//
//             showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "General Settings");
//             if (showGeneralSettings)
//             {
//                 EditorGUILayout.PropertyField(generalSettingsProp);
//                 if (generalSettingsProp.objectReferenceValue != null)
//                 {
//                     UnityEditor.Editor generalSettingsEditor = CreateEditor(generalSettingsProp.objectReferenceValue);
//                     generalSettingsEditor.OnInspectorGUI();
//                 }
//             }
//
//             showShaderSettings = EditorGUILayout.Foldout(showShaderSettings, "Shader Settings");
//             if (showShaderSettings)
//             {
//                 EditorGUILayout.PropertyField(shaderSettingsProp);
//                 if (shaderSettingsProp.objectReferenceValue != null)
//                 {
//                     UnityEditor.Editor shaderSettingsEditor = CreateEditor(shaderSettingsProp.objectReferenceValue);
//                     shaderSettingsEditor.OnInspectorGUI();
//                 }
//             }
//
//             showGroundGeneration = EditorGUILayout.Foldout(showGroundGeneration, "Ground Generation");
//             if (showGroundGeneration)
//             {
//                 EditorGUILayout.PropertyField(groundNoiseSettingsProp);
//                 if (groundNoiseSettingsProp.objectReferenceValue != null)
//                 {
//                     UnityEditor.Editor groundSettingsEditor = CreateEditor(groundNoiseSettingsProp.objectReferenceValue);
//                     groundSettingsEditor.OnInspectorGUI();
//                 }
//             }
//
//             showWaterGeneration = EditorGUILayout.Foldout(showWaterGeneration, "Water Generation");
//             if (showWaterGeneration)
//             {
//                 EditorGUILayout.PropertyField(waterNoiseSettingsProp);
//                 if (waterNoiseSettingsProp.objectReferenceValue != null)
//                 {
//                     UnityEditor.Editor waterSettingsEditor = CreateEditor(waterNoiseSettingsProp.objectReferenceValue);
//                     waterSettingsEditor.OnInspectorGUI();
//                 }
//             }
//
//             serializedObject.ApplyModifiedProperties();
//         }
//     }
// }
