using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NetTask.Data;
using NetTask.Interface;
//using CommonLib;
using System.Threading;
using System.Collections;
using System.ComponentModel;
using NetTask.Design;

namespace NetTask.Core
{
    /// <summary>
    /// xml定义的任务文件
    /// </summary>
    [APIExport(Category = ExportType.INetTask), Serializable]
    public abstract class XmlDefineTask : IXmlFileBasedTask
    {
        /// <summary>
        /// 当前步骤需要配置项集合
        /// </summary>
        protected List<RequriedConfigItem> requreItemList = new List<RequriedConfigItem>();

        /// <summary>
        /// 应用配置绑定词典
        /// </summary>
        protected Dictionary<string, Action<ConfigItem>> ApplyConfigDict = new Dictionary<string, Action<ConfigItem>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// 当前实例绑定的XML文档对象
        /// </summary>
        [NonSerialized]
        protected XmlDocument runtimeDoc;

        /// <summary>
        /// 从特定地址创建实例
        /// </summary>
        [APIExport(Category = ExportType.APIMethod)]
        public static XmlDefineTask CreateFrom(string xmlUrl)
        {

            //string ext = Path.GetExtension(xmlUrl);
            //根据扩展名自动寻找并创建实例
            //CircleContainerTask
            //AcceptExtensionAttribute

            string fileUrl = xmlUrl.Replace('\\', '/');
            //扩展路径
            if (fileUrl.IndexOf(':') == -1)
            {
                fileUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileUrl);
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(fileUrl);

            Type targetType = null;
            if (doc.DocumentElement.HasAttribute("type"))
            {
                targetType = TypeCache.GetRuntimeType(doc.DocumentElement.GetAttribute("type"));
            }
            else
            {
                if (!KnownTaskConfig.Instance.IsKnownTaskByElementName(doc.DocumentElement.Name, ref targetType))
                {
                    throw new InvalidContextDataException(string.Format("文件不被识别，或者格式不正确！", xmlUrl));
                }
            }

            XmlSerializer serializer = new XmlSerializer(targetType, new XmlRootAttribute(doc.DocumentElement.Name));
            XmlNodeReader xmlReader = new XmlNodeReader(doc);
            XmlDefineTask resultTask = (XmlDefineTask)serializer.Deserialize(xmlReader);
            //#if TEST
            //            resultTask.GetXmlDoc(true).WriteIndentedContent(Console.Out);
            //#endif
            resultTask.TaskUrl = fileUrl;
            resultTask.runtimeDoc = doc;
            return resultTask;
        }

        /// <summary>
        /// [*TODO]
        /// </summary>
        void bindRuntimeDoc() { }

        #region IXmlFileBasedTask 成员
        /// <summary>
        /// 从特定文件路径绑定任务
        /// </summary>
        /// <param name="fileUrl">xml文件路径</param>
        public void BindFromXmlFile(string fileUrl)
        {
            TaskUrl = fileUrl;
            XmlDocument doc = new XmlDocument();
            doc.Load(fileUrl);
            runtimeDoc = doc;
            bindRuntimeDoc();
        }

        #endregion

        /// <summary>
        /// 运行条件配置
        /// </summary>
        [Description("获取或设置任务运行条件"), DisplayName("条件配置")]
        public ConditionalConfig Conditional { get; set; }

        /// <summary>
        /// 当前运行步骤的配置
        /// </summary>
        [Description("获取或设置任务配置项"), DisplayName("任务配置")]
        public TaskConfig Config { get; set; }

        /// <summary>
        /// 运行状态
        /// </summary>
        [XmlIgnore]
        public StepStatus Status { get; set; }

        /// <summary>
        /// 最后产生的异常
        /// </summary>
        protected Exception lasException = null;
        /// <summary>
        /// 获取最后产生的异常实例
        /// </summary>
        /// <returns></returns>
        public virtual Exception GetLastException()
        {
            return lasException;
        }

        #region INetTask 成员

        ///// <summary>
        ///// 获取当前运行任务级别
        ///// </summary>
        ///// <value></value>
        //[XmlIgnore]
        //public TaskLevel Level { get; set; }

        /// <summary>
        /// 获取或设置任务区别资源地址
        /// </summary>
        /// <value></value>
        [XmlAttribute, Description("获取或设置任务区别资源地址"), DisplayName("任务标识")]
        [TextBoxMode(BoxMode.SingleLine)]
        public string TaskUrl { get; set; }

