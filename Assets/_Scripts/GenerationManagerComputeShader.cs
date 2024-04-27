/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GenerationManagerComputeShader : MonoBehaviour
    {
        #region Variables

        [Header("General Settings")] [SerializeField]
        private Vector2Int resolution = new(20, 20);

        [SerializeField] private Vector2Int totalResolutionPerMesh = new(64, 64);
        
        [Header("Compute Shader Settings")] 
        [SerializeField] private ComputeShader computeShader;


        // [SerializeField] private MeshRenderer renderer;

        private float[,] _noiseMap;

        public ComputeShaderManager computeShaderManager;
        public MeshGenerator meshGenerator;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            computeShaderManager = new ComputeShaderManager();
            meshGenerator = new MeshGenerator();

            MeshFilter meshFilter = GetComponent<MeshFilter>();

            computeShaderManager.InitializeBuffer(resolution);
            meshGenerator.InitializeMesh(meshFilter, totalResolutionPerMesh);
        }

        private void Start()
        {
            _noiseMap = computeShaderManager.GenerateNoiseMap(computeShader, resolution);

            meshGenerator.DrawNoiseMap(_noiseMap);
        }

        private void Update()
        {
            meshGenerator.UpdateMesh();
        }

        private void OnDestroy()
        {
            computeShaderManager.ReleaseBuffer();
        }

        #endregion

        #region Methods

        #endregion
    }
}