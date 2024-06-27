/*
 * Author: Rebecca Biebl
 * Creation Date: 30-04-2024
 * Description: This scriptable object class contains settings related to noise generation.
 * License: MIT Licence
 */

using System;
using _Scripts.Helpers;
using UnityEngine;

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

        [Header("Seed Settings")] [Tooltip("Determines if a random seed is used for noise generation.")]
        public bool useRandomSeed = true; // Determines if a random seed is used for noise generation.

        [Tooltip("Lock the seed.")] private bool _seedLocked; // Determines if the seed is locked.

        [Tooltip("Seed value if not using a random seed.")]
        public string seed = "Hello World!"; // Seed value if not using a random seed.

        [Tooltip("Scale factor for calculating the seed offset.")] [Range(10000.0f, 1000000.0f)]
        public float seedScale = 100000.0f; // Scale factor for calculating the seed offset.

        #endregion

        #region Noise Parameters

        [Header("General Noise Settings")]
        [Tooltip("The scale for noise generation, affecting the frequency of noise.")]
        [Range(0.0001f, 10.0f)]
        public float noiseScale = 0.5f; // The scale for noise generation, affecting the frequency of noise.

        [Header("FBM")]
        [Tooltip("The number of octaves in the noise function, impacting the level of detail.")]
        [Range(1, 100)]
        public int octaves = 6; // The number of octaves in the noise function, impacting the level of detail.

        [Tooltip("The amplitude for the noise function.")]
        public float amplitude = 1.0f; // The amplitude for the noise function.

        [Tooltip("The frequency of the noise function.")]
        public float frequency = 1.0f; // The frequency of the noise function.

        [Tooltip("The amplitude persistence for each octave.")] [Range(0f, 1f)]
        public float persistence = 0.5f; // The amplitude persistence for each octave.

        [Tooltip("The frequency multiplier for each octave.")] [Range(1.8f, 10.0f)]
        public float lacunarity = 2.01f; // The frequency multiplier for each octave.

        [Tooltip("Array of the noise function of each layer.")]
        // public NoiseLayer[] noiseLayers = Array.Empty<NoiseLayer>(); // Array of the noise function of each layer.
        public NoiseLayerSettings[] noiseLayerSettings = Array.Empty<NoiseLayerSettings>();

        [HideInInspector] public Vector2[] noiseLayerOffsetVectors; // Offset vectors for noise layers.

        [Header("Domain Warping")] [Tooltip("The number of steps for domain warping.")]
        public int warpSteps = 3; // The number of steps for domain warping.

        [Tooltip("The step size for domain warping.")]
        public float warpStepSize = 10; // The step size for domain warping.

        [Tooltip("The multiplicative factor for domain warping.")]
        public float domainWarpingMultiplicative = 4.0f; // The multiplicative factor for domain warping.

        [Tooltip(" The offset vector for domain warping.")]
        public Vector2 offset = new Vector2(16, 16); // The offset vector for domain warping.

        [HideInInspector] public Vector2[] offsetVectors; // Offset vectors for domain warping.

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

        /// <summary>
        /// Called when the object is loaded or initialized.
        /// </summary>
        private void OnEnable()
        {
            UpdateNoiseLayers();
        }
        
        /// <summary>
        /// Called when the script is loaded or a value is changed in the Inspector.
        /// </summary>
        private void OnValidate()
        {
            UpdateNoiseLayers();
        }

        /// <summary>
        /// Updates the noise layers based on the number of octaves.
        /// </summary>
        private void UpdateNoiseLayers()
        {
            // Create a new array for the updated noise layers
            NoiseLayerSettings[] newNoiseLayers = new NoiseLayerSettings[octaves];
            
            // Copy the existing noise layers to the new array
            for (int i = 0; i < Mathf.Min(octaves, noiseLayerSettings.Length); i++)
            {
                newNoiseLayers[i].noiseLayer = noiseLayerSettings[i].noiseLayer;
            }
        
            // Update the noise layers to the new array
            noiseLayerSettings = newNoiseLayers;
        }

        #endregion
    }
}