        /// <summary>
        /// 是否禁止运行（默认否)
        /// </summary>
        [XmlAttribute, Description("是否禁止运行改任务"), DisplayName("禁用")]
        public bool Disabled { get; set; }

        ///// <summary>
        ///// 向上构建，直到任务级别可直接运行
        ///// </summary>
        ///// <param name="currentScope">当前会话作用域</param>
        ///// <returns></returns>
        //public INetTaskEngine BuildUp(NetTaskScope currentScope)
        //{
        //    throw new NotImplementedException();
        //}

        ///// <summary>
        ///// 在当前作用域下是否可运行
        ///// </summary>
        ///// <param name="scope"></param>
        ///// <returns></returns>
        //public bool CanRun(NetTaskScope scope)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 进入作用域
        /// </summary>
        /// <param name="scope">作用域</param>
        public virtual void EnterScope(NetTaskScope scope)
        {
            if (!scope.Path.Equals(ScopePath.Empty))
            {
                Type callerType = (scope.Path.Depth > 0) ? scope.Path.GetCursorType(scope.Path.Cursor)
                    : new StackFrame(2).GetMethod().ReflectedType;
                scope.Path.EnterType(callerType, this.GetType());
            }
            GeneralStepAction.BindArgument(scope, this);
            if (Config != null && Config.Items != null)
            {
                ApplayConfig();
            }
        }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public abstract void ExecuteInScope(NetTaskScope scope);

        /// <summary>
        /// 退出作用域
        /// </summary>
        /// <param name="scope"></param>
        public virtual void ExitScope(NetTaskScope scope)
        {
            GeneralStepAction.ClearArgument(scope, this);
            if (!scope.Path.Equals(ScopePath.Empty))
            {
                scope.Path.ExitType(this.GetType());
            }
        }

        void runCircleTask(ICircleTask IS, NetTaskScope scope, StepAction execFunc)
        {
            switch (IS.BeCircleMode)
            {
                case CircleMode.DoWhile:
                    do
                    {
                        execFunc.Invoke(scope, this);
                    }
                    while (IS.ContinueRun(scope));
                    break;

                case CircleMode.While:
                    while (IS.ContinueRun(scope))
                    {
                        execFunc.Invoke(scope, this);
                    }
                    break;

                case CircleMode.StepIncrement:
                    runStepIncrementCircleTask(scope, execFunc);
                    break;

                case CircleMode.Enumerator:
                    runStepInEnumerableTask(scope, execFunc);
                    break;

                default:
                    break;
            }
        }

        void runStepInEnumerableTask(NetTaskScope scope, StepAction execFunc)
        {
            if (Config != null)
            {
                IEnumerator er = Config.Items.GetEnumerator();
                while (er.MoveNext())
                {
                    ConfigItem item = (ConfigItem)er.Current;
                    scope.SetLastExecResult(item.GetRuntimeValue());
                    execFunc.Invoke(scope, this);
                }
            }
        }

