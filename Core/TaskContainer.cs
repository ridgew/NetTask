using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NetTask.Interface;
using System.ComponentModel;
using NetTask.Design;

namespace NetTask.Core
{
    /// <summary>
    /// 功能步骤通用容器
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "tc", Description = "通用任务容器，可以包含其他任务。"), Serializable]
    public class TaskContainer : XmlDefineTask, ISequentialTask, IXmlSerializable, ITaskContainer
    {
        /// <summary>
        /// 容器内任务列表
        /// </summary>
        List<INetTask> childTaskList = new List<INetTask>();

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (ImplementMode == SequentialMode.Instance)
            {
                INetTask[] tasks = GetAllNetTasks();
                foreach (INetTask task in tasks)
                {
                    if (task.Disabled)
                        continue;

                    if (task is XmlDefineTask)
                    {
                        XmlDefineTask dxml = (XmlDefineTask)task;
                        dxml.RunInScope(scope);
                        if (dxml.Status == StepStatus.Exception)
                        {
                            throw dxml.GetLastException();
                        }
                    }
                    else
                    {
                        task.EnterScope(scope);
                        if (task.Conditional == null || task.Conditional.CanRunInScope(scope))
                        {
                            task.ExecuteInScope(scope);
                        }
                        task.ExitScope(scope);
                    }
                }
            }
            else
            {
                #region 静态委托顺序执行
                EnterScope(scope);
                foreach (string sfunc in GetAllNetTaskDelegates())
                {
                    sfunc.CreateFromConfig<StepAction>().Invoke(scope, this);
                }
                ExitScope(scope);
                #endregion
            }
        }

        /// <summary>
        /// 静态符合委托StepAction的方法
        /// </summary>
        [ContractConfig(IpcContract.Delegate, typeof(StepAction))]
        [Description("静态符合委托StepAction的方法"), DisplayName("方法列表")]
        public string[] SequentialDelegates { get; set; }

        #region ISequentialTask 成员

        /// <summary>
        /// 获取所有需要执行的任务引用
        /// </summary>
        /// <returns></returns>
        public string[] GetAllNetTaskDelegates()
        {
            return SequentialDelegates;
        }

        /// <summary>
        /// 获取或设置实现模型
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("获取或设置实现模型"), DisplayName("顺序模型")]
        [TextBoxMode(BoxMode.SingleLine, Cols = 20)]
        public SequentialMode ImplementMode { get; set; }


        /// <summary>
        /// 直接获取所有需要执行的任务
        /// </summary>
        /// <returns></returns>
        public INetTask[] GetAllNetTasks()
        {
            return childTaskList.ToArray();
        }

        #endregion

        /// <summary>
        /// 添加子级任务
        /// </summary>
        /// <param name="task"></param>
        public void AddSubTask(INetTask task)
        {
            childTaskList.Add(task);
        }

        /// <summary>
        /// 添加多个子级任务
        /// </summary>
        /// <param name="tasks"></param>
        public void AddSubTaskRange(IEnumerable<INetTask> tasks)
        {
            childTaskList.AddRange(tasks);
        }

        #region IXmlSerializable 成员

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
        /// 从xml阅读器中读取属性
        /// </summary>
        /// <param name="reader"></param>
        protected virtual void ReadAttributeXml(XmlReader reader) { }

        /// <summary>
        /// 从xml阅读器中读取节点
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected virtual void ReadElementXml(XmlReader reader) { }

        /// <summary>
        /// 是否可以读取节点
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected virtual bool CanReadElementName(string elementName)
        {
            return false;
        }

        /// <summary>
        /// 从对象的 XML 表示形式生成该对象。
        /// </summary>
        /// <param name="reader">对象从中进行反序列化的 <see cref="T:System.Xml.XmlReader"/> 流。</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader.HasAttributes)
            {
                if (reader.MoveToAttribute("ImplementMode"))
                {
                    ImplementMode = (SequentialMode)Enum.Parse(typeof(SequentialMode), reader.Value);
                }
                ReadAttributeXml(reader);
            }

            int entDepth = reader.Depth;
            while (reader.Read() && reader.Depth >= entDepth)
            {
                //处理开始节点
                while (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name.Equals("Config") || reader.Name.Equals("TaskConfig"))
                    {
                        Config = reader.ObjectReadXml<TaskConfig>();
                    }
                    else if (reader.Name.Equals("TaskContainer"))
                    {
                        //允许继续包含容器任务
                        TaskContainer subCt = reader.ObjectReadXml<TaskContainer>();
                        childTaskList.Add(subCt);
                        continue;
                    }
                    else if (reader.Name.Equals("Conditional") || reader.Name.Equals("ConditionalConfig"))
                    {
                        Conditional = reader.ObjectReadXml<ConditionalConfig>();
                        continue;
                    }
                    else if (reader.Name.Equals("SequentialDelegates"))
                    {
                        SequentialDelegates = reader.ObjectReadXml<string[]>();
                        continue;
                    }
                    else if (CanReadElementName(reader.Name))
                    {
                        ReadElementXml(reader);
                        continue;
                    }
                    childTaskList.ReadXmlEx<INetTask>(reader);
                }
            }

        }

        /// <summary>
        /// 写入属性
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void WriteAttributeXml(XmlWriter writer) { }

        /// <summary>
        /// 写入节点
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void WriteElementXml(XmlWriter writer) { }

        /// <summary>
        /// 将对象转换为其 XML 表示形式。
        /// </summary>
        /// <param name="writer">对象要序列化为的 <see cref="T:System.Xml.XmlWriter"/> 流。</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("ImplementMode", ImplementMode.ToString());
            WriteAttributeXml(writer);

            if (Config != null) Config.ObjectWriteXml(writer, "Config");
            if (Conditional != null) Conditional.ObjectWriteXml(writer, "Conditional");

            WriteElementXml(writer);
            if (ImplementMode == SequentialMode.Instance)
            {
                childTaskList.WriteXmlEx<INetTask>(writer);
            }
            else
            {
                #region 静态委托字符
                writer.WriteStartElement("SequentialDelegates");
                foreach (string sfunc in SequentialDelegates)
                {
                    writer.WriteStartElement("string");
                    writer.WriteValue(sfunc);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                #endregion
            }
        }

        #endregion

    }
}
