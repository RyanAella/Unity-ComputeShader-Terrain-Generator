/*
 * Author: Rebecca Biebl
 * Creation Date: 26-04-2024
 * Description: Class for generating and updating a mesh based on given resolution.
 * License: MIT Licence
 */

using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Manager
{
    /// <summary>
    /// Class for generating and updating a mesh based on given resolution.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GenerationManagerCPU : MonoBehaviour
    {
        #region Variables

        public Vector2Int resolution = new(20, 20);

        private Mesh _mesh;

        private Vector3[] _vertices;
        private int[] _triangles;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Initializes the mesh creation and updates.
        /// </summary>
        public void Start()
        {
            // Create a new Mesh object
            _mesh = new Mesh();

            // Assign the newly created mesh to the MeshFilter component
            GetComponent<MeshFilter>().sharedMesh = _mesh;

            // Set the index format of the mesh to UInt32
            _mesh.indexFormat = IndexFormat.UInt32;

            // Create the initial shape of the mesh
            CreateShape();

            // Update the mesh with the generated vertices and triangles
            UpdateMesh();
        }

        /// <summary>
        /// Creates the shape of the mesh based on the given resolution.
        /// </summary>
        private void CreateShape()
        {
            // Initialize the array to store the vertices
            _vertices = new Vector3[(resolution.x + 1) * (resolution.y + 1)];

            // Populate the vertices array
            for (int i = 0, z = 0; z <= resolution.y; z++)
            for (var x = 0; x <= resolution.x; x++)
            {
                // Create a new Vector3 for the vertex at position (x, 0, z)
                _vertices[i] = new Vector3(x, 0, z);
                i++;
            }

            // Initialize the array to store the triangles
            _triangles = new int[resolution.x * resolution.y * 6];

            var vertex = 0;
            var triangle = 0;
            for (var z = 0; z < resolution.y; z++)
            {
                for (var x = 0; x < resolution.x; x++)
                {
                    // Define the vertices that form each triangle
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

        /// <summary>
        /// Updates the mesh with the generated vertices and triangles.
        /// </summary>
        private void UpdateMesh()
        {
            // Clear the existing mesh data
            _mesh.Clear();

            // Set the vertices of the mesh
            _mesh.vertices = _vertices;

            // Set the triangles of the mesh
            _mesh.triangles = _triangles;

            // Recalculate the normals of the mesh
            _mesh.RecalculateNormals();
        }

        #endregion
    }
}