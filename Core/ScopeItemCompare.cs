using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 作用域字段比较条件
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class ScopeItemCompare<T> : IConditionalItem
        where T : IComparable
    {
        #region IConditionalItem 成员

        /// <summary>
        /// 与本项条件的逻辑运算规则
        /// </summary>
        /// <value></value>
        [XmlAttribute]
        public LogicExpression Logic { get; set; }

        /// <summary>
        /// 作用域中特定键名
        /// </summary>
        [XmlAttribute]
        public string Key { get; set; }

        /// <summary>
        /// 比较模式
        /// </summary>
        [XmlAttribute]
        public CompareMode Mode { get; set; }

        /// <summary>
        /// 比较值
        /// </summary>
        [XmlAttribute]
        public T Value { get; set; }

        /// <summary>
        /// 当前条件是否通过
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <returns></returns>
        public bool IsPassed(NetTaskScope scope)
        {
            if (!scope.ContainsKey(Key)) return false;
            T lVal = default(T);
            if (scope[Key].GetType().Equals(typeof(T)))
            {
                lVal = (T)scope[Key];
            }
            else
            {
                lVal = (T)Convert.ChangeType(scope[Key], typeof(T));
            }
            return CompareBoolean(Mode, lVal, Value);
        }

        #endregion

        /// <summary>
        /// 获取布尔比较值(默认为false)
        /// </summary>
        /// <param name="mode">比较模式</param>
        /// <param name="leftVal">左边值</param>
        /// <param name="rightVal">右边的值</param>
        /// <returns></returns>
        public static bool CompareBoolean(CompareMode mode, T leftVal, T rightVal)
        {
            bool result = false;
            switch (mode)
            {
                case CompareMode.Equal:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) == 0;
                    break;
                case CompareMode.Greater:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) > 0;
                    break;
                case CompareMode.Lower:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) < 0;
                    break;
                case CompareMode.GreaterEqual:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) >= 0;
                    break;
                case CompareMode.LowerEqual:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) <= 0;
                    break;
                case CompareMode.NotEqual:
                    result = Comparer<T>.Default.Compare(leftVal, rightVal) != 0;
                    break;
                default:
                    break;
            }
            return result;
        }

    }

    /// <summary>
    /// 比较模式
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum CompareMode
    {
        /// <summary>
        /// 等于
        /// </summary>
        Equal = 0,

        /// <summary>
        /// 不等于
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// 大于
        /// </summary>
        Greater,

        /// <summary>
        /// 小于
        /// </summary>
        Lower,

        /// <summary>
        /// 大于或等于
        /// </summary>
        GreaterEqual,

        /// <summary>
        /// 小于或等于
        /// </summary>
        LowerEqual
    }
}
