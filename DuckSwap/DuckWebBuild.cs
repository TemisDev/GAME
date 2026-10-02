using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Text;
public static class DuckWebBuild {
 public static void Run() {
 var controls=new StringBuilder();
 var input=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);var axes=input.FindProperty("m_Axes");
 for(int i=0;i<axes.arraySize;i++){var a=axes.GetArrayElementAtIndex(i);var name=a.FindPropertyRelative("m_Name").stringValue;if(name.EndsWith("1")||name.EndsWith("2"))controls.AppendLine(name+": negative="+a.FindPropertyRelative("negativeButton").stringValue+", positive="+a.FindPropertyRelative("positiveButton").stringValue+", alt="+a.FindPropertyRelative("altPositiveButton").stringValue);}
 File.WriteAllText("DuckControls.txt",controls.ToString());
 PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
 PlayerSettings.runInBackground=true;
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName="Builds/PatitosWeb",target=BuildTarget.WebGL,options=BuildOptions.None });
 File.WriteAllText("DuckBuildResult.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nBytes: "+report.summary.totalSize);
 if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("WebGL build failed");
 File.Copy("Assets/Ducks/CREDITS.txt","Builds/PatitosWeb/CREDITS.txt",true);
 }
}
