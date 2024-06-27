/*
 * Author: Rebecca Biebl
 * Creation Date: 21-05-2024
 * Description: This script generates a ground mesh using noise and falloff maps, applies colour based on a gradient palette,
 *              and adjusts the mesh height. It integrates with several manager classes to handle the different aspects of mesh
 *              generation and colour application.
 * License: MIT License
 */

using System.Collections.Generic;
using _Scripts.Helpers;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.TerrainGenerators
{
    /// <summary>
    /// This class is responsible for generating the ground mesh and applying colour to it.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class GroundGenerator : BaseGenerator
    {
        #region Methods

        /// <summary>
        /// Generates the ground mesh and applies colour to it.
        /// </summary>
        /// <param name="managers">The terrain generation managers for noise, mesh, and colour generation.</param>
        /// <param name="terrainSettings">The settings for terrain generation.</param>
        /// <param name="shaders">The shader settings for colour generation.</param>
        /// <param name="colourGradient">The colour gradient palette for mesh colouring.</param>
        /// <param name="colourHeights">The heights at which colours should transition.</param>
        /// <param name="meshFilter">The mesh filter to apply the generated mesh.</param>
        /// <param name="groundColourCount">The number of colours in the palette.</param>
        public void GenerateGround(TerrainGenerationManagers managers, TerrainSettings terrainSettings,
            Shaders shaders, List<Vector4> colourGradient, float[] colourHeights, MeshFilter meshFilter,
            int groundColourCount)
        {
            // Generate the noise and mesh.
            if (!GenerateNoiseAndMesh(managers, terrainSettings, shaders, meshFilter, false))
            {
                return; // Stop further processing if mesh generation fails.
            }

            Material material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;

            // Use the ColourGenerationManager class to colour the mesh using the specified compute shader, mesh filter, chunkSize, min/max values, and colour gradient palette.
            managers.ColourGenerationManager.ColourMesh(shaders.colourGenerationComputeShader, meshFilter,
                terrainSettings.GeneralSettings.resolution, new[] { terrainSettings.GeneralSettings.waterLevel, 1f },
                colourGradient.ToArray(), colourHeights, groundColourCount, false, material);

            // Adjust the mesh height.
            AdjustMeshHeight(meshFilter, terrainSettings.GeneralSettings);
        }

        #endregion
    }
}