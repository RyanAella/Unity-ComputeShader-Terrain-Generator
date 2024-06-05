/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System;
using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/GeneralSettings")]
    [Serializable]
    public class GeneralSettings : ScriptableObject
    {
        #region Variables

        // public Vector2Int chunkSize = new(20, 20); // Resolution of the generated mesh

        public static Vector2Int chunkSize = new(241, 241); // Size of the chunks in the mesh

        [Range(0, 6)] public int levelOfDetail; // For i = 2, 4, 6, 8, 10, 12
        
        public int meshSimplificationIncrement;

        public Gradient colourGradient; // Gradient of the colour of the mesh

        #endregion
    }
}
