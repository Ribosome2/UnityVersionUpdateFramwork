using System.Linq;
using System.Linq.Expressions;
using UnityEditor.VersionControl;
using UnityEngine;    
using System.Collections;    
using System.Collections.Generic;    
using System.Text;    
using System.IO;
using UnityEngineInternal;
using FileMode = System.IO.FileMode;

public class ResUpdate : MonoBehaviour
{
   
    public static readonly string LOCAL_RES_PATH = Application.dataPath + "/Res/";

    private Dictionary<string,string> LocalUpdatedVersion=new Dictionary<string,string>();
    private Dictionary<string,string> LocalResVersion=new Dictionary<string,string>();
    private Dictionary<string,string> ServerResVersion=new Dictionary<string, string>();
    private Dictionary<string, string> NeedDownFiles=new Dictionary<string, string>();
    private bool NeedUpdateLocalVersionFile = false;

    private void Start()
    {
        //初始化    
        LocalResVersion = new Dictionary<string, string>();
        ServerResVersion =new Dictionary<string, string>();
        LocalUpdatedVersion=new Dictionary<string, string>();
        NeedDownFiles = new Dictionary<string, string>();

        //加载本地version配置    
        StartCoroutine(ResUtility.DownLoad(ResUtility.OriginalVersionUrl, delegate(WWW localVersion)
        {
            //保存本地的version    
            ResUtility.ParseVersionFile(localVersion.text, LocalResVersion);

            
            //加载本地version配置    
            StartCoroutine(ResUtility.DownLoad(ResUtility.LocalUpdatedVersionUrl,delegate(WWW updatedVersion)
                {
                    //加载本地已经更新过的版本消息   
                    ResUtility.ParseVersionFile(updatedVersion.text, LocalUpdatedVersion);
                    CombineLocalVersion(); //获取完整的文件信息列表

                    //加载服务端version配置    
                    StartCoroutine(ResUtility.DownLoad(ResUtility.ServerVersionUrl, delegate(WWW serverVersion)
                    {
                        //保存服务端version    
                        ResUtility.ParseVersionFile(serverVersion.text, ServerResVersion);
                        //计算出需要重新加载的资源    
                        CompareVersionAndMakeUpdateList();
                        //加载需要更新的资源    
                        DownLoadRes();
                    }));

                }));

        }));

    }

    Vector2 scrollPos=new Vector2();
    void OnGUI()
    {
        GUILayout.Label("Local resource path"+ResUtility.OriginalVersionUrl);
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
            return;
        }
        
        var file = NeedDownFiles.First();
        NeedDownFiles.Remove(file.Key);

        StartCoroutine(ResUtility.DownLoad(ResUtility.SERVER_RES_URL + file, delegate(WWW w)
        {
            //将下载的资源替换本地就的资源    
            ReplaceLocalRes(file.Key,file.Value, w.bytes);
            DownLoadRes();
        }));
    }

    private void ReplaceLocalRes(string fileName,string strMd5, byte[] data)
    {
        string filePath = LOCAL_RES_PATH + fileName;
        FileStream stream = new FileStream(LOCAL_RES_PATH + fileName, FileMode.Create);
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
        //每更新成功一个文件更新一次version文件
        AddOrUpdateLocalVersionInfo(fileName,strMd5);
        ResUtility.UpdateLocalVersionFile(LocalUpdatedVersion);
    }


    void AddOrUpdateLocalVersionInfo(string fileName,string md5)
    {
        if (LocalUpdatedVersion.ContainsKey(fileName))
        {
            LocalUpdatedVersion[fileName] = md5;
        }
        else
        {
            LocalUpdatedVersion.Add(fileName,md5);
        }
    }

    /// <summary>
    /// 把初始版本信息和已经更新过的版本信息组合，得到本地当前完整的文件信息
    /// </summary>
    void CombineLocalVersion()
    {
        foreach (var version in LocalUpdatedVersion)
        {
            if (LocalResVersion.ContainsKey(version.Key))  //如果重复，就用更新过的信息
            {
                LocalResVersion[version.Key] = version.Value;
            }
            else
            {
                LocalResVersion.Add(version.Key,version.Value);//没有就新增
            }
        }
    }


    /// <summary>
    /// 对比本地版本和服务器版本信息，获得需要更新的列表
    /// </summary>
    private void CompareVersionAndMakeUpdateList()
    {
        NeedDownFiles.Clear();
        foreach (var version in ServerResVersion)
        {
            string fileName = version.Key;
            string serverMd5 = version.Value;
            //新增的资源    
            if (!LocalResVersion.ContainsKey(fileName))
            {
                NeedDownFiles.Add(fileName,serverMd5);
            }
            else
            {
                //需要替换的资源    
                string localMd5;
                LocalResVersion.TryGetValue(fileName, out localMd5);
                if (!serverMd5.Equals(localMd5))
                {
                    NeedDownFiles.Add(fileName,serverMd5);
                }
            }
        }
        //本次有更新，同时更新本地的version.txt    
        NeedUpdateLocalVersionFile = NeedDownFiles.Count > 0;
    }

   

  
}