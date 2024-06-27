/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: This script manages the generation of water using compute shaders in Unity.
 * License: MIT Licence
 */

using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.TerrainGenerators
{
    /// <summary>
    /// Generates water terrain based on provided settings.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class WaterGenerator : BaseGenerator
    {
        #region Methods

        /// <summary>
        /// Generates the water terrain mesh.
        /// </summary>
        /// <param name="managers">The terrain generation managers.</param>
        /// <param name="terrainSettings">The terrain settings.</param>
        /// <param name="shaders">The shaders used for generation.</param>
        /// <param name="colourGradient">The color gradient for water.</param>
        /// <param name="colourHeights">The color heights for water.</param>
        /// <param name="meshFilter">The mesh filter to apply the mesh to.</param>
        /// <param name="maxHeight">The maximum height of the water.</param>
        /// <param name="colourCount">The count of colors for water.</param>
        public void GenerateWater(TerrainGenerationManagers managers, TerrainSettings terrainSettings, Shaders shaders,
            List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter, float maxHeight,
            int colourCount)
        {
            // Generate the noise and mesh.
            if (!GenerateNoiseAndMesh(managers, terrainSettings, shaders, meshFilter, true))
            {
                return; // Stop further processing if mesh generation fails.
            }

            // meshFilter.sharedMesh.colors = groundMeshFilter.sharedMesh.colors;

            // Set the water position.
            var pos = transform.position;
            transform.position = new Vector3(pos.x, maxHeight * terrainSettings.GeneralSettings.waterLevel, pos.z);
        }

        #endregion
    }
}