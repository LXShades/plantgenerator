﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlantGenerator : MonoBehaviour {
    // The mesh to be generated by this plant generator
    public Mesh generatedMesh;

    // Temp: A curve for petals
    public AnimationCurve petalCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    // FOR EASE OF USE: A bool that actually regenerates the mesh whenever you click it. May be removed later
    public bool doRegenerate    = true;
    private bool lastRegenerate = false;

    // FOR EASE OF USE: A list of editable points in 3D space
    public Vector3[] editablePoints = new Vector3[1];

	// Use this for initialization
	void Awake() {
		if (!generatedMesh)
        {
            generatedMesh = new Mesh();
        }
	}
	
	// Update is called once per frame
	void Update () {
        // todo remove this
        doRegenerate = !doRegenerate;
		if (doRegenerate != lastRegenerate)
        {
            Regenerate();

            lastRegenerate = doRegenerate;
        }
	}

    // Regenerates the mesh from the ground up
    void Regenerate()
    {
        // Create a new GenMesh to begin building the mesh
        GenMesh mesh = new GenMesh();

        // and a petal, for good measure
        Vector3 s = editablePoints.Length > 0 ? editablePoints[0] : Vector3.zero, e = editablePoints.Length > 1 ? editablePoints[1] : Vector3.zero;
        new GenShoot(Vector3.zero, s - Vector3.zero, 0.3f, 12).Generate(mesh);
        new GenPetal(s, e - s, 0.3f, petalCurve, 128).Generate(mesh);

        // Copy the GenMesh contents to the mesh
        mesh.ApplyToMesh(generatedMesh);

        // Transfer new mesh contents to filter
        GetComponent<MeshFilter>().sharedMesh = generatedMesh;
    }

    // Regenerates all plants when the script is changed
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        foreach (PlantGenerator generator in FindObjectsOfType<PlantGenerator>())
        {
            generator.Regenerate();
        }
    }
}

// A container class used for meshes undergoing generation
public class GenMesh
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Color32> colors = new List<Color32>();

    public void AddElements(Vector3[] vertices, int[] triangles, Color32[] colors)
    {
        // Append the elements!
        if (vertices != null)
        {
            this.vertices.AddRange(vertices);
        }

        if (colors != null)
        {
            this.colors.AddRange(colors);
        }

        if (triangles != null)
        {
            // Offset the vertex indices to append the new triangles correctly
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] += this.vertices.Count;
            }

            this.triangles.AddRange(triangles);
        }
    }
    
    public void ApplyToMesh(Mesh mesh)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors32 = colors.ToArray();
    }

    public void TriangulateLadder(int ladderStartVertex, int ladderNumVerts, bool doFlip)
    {
        int[] triangles = new int[ladderNumVerts / 2 * 6];
        int numCreatedIndices = 0;
        for (int quad = 0; quad < ladderNumVerts / 2; quad++)
        {
            int baseline = quad * 2;

            triangles[numCreatedIndices + 0] = (baseline + 0) % ladderNumVerts;
            triangles[numCreatedIndices + 1] = (baseline + 2) % ladderNumVerts;
            triangles[numCreatedIndices + 2] = (baseline + 1) % ladderNumVerts;
            triangles[numCreatedIndices + 3] = (baseline + 2) % ladderNumVerts;
            triangles[numCreatedIndices + 4] = (baseline + 3) % ladderNumVerts;
            triangles[numCreatedIndices + 5] = (baseline + 1) % ladderNumVerts;
            numCreatedIndices += 6;
        }

        AddElements(null, triangles, null);
    }
}

// A shoot, in plant terms.
public class GenShoot
{
    Vector3 start;
    Vector3 direction;
    float radius;
    int numRadialVertices;

    public GenShoot(Vector3 start, Vector3 direction, float radius = 1, int numRadialVertices = 8)
    {
        this.start = start;
        this.direction = direction;
        this.radius = radius;
        this.numRadialVertices = numRadialVertices;
    }

