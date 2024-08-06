/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
* Description: ScriptableObject containing general settings for terrain generation, including mesh resolution, size, 
 *              maximum height, and water level.
 * License: MIT License
 */

using System;
using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    /// <summary>
    /// Scriptable object containing general settings for terrain generation.
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/GeneralSettings")]
    [Serializable]
    public class GeneralSettings : ScriptableObject
    {
        #region Variables

        public Vector2Int resolution = new(20, 20); // Resolution of the generated mesh

        public Vector2 chunkSize = new(256, 256); // Size of the mesh

        [Tooltip("The maximum height of the terrain.")]
        [Range(1, 200)]
        public float maxTerrainHeight = 180.0f; // The maximum height of the terrain.
        
        [Tooltip("The water level relative to the maximum terrain height.")]
        [Range(0, 1)]
        public float waterLevel = 0.3f; // The water level relative to the maximum terrain height.

        #endregion
    }
}
