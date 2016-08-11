using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 任务作用域
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class NetTaskScope : Dictionary<string, object>, ICloneable, IDisposable, ILogicalThreadAffinative
    {
        /// <summary>
        /// 初始化 <see cref="NetTaskScope"/> class.
        /// </summary>
        public NetTaskScope()
            : base()
        { }

        /// <summary>
        /// 初始化一个 <see cref="NetTaskScope"/> class 实例。
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public NetTaskScope(IEqualityComparer<string> comparer)
            : base(comparer)
        { }

        /// <summary>
        /// 当前上下文键名称
        /// </summary>
        public const string ContextKey = "NetTask.Core.NetTaskScope!Global";

        /// <summary>
        /// 最近(上一步骤运行结果键值)
        /// </summary>
        public const string LastExecResultKey = "NetTask.Core.NetTaskScope:LastResult";

        /// <summary>
        /// 会话全局实例
        /// </summary>
        public static NetTaskScope Context
        {
            get
            {
                if (null != HttpContext.Current && null != HttpContext.Current.Session)
                {
                    if (null == HttpContext.Current.Session[ContextKey])
                    {
                        HttpContext.Current.Session[ContextKey] = new NetTaskScope();
                    }
                    return HttpContext.Current.Session[ContextKey] as NetTaskScope;
                }
                if (null == CallContext.GetData(ContextKey))
                {
                    CallContext.SetData(ContextKey, new NetTaskScope());
                }
                return CallContext.GetData(ContextKey) as NetTaskScope;
            }
        }

        /// <summary>
        /// 获取上一步运行结果
        /// </summary>
        /// <returns></returns>
        public object GetLastExecResult()
        {
            if (!ContainsKey(LastExecResultKey))
            {
                return null;
            }
            else
            {
                return this[LastExecResultKey];
            }
        }

        /// <summary>
        /// 设置上一步运行结果
        /// </summary>
        /// <param name="result"></param>
        public void SetLastExecResult(object result)
        {
            this[LastExecResultKey] = result;
        }

        ScopePath _path = ScopePath.Empty;
        /// <summary>
        /// 目前运行路径
        /// </summary>
        public ScopePath Path
        {
            get { return _path; }
            set { _path = value; }
        }

        //NetTaskScope parentScope = null;
        ///// <summary>
        ///// 获取父级作用域
        ///// </summary>
        ///// <returns></returns>
        //public NetTaskScope GetParent() 
        //{
        //    return parentScope;
        //}

        #region ICloneable 成员

        /// <summary>
        /// 创建作为当前实例副本的新对象。
        /// </summary>
        /// <returns>作为此实例副本的新对象。</returns>
        public object Clone()
        {
            NetTaskScope ctx = new NetTaskScope(Comparer);
            foreach (var item in Keys)
            {
                ctx.Add(item, this[item]);
            }
            return ctx;
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        #endregion
    }
}
