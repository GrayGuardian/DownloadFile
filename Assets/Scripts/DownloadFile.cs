
using System;
using System.Net;
using System.Threading;

public class DownloadFile
{
    /// <summary>
    /// 主线程
    /// </summary>
    private SynchronizationContext _mainThread;

    public string Url;
    public DownloadFile(string url)
    {
        // 获取主线程
        _mainThread = SynchronizationContext.Current;

        // 突破Http协议的并发连接数限制
        System.Net.ServicePointManager.DefaultConnectionLimit = 512;

        Url = url;
    }
    /// <summary>
    /// 查询文件大小
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// 异步查询文件大小
    /// </summary>
    /// <param name="onTrigger"></param>
    public void GetFileSizeAsyn(Action<long> onTrigger = null)
    {
        ThreadStart threadStart = new ThreadStart(() =>
        {
            PostMainThreadAction<long>(onTrigger, GetFileSize());
        });

        Thread thread = new Thread(threadStart);
        thread.Start();
    }

    /// <summary>
    /// 通知主线程回调
    /// </summary>
    private void PostMainThreadAction(Action action)
    {
        _mainThread.Post(new SendOrPostCallback((o) =>
        {
            Action e = (Action)o.GetType().GetProperty("action").GetValue(o);
            if (e != null) e();
        }), new { action = action });
    }
    private void PostMainThreadAction<T>(Action<T> action, T arg1)
    {
        _mainThread.Post(new SendOrPostCallback((o) =>
        {
            Action<T> e = (Action<T>)o.GetType().GetProperty("action").GetValue(o);
            T t1 = (T)o.GetType().GetProperty("arg1").GetValue(o);
            if (e != null) e(t1);
        }), new { action = action, arg1 = arg1 });
    }
    public void PostMainThreadAction<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        _mainThread.Post(new SendOrPostCallback((o) =>
        {
            Action<T1, T2> e = (Action<T1, T2>)o.GetType().GetProperty("action").GetValue(o);
            T1 t1 = (T1)o.GetType().GetProperty("arg1").GetValue(o);
            T2 t2 = (T2)o.GetType().GetProperty("arg2").GetValue(o);
            if (e != null) e(t1, t2);
        }), new { action = action, arg1 = arg1, arg2 = arg2 });
    }

}
