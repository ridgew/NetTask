using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// URL地址解析任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, Description = "根据正则表达式配置解析URL地址"), Serializable]
    public class UrlParseTask : XmlDefineTask
    {
        /// <summary>
        /// 初始化一个 <see cref="UrlParseTask"/> class 实例。
        /// </summary>
        public UrlParseTask()
            : base()
        {
        }

        /// <summary>
        /// URL解析后产生的解析词典
        /// </summary>
        Dictionary<string, object> parsedDict = new Dictionary<string, object>();

        string[] ResultKeys = new string[0];

        /// <summary>
        /// 解析结果配置词典
        /// </summary>
        Dictionary<string, ParseConfig> ResultConfigDict = new Dictionary<string, ParseConfig>();
        UrlParseResult parseHandler = SimpleParse;

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            string URL = this[this.GetType().FullName + "::URL"].ToString();
            if (Config != null && Config.Items != null) ApplyConfig(Config.Items);
            if (parseHandler != null)
            {
                parsedDict = parseHandler(ResultConfigDict, URL);
            }
            else
            {
                parsedDict = SimpleParse(ResultConfigDict, URL);
            }

            #region 保存到scope
            foreach (var item in parsedDict)
            {
                scope[item.Key] = item.Value;
            }
            #endregion
        }

        /// <summary>
        /// 通过配置值执行初始化等系列操作
        /// </summary>
        /// <param name="configVal">配置值集合</param>
        void ApplyConfig(ConfigItem[] configVal)
        {
            Dictionary<string, Action<ConfigItem>> configDict = new Dictionary<string, Action<ConfigItem>>();
            //ResultItems
            configDict.Add("ResultItems", o =>
            {
                ResultKeys = o.Value.Split(new char[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            });

            XmlDefineTask.ApplayConfigValue(configVal, configDict);
            foreach (var key in ResultKeys)
            {
                configDict.Add("Pattern_" + key, o =>
                {
                    string rKey = (o.Key).StartsWith("Pattern_") ? o.Key.Substring("Pattern_".Length) : o.Key;
                    ResultConfigDict[rKey] = ParseConfig.Parse(o.Value);
                });
            }

            //Adapter
            configDict.Add("Adapter", o =>
            {
                Type type = TypeCache.GetRuntimeType(o.RefType);
                MethodInfo targetMethod = type.GetMethod(o.Value, BindingFlags.Public | BindingFlags.Static);
                parseHandler = (UrlParseResult)Delegate.CreateDelegate(typeof(UrlParseResult), targetMethod, true);
            });
            XmlDefineTask.ApplayConfigValue(configVal, configDict);
        }

        /// <summary>
        /// 简易URL解析处理
        /// </summary>
        /// <param name="parseDict">正则模式解析词典</param>
        /// <param name="urlString">需要分析的URL地址</param>
        /// <returns></returns>
        public static Dictionary<string, object> SimpleParse(Dictionary<string, ParseConfig> parseDict, string urlString)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            Match m = null;
            foreach (var item in parseDict.Keys)
            {
                if (parseDict[item].JustMatch)
                {
                    dict.Add(item, Regex.IsMatch(urlString, parseDict[item].Pattern, RegexOptions.IgnoreCase));
                }
                else
                {
                    m = Regex.Match(urlString, parseDict[item].Pattern, RegexOptions.IgnoreCase);
                    if (!m.Success)
                    {
                        dict.Add(item, "");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(parseDict[item].GroupConfig))
                        {
                            dict.Add(item, "");
                        }
                        else
                        {
                            //可选为任意一组 {0|1|2}
                            string[] gConfigs = parseDict[item].GroupConfig.Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            string gVal = "";
                            for (int i = 0, j = gConfigs.Length; i < j; i++)
                            {
                                gVal = m.Groups[Convert.ToInt32(gConfigs[i])].Value.Trim();
                                if (gVal != string.Empty)
                                {
                                    dict.Add(item, System.Web.HttpUtility.UrlDecode(gVal));
                                    break;
                                }
                            }
                            if (gVal == "") dict.Add(item, "");
                        }
                    }
                }
            }
            return dict;
        }

    }


    /// <summary>
    /// 获取URL解析结果
    /// </summary>
    /// <param name="resultConfigs">结果键值集合</param>
    /// <param name="urlString">需要解析的URL地址</param>
    /// <returns></returns>
    public delegate Dictionary<string, object> UrlParseResult(Dictionary<string, ParseConfig> resultConfigs, string urlString);

    /// <summary>
    /// 解析配置
    /// </summary>
    [Serializable]
    public struct ParseConfig
    {
        /// <summary>
        /// 匹配模式
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// 获取值分组索引配置,可选为任意一组如{1|5|7}或一组{5}。
        /// </summary>
        public string GroupConfig { get; set; }

        /// <summary>
        /// 是否只判断匹配
        /// </summary>
        public bool JustMatch { get; set; }

        /// <summary>
        /// 解析配置到实例
        /// </summary>
        /// <param name="config">当前实例配置的字符信息</param>
        /// <returns></returns>
        public static ParseConfig Parse(string config)
        {
            ParseConfig result = new ParseConfig();
            result.JustMatch = true;
            if (!config.StartsWith("{"))
            {
                result.Pattern = config;
            }
            else
            {
                result.JustMatch = false;
                int idx = config.IndexOf(':');
                result.GroupConfig = config.Substring(0, idx).Trim('{', '}');
                result.Pattern = config.Substring(idx + 1);
            }
            return result;
        }
    }
}
