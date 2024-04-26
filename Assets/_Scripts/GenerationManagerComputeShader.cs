/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GenerationManagerComputeShader : MonoBehaviour
    {
        #region Variables

        [Header("General Settings")] 
        [SerializeField] private Vector2Int resolution = new(20, 20);

        // [SerializeField] private MeshRenderer renderer;

        [Header("Compute Shader Settings")] 
        [SerializeField] private ComputeShader computeShader;

        [SerializeField] private ComputeBuffer _noiseBuffer;

        private static readonly int MapWidth = Shader.PropertyToID("map_width");
        private static readonly int MapHeight = Shader.PropertyToID("map_height");

        private static readonly int NoiseMap = Shader.PropertyToID("NoiseMap");

        private float[,] _noiseMap;

        private Mesh _mesh;

        private Vector3[] _vertices;
        private int[] _triangles;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            CreateBuffer();
        }

        private void Start()
        {
            GenerateNoiseMap();
            
            _mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            
            DrawNoiseMap();
        }

        private void Update()
        {
            UpdateMesh();
        }

        private void OnDestroy()
        {
            ReleaseBuffer();
        }

        #endregion

        #region Methods

        private void CreateBuffer()
        {
            _noiseBuffer = new ComputeBuffer(resolution.x * resolution.y, sizeof(float));
        }

        private void ReleaseBuffer()
        {
            _noiseBuffer.Release();
        }

        private void GenerateNoiseMap()
        {
            _noiseMap = new float[resolution.x, resolution.y];

            var noiseKernel = computeShader.FindKernel("NoiseGenerator");

            computeShader.SetInt(MapWidth, resolution.x);
            computeShader.SetInt(MapHeight, resolution.y);

            computeShader.SetBuffer(0, NoiseMap, _noiseBuffer);

            computeShader.Dispatch(0, resolution.x, resolution.y, 1);
            _noiseBuffer.GetData(_noiseMap);
        }

        private void DrawNoiseMap()
        {
            var width = _noiseMap.GetLength(0);
            var height = _noiseMap.GetLength(1);

            _vertices = new Vector3[(width + 1) * (height + 1)];

            for (var z = 0; z < height; z++)
            for (var x = 0; x < width; x++)
                _vertices[z * width + x] = new Vector3(x, _noiseMap[x, z], z);

            _triangles = new int[(width - 1) * (height - 1) * 6];

            var vertexIndex = 0;
            var triangle = 0;
            for (var y = 0; y < height - 1; y++)
            for (var x = 0; x < width - 1; x++)
            {
                vertexIndex = y * width + x;

                _triangles[triangle + 0] = vertexIndex + 0;
                _triangles[triangle + 1] = vertexIndex + width;
                _triangles[triangle + 2] = vertexIndex + width + 1;
                _triangles[triangle + 3] = vertexIndex + 0;
                _triangles[triangle + 4] = vertexIndex + width + 1;
                _triangles[triangle + 5] = vertexIndex + 1;

                triangle += 6;
            }
        }

        private void UpdateMesh()
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();
        }

        #endregion
    }
}