using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// 数据阅读器接收
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "drec", Description = "接收DataReader对象的数据接收器"), Serializable]
    public class DataReaderReceiver : XmlDefineTask, IDataReceiver
    {
        /// <summary>
        /// 是否能够接收当前数据
        /// </summary>
        /// <param name="objData">当前传输的数据</param>
        /// <returns></returns>
        public bool Accept(object objData)
        {
            return objData != null && objData.GetType().IsSubclassOf(typeof(System.Data.Common.DbDataReader));
        }

        /// <summary>
        /// 数据存储器
        /// </summary>
        /// <value></value>
        [Description("存储数据的存储器"), DisplayName("数据存储器")]
        public ClrDataStorage Storage { get; set; }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (Storage != null)
            {
                object lastExecResult = scope.GetLastExecResult();
                if (lastExecResult != null)
                {
                    DbDataReader r = lastExecResult as DbDataReader;
                    if (r != null)
                    {
                        lastExecResult = GetReaderAsDictionary(r);
                        scope.SetLastExecResult(lastExecResult);

                        Storage.StoreResult = lastExecResult;
                        Storage.RunInScope(scope);
                    }
                }
            }
        }

        /// <summary>
        /// 读取数据为词典
        /// </summary>
        public Dictionary<string, object> GetReaderAsDictionary(DbDataReader reader)
        {
            Dictionary<string, object> objDict = new Dictionary<string, object>();
            int total = reader.FieldCount;
            for (int i = 0; i < total; i++)
            {
                objDict.Add(reader.GetName(i), reader[i]);
            }
            return objDict;
        }

    }
}
