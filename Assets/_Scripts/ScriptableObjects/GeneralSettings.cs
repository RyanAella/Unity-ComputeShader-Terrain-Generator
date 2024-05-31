/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/GeneralSettings")]
    public class GeneralSettings : ScriptableObject
    {
        #region Variables

        // public Vector2Int chunkSize = new(20, 20); // Resolution of the generated mesh

        public int chunkSize = 241; // Size of the chunks in the mesh

        [Range(0, 6)] public int levelOfDetail;
        
        public int meshSimplificationIncrement;

        public Gradient colourGradient; // Gradient of the colour of the mesh

        #endregion
    }
}