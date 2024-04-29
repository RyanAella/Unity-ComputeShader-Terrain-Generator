/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    [Serializable]
    public class MeshGenerator
    {
        #region Variables

        private Mesh _mesh;

        private Vector3[] _vertices;
        private int[] _triangles;

        #endregion

        #region Methods

        public void InitializeMesh(MeshFilter filter, Vector2Int totalResolution)
        {
            _mesh = new Mesh();
            filter.sharedMesh = _mesh;
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        public void DrawNoiseMap(Vector3[] noiseMap, Vector2Int resolution)
        {
            Debug.Log(noiseMap.Length);
            for (int i = 0; i < noiseMap.Length; i++)
            {
                Debug.Log(noiseMap[i]);
            }
            
            if (noiseMap != null)
            {
                var width = resolution.x/*noiseMap.GetLength(0)*/;
                var height = resolution.y/*noiseMap.GetLength(1)*/;

                _vertices = new Vector3[width * height];

                for (var z = 0; z < height; z++)
                for (var x = 0; x < width; x++)
                    _vertices[z * width + x] = noiseMap[z * width + x];

                _triangles = new int[(width - 1) * (height - 1) * 6];

                var triangle = 0;
                for (var y = 0; y < height - 1; y++)
                {
                    for (var x = 0; x < width - 1; x++)
                    {
                        var vertexIndex = y * width + x;

                        _triangles[triangle + 0] = vertexIndex + 0;
                        _triangles[triangle + 1] = vertexIndex + width;
                        _triangles[triangle + 2] = vertexIndex + width + 1;
                        _triangles[triangle + 3] = vertexIndex + 0;
                        _triangles[triangle + 4] = vertexIndex + width + 1;
                        _triangles[triangle + 5] = vertexIndex + 1;

                        triangle += 6;
                    }
                }
            }
            else
            {
                Debug.LogError("noiseMap ist null");
            }
        }

        public void UpdateMesh()
        {
            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.triangles = _triangles;

            _mesh.RecalculateNormals();
        }

        #endregion
    }
}