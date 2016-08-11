using System;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;
using System.ComponentModel;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 线程暂停任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "sleep", Description = "当前线程暂停任务"), Serializable]
    public class ThreadSleepTask : XmlDefineTask
    {
        /// <summary>
        /// 等待毫秒数
        /// </summary>
        [XmlAttribute]
        [DisplayName("等待毫秒数"), Description("配置当前线程需要等待的毫秒数")]
        public string WaitMilliseconds { get; set; }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (string.IsNullOrEmpty(WaitMilliseconds) && scope.ContainsKey("WaitMilliseconds"))
            {
                WaitMilliseconds = scope["WaitMilliseconds"].ToString();
            }

            //转换为毫秒单位
            int duration = GetDurationMilliseconds(WaitMilliseconds);
#if TEST
            Console.WriteLine(string.Format("线程即将等待：{0}毫秒", duration));
#endif
            if (duration > 0) System.Threading.Thread.Sleep(duration);
        }

        /// <summary>
        /// 转换为毫秒单位 间隔时间，接收类似5m(分钟) 1h(小时) 5s(秒) 1000ms(毫秒)等单位，无单位的数字单位默认为分钟。
        /// </summary>
        [APIExport(Category = ExportType.APIMethod)]
        public static int GetDurationMilliseconds(string configVal)
        {
            if (configVal.EndsWith("ms"))
            {
                return Convert.ToInt32(configVal.Substring(0, configVal.Length - 2));
            }
            else if (configVal.EndsWith("d"))
            {
                return Convert.ToInt32(configVal.TrimEnd('d')) * 24 * 60 * 60 * 1000;
            }
            else if (configVal.EndsWith("h"))
            {
                return Convert.ToInt32(configVal.TrimEnd('h')) * 60 * 60 * 1000;
            }
            else if (configVal.EndsWith("m"))
            {
                return Convert.ToInt32(configVal.TrimEnd('m')) * 60 * 1000;
            }
            else if (configVal.EndsWith("s"))
            {
                return Convert.ToInt32(configVal.TrimEnd('s')) * 1000;
            }
            return 5000;
        }

    }
}
