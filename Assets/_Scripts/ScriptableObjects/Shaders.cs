/*
 * Author: Rebecca Biebl
 * Creation Date: 29-05-2024
 * Description: A scriptable object that stores the shaders used in the project.
 * License: MIT Licence
 */


using System;
using UnityEngine;

namespace _Scripts.ScriptableObjects
{
    /// <summary>
    /// Scriptable object that stores the shaders used in the project.
    /// </summary>
    [CreateAssetMenu(fileName = "Shaders", menuName = "Shaders")]
    [Serializable]
    public class Shaders : ScriptableObject
    {
        #region Variables

        // Reference to the compute shader used for generating the vertices and triangles.
        [Header("Compute Shader Settings")]
        public ComputeShader noiseGenerationComputeShader; // Compute shader for noise generation

        public ComputeShader meshGenerationComputeShader; // Compute shader for mesh generation
        
        public ComputeShader valueClampComputeShader; // Compute shader for noise generation
        
        public ComputeShader falloffComputeShader; // Compute shader for falloff generation

        public ComputeShader colourGenerationComputeShader; // Compute shader for colourGradient generation
        
        public ComputeShader waterMovementComputeShader; // Compute shader for water movement

        #endregion
    }
}