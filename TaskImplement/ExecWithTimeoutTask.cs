using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Data;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 设定时间内运行超时
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "tExec", Description = "设定时间内运行超时")]
    public class ExecWithTimeoutTask : XmlDefineTask
    {

        /// <summary>
        /// 初始化 <see cref="ExecWithTimeoutTask"/> class.
        /// </summary>
        public ExecWithTimeoutTask()
            : base()
        {
            #region 设置需要配置的项
            requreItemList.Add(new RequriedConfigItem
            {
                Key = "StaticmethodOrFilePath",
                Optional = false,
                Description = "静态无参数的方法名称获取运行程序的地址",
                Contract = IpcContract.ClrData,
                VerifyBy = typeof(string).FullName
            });
            #endregion

            #region 应用配置绑定
            ApplyConfigDict.Add("StaticmethodOrFilePath", item =>
            {
                StaticmethodOrFilePath = item.Value;
            });
            #endregion
        }


        /// <summary>
        /// 静态无参数的方法名称获取运行程序的地址
        /// </summary>
        [XmlAttribute]
        [DisplayName("静态方法名或地址"), Description("静态无参数的方法名称获取运行程序的地址")]
        public string StaticmethodOrFilePath { get; set; }

        /// <summary>
        /// 默认运行超时时间20秒
        /// </summary>
        int defaultTimeoutSeconds = 20;

        /// <summary>
        /// 超时秒数
        /// </summary>
        [XmlAttribute]
        public int TimeoutSeconds
        {
            get { return defaultTimeoutSeconds; }
            set { defaultTimeoutSeconds = value; }
        }

        /// <summary>
        /// 当前任务是否超时
        /// </summary>
        [XmlIgnore]
        public bool IsTimeout { get; set; }

        /// <summary>
        /// 指定文件路径所传递的参数
        /// </summary>
        [XmlAttribute]
        public string Arguments { get; set; }

        /// <summary>
        /// 当前任务执行后的结果输出
        /// </summary>
        [XmlIgnore]
        public string Output { get; set; }

        /// <summary>
        /// 获取或设置当前任务的工作目录
        /// </summary>
        [XmlAttribute]
        public string WorkDirectory { get; set; }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            ContextArgument timeoutArg = GetArgument("IsTimeOut");
            string cmdFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StaticmethodOrFilePath);
            bool result = false;
            try
            {
                if (!File.Exists(cmdFilePath))
                {
                    result = ExecTimeoutMethod(defaultTimeoutSeconds, StaticmethodOrFilePath.CreateFromConfig<Action>());
                }
                else
                {
                    string cmdOutput = string.Empty;
                    result = RunCmdTimeout(cmdFilePath, WorkDirectory, Arguments, defaultTimeoutSeconds, ref cmdOutput);
                    Output = cmdOutput;

                    ContextArgument outputArg = GetArgument("Output");
                    if (outputArg != null) outputArg.RuntimeValue = cmdOutput;
                }

            }
            catch (Exception exp)
            {
                lasException = exp;
            }
            if (timeoutArg != null) timeoutArg.RuntimeValue = result;
        }

        /// <summary>
        /// 在限制秒数内的执行相关操作，并返回是否超时(默认20秒)。
        /// </summary>
        /// <param name="timeoutSeconds">超时秒数</param>
        /// <param name="act">相关方法操作</param>
        /// <returns>操作是否超时</returns>
        public static bool ExecTimeoutMethod(int? timeoutSeconds, Action act)
        {
            bool isTimeout = false;
            Thread workThread = new Thread(new ThreadStart(act));
            workThread.Start();
            if (!workThread.Join((timeoutSeconds.HasValue && timeoutSeconds.Value > 0) ? timeoutSeconds.Value * 1000 : 20000))
            {
                workThread.Abort();
                isTimeout = true;
            }
            return isTimeout;
        }

        /// <summary>
        /// 运行控制台命令程序并获取运行结果
        /// </summary>
        /// <param name="cmdPath">命令行程序完整路径</param>
        /// <param name="workDir">命令行程序的工作目录</param>
        /// <param name="strArgs">命令行参数</param>
        /// <param name="timeoutSeconds">执行超时秒数，至少为30秒以上。</param>
        /// <param name="output">命令行输出</param>
        /// <returns>操作是否超时</returns>
        public static bool RunCmdTimeout(string cmdPath, string workDir, string strArgs, int timeoutSeconds, ref string output)
        {
            int exitCode = -1, newProcessID = 0;
            string strOutput = "";
            bool hasTimeout = ExecTimeoutMethod(timeoutSeconds, () =>
            {
                #region 限制时间运行
                using (Process proc = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(cmdPath, strArgs);
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardInput = true;
                    startInfo.CreateNoWindow = true;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.WorkingDirectory = workDir ?? Path.GetDirectoryName(cmdPath);
                    proc.StartInfo = startInfo;
                    if (proc.Start()) newProcessID = proc.Id;

                    //proc.OutputDataReceived += new DataReceivedEventHandler((s, e) => {
                    //    Console.WriteLine(e.Data);
                    //});
                    //proc.WaitForExit();

                    while (!proc.HasExited)
                    {
                        strOutput += proc.StandardOutput.ReadToEnd().Replace("\r", "");
                        System.Threading.Thread.Sleep(100);
                    }
                    exitCode = proc.ExitCode;
                    proc.Close();
                }
                #endregion
            });

            if (hasTimeout)
            {
                if (newProcessID > 0)
                {
                    Process fp = null;
                    try
                    {
                        fp = Process.GetProcessById(newProcessID);
                        if (fp != null)
                        {
                            fp.Kill(); fp.Close();
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        if (fp != null) fp.Dispose();
                    }
                }
                strOutput += "* 在指定时间内(" + timeoutSeconds + ")秒执行超时！";
            }
            output = strOutput;
            return hasTimeout;
        }

        /// <summary>
        /// 关闭相关名称的进程
        /// </summary>
        /// <param name="procName">Name of the proc.</param>
        /// <returns></returns>
        public static string KillProcess(string procName)
        {
            Process[] procs = Process.GetProcessesByName(procName);
            foreach (Process p in procs)
            {
                p.Kill();
                p.Close();
                p.Dispose();
            }
            return string.Format("总计{0}已关闭", procs.Length);
        }

    }
}
