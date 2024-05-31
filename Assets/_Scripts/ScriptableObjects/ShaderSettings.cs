/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "ShaderSettings", menuName = "ShaderSettings")]
    public class ShaderSettings : ScriptableObject
    {
        #region Variables

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")]
        public ComputeShader noiseGenerationComputeShader; // Compute shader for noise generation

        public ComputeShader valueClampComputeShader; // Compute shader for noise generation
        
        public ComputeShader falloffComputeShader; // Compute shader for falloff generation

        public ComputeShader colourGenerationComputeShader; // Compute shader for colourGradient generation

        #endregion
    }
}