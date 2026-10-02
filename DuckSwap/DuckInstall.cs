using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Text;
public static class DuckInstall {
 public static void Run() {
 Directory.CreateDirectory("Assets/Ducks/Textures");
 var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Ducks/rubberDuck.fbx");
 importer.ExtractTextures("Assets/Ducks/Textures");
 AssetDatabase.Refresh();
 var report=new StringBuilder();
 var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ducks/rubberDuck.fbx");
 var sample=Object.Instantiate(model);
 foreach(var f in sample.GetComponentsInChildren<MeshFilter>()) {
 var mesh=f.sharedMesh; report.AppendLine("MESH "+mesh.name+" vertices="+mesh.vertexCount+" colors="+mesh.colors.Length+" submeshes="+mesh.subMeshCount);
 for(int i=0;i<mesh.subMeshCount;i++){var ids=mesh.GetTriangles(i).Distinct().ToArray();var center=Vector3.zero;foreach(var id in ids)center+=f.transform.TransformPoint(mesh.vertices[id]);report.AppendLine("SUB "+i+" vertices="+ids.Length+" center="+(center/ids.Length).ToString("F4"));}
 foreach(var c in mesh.colors.Distinct().Take(20))report.AppendLine("COLOR "+c);
 }
 Object.DestroyImmediate(sample);
 File.WriteAllText("DuckDetails.txt",report.ToString());
 }
}
