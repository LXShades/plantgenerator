using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A shoot, in plant terms.
[System.Serializable]
public class GenTube : GenMesh
{
    public Vector3 start;
    public Vector3 direction;
    float radius;
    int numRadialVertices;
    AnimationCurve tubeCurve;

    public GenTube(Vector3 start, Vector3 direction, float radius = 1, int numRadialVertices = 8, AnimationCurve tubeCurve = null)
    {
        this.start = start;
        this.direction = direction;
        this.radius = radius;
        this.numRadialVertices = numRadialVertices;

        if (tubeCurve != null)
        {
            this.tubeCurve = tubeCurve;
        }
        else
        {
            this.tubeCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);
        }
    }

    protected override void OnRegenerate()
    {
        // Get up and right vectors
        // todo: fix for shoots going straight up or straight down
        Vector3 up = direction - start;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 forward = Vector3.Cross(up, right).normalized;

        // Create a cylinder of vertices
        vertices = new List<Vector3>(new Vector3[numRadialVertices * 2]);
        colors = new List<Color32>(new Color32[numRadialVertices * 2]);

        int numCreatedVerts = 0;
        float angle = 0;
        Color32 baseColor = new Color32(0, 0, 255, 255);
        Color32 extColor = new Color32(0, 255, 255, 255);

        for (numCreatedVerts = 0; numCreatedVerts < numRadialVertices * 2;)
        {
            Vector3 radiusOffset = forward * Mathf.Sin(angle) + right * Mathf.Cos(angle);

            vertices[numCreatedVerts] = this.start + radiusOffset * (radius * tubeCurve.Evaluate(0.0f));
            vertices[numCreatedVerts + 1] = this.start + up + radiusOffset * (radius * tubeCurve.Evaluate(1.0f));
            colors[numCreatedVerts] = baseColor;
            colors[numCreatedVerts + 1] = extColor;
            numCreatedVerts += 2;

            angle += 2.0f * Mathf.PI / (float)numRadialVertices;
        }

        // Connect them
        triangles = new List<int>(new int[numCreatedVerts / 2 * 6]);
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
    }
}