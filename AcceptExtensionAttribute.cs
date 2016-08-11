using System;
using System.Collections.Generic;

namespace NetTask.Interface
{
    /// <summary>
    /// 接收扩展名定义
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AcceptExtensionAttribute : Attribute
    {

        /// <summary>
        /// 初始化一个 <see cref="AcceptExtensionAttribute"/> class 实例。
        /// </summary>
        /// <param name="firstExt">The first ext.</param>
        /// <param name="otherExts">The other exts.</param>
        public AcceptExtensionAttribute(string firstExt, params string[] otherExts)
        {
            List<string> extList = new List<string>();
            extList.Add(firstExt);
            if (otherExts != null)
            {
                foreach (string ext in otherExts)
                {
                    extList.Add(ext);
                }
            }
            acceptedExtensions = extList.ToArray();
        }

        string[] acceptedExtensions = new string[0];
        /// <summary>
        /// 获取该类型所有支持的扩展名
        /// </summary>
        /// <returns></returns>
        public string[] GetAllExtensions()
        {
            return acceptedExtensions;
        }
    }

}
