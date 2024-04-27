/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: A brief description of the script.
 * License: Licence
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GenerationManager : MonoBehaviour
    {
        #region Variables

        public Vector2Int resolution = new Vector2Int(20, 20);

        private Mesh _mesh;

        private Vector3[] _vertices;
        private int[] _triangles;

        #endregion

        #region Unity Methods

        public void Start()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
            _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            CreateShape();
            UpdateMesh();
        }

        #endregion

        #region Methods

        private void CreateShape()
        {
            _vertices = new Vector3[(resolution.x + 1) * (resolution.y + 1)];

            for (int i = 0, z = 0; z <= resolution.y; z++)
            {
                for (int x = 0; x <= resolution.x; x++)
                {
                    _vertices[i] = new Vector3(x, 0, z);
                    i++;
                }
            }

            _triangles = new int[resolution.x * resolution.y * 6];

            int vertex = 0;
            int triangle = 0;
            for (int z = 0; z < resolution.y; z++)
            {
                for (int x = 0; x < resolution.x; x++)
                {
                    _triangles[triangle + 0] = vertex + 0;
                    _triangles[triangle + 1] = vertex + resolution.x + 1;
                    _triangles[triangle + 2] = vertex + 1;
                    _triangles[triangle + 3] = vertex + 1;
                    _triangles[triangle + 4] = vertex + resolution.x + 1;
                    _triangles[triangle + 5] = vertex + resolution.x + 2;

                    vertex++;
                    triangle += 6;
                }

                vertex++;
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