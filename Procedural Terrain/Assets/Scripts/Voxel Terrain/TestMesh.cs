using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMesh : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    List<Vector3> vertices = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>(0);

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(0, 0, 1));
        
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        
        colors.Add(Color.red);
        colors.Add(Color.green);
        colors.Add(Color.blue);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(2, 0));

        BuildMesh();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
    }
}
