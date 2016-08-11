using System;
using System.Xml;
using NetTask.Core;
using NetTask.Interface;
using System.ComponentModel;

namespace NetTask.Data
{
    /// <summary>
    /// 数据接收器连接桥
    /// </summary>
    [Serializable]
    public sealed class DataReceiverBridge : TaskContainer, IDataReceiver
    {

        /// <summary>
        /// 是否能够接收当前数据
        /// </summary>
        /// <param name="objData">当前传输的数据</param>
        /// <returns></returns>
        public bool Accept(object objData)
        {
            INetTask[] childTasks = GetAllNetTasks();
            return childTasks.Length > 0 && childTasks[0] is IDataReceiver;
        }

        /// <summary>
        /// 数据存储器
        /// </summary>
        /// <value></value>
        [Description("存储数据的存储器"), DisplayName("数据存储器")]
        public ClrDataStorage Storage { get; set; }

        /// <summary>
        /// 是否可以读取节点
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected override bool CanReadElementName(string elementName)
        {
            return elementName == "Storage";
        }

        /// <summary>
        /// 从xml阅读器中读取节点
        /// </summary>
        /// <param name="reader">The reader.</param>
        protected override void ReadElementXml(XmlReader reader)
        {
            Storage = reader.ObjectReadXml<ClrDataStorage>();
        }

        /// <summary>
        /// 写入节点
        /// </summary>
        /// <param name="writer"></param>
        protected override void WriteElementXml(XmlWriter writer)
        {
            if (Storage != null)
            {
                Storage.ObjectWriteXml(writer, "Storage");
            }
        }

    }
}
