/*
 * Author: Rebecca Biebl
 * Creation Date: 13-05-2024
 * Description: A brief description of the script.
 * License: Licence
 */


using UnityEngine;

namespace _Scripts.Helpers
{
    public class ThreadGroupCreator : MonoBehaviour
    {
        [SerializeField] private Vector3Int resolution;

        public GameObject cubePrefab; // Setzen Sie hier das Prefab Ihres Cubes ein
        public int cubeWidth = 1; // Breite eines einzelnen Cubes
        public int cubeHeight = 1; // Höhe eines einzelnen Cubes
        public int cubeDepth = 1; // Tiefe eines einzelnen Cubes
        private float gap = 0.5f;

        void Start()
        {
            // Berechnen Sie die Anzahl der Cubes in jeder Dimension basierend auf der Bildschirmgröße
            int numberOfCubesInRow = resolution.x / cubeWidth;
            int numberOfRows = resolution.y / cubeHeight;
            int numberInDepth = resolution.z / cubeDepth;

            // Erstellen Sie die Cubes in einer Grid-Struktur
            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfCubesInRow; j++)
                {
                    for (int k = 0; k < numberInDepth; k++)
                    {
                        Vector3 cubePosition =
                            new Vector3(j * cubeWidth, i * cubeHeight, k * cubeDepth); // Position jedes Cubes
                        GameObject cube = Instantiate(cubePrefab, cubePosition, Quaternion.identity); // Erstellen Sie das Cube-Objekt
                        cube.transform.localPosition = new Vector3(j + j * gap, i + i * gap, k + k * gap );
                    }
                }
            }
        }
    }
}