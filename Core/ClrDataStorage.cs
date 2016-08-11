using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Common;
using System.Xml.Serialization;
//using CommonLib;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 数据存储
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "store", Description = "存储步骤运行过程中的数据"), Serializable]
    public class ClrDataStorage : XmlDefineTask
    {
        /// <summary>
        /// 初始化 <see cref="ClrDataStorage"/> class.
        /// </summary>
        public ClrDataStorage()
            : base()
        {
            ApplyConfigDict.Add("SplitStorage", o =>
            {
                splitStorage = Convert.ToBoolean(o.Value);
                if (!string.IsNullOrEmpty(o.RefType))
                {
                    splitDataTypeConfig = o.RefType;
                }
            });
            ApplyConfigDict.Add("StorageKeys", o =>
            {
                storageKeys = o.Value;
                if (!string.IsNullOrEmpty(o.RefType))
                {
                    mappingKeys = o.RefType;
                }
            });
        }

        ///// <summary>
        ///// 存储配置引用标识（全局）
        ///// </summary>
        //[XmlAttribute]
        //public string StoreRefId { get; set; }

        ///// <summary>
        ///// 指定应用到的标识
        ///// </summary>
        //[XmlAttribute]
        //public string ApplyForId { get; set; }

        /// <summary>
        /// 在当前转换过程中数据库连接对象（当数据源为DataReader时）
        /// </summary>
        [XmlIgnore]
        public DbConnection CurrentConnection { get; set; }

        /// <summary>
        /// 存储结果值(运行时)
        /// </summary>
        [XmlIgnore]
        public object StoreResult { get; set; }

        /// <summary>
        /// 存储位置方向（默认为上下文）
        /// </summary>
        [XmlAttribute]
        public StoreDirection Direction { get; set; }

        /// <summary>
        /// 当前数据存储是否分拆存储
        /// </summary>
        bool splitStorage = false;

        /// <summary>
        /// 拆分存储类型列表
        /// </summary>
        string splitDataTypeConfig = "Dictionary<string,object>; NameValueCollection";

        /// <summary>
        /// 分拆存储的键名称
        /// </summary>
        string storageKeys = "*";
        /// <summary>
        /// 映射键值
        /// </summary>
        string mappingKeys = "";

        bool ArrayContains(string[] array, string item, ref int idx)
        {
            idx = Array.FindIndex<string>(array, E => E.Equals(item, StringComparison.InvariantCultureIgnoreCase));
            return idx > -1;
        }

        /// <summary>
        /// 在当前作用域下运行
        /// </summary>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            Type dataType = StoreResult.GetType();
            if (Direction == StoreDirection.Context)
            {
                if (!splitStorage)
                {
                    scope.SetLastExecResult(StoreResult);
                }
                else
                {
                    string[] mappingKeyArray = mappingKeys.AsStringArray();
                    int mIdx = -1;

                    if (splitStorage &&
                        Array.Exists<string>(splitDataTypeConfig.AsStringArray(';'),
                        T => T.Equals(dataType.ToSimpleType(), StringComparison.InvariantCultureIgnoreCase))
                        )
                    {

                        string[] storeKeyArray = storageKeys.AsStringArray();

                        #region 分拆存储到上下文中
                        if (dataType.Equals(typeof(NameValueCollection)))
                        {
                            NameValueCollection nv = (NameValueCollection)StoreResult;
                            foreach (string item in nv.AllKeys)
                            {
                                if (storageKeys == "*"
                                    || ArrayContains(storeKeyArray, item, ref mIdx))
                                {
                                    if (mIdx > -1 && mIdx < mappingKeyArray.Length)
                                    {
                                        scope[mappingKeyArray[mIdx]] = nv[item];
                                    }
                                    else
                                    {
                                        scope[item] = nv[item];
                                    }
                                }
                            }
                        }
                        else if (dataType.Equals(typeof(Dictionary<string, object>)))
                        {
                            Dictionary<string, object> resultDict = (Dictionary<string, object>)StoreResult;
                            foreach (var item in resultDict)
                            {
                                if (storageKeys == "*"
                                    || ArrayContains(storeKeyArray, item.Key, ref mIdx))
                                {
                                    if (mIdx > -1 && mIdx < mappingKeyArray.Length)
                                    {
                                        scope[mappingKeyArray[mIdx]] = item.Value;
                                    }
                                    else
                                    {
                                        scope[item.Key] = item.Value;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        //Status = ExecuteStatus.Exception;
                        lasException = new System.Configuration.ConfigurationErrorsException(string.Format("数据类型{0}不在支持拆分数据类型列表[{1}]中。",
                            dataType.ToSimpleType(), splitDataTypeConfig));

                        return;
                    }
                }
            }
            else
            {
                if (Direction == StoreDirection.SqlDataBase)
                {

                }
            }

        }
    }

    /// <summary>
    /// 存储方向（位置）
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum StoreDirection
    {
        /// <summary>
        /// 上下文存储
        /// </summary>
        Context = 0,
        /// <summary>
        /// SQL数据库
        /// </summary>
        SqlDataBase = 1,
        /// <summary>
        /// 文件存储
        /// </summary>
        FileStore,
        /// <summary>
        /// HTTP请求发送
        /// </summary>
        HttpRequest
    }
}
