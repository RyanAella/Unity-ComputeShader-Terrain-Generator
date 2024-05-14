/*
 * Author: Rebecca Biebl
 * Creation Date: 30-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    /// <summary>
    /// This ScriptableObject class contains settings related to noise generation.
    /// </summary>
    [CreateAssetMenu(menuName =
        "ScriptableObjects/Settings/Noise")] // This attribute creates a menu entry in the Unity Editor to create a NoiseSettings ScriptableObject.
    public class NoiseSettings : ScriptableObject
    {
        #region Seed Settings

        [Header("Seed")] // Groups the following fields under the "Seed" header in the Inspector.
        public bool useRandomSeed = true; // Determines if a random seed is used for noise generation.

        private bool _seedLocked; // A flag to prevent changing the seed after it is set.
        
        public string seed = "Hello World!"; // Default seed value if not using a random seed.

        [Range(100000.0f, 1000000.0f)]
        public float seedScale = 100000.0f; // Scale factor for calculating the seed offset.

        #endregion

        #region Noise Parameters

        [Header("Noise")] // Groups the following fields under the "Noise" header in the Inspector.
        
        [Range(0.1f, 1.0f)]
        public float noiseScale = 0.5f; // The scale for noise generation, affecting the frequency of noise.

        [Range(0.5f, 5.0f)]
        public float noiseHeight = 2.0f; // The height factor for the noise, affecting how much variation there is in the generated terrain.

        [Range(1, 8)]
        public int octaves = 6; // The number of octaves in the noise function, impacting the level of detail.

        [Range(0.3f, 0.7f)] 
        public float persistence = 0.5f; // The amplitude persistence for each octave.

        [Range(1.8f, 2.7f)] 
        public float lacunarity = 2.01f; // The frequency multiplier for each octave.

        [Range(1, 200)] 
        public float maxTerrainHeight = 180.0f;

        #endregion

        #region Methods

        /// <summary>
        /// Sets the seed for noise generation if it's not locked.
        /// </summary>
        /// <param name="inSeed">The seed value to set.</param>
        public void SetSeed(string inSeed)
        {
            if (_seedLocked) return; // If the seed is locked, prevent changes.

            seed = inSeed; // Set the seed to the new value.
            _seedLocked = true; // Lock the seed to prevent further changes.
        }

        /// <summary>
        /// Retrieves the current seed value.
        /// </summary>
        /// <returns>The current seed as a string.</returns>
        public string GetSeed()
        {
            return seed; // Returns the current seed value.
        }

        #endregion
    }
}