/*
 * Author: Rebecca Biebl
 * Creation Date: 11-06-2024
 * Description: A collection of structs used in various scripts.
 * License: Licence
 */

using System;
using _Scripts.Manager;
using _Scripts.ScriptableObjects;
using UnityEngine;

namespace _Scripts.Helpers
{
    #region Structs
    
    public struct TerrainSettings
    {
        public GeneralSettings GeneralSettings;
        public NoiseSettings GroundNoiseSettings;
        public NoiseSettings WaterNoiseSettings;
    }

    public struct TerrainGenerationManagers
    {
        public NoiseGenerationManager NoiseGenerationManager { get; set; }
        public FalloffMapManager FalloffMapManager { get; set; }
        public MeshGenerationManager MeshGenerationManager { get; set; }
        public ColourGenerationManager ColourGenerationManager { get; set; }

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
    
    [Serializable]
    public struct TerrainColour
    {
        public float height;
        public Color colour;
    }
    
    [Serializable]
    public struct NoiseLayerSettings
    {
        public NoiseLayer noiseLayer;
    }
    
    #endregion

    #region Enums

    public enum NoiseLayer
    {
        Regular = 0,
        Billow = 1,
        Ridge = 2,
    }

    #endregion
}
