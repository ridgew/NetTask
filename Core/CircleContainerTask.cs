using System;
using NetTask.Interface;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using NetTask.Design;

namespace NetTask.Core
{
    /// <summary>
    /// 循环任务容器
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "cct", Description = "支持循环运行的任务容器"), Serializable]
    public class CircleContainerTask : TaskContainer, IStepIncrementCircleTask
    {
        #region IStepIncrementCircleTask 成员
        /// <summary>
        /// 获取或设置初始化委托字符
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("获取或设置初始化委托字符"), DisplayName("初始化委托")]
        [TextBoxMode(BoxMode.SingleLine)]
        public string CircleInitialActionDelegate { get; set; }

        /// <summary>
        /// 获取或设置增量委托字符
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("获取或设置增量委托字符"), DisplayName("增量委托")]
        [TextBoxMode(BoxMode.SingleLine)]
        public string CircleIncrementActionDelegate { get; set; }

        #endregion

        #region ICircleTask 成员

        /// <summary>
        /// 循环执行模式
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("循环执行模式"), DisplayName("循环模式")]
        public CircleMode BeCircleMode { get; set; }

        /// <summary>
        /// 获取或设置主要执行的委托字符
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("获取或设置主要执行的委托字符"), DisplayName("主循环委托")]
        [TextBoxMode(BoxMode.SingleLine)]
        public string CircleActionDelegate { get; set; }

        /// <summary>
        /// 在当前作用中是否继续执行
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool ContinueRun(NetTaskScope scope)
        {
            return (Conditional != null && Conditional.CanRunInScope(scope));
        }

        #endregion

        /// <summary>
        /// 从xml阅读器中读取属性
        /// </summary>
        /// <param name="reader"></param>
        protected override void ReadAttributeXml(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "BeCircleMode")
                {
                    BeCircleMode = (CircleMode)Enum.Parse(typeof(CircleMode), reader.Value);
                }
                else if (reader.Name == "CircleInitialActionDelegate")
                {
                    CircleInitialActionDelegate = reader.Value;
                }
                else if (reader.Name == "CircleActionDelegate")
                {
                    CircleActionDelegate = reader.Value;
                }
                else if (reader.Name == "CircleIncrementActionDelegate")
                {
                    CircleIncrementActionDelegate = reader.Value;
                }
            }
        }

        /// <summary>
        /// 写入属性
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteAttributeXml(XmlWriter writer)
        {
            writer.WriteAttributeString("BeCircleMode", BeCircleMode.ToString());

            if (!string.IsNullOrEmpty(CircleInitialActionDelegate))
            {
                writer.WriteAttributeString("CircleInitialActionDelegate", CircleInitialActionDelegate);
            }

            if (!string.IsNullOrEmpty(CircleActionDelegate))
            {
                writer.WriteAttributeString("CircleActionDelegate", CircleActionDelegate);
            }

            if (!string.IsNullOrEmpty(CircleIncrementActionDelegate))
            {
                writer.WriteAttributeString("CircleIncrementActionDelegate", CircleIncrementActionDelegate);
            }
        }
    }
}
