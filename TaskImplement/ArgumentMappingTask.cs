using System;
using System.Collections;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 通过配置项及接收参数值进行映射转换任务
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "map", Description = "通过配置项及接收参数值进行映射转换任务")]
    public class ArgumentMappingTask : XmlDefineTask
    {
        /// <summary>
        /// 原始数据来自上下文参数名称，如果为null则为上一步骤运算结果。
        /// </summary>
        [XmlAttribute]
        public string ArgumentKey { get; set; }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (Config == null)
            {
                throw new InvalidContextDataException("任务配置项错误，至少应配置参数及配置项以进行数据映射任务！");
            }
            else
            {
                object argResult = this[ArgumentKey];
                if (argResult == null)
                {
                    throw new InvalidContextDataException("任务配置项错误，配置参数没能获取计算值以进行比较！");
                }

                IEnumerator er = Config.Items.GetEnumerator();
                bool hasResult = false;
                while (er.MoveNext())
                {
                    ConfigItem item = (ConfigItem)er.Current;
                    if (item.Key.Equals(argResult.ToString()))
                    {
                        scope.SetLastExecResult(item.GetRuntimeValue());
                        hasResult = true;
                        break;
                    }
                }
                if (!hasResult) scope.SetLastExecResult(null);

            }
        }

    }
}
