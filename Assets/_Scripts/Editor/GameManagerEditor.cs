/*
 * Author: Rebecca Biebl
 * Creation Date: 07-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        #region Variables

        private bool _meshGenerated = false;

        #endregion

        #region Unity Methods

        public override void OnInspectorGUI()
        {
            // Verweis auf das GameManager-Script
            GameManager gameManager = (GameManager)target;

            // Zeichnen des Standard-Inspectors, um die bestehenden Felder beizubehalten
            DrawDefaultInspector();

            // Schaltfläche zum Generieren des Meshs
            if (GUILayout.Button("Generate Mesh"))
            {
                // Aufrufen der Mesh-Generierungsfunktion im GameManager
                _meshGenerated = gameManager.GenerateMesh();
            }

            // Schaltfläche zum Färben des Meshs
            if (GUILayout.Button("Colour Mesh"))
            {
                // Aufrufen der Funktion zum Färben des Meshs im GameManager
                gameManager.ColourMesh(_meshGenerated);
            }
        }

        #endregion
    }
}
