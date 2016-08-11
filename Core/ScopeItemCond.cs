using System;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;
using System.ComponentModel;

namespace NetTask.Core
{
    /// <summary>
    /// 作用域逻辑字段条件
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable, Description("运行域逻辑字段条件")]
    public class ScopeItemCond : IConditionalItem
    {

        /// <summary>
        /// 与本项条件的逻辑运算规则
        /// </summary>
        /// <value></value>
        [XmlAttribute]
        public LogicExpression Logic { get; set; }

        /// <summary>
        /// 作用域中特定键名
        /// </summary>
        [XmlAttribute, Description("运行域键名")]
        public string Key { get; set; }

        /// <summary>
        /// 当前条件是否通过
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <returns>
        /// 	<c>true</c> if the specified scope is passed; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPassed(NetTaskScope scope)
        {
            return scope.ContainsKey(Key) && Convert.ToBoolean(scope[Key]);
        }
    }
}
