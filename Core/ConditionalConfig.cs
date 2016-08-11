using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NetTask.Design;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 运行条件配置
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract, IdentityName = "if"), Serializable]
    //[Designer(typeof(ConditionalConfigDesigner))]
    public class ConditionalConfig : IXmlSerializable
    {
        /// <summary>
        /// 属性设置默认是否可运行
        /// </summary>
        [XmlAttribute]
        [DisplayName("是否运行"), Description("获取或设置默认是否可运行")]
        public bool Executable { get; set; }

        List<IConditionalItem> condItemList = new List<IConditionalItem>();

        /// <summary>
        /// 此方法是保留方法，请不要使用。在实现 IXmlSerializable 接口时，应从此方法返回 null（在 Visual Basic 中为 Nothing），如果需要指定自定义架构，应向该类应用 <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/>。
        /// </summary>
        /// <returns>
        /// 	<see cref="T:System.Xml.Schema.XmlSchema"/>，描述由 <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 方法产生并由 <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> 方法使用的对象的 XML 表示形式。
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 从对象的 XML 表示形式生成该对象。
        /// </summary>
        /// <param name="reader">对象从中进行反序列化的 <see cref="T:System.Xml.XmlReader"/> 流。</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes
                && reader.MoveToAttribute("Executable"))
            {
                Executable = Convert.ToBoolean(reader.Value);
            }

            // 读到节点名称为：ScopeItemCond / ScopeItemCompareOfString
            //reader.ReadToNodeType(XmlNodeType.Element);

            int entDepth = reader.Depth;
            while (reader.Read() && reader.Depth >= entDepth)
            {
                //处理开始节点
                while (reader.NodeType == XmlNodeType.Element)
                {
                    condItemList.ReadXmlEx<IConditionalItem>(reader);
                }
            }
        }

        /// <summary>
        /// 将对象转换为其 XML 表示形式。
        /// </summary>
        /// <param name="writer">对象要序列化为的 <see cref="T:System.Xml.XmlWriter"/> 流。</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Executable", Executable.ToString());
            condItemList.WriteXmlEx<IConditionalItem>(writer);
        }


        /// <summary>
        /// 添加条件判断项
        /// </summary>
        /// <param name="item">新的条件判断项</param>
        public void AddItem(IConditionalItem item)
        {
            condItemList.Add(item);
        }

        /// <summary>
        /// 清除所有配置条件
        /// </summary>
        public void Clear()
        {
            condItemList.Clear();
        }

        /// <summary>
        /// 获取所有的条件配置项
        /// </summary>
        public IConditionalItem[] GetAllConditionals()
        {
            return condItemList.ToArray();
        }

        /// <summary>
        /// 判断是否能在当前作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <returns></returns>
        public bool CanRunInScope(NetTaskScope scope)
        {
            bool result = Executable;
            foreach (IConditionalItem item in condItemList)
            {
                switch (item.Logic)
                {
                    case LogicExpression.None:
                        result = item.IsPassed(scope);
                        break;
                    case LogicExpression.AND:
                        result = result && item.IsPassed(scope);
                        break;
                    case LogicExpression.OR:
                        result = result || item.IsPassed(scope);
                        break;
                    case LogicExpression.AndNot:
                        result = result && !item.IsPassed(scope);
                        break;
                    case LogicExpression.OrNot:
                        result = result || !item.IsPassed(scope);
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

    }

    /// <summary>
    /// 条件配置项
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface IConditionalItem
    {
        /// <summary>
        /// 与本项条件的逻辑运算规则
        /// </summary>
        [Description("逻辑运算规则")]
        LogicExpression Logic { get; set; }

        /// <summary>
        /// 当前条件是否通过
        /// </summary>
        /// <param name="scope">作用域</param>
        bool IsPassed(NetTaskScope scope);

    }

    /// <summary>
    /// 逻辑运算规则
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum LogicExpression
    {
        /// <summary>
        /// 没有设置
        /// </summary>
        [Description("无")]
        None = 0,

        /// <summary>
        /// 并且
        /// </summary>
        [Description("并且")]
        AND,

        /// <summary>
        /// 或者
        /// </summary>
        [Description("或者")]
        OR,

        /// <summary>
        /// 并且不
        /// </summary>
        [Description("并且不")]
        AndNot,

        /// <summary>
        /// 或者不
        /// </summary>
        [Description("或者不")]
        OrNot

    }

}
