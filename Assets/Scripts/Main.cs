using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private DownloadFile _downloadFile;
    // Start is called before the first frame update
    void Start()
    {
        var url = "https://dlc2.pconline.com.cn/filedown_1117483_12749837/KuVu8l3Y/pconline1552198052014.zip";

        _downloadFile = new DownloadFile(url);

        _downloadFile.GetFileSizeAsyn((size) =>
        {
            UnityEngine.Debug.LogFormat("异步读取文件尺寸：{0}", size);
        });

        UnityEngine.Debug.LogFormat("同步读取文件尺寸：{0}", _downloadFile.GetFileSize());


    }

    // Update is called once per frame
    void Update()
    {

    }
}
