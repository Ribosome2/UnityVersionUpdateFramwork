using System.Linq.Expressions;
using UnityEngine;    
using System.Collections;    
using System.Collections.Generic;    
using System.Text;    
using System.IO;
using UnityEngineInternal;

public class ResUpdate : MonoBehaviour
{
    public static readonly string VERSION_FILE = "version.txt";
    public static readonly string LOCAL_RES_URL = "file:///E:/U3DProjects/VersionUpdate/FakeLocal/";
    //public static string LOCAL_RES_URL
    //{
    //    get
    //    {
    //        switch (Application.platform)
    //        {
    //                case RuntimePlatform.Android:
    //                return "jar:file://" + Application.dataPath + "!/assets/";
    //            case RuntimePlatform.IPhonePlayer:
    //                return Application.dataPath + "/Raw";
    //            case RuntimePlatform.WindowsPlayer:
    //                //Application.dataPath + "/Streamming Assets";
    //            default: 
    //                return "file://" + Application.dataPath + "/StreammingAssets/";
                    
    //        }
            
    //    }
    //} 
    public static readonly string SERVER_RES_URL = "file:///E:/U3DProjects/VersionUpdate/FakeWebServer/";
    public static readonly string LOCAL_RES_PATH = Application.dataPath + "/Res/";

    private Dictionary<string,string> LocalResVersion=new Dictionary<string,string>();
    private Dictionary<string,string> ServerResVersion=new Dictionary<string, string>();
    private List<string> NeedDownFiles;
    private bool NeedUpdateLocalVersionFile = false;

    private void Start()
    {
        //初始化    
        LocalResVersion = new Dictionary<string, string>();
        ServerResVersion =new Dictionary<string, string>();
        NeedDownFiles = new List<string>();

        //加载本地version配置    
        StartCoroutine(DownLoad(LOCAL_RES_URL + VERSION_FILE, delegate(WWW localVersion)
        {
            //保存本地的version    
            ParseVersionFile(localVersion.text, LocalResVersion);
            //加载服务端version配置    
            StartCoroutine(this.DownLoad(SERVER_RES_URL + VERSION_FILE, delegate(WWW serverVersion)
            {
                //保存服务端version    
                ParseVersionFile(serverVersion.text, ServerResVersion);
                //计算出需要重新加载的资源    
                CompareVersion();
                //加载需要更新的资源    
                DownLoadRes();
            }));

        }));
    }
    Vector2 scrollPos=new Vector2();
    void OnGUI()
    {
        GUILayout.Label("Local resource path"+LOCAL_RES_URL);
        GUILayout.Label("PersistenDataPath: "+Application.persistentDataPath);
        GUILayout.BeginScrollView(scrollPos);
        IEnumerator iter = LocalResVersion.GetEnumerator();
        while (iter.MoveNext())
        {
            var versionInfo = iter.Current ;
            GUILayout.Label(versionInfo + "/version" + versionInfo as string);
        }
        GUILayout.EndScrollView();
    }
    //依次加载需要更新的资源    
    private void DownLoadRes()
    {
        if (NeedDownFiles.Count == 0)
        {
            UpdateLocalVersionFile();
            return;
        }

        string file = NeedDownFiles[0];
        NeedDownFiles.RemoveAt(0);

        StartCoroutine(this.DownLoad(SERVER_RES_URL + file, delegate(WWW w)
        {
            //将下载的资源替换本地就的资源    
            ReplaceLocalRes(file, w.bytes);
            DownLoadRes();
        }));
    }

    private void ReplaceLocalRes(string fileName, byte[] data)
    {
        string filePath = LOCAL_RES_PATH + fileName;
        FileStream stream = new FileStream(LOCAL_RES_PATH + fileName, FileMode.Create);
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }

    
    //显示资源    
    private IEnumerator Show()
    {
        WWW asset = new WWW(LOCAL_RES_URL + "Cube.assetbundle");
        yield return asset;
        AssetBundle bundle = asset.assetBundle;
        Instantiate(bundle.LoadAsset("Cube"));
        bundle.Unload(false);
    }

    //更新本地的version配置    
    private void UpdateLocalVersionFile()
    {
        if (NeedUpdateLocalVersionFile)
        {
            StringBuilder versions = new StringBuilder();
            foreach (var item in ServerResVersion)
            {
                versions.Append(item.Key).Append(",").Append(item.Value).Append("\n");
            }

            FileStream stream = new FileStream(LOCAL_RES_PATH + VERSION_FILE, FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
        }
        //加载显示对象    
        StartCoroutine(Show());
    }

    private void CompareVersion()
    {
        foreach (var version in ServerResVersion)
        {
            string fileName = version.Key;
            string serverMd5 = version.Value;
            //新增的资源    
            if (!LocalResVersion.ContainsKey(fileName))
            {
                NeedDownFiles.Add(fileName);
            }
            else
            {
                //需要替换的资源    
                string localMd5;
                LocalResVersion.TryGetValue(fileName, out localMd5);
                if (!serverMd5.Equals(localMd5))
                {
                    NeedDownFiles.Add(fileName);
                }
            }
        }
        //本次有更新，同时更新本地的version.txt    
        NeedUpdateLocalVersionFile = NeedDownFiles.Count > 0;
    }

    private void ParseVersionFile(string content, Dictionary<string,string> dict)
    {
        if (content == null || content.Length == 0)
        {
            return;
        }
        string[] items = content.Split(new char[] {'\n'});
        foreach (string item in items)
        {
            string[] info = item.Split(new char[] {','});
            if (info != null && info.Length == 2)
            {
                dict.Add(info[0], info[1]);
            }
        }

    }

    private IEnumerator DownLoad(string url, HandleFinishDownload finishFun)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            Debug.LogError(string.Format("Load {0} error{1}",url,www.error));
        }
        if (finishFun != null)
        {
            finishFun(www);
        }
        www.Dispose();
    }

    public delegate void HandleFinishDownload(WWW www);
}