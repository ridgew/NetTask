using System;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// 数据转换操作
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "convert", Description = "数据转换任务操作")]
    public class ClrDataConvert : XmlDefineTask
    {
        
        /// <summary>
        /// 在当前转换过程中数据库连接对象（当数据源为DataReader时）
        /// </summary>
        [XmlIgnore]
        public DbConnection CurrentConnection { get; set; }

        /// <summary>
        /// 原始数据来自上下文参数名称，如果为null则为上一步骤运算结果。
        /// </summary>
        [XmlAttribute]
        public string ArgumentKey { get; set; }

        /// <summary>
        /// 指定应用到的标识
        /// </summary>
        [XmlAttribute]
        public string ApplyForId { get; set; }

        /// <summary>
        /// 转换委托类型
        /// </summary>
        [XmlAttribute]
        public string DeletgateType { get; set; }

        /// <summary>
        /// 转换类型（默认为一对一）
        /// </summary>
        [XmlAttribute]
        public ConvertDirection Direction { get; set; }

        /// <summary>
        /// 处理静态方法，包含类型全称。
        /// </summary>
        [XmlAttribute]
        public string ProcessStaticMethod { get; set; }

        /// <summary>
        /// 处理结果存储器
        /// </summary>
        public ClrDataStorage Storage { get; set; }

        Delegate hander = null;

        void initialHandler()
        {
            if (hander == null)
            {
                Type delegateType = Type.GetType(DeletgateType, true);
                string staticCfgMethod = ProcessStaticMethod;
                string pattern = "::([^,]+)";
                Match m = Regex.Match(staticCfgMethod, pattern, RegexOptions.IgnoreCase);
                if (!m.Success)
                {
                    throw new System.Configuration.ConfigurationErrorsException("委托配置方法(" + staticCfgMethod + ")配置错误！");
                }
                else
                {
                    Type methodType = Type.GetType(staticCfgMethod.Replace(m.Value, string.Empty));
                    MethodInfo method = methodType.GetMethod(m.Groups[1].Value, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                    hander = Delegate.CreateDelegate(delegateType, method, true);
                }
            }
        }

        object getSourceData(NetTaskScope scope)
        {
            if (ArgumentKey == null)
            {
                return scope.GetLastExecResult();
            }
            else
            {
                return this[ArgumentKey];
            }
        }

        /// <summary>
        /// 在当前作用域下运行
        /// </summary>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            initialHandler();

            #region 存储方向
            switch (Direction)
            {
                case ConvertDirection.OneToOne:
                    object objResult = hander.DynamicInvoke(getSourceData(scope));
                    if (Storage != null)
                    {
                        Storage.CurrentConnection = CurrentConnection;
                        //if (Storage.Config != null)
                        //{
                        //    Storage.ApplyConfig(Storage.Config.ConfigValue);
                        //}
                        Storage.StoreResult = objResult;
                        Storage.ExecuteInScope(scope);
                    }
                    break;

                case ConvertDirection.OneToMore:
                    break;

                case ConvertDirection.MoreToOne:
                    break;

                case ConvertDirection.MoreToMore:
                    break;

                default:
                    break;
            }
            #endregion

        }

    }

    /// <summary>
    /// 数据转换方向
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum ConvertDirection
    {
        /// <summary>
        /// 一对一转换 = 0
        /// </summary>
        OneToOne = 0,
        /// <summary>
        /// 一对多转换
        /// </summary>
        OneToMore = 1,
        /// <summary>
        /// 多对一转换
        /// </summary>
        MoreToOne = 2,
        /// <summary>
        /// 多对多转换
        /// </summary>
        MoreToMore = 3,
    }
}
