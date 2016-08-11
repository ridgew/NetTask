using System;
using NetTask.Core;

namespace NetTask
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { "test.ntml" };
            Console.Title = typeof(Program).Assembly.GetName().Name;
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("请输入需要执行任务的文件地址！");
            }
            else
            {
                foreach (string fitem in args)
                {
                    try
                    {
                        NetTaskScope gs = new NetTaskScope();
                        XmlDefineTask task = XmlDefineTask.CreateFrom(fitem);
                        //task.BindFromXmlFile(fitem);
                        if (task == null)
                        {
                            Console.WriteLine("还原任务文件失败，可能文件格式不正确！");
                        }
                        else
                        {
                            task.RunInScope(gs);
                            Console.WriteLine("正常执行任务文件：" + fitem);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("执行任务文件{0}失败：\n{1}！", fitem, ex);
                    }
                }
            }
        }
    }
}