        void runStepIncrementCircleTask(NetTaskScope scope, StepAction execFunc)
        {
            if (this is IStepIncrementCircleTask)
            {
                IStepIncrementCircleTask iST = this as IStepIncrementCircleTask;
                if (!String.IsNullOrEmpty(iST.CircleInitialActionDelegate)
                    && !string.IsNullOrEmpty(iST.CircleIncrementActionDelegate))
                {
                    #region 完整定义
                    StepAction initialFunc = iST.CircleInitialActionDelegate.CreateFromConfig<StepAction>();
                    StepAction incrementFunc = iST.CircleIncrementActionDelegate.CreateFromConfig<StepAction>();
                    for (initialFunc.Invoke(scope, this);
                        iST.ContinueRun(scope); incrementFunc.Invoke(scope, this))
                    {
                        execFunc.Invoke(scope, this);
                    }
                    #endregion
                }
                else
                {
                    if (string.IsNullOrEmpty(iST.CircleInitialActionDelegate) && string.IsNullOrEmpty(iST.CircleIncrementActionDelegate))
                    {
                        #region 无定义while
                        for (; iST.ContinueRun(scope); )
                        {
                            execFunc.Invoke(scope, this);
                        }
                        #endregion
                    }
                    else
                    {
                        #region 任一为空
                        if (string.IsNullOrEmpty(iST.CircleInitialActionDelegate))
                        {
                            StepAction incrementFunc = iST.CircleIncrementActionDelegate.CreateFromConfig<StepAction>();
                            for (; iST.ContinueRun(scope); incrementFunc.Invoke(scope, this))
                            {
                                execFunc.Invoke(scope, this);
                            }
                        }
                        else
                        {
                            StepAction initialFunc = iST.CircleInitialActionDelegate.CreateFromConfig<StepAction>();
                            for (initialFunc.Invoke(scope, this); iST.ContinueRun(scope); )
                            {
                                execFunc.Invoke(scope, this);
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// 在进入作用域后发生
        /// </summary>
        public event ScopeTaskEvent AfterEnterScope;

        /// <summary>
        /// 在退出作用域后发生
        /// </summary>
        public event ScopeTaskEvent BeforeExitScope;

        /// <summary>
        /// [API]在当前作用域下运行(自动判断是否运行)
        /// </summary>
        /// <param name="scope">当前作用域</param>
        [APIExport(Category = ExportType.APIMethod)]
        public void RunInScope(NetTaskScope scope)
        {
            EnterScope(scope);
            if (AfterEnterScope != null) AfterEnterScope(scope, this);

            if (!(this is ICircleTask))
            {
                if (Conditional == null || Conditional.CanRunInScope(scope))
                {
                    ExecuteInScope(scope);
                }
            }
            else
            {
                ICircleTask IS = this as ICircleTask;
                if (string.IsNullOrEmpty(IS.CircleActionDelegate))
                {
                    //System.Diagnostics.Debugger.Break();
                    //检查运行域变量设置：scope.Keys.Count;
                    //throw new InvalidContextDataException(string.Format("{0}作为循环任务运行，但没有指定循环体CircleActionDelegate！", this.GetType().FullName));
                    #region 没有指定循环体，则自执行
                    runCircleTask(IS, scope, (s, t) =>
                            {
                                ExecuteInScope(s);
                            });
                    #endregion
                }
                else
                {
                    #region 循环任务
                    StepAction execFunc = IS.CircleActionDelegate.CreateFromConfig<StepAction>();
                    runCircleTask(IS, scope, execFunc);
                    #endregion
                }
            }

            if (BeforeExitScope != null) BeforeExitScope(scope, this);
            ExitScope(scope);
        }

        #endregion

        /// <summary>
        /// 获取当前步骤的命名参数的运行时值
        /// </summary>
        /// <param name="argName">参数名称</param>
        public object this[string argName]
        {
            get
            {
                ContextArgument targetArg = GetArgument(argName);
                if (targetArg != null)
                    return targetArg.RuntimeValue;
                return null;
            }
        }

        /// <summary>
        /// 获取当前步骤的名称参数对象。
        /// </summary>
        /// <param name="argName">参数名称</param>
        /// <returns></returns>
        public ContextArgument GetArgument(string argName)
        {
            if (!(Config != null && Config.ArgumentConfig.Arguments != null))
            {
                return null;
            }
            else
            {
                return Array.Find<ContextArgument>(Config.ArgumentConfig.Arguments,
                     arg => arg.Name.Equals(argName, StringComparison.InvariantCultureIgnoreCase));
            }
        }


        bool configAppled = false;
        /// <summary>
        /// 应用配置参数信息
        /// </summary>
        protected void ApplayConfig()
        {
            if (!configAppled)
            {
                ApplayConfigValue(Config.Items, ApplyConfigDict);
                configAppled = true;
            }
        }

        /// <summary>
        /// 应用配置信息
        /// </summary>
        [APIExport(Category = ExportType.APIMethod)]
        public static void ApplayConfigValue(ConfigItem[] configVal, Dictionary<string, Action<ConfigItem>> configActions)
        {
            foreach (ConfigItem item in configVal)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    throw new System.Configuration.ConfigurationErrorsException("配置错误：发现配置项的键值为null！");
                }
                else
                {
                    if (configActions.ContainsKey(item.Key))
                    {
                        configActions[item.Key](item);
                    }
                }
            }
        }

    }

    /// <summary>
    /// 作用域内任务事件委托
    /// </summary>
    /// <param name="scope">作用域</param>
    /// <param name="instance">任务实例</param>
    public delegate void ScopeTaskEvent(NetTaskScope scope, INetTask instance);
}
