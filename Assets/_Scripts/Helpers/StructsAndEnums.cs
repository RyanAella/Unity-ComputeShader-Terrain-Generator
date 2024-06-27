/*
 * Author: Rebecca Biebl
 * Creation Date: 11-06-2024
 * Description: A collection of structs used in various scripts.
 * License: MIT License
 */

using System;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
    /// <summary>
    /// Struct containing settings for terrain generation.
    /// </summary>
    public struct TerrainSettings
    {
        public GeneralSettings GeneralSettings; // General settings for terrain generation.
        public NoiseSettings NoiseSettings; // Noise settings for terrain generation.
    }

    /// <summary>
    /// Struct containing managers for terrain generation.
    /// </summary>
    public struct TerrainGenerationManagers
    {
        public NoiseGenerationManager NoiseGenerationManager { get; set; } // Manages noise generation.
        public FalloffMapManager FalloffMapManager { get; set; } // Manages falloff map generation.
        public MeshGenerationManager MeshGenerationManager { get; set; } // Manages mesh generation.
        public ColourGenerationManager ColourGenerationManager { get; set; } // Manages color generation.

        public TerrainGenerationManagers(
            NoiseGenerationManager noiseGenerationManager,
            FalloffMapManager falloffMapManager,
            MeshGenerationManager meshGenerationManager,
            ColourGenerationManager colourGenerationManager)
        {
            NoiseGenerationManager = noiseGenerationManager;
            FalloffMapManager = falloffMapManager;
            MeshGenerationManager = meshGenerationManager;
            ColourGenerationManager = colourGenerationManager;
        }
    }

    /// <summary>
    /// Struct for generators.
    /// </summary>
    public struct Generators
    {
        // Add generator-related properties here.
    }

    /// <summary>
    /// Struct representing a terrain color.
    /// </summary>
    [Serializable]
    public struct TerrainColour
    {
        public float height; // The height at which this color applies.
        public Color colour; // The color associated with the height.
    }

    /// <summary>
    /// Struct representing noise layer settings.
    /// </summary>
    [Serializable]
    public struct NoiseLayerSettings
    {
        public NoiseLayer noiseLayer; // The type of noise layer.
    }

    #region Enums

    /// <summary>
    /// Enumeration representing different noise layer types.
    /// </summary>
    public enum NoiseLayer
    {
        Regular = 0, // Regular noise layer.
        Billow = 1, // Billow noise layer.
        Ridge = 2, // Ridge noise layer.
    }

    #endregion
}