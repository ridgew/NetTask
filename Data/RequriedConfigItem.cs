using System;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// 所有需要配置的项
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract, IdentityName = "require"), Serializable]
    public struct RequriedConfigItem
    {
        /// <summary>
        /// 键名称
        /// </summary>
        [XmlAttribute]
        public string Key { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        [XmlAttribute]
        public string Description { get; set; }

        /// <summary>
        /// 是否可选
        /// </summary>
        [XmlAttribute]
        public bool Optional { get; set; }

        /// <summary>
        /// 契约类型
        /// </summary>
        [XmlAttribute]
        public IpcContract Contract { get; set; }

        /// <summary>
        /// 验证参数
        /// </summary>
        [XmlAttribute]
        public string VerifyBy { get; set; }

    }

}
