using System;
using System.Diagnostics;
using System.Xml.Serialization;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 上下文中的某一个参数定义
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    [DebuggerDisplay("Name = {Name}, Value = {Value,nq}, DelayClear = {DelayClear}")]
    public class ContextArgument
    {
        /// <summary>
        /// 参数名称(必需)
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// 绑定上下文参数名称
        /// </summary>
        [XmlAttribute]
        public string ContextName { get; set; }

        /// <summary>
        /// 参数数据类型
        /// </summary>
        [XmlAttribute]
        public string Type { get; set; }

        /// <summary>
        /// 以字符串表现的值
        /// </summary>
        [XmlAttribute]
        public string Value { get; set; }

        object runtimeVal = null;
        /// <summary>
        /// 运行时参数
        /// </summary>
        [XmlIgnore]
        public object RuntimeValue
        {
            get
            {
                if (runtimeVal == null && Value != null)
                {
                    bindArgumentVal(Value);
                }
                return runtimeVal;
            }
            set
            {
                runtimeVal = value;
            }
        }

        /// <summary>
        /// 是否是可选参数
        /// </summary>
        [XmlAttribute]
        public bool Optional { get; set; }

        /// <summary>
        /// 最近作用域运算结果
        /// </summary>
        [XmlAttribute]
        public bool LastResult { get; set; }

        ArgumentExchange exchangeFlag = ArgumentExchange.Duplex;
        /// <summary>
        /// 该参数的值自动绑定上下文，同时也支持自动移除（指定<c>DelayClear</c>除外）。
        /// </summary>
        [XmlAttribute]
        public ArgumentExchange ExchangeFlag
        {
            get { return exchangeFlag; }
            set { exchangeFlag = value; }
        }

        /// <summary>
        /// 判断是否延迟清除
        /// </summary>
        [XmlAttribute]
        public bool DelayClear { get; set; }

        /// <summary>
        /// 判断是否是传输参数
        /// </summary>
        public bool IsTransferArgument()
        {
            return DelayClear && RuntimeValue != null;
        }

        #region 静态方法

        /// <summary>
        /// 创建上下问传输参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="argVal">参数值</param>
        /// <returns></returns>
        public static ContextArgument CreateTransferArgument(string name, object argVal)
        {
            ContextArgument arg = Create(name, argVal, false);
            arg.DelayClear = true;
            arg.ExchangeFlag = ArgumentExchange.AutoApply;
            return arg;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="argVal">参数值</param>
        /// <param name="isOptional">是否是可选值</param>
        /// <returns></returns>
        public static ContextArgument Create(string name, object argVal, bool isOptional)
        {
            ContextArgument arg = new ContextArgument();
            arg.Name = name;
            arg.Optional = isOptional;
            arg.RuntimeValue = argVal;
            if (argVal != null)
            {
                arg.Type = argVal.GetType().ToSimpleType();
                arg.Value = argVal.ToStringValue();
            }
            return arg;
        }

        /// <summary>
        /// 创建必要参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="argVal">参数值</param>
        /// <returns></returns>
        public static ContextArgument Create(string name, object argVal)
        {
            return Create(name, argVal, false);
        }

        #endregion

        void bindArgumentVal(object objVal)
        {
            if (string.IsNullOrEmpty(Type))
            {
                RuntimeValue = objVal;
                Type = objVal.GetType().ToSimpleType();

                Value = objVal.ToStringValue();
            }
            else
            {
                Type argType = TypeCache.GetRuntimeType(Type);
                Type srcType = objVal.GetType();
                if (argType.Equals(srcType))
                {
                    RuntimeValue = objVal;
                    Value = objVal.ToStringValue();
                }
                else
                {
                    RuntimeValue = Convert.ChangeType(objVal, argType);
                    Value = RuntimeValue.ToStringValue();
                }
            }
        }

        /// <summary>
        /// 从上下文绑定
        /// </summary>
        /// <param name="ctxKeyName">存储在上下文中的键值</param>
        /// <param name="scope">运行时上下文</param>
        void BindWithContextKey(string ctxKeyName, NetTaskScope scope)
        {
            if (scope.ContainsKey(ctxKeyName))
            {
                bindArgumentVal(scope[ctxKeyName]);
            }
            else
            {
                if (LastResult)
                {
                    #region 绑定或设置最后运行结果
                    if (scope.GetLastExecResult() != null)
                    {
                        bindArgumentVal(scope.GetLastExecResult());
                    }
                    else
                    {
                        if (RuntimeValue != null)
                        {
                            scope.SetLastExecResult(runtimeVal);
                        }
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 从上下文绑定
        /// </summary>
        /// <param name="scope">运行时上下文</param>
        public void BindWithContext(NetTaskScope scope)
        {
            BindWithContextKey(ContextName ?? Name, scope);
        }

    }
}
