using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GenSymmetrise : GenMesh
{
    Vector3 rotationOrigin;
    Vector3 lineOfSymmetry;
    int numberOfCopies;
    GenMesh sourceMesh;

    public GenSymmetrise(GenMesh source, int numberOfCopies, Vector3 rotationOrigin, Vector3 lineOfSymmetry)
    {
        this.rotationOrigin = rotationOrigin;
        this.lineOfSymmetry = lineOfSymmetry;
        this.numberOfCopies = numberOfCopies;
        this.sourceMesh = source;
    }

    protected override void OnRegenerate()
    {
        float rotationAngle = 360.0f / numberOfCopies;

        // Allocate the elements
        vertices.Clear();
        colors.Clear();
        triangles.Clear();

        sourceMesh.Validate();

        vertices.Capacity = numberOfCopies * sourceMesh.vertices.Count;
        colors.Capacity = numberOfCopies * sourceMesh.vertices.Count;
        triangles.Capacity = numberOfCopies * sourceMesh.triangles.Count;

        // Create a copy rotated around this axis
        for (int i = 0; i < numberOfCopies; i++)
        {
            int baseVertexIndex = sourceMesh.vertices.Count * i;
            int baseTriangleIndex = sourceMesh.triangles.Count * i;
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle * i, lineOfSymmetry);

            vertices.AddRange(sourceMesh.vertices);
            colors.AddRange(sourceMesh.colors);
            triangles.AddRange(sourceMesh.triangles);

            // Copy all vertices rotated around the axis
            for (int v = 0; v < sourceMesh.vertices.Count; v++)
            {
                vertices[baseVertexIndex + v] = rotation * (sourceMesh.vertices[v] - rotationOrigin) + rotationOrigin;
            }

            for (int t = 0; t < sourceMesh.triangles.Count; t++)
            {
                triangles[baseTriangleIndex + t] += baseVertexIndex;
            }
        }
    }
}