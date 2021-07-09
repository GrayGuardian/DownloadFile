
using System;
using System.Net;

public class DownloadFile
{
    public string Url;
    public DownloadFile(string url)
    {
        Url = url;
    }
    public long GetFileSize()
    {
        HttpWebRequest request;
        HttpWebResponse response;
        try
        {
            request = (HttpWebRequest)HttpWebRequest.CreateHttp(new Uri(Url));
            request.Method = "HEAD";
            response = (HttpWebResponse)request.GetResponse();
            // 获得文件长度
            long contentLength = response.ContentLength;

            response.Close();
            request.Abort();

            return contentLength;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void GetFileSizeAsyn(Action<long> onTrigger = null)
    {

    }



}
