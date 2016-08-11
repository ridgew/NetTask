using System;
using System.IO;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;
using NetTask.Util;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 通过HTTP下载文件(通过配置项下载文件)
    /// Key = URL, RefType = HTTPHeader, Value= HeadValue
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "httpDown", Description = "通过HTTP下载文件")]
    public class HttpDownloadTask : XmlDefineTask
    {
        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (Config != null && Config.Items.Length > 0)
            {
                if (!Directory.Exists(SaveDirectory)) Directory.CreateDirectory(SaveDirectory);
                using (HttpClient http = new HttpClient())
                {
                    foreach (ConfigItem item in Config.Items)
                    {
                        if (!String.IsNullOrEmpty(item.Value) && !string.IsNullOrEmpty(item.RefType))
                        {
                            http.Headers.Set(item.RefType, item.Value);
                        }
                        http.DownloadFile(item.Key, Path.Combine(SaveDirectory, Path.GetFileName(item.Key)));
                    }
                }
            }
        }

        /// <summary>
        /// 获取或设置文件保持目录
        /// </summary>
        [XmlAttribute]
        public string SaveDirectory { get; set; }

    }

}
