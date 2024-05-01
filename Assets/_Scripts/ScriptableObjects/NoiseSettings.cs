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
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/Settings/Noise")]
    public class NoiseSettings : ScriptableObject
    {
        [Range(0.0f, 0.1f)] public float noiseScale = 0.0768f;

        public float noiseHeight = 8f;

        public int octaves = 8;
        public float persistence = 0.5f;
        public float lacunarity = 2.01f;
    }
}