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
        public ColourGenerationManager GroundColourGenerationManager { get; set; }
        public ColourGenerationManager WaterColourGenerationManager { get; set; }

        public TerrainGenerationManagers(NoiseGenerationManager noiseGenerationManager,
            FalloffMapManager falloffMapManager,
            MeshGenerationManager meshGenerationManager,
            ColourGenerationManager groundColourGenerationManager,
            ColourGenerationManager waterColourGenerationManager)
        {
            NoiseGenerationManager = noiseGenerationManager;
            FalloffMapManager = falloffMapManager;
            MeshGenerationManager = meshGenerationManager;
            GroundColourGenerationManager = groundColourGenerationManager;
            WaterColourGenerationManager = waterColourGenerationManager;
        }
    }
    
    [Serializable]
    public struct TerrainColour
    {
        public float Height;
        public Color Colour;
    }
    
    [Serializable]
    public struct NoiseLayerSettings
    {
        public NoiseLayer noiseLayer;
        public float strength;
    }
}
