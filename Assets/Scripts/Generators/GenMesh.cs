using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A flexible mesh object that can be cloned, appended to, and transferred into a Unity mesh.
/// Supports the += operator for appending.
/// </summary>
[System.Serializable]
public class GenMesh
{
    // Private properties
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Color32> colors = new List<Color32>();

    /// <summary>
    /// Whether the mesh needs regenerating to use
    /// </summary>
    private bool isDirty = true;

    /// <summary>
    /// Default GenMesh constructor
    /// </summary>
    public GenMesh()
    {
        return;
    }

    /// <summary>
    /// Constructs a deep clone of an exisitng GenMesh
    /// </summary>
    /// <param name="source">The exisitng GenMesh to clone into this one</param>
    public GenMesh(GenMesh source)
    {
        vertices = new List<Vector3>(source.vertices);
        triangles = new List<int>(source.triangles);
        colors = new List<Color32>(source.colors);
    }

    /// <summary>
    /// Appends a set of elements from another mesh to this mesh
    /// </summary>
    /// <param name="vertices">The source mesh's vertices</param>
    /// <param name="triangles">The source mesh's triangles</param>
    /// <param name="colors">The source mesh's colors</param>
    public void AddElements(Vector3[] vertices, int[] triangles, Color32[] colors)
    {
        // Append the elements!
        if (triangles != null)
        {
            // Offset the triangle's vertex indices to fit the extended mesh
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] += this.vertices.Count;
            }

            this.triangles.AddRange(triangles);
        }

        if (vertices != null)
        {
            this.vertices.AddRange(vertices);
        }

        if (colors != null)
        {
            this.colors.AddRange(colors);
        }
    }

    /// <summary>
    /// Copies the mesh data to a Unity mesh
    /// </summary>
    /// <param name="mesh">The mesh to copy to</param>
    public void CopyToUnityMesh(Mesh mesh)
    {
        // Validate this mesh
        Validate();

        // Copy over to unity-compatible mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors32 = colors.ToArray();
    }

    /// <summary>
    /// Triangulates a ladder of vertices. This is a set of vertices that takes place in an alternating pattern like so:
    /// 0  2  4
    /// 1  3  5
    /// </summary>
    /// <param name="ladderStartVertex">The starting vertex of the ladder</param>
    /// <param name="ladderNumVerts">The number of vertices in the ladder</param>
    /// <param name="doFlip">Whether to reverse the order of of vertices</param>
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

    /// <summary>
    /// Appends a GenMesh to another
    /// </summary>
    public static GenMesh operator +(GenMesh left, GenMesh right)
    {
        GenMesh outputMesh = new GenMesh(left);

        left.Validate();
        right.Validate();
        outputMesh.AddElements(right.vertices.ToArray(), right.triangles.ToArray(), right.colors.ToArray());

        return outputMesh;
    }

    /// <summary>
    /// Generates the mesh
    /// </summary>
    /// <param name="alwaysRegenerate">Whether the mesh should be regenerated even if it is valid</param>
    public void Validate(bool alwaysRegenerate = false)
    {
        if (isDirty || alwaysRegenerate)
        {
            // Clear dirty flag
            isDirty = false;

            // Call type-specific OnRegenerate
            OnRegenerate();
        }
    }

    /// <summary>
    /// Called when the mesh should be regenerated
    /// </summary>
    protected virtual void OnRegenerate()
    {
        return;
    }
}