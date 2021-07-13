
using System;
using System.Net;
using System.Threading;
using System.IO;

public class DownloadFile
{
    /// <summary>
    /// 主线程
    /// </summary>
    private SynchronizationContext _mainThreadSynContext;

    /// <summary>
    /// 下载网址
    /// </summary>
    public string Url;

    /// <summary>
    /// 主要用于关闭线程
    /// </summary>
    private bool _isDownload = true;
    public DownloadFile(string url)
    {
        // 主线程赋值
        _mainThreadSynContext = SynchronizationContext.Current;
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
    /// 多线程下载文件至内存
    /// </summary>
    /// <param name="threadCnt"></param>
    /// <param name="onDownloading"></param>
    /// <param name="onTrigger"></param>
    public void DownloadToMemory(int threadCnt, Action<long, long> onDownloading = null, Action<byte[]> onTrigger = null)
    {
        _isDownload = true;

        GetFileSizeAsyn((size) =>
        {
            long csize = 0;
            int ocnt = 0;
            byte[] cdata = new byte[size];
            Action<int, long, byte[], byte[]> t_onDownloading = (index, rsize, rbytes, data) =>
            {
                csize += rsize;
                //  UnityEngine.Debug.LogFormat("[{0}]正在下载 >>> 单次下载数据：{1}  单次总数据：{2}  {3}/{4}", index, rsize, data.LongLength, csize, size);

                PostMainThreadAction<long, long>(onDownloading, csize, size);
            };
            Action<int, byte[]> t_onTrigger = (index, data) =>
            {
                //  UnityEngine.Debug.LogFormat("[{0}]下载完毕 >>> {1}", index, data.LongLength);

                long dIndex = (long)Math.Ceiling((double)(size * index / threadCnt));

                Array.Copy(data, 0, cdata, dIndex, data.Length);

                ocnt++;
                if (ocnt >= threadCnt)
                {
                    //  UnityEngine.Debug.Log("下载完毕>>>" + cdata.Length);

                    PostMainThreadAction<byte[]>(onTrigger, cdata);
                }
            };
            // UnityEngine.Debug.LogFormat("异步读取文件尺寸：{0}", size);
            long[] sizes = SplitFileSize(size, threadCnt);
            for (int i = 0; i < sizes.Length; i = i + 2)
            {
                long min = sizes[i];
                long max = sizes[i + 1];
                _threadDownload(i / 2, min, max, t_onDownloading, t_onTrigger);
                // UnityEngine.Debug.LogFormat("min>>{0} max>>{1}", min, max);
            }
        });
    }
    private void _threadDownload(int index, long min, long max, Action<int, long, byte[], byte[]> onDownloading = null, Action<int, byte[]> onTrigger = null)
    {
        Thread thread = new Thread(new ThreadStart(() =>
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(new Uri(Url));
            request.AddRange(min, max);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream ns = response.GetResponseStream();

            byte[] rbytes = new byte[8 * 1024];
            int rSize = 0;
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                if (!_isDownload) return;
                rSize = ns.Read(rbytes, 0, rbytes.Length);

                if (rSize <= 0) break;
                ms.Write(rbytes, 0, rSize);
                if (onDownloading != null) onDownloading(index, rSize, rbytes, ms.ToArray());
            }
            ns.Close();
            response.Close();
            request.Abort();

            if (onTrigger != null) onTrigger(index, ms.ToArray());
        }));
        thread.Start();
    }

    public void Close()
    {
        _isDownload = false;
    }

    /// <summary>
    /// 分割文件大小
    /// </summary>
    /// <returns></returns>
    private long[] SplitFileSize(long size, int count)
    {
        long[] result = new long[count * 2];
        for (int i = 0; i < count; i++)
        {
            long min = (long)Math.Ceiling((double)(size * i / count));
            long max = (long)Math.Ceiling((double)(size * (i + 1) / count)) - 1;
            result[i * 2] = min;
            result[i * 2 + 1] = max;
        }

        return result;
    }


    /// <summary>
    /// 通知主线程回调
    /// </summary>
    private void PostMainThreadAction(Action action)
    {
        _mainThreadSynContext.Post(new SendOrPostCallback((o) =>
        {
            Action e = (Action)o.GetType().GetProperty("action").GetValue(o);
            if (e != null) e();
        }), new { action = action });
    }
    private void PostMainThreadAction<T>(Action<T> action, T arg1)
    {
        _mainThreadSynContext.Post(new SendOrPostCallback((o) =>
        {
            Action<T> e = (Action<T>)o.GetType().GetProperty("action").GetValue(o);
            T t1 = (T)o.GetType().GetProperty("arg1").GetValue(o);
            if (e != null) e(t1);
        }), new { action = action, arg1 = arg1 });
    }
    public void PostMainThreadAction<T1, T2>(Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        _mainThreadSynContext.Post(new SendOrPostCallback((o) =>
         {
             Action<T1, T2> e = (Action<T1, T2>)o.GetType().GetProperty("action").GetValue(o);
             T1 t1 = (T1)o.GetType().GetProperty("arg1").GetValue(o);
             T2 t2 = (T2)o.GetType().GetProperty("arg2").GetValue(o);
             if (e != null) e(t1, t2);
         }), new { action = action, arg1 = arg1, arg2 = arg2 });
    }
    public void PostMainThreadAction<T1, T2, T3>(Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
    {
        _mainThreadSynContext.Post(new SendOrPostCallback((o) =>
         {
             Action<T1, T2, T3> e = (Action<T1, T2, T3>)o.GetType().GetProperty("action").GetValue(o);
             T1 t1 = (T1)o.GetType().GetProperty("arg1").GetValue(o);
             T2 t2 = (T2)o.GetType().GetProperty("arg2").GetValue(o);
             T3 t3 = (T3)o.GetType().GetProperty("arg3").GetValue(o);
             if (e != null) e(t1, t2, t3);
         }), new { action = action, arg1 = arg1, arg2 = arg2, arg3 = arg3 });
    }
    public void PostMainThreadAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        _mainThreadSynContext.Post(new SendOrPostCallback((o) =>
         {
             Action<T1, T2, T3, T4> e = (Action<T1, T2, T3, T4>)o.GetType().GetProperty("action").GetValue(o);
             T1 t1 = (T1)o.GetType().GetProperty("arg1").GetValue(o);
             T2 t2 = (T2)o.GetType().GetProperty("arg2").GetValue(o);
             T3 t3 = (T3)o.GetType().GetProperty("arg3").GetValue(o);
             T4 t4 = (T4)o.GetType().GetProperty("arg4").GetValue(o);
             if (e != null) e(t1, t2, t3, t4);
         }), new { action = action, arg1 = arg1, arg2 = arg2, arg3 = arg3, arg4 = arg4 });
    }
}
