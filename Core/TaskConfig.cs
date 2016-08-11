using System;
using NetTask.Interface;
using System.ComponentModel;

namespace NetTask.Core
{
    /// <summary>
    /// 任务配置项
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class TaskConfig
    {
        /// <summary>
        /// 当前步骤需要的参数
        /// </summary>
        [DisplayName("参数配置"), Description("配置当前任务运行所需的相关交互参数")]
        public StepArgument ArgumentConfig { get; set; }

        ConfigItem[] _items = new ConfigItem[0];

        /// <summary>
        /// 获取或设置步骤相关配置项(总不为null)
        /// </summary>
        [DisplayName("键值配置"), Description("配置当前任务运行所需的值")]
        public ConfigItem[] Items
        {
            get { return _items; }
            set { _items = value; }
        }

        /// <summary>
        /// 通过参数配置创建
        /// </summary>
        /// <param name="args">上下文参数配置</param>
        /// <returns></returns>
        public static TaskConfig CreateWithArgument(params ContextArgument[] args)
        {
            return new TaskConfig
            {
                ArgumentConfig = new StepArgument { Arguments = args }
            };
        }

    }
}
