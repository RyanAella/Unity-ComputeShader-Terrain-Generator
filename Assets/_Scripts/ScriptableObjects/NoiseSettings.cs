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
        [Header("Seed Settings")] 
        [Tooltip("Determines if a random seed is used for noise generation.")]
        public bool useRandomSeed = true;

        [Tooltip("Lock the seed.")] 
        private bool _seedLocked;

        [Tooltip("Seed value if not using a random seed.")]
        public string seed = "Hello World!";

        [Tooltip("Scale factor for calculating the seed offset.")] 
        [Range(10000.0f, 1000000.0f)]
        public float seedScale = 100000.0f;

        [Header("General Noise Settings")]
        [Tooltip("The scale for noise generation.")]
        public float noiseScale = 0.5f;

        [Header("FBM")]
        [Tooltip("The number of octaves in the noise function, impacting the level of detail.")]
        [Range(1, 100)]
        public int octaves = 6;

        [Tooltip("The amplitude for the noise function.")]
        public float amplitude = 1.0f;

        [Tooltip("The frequency of the noise function.")]
        public float frequency = 1.0f;

        [Tooltip("The amplitude persistence for each octave.")] [Range(0f, 1f)]
        public float persistence = 0.5f;

        [Tooltip("The frequency multiplier for each octave.")] [Range(1f, 10.0f)]
        public float lacunarity = 2.01f;
        
        [Header("Domain Warping")] 
        [Tooltip("The number of steps for domain warping.")]
        public int warpSteps = 3;

        [Tooltip("The step size for domain warping.")]
        public float warpStepSize = 10;

        [Tooltip("The multiplicative factor for domain warping.")]
        public float domainWarpingMultiplicative = 4.0f;

        [Tooltip(" The offset vector for domain warping.")]
        public Vector2 offset = new Vector2(16, 16);
        
        [Tooltip("Array of the noise function of each layer.")]
        public NoiseLayerSettings[] noiseLayerSettings = Array.Empty<NoiseLayerSettings>();

        [HideInInspector] 
        public Vector2[] offsetVectors;
        
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