using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public bool smoothTerrain =  true;
	public bool flatShaded = false;

	public GameObject chunkObject;
	MeshFilter meshFilter;
	MeshCollider meshCollider;
	MeshRenderer meshRenderer;

	Vector3Int chunkPosition;

	public List<Vector3> vertices = new List<Vector3>();
	public List<int> vertexTextureIndex = new List<int>();
	List<Color> colors = new List<Color>();
	List<int> triangles = new List<int>();
	List<Vector2> uvs = new List<Vector2>(0);

	public int resolutionLevel;
	public int terrainWidth;
	public int terrainHeight;


	public TerrainPoint[,,] terrainMap;

	float minValue = float.MaxValue, maxValue = float.MinValue;

	int resolution { get { return GameData.ChunkResolution; } }
	public int width { get { return GameData.ChunkWidth * GameData.ChunkResolution; } }
	public int height { get { return GameData.ChunkHeight * GameData.ChunkResolution; } }
	float terrainSurface { get { return GameData.terrainSurface; } }

	public Chunk(Vector3Int _position)
    {
		chunkObject = new GameObject();
		chunkObject.name = string.Format("Chunk {0}, {1}", _position.x, _position.z);
		chunkPosition = _position;
		chunkObject.transform.position = chunkPosition;

		meshFilter = chunkObject.AddComponent<MeshFilter>();
		meshCollider = chunkObject.AddComponent<MeshCollider>();
		meshRenderer = chunkObject.AddComponent<MeshRenderer>();
		meshRenderer.material = Resources.Load<Material>("Materials/Terrain");
		meshRenderer.material.SetTexture("_TexArr", World.Instance.terrainTexArray);

		chunkObject.transform.tag = "Terrain";
		terrainMap = new TerrainPoint[width + 1, height + 1, width + 1];

		PopulateTerrainMap();
		CreateMeshData();
		BuildMesh();
    }

	void PopulateTerrainMap()
    {
		float[,] heightMap = Noise.GenerateNoiseMap(width + 1, width + 1, 1, 50, 5, 0.5f, 2, Vector2.zero, Noise.NormalizeMode.Global);

		for (int x = 0; x < width + 1; x++)
        {
			for (int y = 0; y < height + 1; y++)
            {
				for (int z = 0; z < width + 1; z++)
                {

					float thisHeight = GameData.GetTerrainHeight(x + chunkPosition.x, z + chunkPosition.z);
					//thisHeight = (float)height * heightMap[x, z];
					
					//Debug.Log(thisHeight);

					float point = (float)y - thisHeight;
					//Debug.Log("X:" + x + " Y:" + y + " Z:" + z + " Value: " + point);

					/*if (x > 5 && x < 10 && z > 14 && z < 20 && y > 1)
                    {
						point = 1;
                    }*/

					if (point > maxValue)
						maxValue = point;
					if (point < minValue)
						minValue = point;
		
					terrainMap[x, y, z] = new TerrainPoint(point, Random.Range(0, World.Instance.terrainTextures.Length));
                }
            }
        }
    }

	void CreateMeshData()
    {
		ClearMeshData();

		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				for (int z = 0; z < width; z++)
				{
					float[] cube = new float[8];
					//Debug.Log(terrainMap[x, y, z]);
					for (int i = 0; i < 8; i++)
                    {
						Vector3Int corner = new Vector3Int(x, y, z) + GameData.CornerTable[i];
						cube[i] = terrainMap[corner.x, corner.y, corner.z].dstToSurface;
                    }

					MarchCube(new Vector3(x, y, z) / resolution, cube);
				}
			}
		}
		BuildMesh();
	}

	// Determine the index into the edge table which tells us which vertices are inside of the surface
	int FindVerticesInsideSurface(float[] cube)
	{
		int configurationIndex = 0;

		for (int i = 0; i < 8; i++)
        {
			if (cube[i] > terrainSurface)
            {
				configurationIndex |= 1 << i;
            }
        }

		return configurationIndex;
	}

    void MarchCube(Vector3 position, float[] cube)
    {
		int configIndex = FindVerticesInsideSurface(cube); 

		if (configIndex == 0 || configIndex == 255)
			return;

		int edgeIndex = 0;
		for (int i = 0; i < 5; i++)
        {
			for (int j = 0; j < 3; j++)
            {
				int indice = GameData.TriangleTable[configIndex, edgeIndex];

				if (indice == -1)
					return;

				Vector3 vert1 = position + (Vector3)GameData.CornerTable[GameData.EdgeIndexes[indice, 0]] / resolution;
				Vector3 vert2 = position + (Vector3)GameData.CornerTable[GameData.EdgeIndexes[indice, 1]] / resolution;

				Vector3 vertPosition;
				if (smoothTerrain)
				{
					// Get the terrain values at either end of the current edge from the cube array
					float vert1Sample = cube[GameData.EdgeIndexes[indice, 0]];
					float vert2Sample = cube[GameData.EdgeIndexes[indice, 1]];

					// Calculate the differencce between the terrain values
					float difference = vert2Sample - vert1Sample;

					// if the difference is 0, then the terrain passes through the middle
					if (difference == 0)
						difference = terrainSurface;
					else
						difference = (terrainSurface - vert1Sample) / difference;

					// Calculate the point along the edge that passes through
					vertPosition = vert1 + ((vert2 - vert1) * difference);

				}
				else
				{
					vertPosition = (vert1 + vert2) / 2f;
				}

				if (flatShaded)
				{
					vertices.Add(vertPosition);
					uvs.Add(new Vector2(terrainMap[(int)(position.x * resolution), (int)(position.y * resolution), (int)(position.z * resolution)].textureID, 0));
					colors.Add(TerrainPoint.colors[terrainMap[(int)(position.x * resolution), (int)(position.y * resolution), (int)(position.z * resolution)].textureID]);
					triangles.Add(vertices.Count - 1);
				}
				else
                {
					triangles.Add(VertForIndice(vertPosition, position));
                }
				edgeIndex++;
            }
        }
    }

	/*void MarchCube2(Vector3 position, float[] cube)
    {
		Vector3[] edgeVertex = new Vector3[12];
		int cubeIndex = 0;

		// Find which vertices are inside of the surface and which are outside
		cubeIndex = FindVerticesInsideSurface(cube); // the vertices are in binary format

		//Find which edges are intersected by the surface
		int edges = EdgeTable[cubeIndex]; // the edges are in binary format

		// If the cube is entirely inside or outside of the surface (no intersections)
		if (edges == 0)
			return;

		// Find the point of intersection of the surface with each edge
		for (int i = 0; i < 12; i++)
        {
			if ((edges & (1 << i)) != 0)
            {
				Vector3 vert1 = position + CornerTable[EdgeIndexes[i, 0]];
				Vector3 vert2 = position + CornerTable[EdgeIndexes[i, 1]];

				Vector3 vertPosition = (vert1 + vert2) / 2f;

				edgeVertex[i] = position + vertPosition;
			}
        }

		// Save the triangles that were found. There can be up to five per cube
		for (int i = 0; i < 5; i++)
        {
			if (TriangleTable[cubeIndex, 3 * i] < 0) break;

			int idx = vertices.Count;

			for (int j = 0; j < 3; j++)
            {
				int vert = TriangleTable[cubeIndex, 3 * i + j];
				triangles.Add(idx + j);
				vertices.Add(edgeVertex[vert]);
            }
        }
	}*/

	public void PlaceTerrain(Vector3 pos)
    {
		Vector3Int intPos = new Vector3Int(Mathf.CeilToInt(pos.x * resolution), Mathf.CeilToInt(pos.y * resolution) , Mathf.CeilToInt(pos.z * resolution));
		intPos -= chunkPosition;
		Debug.Log(intPos);
		terrainMap[intPos.x, intPos.y, intPos.z].dstToSurface = 0f;

		CreateMeshData();

    }

	public void RemoveTerrain(Vector3 pos)
    {
		Vector3Int intPos = new Vector3Int(Mathf.FloorToInt(pos.x * resolution), Mathf.FloorToInt(pos.y * resolution), Mathf.FloorToInt(pos.z * resolution));
		intPos -= chunkPosition;
		Debug.Log(intPos);
		terrainMap[intPos.x, intPos.y, intPos.z].dstToSurface = 1f;

		CreateMeshData();
	}

	float SampleTerrain(Vector3Int point)
    {
		return terrainMap[point.x, point.y, point.z].dstToSurface;
    }

	/*void AddTriangleColors()
    {
		for (int  i = 0; i < triangles.Count; i = i + 3)
        {
			if (vertexColors[triangles[i]] == Color.black)
            {

            }
			terrainMap[(int)vertices[i].x, (int) vertices[i].y, (int) vertices[i].z].vertexColor 
        }
    }*/

	// Check that every vertice is added only once
	int VertForIndice(Vector3 vert, Vector3 position)
    {
		for (int i = 0; i < vertices.Count; i++)
        {
			if (vertices[i] == vert)
				return i;
        }

		vertices.Add(vert);

		int textureId = terrainMap[(int)(position.x * resolution), (int)(position.y * resolution), (int)(position.z * resolution)].textureID;
		uvs.Add(new Vector2(textureId, 0));
		vertexTextureIndex.Add(textureId);
		//colors.Add(TerrainPoint.colors[terrainMap[(int)(position.x * resolution), (int)(position.y * resolution), (int)(position.z * resolution)].textureID]);

		return vertices.Count - 1;
    }

    void ClearMeshData()
    {
		vertices.Clear();
		triangles.Clear();
		uvs.Clear();
    }

	void BuildMesh()
    {
		Debug.Log(vertices.Count);
        Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		//mesh.colors = colors.ToArray();
		mesh.RecalculateNormals();
		meshFilter.sharedMesh = mesh;
		meshCollider.sharedMesh = mesh;
    }

	float ReMap(float a1, float a2, float b1, float b2, float s)
    {
		return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
