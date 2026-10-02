using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
public static class DuckProbe {
 public static void Run() {
 var sb=new StringBuilder();
 foreach(var path in new[]{"Assets/Ducks/rubberDuck.fbx","Assets/Prefabs/Tank.prefab","Assets/_Completed-Assets/Prefabs/CompleteTank.prefab"}) {
 var obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path)); sb.AppendLine(path);
 foreach(var r in obj.GetComponentsInChildren<Renderer>(true)) {sb.AppendLine(r.name+" bounds="+r.bounds+" pos="+r.transform.position);foreach(var m in r.sharedMaterials)sb.AppendLine("MAT "+m.name+" color="+(m.HasProperty("_Color")?m.color.ToString():"none"));}
 foreach(var c in obj.GetComponentsInChildren<Collider>())sb.AppendLine("COL "+c.name+" "+c.bounds);
 foreach(var t in obj.GetComponentsInChildren<Transform>())if(t.name.Contains("Fire"))sb.AppendLine("FIRE "+t.localPosition+" "+t.localEulerAngles);
 Object.DestroyImmediate(obj);
 }
 File.WriteAllText("DuckProbe.txt",sb.ToString());
 }
}
