using UnityEditor;
using UnityEngine;

public class DroneFbxPostprocessor : AssetPostprocessor
{
    private const string DroneFbxPath = "Assets/Art/Models/Drone.fbx";

    private void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.Equals(DroneFbxPath, System.StringComparison.OrdinalIgnoreCase))
            return;

        foreach (Transform t in g.GetComponentsInChildren<Transform>(true))
        {
            if (!t.name.StartsWith("Helix"))
                continue;

            MeshFilter mf = t.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                continue;

            Mesh mesh = mf.sharedMesh;
            Vector3 center = mesh.bounds.center;
            if (center == Vector3.zero)
                continue;

            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
                verts[i] -= center;

            mesh.vertices = verts;
            mesh.RecalculateBounds();

            t.localPosition += t.localRotation * Vector3.Scale(center, t.localScale);
        }
    }
}
