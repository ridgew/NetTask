using System;
using System.Xml.Serialization;
using NetTask.Interface;
using System.ComponentModel;

namespace NetTask.Core
{
    /// <summary>
    /// 作用域字段执行数学运算条件
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable, Description("运行域字段执行数学运算")]
    public class ScopeItemMatch<T> : IConditionalItem
        where T : struct, IComparable
    {
        /// <summary>
        /// 作用域中特定键名
        /// </summary>
        [XmlAttribute, Description("运行域键名")]
        public string Key { get; set; }

        /// <summary>
        /// 数学操作
        /// </summary>
        [XmlAttribute, Description("数学操作")]
        public MathOperation Operation { get; set; }

        /// <summary>
        /// 执行操作后是否更新操作域的数(默认不更新，更新一般用来做步进器)
        /// </summary>
        [XmlAttribute, Description("执行操作后是否更新")]
        public bool RefreshScope { get; set; }

        CompareMode mode = CompareMode.Equal;
        /// <summary>
        /// 比较模式
        /// </summary>
        [XmlAttribute, Description("比较模式")]
        public CompareMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        /// <summary>
        /// 期望运算结果
        /// </summary>
        [XmlAttribute, Description("期望运算结果")]
        public T DesiredValue { get; set; }

        /// <summary>
        /// 左右运算的右边值(或者自加减的步长)
        /// </summary>
        [XmlAttribute, Description("左右运算的右边值(或步长)")]
        public string RightValue { get; set; }

        double opValLeft = 0, opValRight = 0, opValDesired = 0;

        #region IConditionalItem 成员
        /// <summary>
        /// 与本项条件的逻辑运算规则
        /// </summary>
        /// <value></value>
        [XmlAttribute]
        public LogicExpression Logic { get; set; }

        /// <summary>
        /// 当前条件是否通过
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <returns></returns>
        public virtual bool IsPassed(NetTaskScope scope)
        {
            if (!scope.ContainsKey(Key))
            {
                if (RefreshScope) scope[Key] = "0.00";
            }
            else
            {
                opValLeft = Convert.ToDouble(scope[Key]);
            }

            if (!string.IsNullOrEmpty(RightValue))
            {
                opValRight = Convert.ToDouble(RightValue);
            }
            else
            {
                opValRight = 1.00;
            }
            opValDesired = Convert.ToDouble(DesiredValue);

            bool result = false;
            switch (Operation)
            {
                case MathOperation.Add:
                case MathOperation.SelfAdd:
                    result = ScopeItemCompare<Double>.CompareBoolean(Mode, opValLeft + opValRight, opValDesired);
                    if (RefreshScope) scope[Key] = opValLeft + opValRight;
                    break;

                case MathOperation.Subtract:
                case MathOperation.SelfSubtract:
                    result = ScopeItemCompare<Double>.CompareBoolean(Mode, opValLeft - opValRight, opValDesired);
                    if (RefreshScope) scope[Key] = opValLeft - opValRight;
                    break;

                case MathOperation.Multiple:
                    result = ScopeItemCompare<Double>.CompareBoolean(Mode, opValLeft * opValRight, opValDesired);
                    if (RefreshScope) scope[Key] = opValLeft * opValRight;
                    break;

                case MathOperation.Division:
                    result = ScopeItemCompare<Double>.CompareBoolean(Mode, opValLeft / opValRight, opValDesired);
                    if (RefreshScope) scope[Key] = opValLeft / opValRight;
                    break;

                case MathOperation.Mod:
                    result = ScopeItemCompare<Double>.CompareBoolean(Mode, opValLeft % opValRight, opValDesired); ;
                    if (RefreshScope) scope[Key] = opValLeft % opValRight;
                    break;

                default:
                    break;
            }
            return result;
        }

        #endregion

        /// <summary>
        /// 在指定作用域创建特定操作模式的步进器
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <param name="applyKey">作用域应用键名程</param>
        /// <param name="initialVal">初始值</param>
        /// <param name="rightVal">比较值或步长值设置</param>
        /// <param name="desiredVal">期望比较值</param>
        /// <param name="cmp">布尔比较模式</param>
        /// <param name="op">数学操作模式</param>
        /// <returns></returns>
        public static ScopeItemMatch<T> CreateStepCounterInScope(NetTaskScope scope, string applyKey,
            T initialVal, string rightVal, T desiredVal,
            CompareMode cmp, MathOperation op)
        {
            scope[applyKey] = initialVal;
            return new ScopeItemMatch<T>()
            {
                Key = applyKey,
                RefreshScope = true,
                Logic = LogicExpression.None,
                Mode = cmp,
                RightValue = rightVal,
                DesiredValue = desiredVal,
                Operation = op
            };
        }

    }

    /// <summary>
    /// 数学操作符号
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum MathOperation
    {
        /// <summary>
        /// 加法
        /// </summary>
        Add,
        /// <summary>
        /// 自加
        /// </summary>
        SelfAdd,
        /// <summary>
        /// 减法
        /// </summary>
        Subtract,
        /// <summary>
        /// 自减
        /// </summary>
        SelfSubtract,
        /// <summary>
        /// 乘法
        /// </summary>
        Multiple,
        /// <summary>
        /// 除法
        /// </summary>
        Division,
        /// <summary>
        /// 取模
        /// </summary>
        Mod
    }

}
