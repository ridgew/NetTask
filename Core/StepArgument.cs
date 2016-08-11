using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NetTask.Interface;
using System.Linq;

namespace NetTask.Core
{
    /// <summary>
    /// 当进入任何功能步骤时，当前步骤传递的参数。
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public struct StepArgument
    {
        /// <summary>
        /// 判断是否有参数
        /// </summary>
        public bool IsEmpty()
        {
            return Arguments == null || Arguments.Length == 0;
        }

        /// <summary>
        /// 当前步骤的参数
        /// </summary>
        [XmlElement(ElementName = "arg")]
        [DisplayName("参数集"), Description("配置任务参数列表")]
        public ContextArgument[] Arguments { get; set; }

        /// <summary>
        /// 获取唯一一个可存储的参数值
        /// </summary>
        public ContextArgument GetStorageArgument()
        {
            if (IsEmpty())
            {
                return null;
            }
            else
            {
                return Arguments.FirstOrDefault<ContextArgument>(t => t.DelayClear);
            }
        }

        /// <summary>
        /// 应用延迟删除参数的值设置
        /// </summary>
        /// <param name="argumentVals">所有需要应用到的参数值集合</param>
        public void ApplyDelayArgumentValue(params object[] argumentVals)
        {
            if (argumentVals != null && argumentVals.Length > 0 && !IsEmpty())
            {
                ContextArgument[] argsRef = Arguments.Where<ContextArgument>(t => t.DelayClear)
                    .ToArray<ContextArgument>();

                if (argsRef != null && argsRef.Length > 0)
                {
                    int j = argumentVals.Length;
                    if (argsRef.Length < argumentVals.Length)
                    {
                        j = argsRef.Length;
                    }

                    for (int i = 0; i < j; i++)
                    {
                        argsRef[i].RuntimeValue = argumentVals[i];
                    }
                }
            }
        }

    }
}
