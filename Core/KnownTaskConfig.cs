using System;
using System.Collections.Specialized;
using System.Configuration;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 已知的任务名称
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public class KnownTaskConfig
    {
        /// <summary>
        /// 初始化 <see cref="KnownTaskConfig"/> class.
        /// </summary>
        private KnownTaskConfig()
        {
            APIExportAttribute.ExportApply(ExportType.INetTask, (t, exp) =>
            {
                if (!string.IsNullOrEmpty(exp.IdentityName))
                {
                    configVals[exp.IdentityName] = t.GetNoVersionTypeName();
                }
            });

            try
            {
                NameValueCollection nvc = ConfigurationManager.GetSection("KnownTaskConfig") as NameValueCollection;
                if (nvc != null)
                {
                    //throw new ConfigurationErrorsException("没有在应用程序配置文件中找到名为‘KnownTaskConfig’的节点，其类型为System.Configuration.NameValueFileSectionHandler配置！");
                    foreach (string item in nvc)
                    {
                        configVals[item] = nvc[item];
                    }
                }
            }
            catch (Exception) { }
        }

        static KnownTaskConfig config = null;

        /// <summary>
        /// 配置实例
        /// </summary>
        public static KnownTaskConfig Instance
        {
            get
            {
                if (config == null)
                {
                    //APIExportAttribute
                    config = new KnownTaskConfig();
                }
                return config;
            }
        }

        /// <summary>
        /// 已知类型配置
        /// </summary>
        NameValueCollection configVals = new NameValueCollection(StringComparer.Ordinal);

        /// <summary>
        /// 根据节点名称判断是否是已知的任务名称
        /// </summary>
        /// <param name="elementName">节点名称</param>
        /// <param name="knownType">已知的任务CLR类型实例</param>
        public bool IsKnownTaskByElementName(string elementName, ref Type knownType)
        {
            string configVal = configVals[elementName];
            if (string.IsNullOrEmpty(configVal))
            {
                return false;
            }
            knownType = TypeCache.GetRuntimeType(configVal);
            return true;
        }

    }
}
