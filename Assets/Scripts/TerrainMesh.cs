using UnityEngine;

[RequireComponent(typeof(HeightMap))]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainMesh : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int resolution = 50;
    [SerializeField] private float size = 10f;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private HeightMap heightMap;

    public void ResetTerrain()
    {
        heightMap.ResetHeights();
        UpdateMesh();
    }

    private void Awake()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        int vertexCount = (resolution + 1) * (resolution + 1);
        vertices = new Vector3[vertexCount];
        triangles = new int[resolution * resolution * 6];

        float step = size / resolution;

        for (int z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                int index = z * (resolution + 1) + x;

                vertices[index] = new Vector3(
                    x * step - size / 2f,
                    0f,
                    z * step - size / 2f
                );
            }
        }

        int triangleIndex = 0;

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * (resolution + 1) + x;

                triangles[triangleIndex++] = i;
                triangles[triangleIndex++] = i + resolution + 1;
                triangles[triangleIndex++] = i + 1;

                triangles[triangleIndex++] = i + 1;
                triangles[triangleIndex++] = i + resolution + 1;
                triangles[triangleIndex++] = i + resolution + 2;
            }
        }

        mesh = new Mesh();
        mesh.name = "Generated Terrain";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        heightMap = GetComponent<HeightMap>();
        heightMap.Initialize(vertices.Length);
        mesh.MarkDynamic();
    }

    public void UpdateMesh()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = heightMap.GetHeight(i);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void AddMaterial(int index, float amount)
    {
        heightMap.AddHeight(index, amount);
    }

    public int Resolution => resolution;
    public float Size => size;
    public float CellSize => size / resolution;

    public Vector3 GetVertexPosition(int index)
    {
        return vertices[index];
    }
}
