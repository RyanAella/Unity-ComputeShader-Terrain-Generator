using UnityEngine;

public class ProceduralMeshGeneration : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public float scale = 20f;
    public float heightMultiplier = 10f;
    public Texture2D falloffMap; // Die Textur für die Falloff Map
    public float falloffDistance = 10f;

    public Gradient heightGradient; // Gradient zur Einfärbung basierend auf der Höhe

    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        float[,] falloffMapData = FalloffGenerator.GenerateFalloffMap(width);
        CreateShape(mesh, falloffMapData);
        ApplyColorGradient(mesh);
    }

    void CreateShape(Mesh mesh, float[,] falloffMapData)
    {
        Vector3[] vertices = new Vector3[width * height];
        int[] triangles = new int[(width - 1) * (height - 1) * 6];
        int triIndex = 0;

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.1f, z * 0.1f);
                
                y -= falloffMapData[x, z];

                y = Mathf.Clamp01(y)  * heightMultiplier;

                vertices[x + z * width] = new Vector3(x, y, z);

                if (x != width - 1 && z != height - 1)
                {
                    triangles[triIndex + 0] = x + z * width;
                    triangles[triIndex + 1] = x + z * width + width;
                    triangles[triIndex + 2] = x + z * width + 1;

                    triangles[triIndex + 3] = x + z * width + 1;
                    triangles[triIndex + 4] = x + z * width + width;
                    triangles[triIndex + 5] = x + z * width + width + 1;
                    triIndex += 6;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void ApplyColorGradient(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            float normalizedHeight = vertices[i].y / heightMultiplier; // Normalisierte Höhe zwischen 0 und 1
            colors[i] = heightGradient.Evaluate(normalizedHeight); // Wert des Color Gradient bei der normalisierten Höhe
        }

        mesh.colors = colors;
    }
}

public static class FalloffGenerator {

    public static float[,] GenerateFalloffMap(int size) {
        float[,] map = new float[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max (Mathf.Abs (x), Mathf.Abs (y));
                map [i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value) {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow (value, a) / (Mathf.Pow (value, a) + Mathf.Pow (b - b * value, a));
    }
}
