using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 关键字解析任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, Description = "针对URL地址中的关键字编码字符解析"), Serializable]
    public class KeywordParseTask : XmlDefineTask
    {
        /// <summary>
        /// 初始化一个 <see cref="KeywordParseTask"/> class 实例。
        /// </summary>
        public KeywordParseTask()
            : base()
        {
            ApplyConfigDict.Add("KeywordPattern", o =>
            {
                KeywordPattern = o.Value;
            });
            ApplyConfigDict.Add("Adapter", o =>
            {
                Type type = TypeCache.GetRuntimeType(o.RefType);
                MethodInfo targetMethod = type.GetMethod(o.Value, BindingFlags.Public | BindingFlags.Static);
                keywordHandler = (GetKeywordFromUrl)Delegate.CreateDelegate(typeof(GetKeywordFromUrl), targetMethod, true);
            });

        }

        string KeywordPattern = "&keyword=([^&]+)";
        GetKeywordFromUrl keywordHandler = null;

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (keywordHandler == null)
            {
                scope["关键字"] = string.Empty;
            }
            else
            {
                scope["关键字"] = keywordHandler(KeywordPattern, this["RawURL"].ToString());
            }
            scope.SetLastExecResult(scope["关键字"]);
        }

        /// <summary>
        /// 简易关键字处理委托
        /// </summary>
        /// <param name="pattern">正则匹配模式</param>
        /// <param name="urlString">需要匹配的URL地址</param>
        /// <returns></returns>
        public static string SimpleKeywordHandler(string pattern, string urlString)
        {
            Match m = Regex.Match(urlString, pattern, RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return string.Empty;
            }
            else
            {
                return System.Web.HttpUtility.UrlDecode(m.Groups[1].Value);
            }
        }
    }

    /// <summary>
    /// 从URL地址中获取关键字的委托
    /// </summary>
    public delegate string GetKeywordFromUrl(string capturePattern, string urlStr);
}
