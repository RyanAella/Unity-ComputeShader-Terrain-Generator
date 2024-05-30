/*
 * Author: Rebecca Biebl
 * Creation Date: 13-05-2024
 * Description: This script generates a grid of cubes based on the specified resolution and cube dimensions.
 * License: Licence
 */

using UnityEngine;

namespace _Scripts.Helpers.Documentation
{
    public class ThreadGroupCreator : MonoBehaviour
    {
        [SerializeField] private Vector3Int resolution; // Resolution of the grid in x, y, and z dimensions

        public GameObject cubePrefab; // Prefab for the cube
        public int cubeWidth = 1; // Width of each individual cube
        public int cubeHeight = 1; // Height of each individual cube
        public int cubeDepth = 1; // Depth of each individual cube

        private const float Gap = 0.5f; // Gap between cubes

        private void Start()
        {
            // Calculate the number of cubes in each dimension based on the resolution
            int numberOfCubesInRow = resolution.x / cubeWidth;
            int numberOfRows = resolution.y / cubeHeight;
            int numberInDepth = resolution.z / cubeDepth;

            // Create the cubes in a grid structure
            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfCubesInRow; j++)
                {
                    for (int k = 0; k < numberInDepth; k++)
                    {
                        // Calculate the position for each cube
                        Vector3 cubePosition = new Vector3(j * cubeWidth, i * cubeHeight, k * cubeDepth);
                        
                        // Instantiate the cube prefab at the calculated position
                        GameObject cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity);
                        
                        // Adjust the local position of the cube to include the gap
                        cube.transform.localPosition = new Vector3(j * (cubeWidth + Gap), i * (cubeHeight + Gap), k * (cubeDepth + Gap));
                    }
                }
            }
        }
    }
}
