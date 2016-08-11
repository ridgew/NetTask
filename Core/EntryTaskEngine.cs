using System;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class EntryTaskEngine : INetTaskEngine
    {

        #region INetTaskEngine 成员

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取版本
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取描述
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 以任务实例为会话入口开始会话
        /// </summary>
        /// <param name="taskSession">任务会话实例</param>
        /// <returns></returns>
        public IEngineSession BeginSession(INetTask taskSession)
        {
            throw new NotImplementedException();
        }

        INetTaskEngine processorEngine = null;

        /// <summary>
        /// 消费会话，执行会话任务
        /// </summary>
        /// <param name="session">任务会话实例</param>
        /// <param name="scope">当前会话作用域</param>
        /// <returns></returns>
        public ExecuteResult ConsumeSession(IEngineSession session, NetTaskScope scope)
        {
            ExecuteResult result = ExecuteResult.Ok;
            session.Task.EnterScope(scope);
            try
            {
                //TaskLevel currentLev = session.Task.Level;
                //while (currentLev > TaskLevel.Instance)
                //{
                //    processorEngine = session.Task.BuildUp(scope);
                //    currentLev = session.Task.Level;
                //}
                session.Task.ExecuteInScope(scope);
            }
            catch (Exception)
            {
                result = ExecuteResult.Exception;
            }
            session.Task.ExitScope(scope);
            return result;
        }

        /// <summary>
        /// 会话完成，并返回实际执行的引擎
        /// </summary>
        /// <param name="session">任务会话实例</param>
        /// <returns></returns>
        public INetTaskEngine EndSession(IEngineSession session)
        {
            return processorEngine;
        }

        /// <summary>
        /// 最近产生的异常
        /// </summary>
        /// <returns></returns>
        public Exception GetLastException()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {

        }

        #endregion
    }
}
