/*
 * Author: Rebecca Biebl
 * Creation Date: 25-07-2024
 * Description: Custom Editor script for the EnvironmentGenerator component.
 *              This script extends the Unity Editor to provide a custom
 *              Inspector interface for the EnvironmentGenerator, allowing
 *              for automatic updates and manual generation of terrain.

 * License: MIT Licence
 */


using _Scripts.Generators;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(EnvironmentGenerator))]
    public class EnvironmentGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EnvironmentGenerator environmentGenerator = (EnvironmentGenerator)target;

            if (DrawDefaultInspector())
            {
                if (environmentGenerator.autoUpdate)
                {
                    environmentGenerator.StartGeneration();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                environmentGenerator.StartGeneration();
            }
        }
    }
}
