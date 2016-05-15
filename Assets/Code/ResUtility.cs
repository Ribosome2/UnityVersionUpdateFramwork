using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using UnityEngineInternal;

public class ResFileInfo
{
    public string fileName;
    public string md5Code;
}

public class ResUtility : MonoBehaviour {
    public const string VERSION_FILE = "version.txt";

    public const string SERVER_RES_URL = "http://127.0.0.1:8080/";
    public static  IEnumerator DownLoad(string url, HandleFinishDownload finishFun)
    {
        WWW www = new WWW(url);
        yield return www;
        if (string.IsNullOrEmpty(www.error) == false)
        {
            Debug.LogError(string.Format("Load {0} error{1}", url, www.error));
        }
        if (finishFun != null)
        {
            finishFun(www);
        }
        www.Dispose();
    }

    public delegate void HandleFinishDownload(WWW www);
    /// <summary>
    /// 原始版本文件的路径
    /// </summary>
    public static string OriginalVersionUrl
    {
        get
        {
            string prefx = "";
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                   prefx="jar:file://" + Application.dataPath + "!/assets/";break;
                case RuntimePlatform.IPhonePlayer:
                   prefx= Application.dataPath + "/Raw/";break;
                default:prefx="file://" + Application.dataPath + "/StreamingAssets/";break;
            }
            return prefx + VERSION_FILE;
        }
    }

    /// <summary>
    /// 本地版本更新后存放文件的路径
    /// </summary>
    public static string LocalUpdatedVersionUrl
    {
        get
        {
              return Application.persistentDataPath + "/"+VERSION_FILE;
        }
    }

    public static void MakeSureFileExist(string path)
    {
        if (!File.Exists(path))
        {
            File.Create(path);
        }
    }
    public static string ServerVersionUrl
    {
        get { return SERVER_RES_URL + "/" + VERSION_FILE; }
    }
    
     public static  void ParseVersionFile(string content, Dictionary<string, string> dict)
     {
         if (content == null || content.Length == 0)
         {
             return;
         }
         string[] items = content.Split(new char[] { '\n' });
         foreach (string item in items)
         {
             string[] info = item.Split(new char[] { ',' });
             if (info != null && info.Length == 2)
             {
                 dict.Add(info[0], info[1]);
             }
         }

     }

     /// <summary>
     /// 更新本地更新过的version文件信息， 注意，这里的文件信息要包含所有更新过的文件
     /// </summary>
     /// <param name="fileDict"></param>

     public static  void UpdateLocalVersionFile(Dictionary<string, string> fileDict)
     {
         StringBuilder versions = new StringBuilder();
         foreach (var item in fileDict)
         {
             versions.Append(item.Key).Append(",").Append(item.Value).Append("\n");
         }

         FileStream stream = new FileStream(ResUtility.LocalUpdatedVersionUrl, FileMode.Create);
         byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
         stream.Write(data, 0, data.Length);
         stream.Flush();
         stream.Close();
     }
	
}
