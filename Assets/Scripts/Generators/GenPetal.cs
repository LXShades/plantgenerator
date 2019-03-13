using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenPetal : GenMesh
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

    protected override void OnRegenerate()
    {
        // Create the directional vectors
        Vector3 forward = direction;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

        // Create a mirrored semi-circle along the given curve
        vertices = new List<Vector3>(new Vector3[numLengthVertices]);
        colors = new List<Color32>(new Color32[numLengthVertices]);
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
        triangles = new List<int>(new int[(numLengthVertices / 2 - 1) * 6]);
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
    }
}