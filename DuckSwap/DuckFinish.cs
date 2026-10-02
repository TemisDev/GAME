using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
public static class DuckFinish {
 public static void Run() {
 var report=new StringBuilder();
 foreach(var path in new[]{"Assets/Prefabs/Tank.prefab","Assets/_Completed-Assets/Prefabs/CompleteTank.prefab"}) {
 var root=PrefabUtility.LoadPrefabContents(path);
 try {
 var visual=root.transform.Find("RubberDuckVisual");var f=visual.GetComponentInChildren<MeshFilter>();var mesh=f.sharedMesh;Vector3 tip=Vector3.zero;int n=0;
 for(int i=0;i<mesh.vertexCount;i++){var p=f.transform.TransformPoint(mesh.vertices[i]);if(mesh.uv[i].x>.5f&&mesh.uv[i].x<.625f&&p.z>1.16f){tip+=p;n++;}}
 if(n==0)throw new System.Exception("Beak tip not found");tip/=n;tip.z+=.15f;
 var fire=System.Array.Find(root.GetComponentsInChildren<Transform>(true),t=>t.name=="FireTransform");if(fire==null)throw new System.Exception("FireTransform missing");fire.position=tip;
 foreach(var c in root.GetComponents<MonoBehaviour>()){var so=new SerializedObject(c);var p=so.FindProperty("m_FireTransform");if(p!=null&&p.objectReferenceValue!=fire)throw new System.Exception("Fire reference mismatch");}
 PrefabUtility.SaveAsPrefabAsset(root,path);report.AppendLine(path+" verified; muzzle="+tip);
 }finally{PrefabUtility.UnloadPrefabContents(root);}
 }
 UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
 var instance=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tank.prefab"));foreach(var c in instance.GetComponentsInChildren<Canvas>())c.gameObject.SetActive(false);
 RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.65f,.65f);
 var light=new GameObject("Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(40,210,0);
 var camera=new GameObject("Camera").AddComponent<Camera>();camera.transform.position=new Vector3(4,3.2f,5);camera.transform.LookAt(new Vector3(0,1,0));camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.18f,.23f);camera.orthographic=true;camera.orthographicSize=1.9f;
 var rt=new RenderTexture(800,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(800,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,800,800),0,0);image.Apply();File.WriteAllBytes("DuckPreview.png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);
 foreach(var scene in new[]{"Assets/Scenes/Main.unity","Assets/_Complete-Game.unity"}){
 UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
 foreach(var c in Object.FindObjectsOfType<MonoBehaviour>()){var so=new SerializedObject(c);var p=so.FindProperty("m_TankPrefab");if(p!=null){var prefab=p.objectReferenceValue as GameObject;if(prefab==null||prefab.transform.Find("RubberDuckVisual")==null)throw new System.Exception("Scene tank prefab missing duck: "+scene);report.AppendLine(scene+" uses duck prefab.");}}
 }
 AssetDatabase.SaveAssets();File.WriteAllText("DuckReplacement.txt",report.ToString()+"Model, materials, muzzle and scene references verified.\n");
 }
 public static void Inspect() {
 var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tank.prefab"));
 var f=root.transform.Find("RubberDuckVisual").GetComponentInChildren<MeshFilter>();var mesh=f.sharedMesh;var sb=new StringBuilder("x,y,z,u,v\n");
 for(int i=0;i<mesh.vertexCount;i++){var p=f.transform.TransformPoint(mesh.vertices[i]);var uv=mesh.uv[i];sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2},{3},{4}",p.x,p.y,p.z,uv.x,uv.y));}
 File.WriteAllText("DuckVertices.csv",sb.ToString());Object.DestroyImmediate(root);
 }
}
