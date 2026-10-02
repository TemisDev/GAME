using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;
public static class DuckReplace {
 static Bounds BoundsOf(GameObject obj) {var rs=obj.GetComponentsInChildren<MeshRenderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
 public static void Run() {
 var report=new StringBuilder();
 string texturePath="Assets/Ducks/Textures/gradient_albedo.png";
 var ti=(TextureImporter)AssetImporter.GetAtPath(texturePath); ti.isReadable=true;ti.SaveAndReimport();
 var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
 var mat=new Material(Shader.Find("Ducks/Player Rubber"));mat.mainTexture=tex;mat.color=new Color(1,.83f,.04f);
 AssetDatabase.CreateAsset(mat,"Assets/Ducks/DuckPlayer.mat");
 var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ducks/rubberDuck.fbx");
 foreach(var path in new[]{"Assets/Prefabs/Tank.prefab","Assets/_Completed-Assets/Prefabs/CompleteTank.prefab"}) {
 Directory.CreateDirectory("DuckBackup");var backup="DuckBackup/"+Path.GetFileName(path);if(!File.Exists(backup))File.Copy(path,backup);
 var root=PrefabUtility.LoadPrefabContents(path);
 try {
 if(root.transform.Find("RubberDuckVisual")!=null)throw new System.Exception("Duck already installed");
 foreach(var r in root.GetComponentsInChildren<MeshRenderer>(true))r.enabled=false;
 var holder=new GameObject("RubberDuckVisual");holder.transform.SetParent(root.transform,false);
 var duck=(GameObject)PrefabUtility.InstantiatePrefab(model);duck.transform.SetParent(holder.transform,false);
 foreach(var r in duck.GetComponentsInChildren<MeshRenderer>())r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();
 // Locate the orange beak using the original texture, in imported coordinates.
 Vector3 beak=Vector3.zero;int count=0;
 foreach(var f in duck.GetComponentsInChildren<MeshFilter>()) {var m=f.sharedMesh;var vs=m.vertices;var uv=m.uv;for(int i=0;i<vs.Length;i++){var c=tex.GetPixelBilinear(uv[i].x,uv[i].y);if(c.r>.5f&&c.g/c.r>.15f&&c.g/c.r<.69f&&c.b<.35f){beak+=holder.transform.InverseTransformPoint(f.transform.TransformPoint(vs[i]));count++;}}}
 if(count==0)throw new System.Exception("Cannot identify beak; no prefabs saved");beak/=count;
 var flat=new Vector3(beak.x,0,beak.z);holder.transform.localRotation=Quaternion.FromToRotation(flat.normalized,Vector3.forward);
 var b=BoundsOf(duck);holder.transform.localScale=Vector3.one*(2.4f/Mathf.Max(b.size.x,b.size.z));
 b=BoundsOf(duck);holder.transform.position+=new Vector3(-b.center.x,.03f-b.min.y,-b.center.z);
 var muzzle=holder.transform.TransformPoint(beak);muzzle.z=BoundsOf(duck).max.z+.12f;
 foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="FireTransform")t.position=muzzle;
 PrefabUtility.SaveAsPrefabAsset(root,path);
 report.AppendLine(path+" OK; beak samples="+count+" bounds="+BoundsOf(duck)+" muzzle="+muzzle);
 } finally {PrefabUtility.UnloadPrefabContents(root);}
 }
 File.WriteAllText("Assets/Ducks/CREDITS.txt","Rubber Duck by J-Toastie\nhttps://poly.pizza/m/71P9WRRZ4F\nLicense: Creative Commons Attribution (see source listing).\nAdaptations: scale, orientation and player-color material.\n");
 AssetDatabase.SaveAssets();
 EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 var preview=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tank.prefab"));
 foreach(var c in preview.GetComponentsInChildren<Canvas>())c.gameObject.SetActive(false);
 var light=new GameObject("PreviewLight").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.Euler(45,-35,0);
 RenderSettings.ambientLight=new Color(.55f,.55f,.55f);
 var camera=new GameObject("PreviewCamera").AddComponent<Camera>();camera.transform.position=new Vector3(4,3.2f,5);camera.transform.LookAt(new Vector3(0,1,0));camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.18f,.23f);camera.orthographic=true;camera.orthographicSize=1.9f;
 var rt=new RenderTexture(800,800,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(800,800,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,800,800),0,0);image.Apply();File.WriteAllBytes("DuckPreview.png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);
 File.WriteAllText("DuckReplacement.txt",report.ToString()+"Preview rendered successfully.\n");
 }
}
