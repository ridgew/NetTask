using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// SQL数据库脚本
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class SqlScript
    {
        /// <summary>
        /// SQL语句标识
        /// </summary>
        [XmlAttribute]
        [DisplayName("标识"), Description("SQL语句标识")]
        public string SqlId { get; set; }

        /// <summary>
        /// 该sql脚本的存储路径
        /// </summary>
        [XmlAttribute]
        [DisplayName("脚本存储路径"), Description("SQL语句脚本的存储路径")]
        public string UrlPath { get; set; }

        /// <summary>
        /// SQL脚本内容
        /// </summary>
        [DisplayName("脚本内容"), Description("SQL语句脚本内容")]
        public string Text { get; set; }

        /// <summary>
        /// 当前脚本是否禁止运行
        /// </summary>
        [XmlAttribute]
        [DisplayName("禁止运行"), Description("当前脚本是否禁止运行")]
        public bool Disabled { get; set; }

        /// <summary>
        /// 查询类型
        /// </summary>
        [XmlAttribute]
        [DisplayName("查询类型"), Description("SQL语句返回的查询类型")]
        public SqlQueryType QueryType { get; set; }

        /// <summary>
        /// 结果类型
        /// </summary>
        [XmlAttribute]
        [DisplayName("结果类型"), Description("SQL语句返回的结果类型")]
        public SqlResultType ResultType { get; set; }

        SqlParameter[] sqlParams = new SqlParameter[0];
        /// <summary>
        /// 该脚本的相关参数
        /// </summary>
        [XmlArrayItem(ElementName = "param")]
        [DisplayName("脚本参数集"), Description("配置运行该SQL脚本的参数集合")]
        public SqlParameter[] SqlParams
        {
            get { return sqlParams; }
            set { sqlParams = value; }
        }
    }
}
