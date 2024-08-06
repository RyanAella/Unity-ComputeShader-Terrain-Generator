/*
 * Author: Rebecca Biebl
 * Creation Date: 25-07-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections;
using System.Collections.Generic;
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
