using System;
using System.Net;

namespace Common
{
    /// <summary>
    /// 爬虫通信基础类（Model）
    /// Author：Carey
    /// CreateDate：2019年4月10日18:00:12
    /// </summary>
    public class RequestOptions
    {

        #region Constructor

        public RequestOptions()
        {
            WebHeader = new WebHeaderCollection();
            Timeout = 15000;
            KeepAlive = true;
            AllowAutoRedirect = false;
            ConnectionLimit = int.MaxValue;
            RequestNum = 3;
            Accept = "*/*";
            ContentType = "application/x-www-form-urlencoded";
        }

        #endregion

        #region Propreties

        /// <summary>
        /// 请求方式：GET或者POST
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// 上一级历史记录链接
        /// </summary>
        public string Referer { get; set; }

        /// <summary>
        /// 超时时间（毫秒）
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 是否启动长连接
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// 是否允许自动跳转
        /// </summary>
        public bool AllowAutoRedirect { get; set; }

        /// <summary>
        /// 定义最大连接数
        /// </summary>
        public int ConnectionLimit { get; set; }

        /// <summary>
        /// 请求次数
        /// </summary>
        public int RequestNum { get; set; }

        /// <summary>
        /// 可通过文件上传提交的文件类型
        /// </summary>
        public string Accept { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 头部信息
        /// </summary>
        public WebHeaderCollection WebHeader { get; set; }

        /// <summary>
        /// 定义请求Cookie字符串
        /// </summary>
        public string RequestCookies { get; set; }

        /// <summary>
        /// 异步参数数据
        /// </summary>
        public string XHRParams { get; set; }

        #endregion
    }
}