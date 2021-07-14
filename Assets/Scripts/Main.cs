using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private DownloadFile _downloadFile;
    // Start is called before the first frame update

    void Start()
    {
        var savePath = System.IO.Path.Combine(Application.dataPath, "../SaveFiles");
        string filePath;
        // var url = "https://dlc2.pconline.com.cn/filedown_1117483_12749837/gFETfFfp/pconline1552198052014.zip";
        // var url = "https://down.sandai.net/thunder11/XunLeiWebSetup11.2.4.1750dl.exe";
        var url = "http://127.0.0.1/Download/111.exe";

        _downloadFile = new DownloadFile(url);

        _downloadFile.OnError += (ex) =>
        {
            UnityEngine.Debug.Log("捕获异常 >>> " + ex);
        };


        // 多线程下载文件至内存 无法断点续传
        // filePath = System.IO.Path.Combine(savePath, "./多线程下载至内存.exe");
        // _downloadFile.DownloadToMemory(
        //     4,
        //     (size, count) =>
        //     {
        //         UnityEngine.Debug.LogFormat("[{0}]下载进度 >>> {1}/{2}", "多线程下载至内存", size, count);
        //     },
        //     (data) =>
        //     {
        //         UnityEngine.Debug.LogFormat("[{0}]下载完毕>>>{1}", "多线程下载至内存", data.Length);

        //         // 下载至内存后保存到文件
        //         if (!System.IO.File.Exists(filePath))
        //         {
        //             System.IO.File.Create(filePath).Dispose();
        //         }
        //         System.IO.File.WriteAllBytes(filePath, data);

        //     }
        // );

        // 多线程下载文件至本地 支持断点续传
        filePath = System.IO.Path.Combine(savePath, "./多线程下载至本地.exe");
        _downloadFile.DownloadToFile(
            4,
            filePath,
            (size, count) =>
            {
                UnityEngine.Debug.LogFormat("[{0}]下载进度 >>> {1}/{2}", "多线程下载至本地", size, count);
            },
            (data) =>
            {
                UnityEngine.Debug.LogFormat("[{0}]下载完毕>>>{1}", "多线程下载至本地", data.Length);
            }
        );

    }

    private void OnDestroy()
    {
        _downloadFile.Close();
    }

}


