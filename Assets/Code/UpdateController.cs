using UnityEngine;
using System.Collections;

/// <summary>
/// 当前更新过程的状态，从上往下切换状态
/// </summary>
public enum UpdateState
{
    NotStated,
    LoadingRemoteFileList,
    LoadingLocalFileList,
}

public class UpdateController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
