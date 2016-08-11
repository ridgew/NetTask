//using CommonLib;
using NetTask.Interface;
using System;

namespace NetTask.Core
{
    /// <summary>
    /// 对当前上下问应用的步骤动作
    /// </summary>
    /// <param name="context">运行上下文</param>
    /// <param name="currentStep">当前运行步骤</param>
    [APIExport(Category = ExportType.APIDataContract)]
    public delegate void StepAction(NetTaskScope context, INetTask currentStep);

    /// <summary>
    /// 步骤常用相关操作
    /// </summary>
    public static class GeneralStepAction
    {
        /// <summary>
        /// 进入步骤时绑定当前步骤的参数
        /// </summary>
        /// <param name="context">运行上下文</param>
        /// <param name="currentStep">当前运行步骤</param>
        public static void BindArgument(NetTaskScope context, INetTask currentStep)
        {
            if (currentStep.Config == null) return;

            StepArgument argument = currentStep.Config.ArgumentConfig;
            if (!argument.IsEmpty())
            {
                foreach (ContextArgument arg in argument.Arguments)
                {
                    if (arg.ExchangeFlag.Has<ArgumentExchange>(ArgumentExchange.AutoApply))
                    {
                        try
                        {
                            arg.BindWithContext(context);
                        }
                        catch (Exception bindEx)
                        {
                            throw new InvalidContextDataException(string.Format("*绑定上下文参数值出现错误[TaskUrl:{0}]：{0}",
                                currentStep.TaskUrl,
                                bindEx.ToString()));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 退出步骤时清除当前步骤的参数
        /// </summary>
        /// <param name="context">运行上下文</param>
        /// <param name="currentStep">当前运行步骤</param>
        public static void ClearArgument(NetTaskScope context, INetTask currentStep)
        {
            if (currentStep.Config == null) return;

            StepArgument argument = currentStep.Config.ArgumentConfig;
            if (!argument.IsEmpty())
            {
                foreach (ContextArgument arg in argument.Arguments)
                {
                    string contextKey = arg.ContextName ?? arg.Name;
                    #region 自动移除或更新绑定数据
                    if (arg.IsTransferArgument())
                    {
                        #region 退出时传输参数调试
                        //if (currentStep.GetType().FullName.StartsWith("NetTask.TaskImplement.SqlNetTask"))
                        //{
                        //    if (currentStep.TaskUrl == "sqlr-savep")
                        //    {
                        //        if (contextKey == "CounterPercent")
                        //        {
                        //            System.Console.WriteLine("****{0}", currentStep.TaskUrl);
                        //            System.Diagnostics.Debugger.Break();
                        //        }
                        //    }
                        //}

                        //if (currentStep.GetType().FullName.StartsWith("NetTask.TaskImplement.ScopeSetTask"))
                        //{
                        //    if (currentStep.TaskUrl == "set-resetcounter")
                        //    {
                        //          System.Console.WriteLine("****{0}", currentStep.TaskUrl);
                        //          System.Diagnostics.Debugger.Break();
                        //    }
                        //}
                        #endregion

                        if (arg.LastResult && !context.GetLastExecResult().Equals(arg.RuntimeValue))
                        {
                            context[contextKey] = context.GetLastExecResult();
                        }
                        else
                        {
                            context[contextKey] = arg.RuntimeValue;
                        }
                    }
                    else
                    {
                        if (context.ContainsKey(contextKey))
                        {
                            if (arg.ExchangeFlag.Has<ArgumentExchange>(ArgumentExchange.AutoClear))
                            {
                                context.Remove(contextKey);
                            }
                        }
                    }
                    #endregion

                }
            }
        }

    }
}
