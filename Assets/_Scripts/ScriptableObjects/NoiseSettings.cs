/*
 * Author: Rebecca Biebl
 * Creation Date: 30-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System;
using _Scripts.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.ScriptableObjects
{
    /// <summary>
    /// This ScriptableObject class contains settings related to noise generation.
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/Noise")]
    [Serializable]
    public class NoiseSettings : ScriptableObject
    {
        #region Seed Settings

        [Header("Seed Settings")]
        [Tooltip("Determines if a random seed is used for noise generation.")]
        public bool useRandomSeed = true;

        private bool _seedLocked;

        [Tooltip("Seed value if not using a random seed.")]
        public string seed = "Hello World!";

        [Tooltip("Scale factor for calculating the seed offset.")]
        [Range(10000.0f, 1000000.0f)]
        public float seedScale = 100000.0f;

        #endregion

        #region Noise Parameters

        [Header("General Noise Settings")]
        [Tooltip("The scale for noise generation, affecting the frequency of noise.")]
        [Range(0.0001f, 10.0f)]
        public float noiseScale = 0.5f;
        [Tooltip("The maximum height of the terrain.")]
        [Range(1, 200)]
        public float maxTerrainHeight = 180.0f;
        [Tooltip("The water level relative to the maximum terrain height.")]
        [Range(0, 1)]
        public float waterLevel = 0.3f;

        [Header("FBM")]
        [Tooltip("The number of octaves in the noise function, impacting the level of detail.")]
        [Range(1, 10)]
        public int octaves = 6;

        public float amplitude = 1.0f;
        public float frequency = 1.0f;

        [Tooltip("The amplitude persistence for each octave.")]
        [Range(0f, 1f)]
        public float persistence = 0.5f;

        [Tooltip("The frequency multiplier for each octave.")]
        [Range(1.8f, 10.0f)]
        public float lacunarity = 2.01f;

        public NoiseLayer[] noiseLayers = Array.Empty<NoiseLayer>();
        [HideInInspector] public Vector2[] noiseLayerOffsetVectors;

        [Header("Domain Warping")]
        public int warpSteps = 3;
        public float warpStepSize = 10;
        public float domainWarpingMultiplicative = 4.0f;
        public Vector2 offset = new Vector2(16, 16);
        [HideInInspector]
        public Vector2[] offsetVectors;
        
        // ToDo: Check if I need them
        public NoiseType noiseType;
        public float sharpnessScalar = 0.25f;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the seed for noise generation if it's not locked.
        /// </summary>
        /// <param name="inSeed">The seed value to set.</param>
        public void SetSeed(string inSeed)
        {
            if (_seedLocked) return;
            seed = inSeed;
            _seedLocked = true;
        }

        /// <summary>
        /// Retrieves the current seed value.
        /// </summary>
        /// <returns>The current seed as a string.</returns>
        public string GetSeed()
        {
            return seed;
        }

        private void OnEnable()
        {
            UpdateNoiseLayers();
        }

        private void OnValidate()
        {
            UpdateNoiseLayers();
        }

        private void UpdateNoiseLayers()
        {
            NoiseLayer[] newNoiseLayers = new NoiseLayer[octaves];
            for (int i = 0; i < Mathf.Min(octaves, noiseLayers.Length); i++)
            {
                newNoiseLayers[i] = noiseLayers[i];
            }
            noiseLayers = newNoiseLayers;
        }

        #endregion
    }
}


