using System;
using System.IO;
using System.Text;
using UnityEditor;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetBundleToolWnd : EditorWindow
{

    [MenuItem("Tools/打包工具")]
    public static void OpenWindow()
    {
        GetWindow<AssetBundleToolWnd>();
    }
    static BuildTarget mBuildTarget=BuildTarget.StandaloneWindows;
    private void OnGUI()
    {
        if(GUILayout.Button("Build选择的资源",GUILayout.Height(50)))
        {
            ExportResource();
        }
        
        if(GUILayout.Button("Build选择的场景",GUILayout.Height(50)))
        {
            ExportScene();
        }

        GUILayout.BeginHorizontal();
        mBuildTarget=(BuildTarget)EditorGUILayout.EnumPopup(mBuildTarget,GUILayout.Width(200));
        if(GUILayout.Button("Build所有的AB",GUILayout.Height(50)))
        {
            BuildAllAssetBundles();
        }
        GUILayout.EndHorizontal();
        
        if(GUILayout.Button("生成文件版本信息列表",GUILayout.Height(50)))
        {
            GenerateConfig();
        }
        
        
        
        void BuildAllAssetBundles()
        {
            string assetBundleDirectory = "AssetBundles";
            if(!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory, 
                BuildAssetBundleOptions.None, 
                mBuildTarget);
        }
        
        
    }


    [MenuItem("Build/ExportResource")]
    static void ExportResource()
    {
        // 打开保存面板，获取用户选择的路径  
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "assetbundle");
        if (path.Length != 0)
        {
            // 选择的要保存的对象  
            Object[] selection = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            // 打包  
            BuildPipeline.BuildAssetBundle(Selection.activeObject, selection, path, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, mBuildTarget);
        }
    }
    [MenuItem("Build/ExportScene")]
    static void ExportScene()
    {
        // 打开面板，选择用户保存的路径  
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
        if (path.Length != 0)
        {
            // 要打包的场景  
            string[] scenes = { "Assets/Scenes/scene1.unity" };
            // 打包  
            BuildPipeline.BuildPlayer(scenes, path, mBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);
        }
    }

    [MenuItem("Build/GenerateConfigFile")]
    private static void GenerateConfig()
    {
        string resPath = Application.dataPath+"/Streaming Assets";
        // 获取Res文件夹下所有文件的相对路径和MD5值  
        string[] files = Directory.GetFiles(resPath, "*", SearchOption.AllDirectories);
        if (files.Length == 0)
        {
            Debug.LogError("路径"+resPath+"没有文件");
            return;
        }
        StringBuilder versions = new StringBuilder();
        for (int i = 0, len = files.Length; i < len; i++)
        {
            string filePath = files[i];
            string extension = filePath.Substring(files[i].LastIndexOf("."));
            if (extension != ".meta")
            {
                string relativePath = filePath.Replace(resPath, "").Replace("\\", "/");
                string md5 = MD5File(filePath);
                versions.Append(relativePath).Append(",").Append(md5).Append("\n");
            }
        }
        // 生成配置文件  
        FileStream stream = new FileStream(resPath + "/version.txt", FileMode.Create);
        //File.WriteAllText(resPath,);
        byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }



    public static string MD5File(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }  

}
