using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A shoot, in plant terms.
[System.Serializable]
public class GenTube : GenMesh
{
    public Vector3 start;
    public Vector3 direction;
    public float radius = 1;
    public int numRadialVertices = 8;
    public int numSegmentVertices = 2;

    /// <summary>
    /// Curve representing the width of the tube over height
    /// </summary>
    public AnimationCurve widthCurve = AnimationCurve.Constant(0, 1, 1);

    /// <summary>
    /// Curve representing the length of the tube over height
    /// </summary>
    public AnimationCurve lengthCurve = AnimationCurve.Linear(0, 0, 1, 1);

    /// <summary>
    /// Curve along the forward axis over height
    /// </summary>
    public AnimationCurve upCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    /// <summary>
    /// Curve that determines the horizontal angle of the forward axis over height
    /// </summary>
    public AnimationCurve spiralCurve = AnimationCurve.Constant(0, 1, 0);

    public GenTube(Vector3 start, Vector3 direction)
    {
        this.start = start;
        this.direction = direction;
    }

    protected override void OnRegenerate()
    {
        // Validate the parameters
        numSegmentVertices = Mathf.Max(numSegmentVertices, 2);
        numRadialVertices = Mathf.Max(numRadialVertices, 2);

        // Get up and right vectors
        // todo: fix for shoots going straight up or straight down
        Vector3 up = direction - start;
        Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 forward = Vector3.Cross(up, right).normalized;

        // Create a cylinder of vertices
        vertices = new List<Vector3>(new Vector3[numRadialVertices * numSegmentVertices]);
        colors = new List<Color32>(new Color32[numRadialVertices * numSegmentVertices]);
        triangles.Clear();

        int numCreatedVerts = 0;
        Color32 baseColor = new Color32(0, 0, 255, 255);
        Color32 extColor = new Color32(0, 255, 255, 255);

        // Create segments
        for (int segment = 0; segment < numSegmentVertices; segment++)
        {
            float segmentProgress = (float)segment / (float)(numSegmentVertices - 1);
            up = (direction - start) * segmentProgress + forward * upCurve.Evaluate(segmentProgress);

            // Create the circle for this segment
            Vector3 segmentCentre = start + up;

            for (int radial = 0; radial < numRadialVertices; radial++)
            {
                float angle = Mathf.PI * 2.0f * (float)radial / (float)numRadialVertices;
                // RadiusOffset is the direction perpendicular to the tube's up vector for this vertex
                Vector3 radiusOffset = forward * (Mathf.Sin(angle) * widthCurve.Evaluate(segmentProgress)) + right * (Mathf.Cos(angle) * lengthCurve.Evaluate(segmentProgress));

                // Add the radial vertex
                vertices[numCreatedVerts] = segmentCentre + radiusOffset * radius;
                colors[numCreatedVerts] = Color32.Lerp(baseColor, extColor, segmentProgress);
                numCreatedVerts++;
            }
        }

        // Connect them
        TriangulateRings(0, numRadialVertices, numSegmentVertices, false);
    }
}