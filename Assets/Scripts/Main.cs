using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private DownloadFile _downloadFile;
    // Start is called before the first frame update

    void Start()
    {
        var savePath = System.IO.Path.Combine(Application.dataPath, "./SaveFiles");

        // var url = "https://redirector.gvt1.com/edgedl/android/studio/install/4.2.2.0/android-studio-ide-202.7486908-windows.exe";
        var url = "https://down.sandai.net/thunder11/XunLeiWebSetup11.2.4.1750dl.exe";

        _downloadFile = new DownloadFile(url);


        // 多线程下载文件至内存 无法断点续传
        _downloadFile.DownloadToMemory(
            4,
            (size, count) =>
            {
                UnityEngine.Debug.LogFormat("下载进度 >>> {0}/{1}", size, count);
            },
            (data) =>
            {
                UnityEngine.Debug.Log("下载完毕>>>" + data.Length);

                string filePath = System.IO.Path.Combine(savePath, "./1.exe");
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.Create(filePath);
                }

                System.IO.File.WriteAllBytes(filePath, data);

            }
        );


    }

    private void OnDestroy()
    {
        _downloadFile.Close();
    }

}


