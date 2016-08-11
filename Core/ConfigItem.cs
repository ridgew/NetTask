using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 所有配置项
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public struct ConfigItem
    {
        /// <summary>
        /// 键名称
        /// </summary>
        [XmlAttribute]
        [Description("键名称")]
        public string Key { get; set; }

        /// <summary>
        /// 键值
        /// </summary>
        [XmlAttribute]
        [Description("键值")]
        public string Value { get; set; }

        /// <summary>
        /// 配置值引用类型
        /// </summary>
        [XmlAttribute]
        [Description("引用类型")]
        public string RefType { get; set; }


        /// <summary>
        /// 获取当前配置项的运行时值
        /// </summary>
        /// <returns></returns>
        public object GetRuntimeValue()
        {
            if (string.IsNullOrEmpty(RefType))
            {
                return Value;
            }
            else
            {
                return Convert.ChangeType(Value, TypeCache.GetRuntimeType(RefType));
            }
        }
    }
}
