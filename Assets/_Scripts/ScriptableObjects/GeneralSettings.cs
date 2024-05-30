/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/GeneralSettings")]
    public class GeneralSettings : ScriptableObject
    {
        #region Variables
        
        public Vector2Int resolution = new(20, 20); // Resolution of the generated mesh
        public float islandRadius; // Radius of the island.
        public Gradient colourGradient; // Gradient of the colour of the mesh

        #endregion
    }
}
