using UnityEngine;
using UnityEditor;

public class MeshFixer : Editor
{
    [MenuItem("Tools/Make Mesh Readable")]
    private static void MakeReadable()
    {
        Mesh mesh = Selection.activeObject as Mesh;

        if (mesh != null)
        {
            mesh.UploadMeshData(false);
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssets();
            Debug.Log("Mesh converted");
        }
    }
}