    public void Generate(GenMesh mesh)
    {
        // Get up and right vectors
        // todo: fix for shoots going straight up or straight down
        Vector3 up = direction - start;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 forward = Vector3.Cross(up, right).normalized;

        // Create a cylinder of vertices
        Vector3[] radialVerts = new Vector3[numRadialVertices * 2];
        Color32[] radialVertColours = new Color32[numRadialVertices * 2];
        int numCreatedVerts = 0;
        float angle = 0;
        Color32 baseColor = new Color32(0, 255, 255, 255);
        Color32 extColor = new Color32(0, 255, 255, 255);

        for (numCreatedVerts = 0; numCreatedVerts < numRadialVertices * 2; )
        {
            radialVerts[numCreatedVerts] = this.start + forward * Mathf.Sin(angle) * radius + right * (Mathf.Cos(angle) * radius);
            radialVerts[numCreatedVerts + 1] = radialVerts[numCreatedVerts] + up;
            radialVertColours[numCreatedVerts] = baseColor;
            radialVertColours[numCreatedVerts + 1] = extColor;
            numCreatedVerts += 2;

            angle += 2.0f * Mathf.PI / (float)numRadialVertices;
        }

        // Connect them
        int[] triangles = new int[numCreatedVerts / 2 * 6];
        int numCreatedIndices = 0;
        for (int quad = 0; quad < numCreatedVerts / 2; quad++)
        {
            int baseline = quad * 2;

            triangles[numCreatedIndices + 0] = (baseline + 0) % numCreatedVerts;
            triangles[numCreatedIndices + 1] = (baseline + 2) % numCreatedVerts;
            triangles[numCreatedIndices + 2] = (baseline + 1) % numCreatedVerts;
            triangles[numCreatedIndices + 3] = (baseline + 2) % numCreatedVerts;
            triangles[numCreatedIndices + 4] = (baseline + 3) % numCreatedVerts;
            triangles[numCreatedIndices + 5] = (baseline + 1) % numCreatedVerts;
            numCreatedIndices += 6;
        }

        // Add them to the mesh
        mesh.AddElements(radialVerts, null, radialVertColours);
    }
}

public class GenPetal
{
    Vector3 start;
    Vector3 direction;
    float radius;
    AnimationCurve outline;
    int numLengthVertices = 0;

    public GenPetal(Vector3 start, Vector3 direction, float radius, AnimationCurve outline, int numLengthVertices)
    {
        this.start = start;
        this.direction = direction;
        this.radius = radius;
        this.outline = outline;
        this.numLengthVertices = (numLengthVertices + 1) / 2 * 2; // temp: numRadialVertices should be even for now
    }

    public void Generate(GenMesh mesh)
    {
        // Create the directional vectors
        Vector3 forward = direction;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

        // Create a mirrored semi-circle along the given curve
        Vector3[] vertices = new Vector3[numLengthVertices];
        Color32[] colors = new Color32[numLengthVertices];
        Color32 baseColor = new Color32(0, 255, 0, 255);
        Color32 extColor = new Color32(0, 255, 0, 255);
        float progress = 0.0f;

        for (int i = 0; i < numLengthVertices / 2; i++)
        {
            float width = outline.Evaluate((float)i / (numLengthVertices / 2));
            vertices[i << 1] = start + right * width + forward * progress;
            vertices[(i << 1) + 1] = start - right * width + forward * progress;
            colors[i << 1] = baseColor;
            colors[(i << 1) + 1] = extColor;

            progress += 1.0f / ((float)numLengthVertices / 2 - 1);
        }

        // Join the sides of the semi-circle
        int[] triangles = new int[(numLengthVertices / 2 - 1) * 6];
        int numCreatedIndices = 0;

        for (int quad = 0; quad < numLengthVertices / 2 - 1; quad++)
        {
            int baseline = quad * 2;

            triangles[numCreatedIndices + 2] = (baseline + 1) % numLengthVertices;
            triangles[numCreatedIndices + 1] = (baseline + 0) % numLengthVertices;
            triangles[numCreatedIndices + 0] = (baseline + 2) % numLengthVertices;
            triangles[numCreatedIndices + 5] = (baseline + 1) % numLengthVertices;
            triangles[numCreatedIndices + 4] = (baseline + 2) % numLengthVertices;
            triangles[numCreatedIndices + 3] = (baseline + 3) % numLengthVertices;
            numCreatedIndices += 6;
        }

        // Add them to the object!
        mesh.AddElements(vertices, triangles, colors);
    }
}