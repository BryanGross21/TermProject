using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// Adapted from Joe Rickwoods Video sourced here https://www.youtube.com/watch?v=gBXLyKaO6l8&ab_channel=JoeRickwood
/// </summary>
public class ChunkManager : MonoBehaviour
{
	/// <summary>
	/// We can call chunkmanger without having a direct instance
	/// </summary>
	public static ChunkManager instance;

	public int currentResolution = 16;
	public float currentIslandRadius = 900f;
	public float islandRadiusNoiseScale = .005f;
	public float noiseScale = .04f;
	public float noiseScale2 = .02f;
	public float noiseScale3 = .007f;



	private int pastResolution;
	private float pastIslandRadius;
	private float pastIslandRadiusNoiseScale;
	private float pastNoiseScale;
	private float pastNoiseScale2;
	private float pastNoiseScale3;

	private Vector2 worldSize = new Vector2(8f, 8f);

	public Material Terrain;

	public Vector2 worldCenter;

	private List<GameObject> chunks = new();

	/// <summary>
	/// Sets the chunk manager instance upon the project starting
	/// </summary>
	void Awake()
	{
		instance = this;
	}

	public void Start()
	{

		worldCenter = new Vector2((worldSize.x / 2f) * 128, (worldSize.y / 2f) * 128);
		pastIslandRadius = currentIslandRadius;
		pastResolution = currentResolution;
		pastIslandRadiusNoiseScale = islandRadiusNoiseScale;
		pastNoiseScale = noiseScale;
		pastNoiseScale2 = noiseScale2;
		pastNoiseScale3 = noiseScale3;
		StartCoroutine(GenerateChunks());
	}

	public void Update() 
	{
		if (currentResolution < 1)
		{
			currentResolution = 1;
		}
		else if (currentResolution > 100) 
		{
			currentResolution = 100;
		}
		if (currentIslandRadius < 1)
		{
			currentIslandRadius = 1;
		}
		else if (currentIslandRadius > 700) 
		{
			currentIslandRadius = 700;
		}
		if (currentResolution != pastResolution) 
		{
			pastResolution = currentResolution;
			UpdateChunks();
		}
		if (currentIslandRadius != pastIslandRadius) 
		{
			pastIslandRadius = currentIslandRadius;
			UpdateChunks();
		}
		if (islandRadiusNoiseScale != pastIslandRadiusNoiseScale)
		{
			pastIslandRadiusNoiseScale = islandRadiusNoiseScale;
			UpdateChunks();
		}
		if (noiseScale != pastNoiseScale)
		{
			pastNoiseScale = noiseScale;
			UpdateChunks();
		}
		if (noiseScale2 != pastNoiseScale2)
		{
			pastNoiseScale2 = noiseScale2;
			UpdateChunks();
		}
		if (noiseScale3 != pastNoiseScale3)
		{
			pastNoiseScale3 = noiseScale3;
			UpdateChunks();
		}
	}

	public void UpdateChunks()
	{
		ClearChunks();
		StartCoroutine(GenerateChunks());
	}

	private void ClearChunks()
	{
		foreach (var chunk in chunks)
		{
			Destroy(chunk);
		}
		chunks.Clear();
	}


	IEnumerator GenerateChunks()
	{
		for (int x = 0; x < worldSize.x; x++)
		{
			for (int y = 0; y < worldSize.y; y++)
			{
				TerrainMeshGenerator tmg = new();
				GameObject current = new("Terrain" + (x * y), typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
				current.transform.parent = transform;
				current.transform.localPosition = new Vector3(x * 128f, 0, y * 128f);

				tmg.Init(current);
				tmg.Generate(Terrain,islandRadiusNoiseScale, noiseScale, noiseScale2, noiseScale3);

				chunks.Add(current);
				yield return new WaitForSeconds(0.1f);
			}
		}
	}
}

class TerrainMeshGenerator
{
	MeshFilter filter;
	MeshRenderer renderer;
	MeshCollider collider;
	Mesh mesh;

	Vector3[] verts;
	int[] triangles;
	Vector2[] uvs;

	Vector2 worldCenter = ChunkManager.instance.worldCenter;

	public void Init(GameObject cur)
	{
		filter = cur.GetComponent<MeshFilter>();
		renderer = cur.GetComponent<MeshRenderer>();
		collider = cur.GetComponent<MeshCollider>();
		mesh = new();	
	}

	public void Generate(Material mat, float islandRadiusScale, float noiseScale1, float noiseScale2, float noiseScale3)
	{
		Vector2 worldPos = new Vector2(filter.gameObject.transform.localPosition.x, filter.gameObject.transform.localPosition.z);
		int resolution = ChunkManager.instance.currentResolution;
		float islandRadius = ChunkManager.instance.currentIslandRadius;

		verts = new Vector3[(resolution + 1) * (resolution + 1)];
		uvs = new Vector2[(verts.Length)];

		Vector2 worldCenter = ChunkManager.instance.worldCenter;

		for (int i = 0, x = 0; x <= resolution; x++)
		{
			for (int z = 0; z <= resolution; z++)
			{
				Vector2 vertexWorldPos = new(worldPos.x + (x * (128f / resolution)), worldPos.y + (z * (128f / resolution)));

				float distance = Vector2.Distance(worldCenter, vertexWorldPos);
				float sin = Mathf.Sin(Mathf.Clamp(((1 + distance) / islandRadius), 0f, 1f) + 90f);
				float islandMultiplier = sin * Mathf.PerlinNoise(vertexWorldPos.x * islandRadiusScale, vertexWorldPos.y * 0.005f);
				islandMultiplier += Mathf.PerlinNoise(vertexWorldPos.x * noiseScale1, vertexWorldPos.y * noiseScale1) * .5f * sin;
				islandMultiplier += Mathf.PerlinNoise(vertexWorldPos.x * noiseScale2, vertexWorldPos.y * noiseScale2) * .3f * sin;
				islandMultiplier += Mathf.PerlinNoise(vertexWorldPos.x * noiseScale3, vertexWorldPos.y * noiseScale3) * .3f * sin;
				float y = islandMultiplier * 150f;
				verts[i] = new Vector3(x * (128f / resolution), y, z * (128f / resolution));
				i++;
			}
		}

		//Gets uv data for texture mapping
		for (int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(verts[i].x + worldPos.x, verts[i].z + worldPos.y);
		}

		//Sets up the triangles for the mesh
		triangles = new int[resolution * resolution * 6];
		int tris = 0;
		int vert = 0;

		//Generates the mesh itself
		for (int x = 0; x < resolution; x++)
		{
			for (int y = 0; y < resolution; y++)
			{
				triangles[tris] = vert;
				triangles[tris + 1] = vert + 1;
				triangles[tris + 2] = (int)(vert + resolution + 1);
				triangles[tris + 3] = vert + 1;
				triangles[tris + 4] = (int)(vert + resolution + 2);
				triangles[tris + 5] = (int)(vert + resolution + 1);

				vert++;
				tris += 6;

			}
			vert++;
		}

		mesh.Clear();
		mesh.vertices = verts;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		collider.sharedMesh = mesh;

		filter.mesh = mesh;

		renderer.material = mat;
	}
}