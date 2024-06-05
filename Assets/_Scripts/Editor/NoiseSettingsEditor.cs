// /*
//  * Author: Rebecca Biebl
//  * Creation Date: 06-06-2024
//  * Description: A brief description of the script.
//  * License: Licence
//  */
//
//
// using System;
// using _Scripts.Helpers;
// using _Scripts.ScriptableObjects;
// using UnityEditor;
// using UnityEngine;
//
// namespace _Scripts.Editor
// {
//     [CustomEditor(typeof(NoiseSettings))]
//     public class NoiseSettingsEditor : UnityEditor.Editor
//     {
//         SerializedProperty octavesProp;
//         SerializedProperty noiseLayersProp;
//
//         private void OnEnable()
//         {
//             octavesProp = serializedObject.FindProperty("octaves");
//             noiseLayersProp = serializedObject.FindProperty("noiseLayers");
//         }
//
//         public override void OnInspectorGUI()
//         {
//             serializedObject.Update();
//
//             NoiseSettings noiseSettings = (NoiseSettings)target;
//
//             EditorGUILayout.PropertyField(octavesProp);
//             EditorGUILayout.PropertyField(noiseLayersProp, true);
//
//             if (noiseSettings.noiseLayers == null || noiseSettings.noiseLayers.Length != noiseSettings.octaves)
//             {
//                 // Resize the noiseLayers array to match warpSteps
//                 Array.Resize(ref noiseSettings.noiseLayers, noiseSettings.octaves);
//
//                 // Initialize the array elements if they are null
//                 for (int i = 0; i < noiseSettings.noiseLayers.Length; i++)
//                 {
//                     if (noiseSettings.noiseLayers[i] == NoiseLayer.None)
//                     {
//                         noiseSettings.noiseLayers[i] = NoiseLayer.Regular;
//                     }
//                 }
//
//                 // Mark the ScriptableObject as dirty to ensure changes are saved
//                 EditorUtility.SetDirty(noiseSettings);
//             }
//
//             serializedObject.ApplyModifiedProperties();
//         }
//     }
// }