using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Common
{
    /// <summary>
    /// 爬虫基础工具类（Helper）
    /// Author：Carey
    /// CreateDate：2019年4月11日09:29:53
    /// </summary>
    public class BaseCrawler
    {
        /// <summary>
        /// 爬虫基础方法
        /// </summary>
        /// <param name="requestOption"></param>
        public static string RequestAction(RequestOptions requestOption)
        {
            string result=string.Empty;
            //IWebProxy proxy=GetProxy();
            IWebProxy proxy = null;
            var request=(HttpWebRequest)WebRequest.Create(requestOption.Uri);
            request.Accept=requestOption.Accept;
            //在使用curl做POST的时候, 当要POST的数据大于1024字节的时候, curl并不会直接就发起POST请求, 而是会分为俩步,
            //发送一个请求, 包含一个Expect: 100 -continue, 询问Server使用愿意接受数据
            //接收到Server返回的100 - continue应答以后, 才把数据POST给Server
            //并不是所有的Server都会正确应答100 -continue, 比如lighttpd, 就会返回417 “Expectation Failed”, 则会造成逻辑出错.
            request.ServicePoint.Expect100Continue=false;
            request.ServicePoint.UseNagleAlgorithm=false;//禁止Nagle算法加快载入速度
            if(!string.IsNullOrEmpty(requestOption.XHRParams))
            {
                request.AllowWriteStreamBuffering = true;
            }
            else
            {
                request.AllowWriteStreamBuffering = false;
            }//禁止换从加快载入速度
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
            request.ContentType = requestOption.ContentType;//定义文档类型以及编码
            request.AllowAutoRedirect = requestOption.AllowAutoRedirect;//禁止自动跳转
            request.UserAgent = "Mozilla/5.0(Windows NT 6.0;Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/66.0.3359.181 Safari/537.36";//设置User-Agent，伪装成Google Chrome浏览器
            request.Timeout = requestOption.Timeout;//定义请求超时时间为5sec
            request.KeepAlive = requestOption.KeepAlive;//启动长连接
            if (!string.IsNullOrEmpty(requestOption.Referer))
            {
                request.Referer = requestOption.Referer;//返回上一级历史连接
            }
            request.Method = requestOption.Method;//定义请求方式GET
            if (proxy!=null)
            {
                request.Proxy = proxy;//设置代理IP，伪装成请求地址
            }
            if (!string.IsNullOrWhiteSpace(requestOption.RequestCookies))
            {
                request.Headers[HttpRequestHeader.Cookie] = requestOption.RequestCookies;
            }
            request.ServicePoint.ConnectionLimit = requestOption.ConnectionLimit;
            if (requestOption.WebHeader!=null && requestOption.WebHeader.Count>0)
            {
                request.Headers.Add(requestOption.WebHeader);
            }
            if (!string.IsNullOrEmpty(requestOption.XHRParams))
            {
                //如果是POST请求，加入POST数据
                byte[] buffer = Encoding.UTF8.GetBytes(requestOption.XHRParams);
                if (buffer!=null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                //获取请求响应
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    //解压
                    using(GZipStream stream=new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        result = ResponseToRead(stream);
                    }
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    //解压
                    using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        result = ResponseToRead(stream);
                    }
                }
                else
                {
                    using(Stream stream = response.GetResponseStream())
                    {
                        result = ResponseToRead(stream);
                    }
                }
            }
                return result;
        }

        /// <summary>
        /// 设置代理
        /// </summary>
        private static WebProxy GetProxy()
        {
            WebProxy webProxy = null;
            try
            {
                string proxyHost = "192.168.1.1";
                string proxyPort = "9030";

                //设置代理服务器
                webProxy = new WebProxy();
                //设置代理地址和端口
                webProxy.Address=new Uri(string.Format("{0}：{1}", proxyHost,proxyPort));
                //如果只是设置代理IP端口，eg：192.168.1.1:80，这里直接注释该段代码，则不需要设置提交给代理服务器进行身份验证的账号跟密码。
                //webProxy.Credentials = new NetworkCredential(proxyUser, proxyPwd);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Message:{0},StackTrace:{1}", ex.Message, ex.StackTrace));
            }
            return webProxy;
        }

        /// <summary>
        /// 优化后的读取HTTP字符串
        /// </summary>
        private static string ResponseToRead(Stream stream)
        {
            MemoryStream memoryStream = new MemoryStream();
            string resultStr;
            byte[] byteArry = new byte[1024];
            int size = stream.Read(byteArry, 0, (int)byteArry.Length);
            while (size>0)
            {
                memoryStream.Write(byteArry, 0, size);
                size = stream.Read(byteArry, 0, (int)byteArry.Length);
            }
            resultStr = Encoding.UTF8.GetString(memoryStream.ToArray());

            return resultStr;
        }
    }
}