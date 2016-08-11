using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTask.Interface;
using NetTask.Core;

namespace NetTask
{

    class Program : INetTaskHost
    {
        static void Main(string[] args)
        {
            Program host = new Program();

            host.StartTaskService(args);
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("服务已运行，输入Q退出!");
                while (Console.ReadLine() != "Q") { }
            }

            host.StopTaskService();
        }

        #region INetTaskHost 成员

        /// <summary>
        /// 判断是否在运行
        /// </summary>
        /// <returns></returns>
        public bool IsWorking()
        {
            return true;
        }

        /// <summary>
        /// 启动任务控制服务
        /// </summary>
        /// <param name="args">可选的启动参数集合</param>
        public void StartTaskService(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                #region 服务状态
                Console.WriteLine("进入服务运行模式");
                #endregion
            }
            else
            {
                foreach (string fitem in args)
                {
                    if (System.IO.File.Exists(fitem))
                    {
                        NetTaskScope gs = new NetTaskScope();
                        IXmlFileBasedTask task = XmlDefineTask.CreateFrom(fitem);
                        task.BindFromXmlFile(fitem);

                        EntryTaskEngine engine = new EntryTaskEngine();
                        //if (!task.CanRun(gs))
                        //{
                        //    Console.WriteLine("任务已跳过");
                        //}
                        //else
                        //{

                            task.EnterScope(gs);
                            task.ExecuteInScope(gs);
                            task.ExitScope(gs);

                            Console.WriteLine("正常执行任务");
                        //}

                        Console.WriteLine("运行完毕");
                    }
                }
            }
        }

        /// <summary>
        /// 关闭任务控制服务
        /// </summary>
        public void StopTaskService()
        {

        }

        /// <summary>
        /// 重启任务控制服务
        /// </summary>
        public void RestartTaskService()
        {

        }

        /// <summary>
        /// 获取所有运行任务引擎
        /// </summary>
        /// <returns></returns>
        public INetTaskEngine[] GetAllEngine()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
