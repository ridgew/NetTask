using System.Collections.Generic;
using System.Data.Common;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// 通用操作辅助
    /// </summary>
    public static class GeneralUtil
    {
        /// <summary>
        /// 读取数据为词典
        /// </summary>
        public static Dictionary<string, object> GetReaderAsDictionary(DbDataReader reader)
        {
            Dictionary<string, object> objDict = new Dictionary<string, object>();
            int total = reader.FieldCount;
            for (int i = 0; i < total; i++)
            {
                objDict.Add(reader.GetName(i), reader[i]);
            }
            return objDict;
        }


        ///// <summary>
        ///// [TODO]转换字段映射的值为其他值词典
        ///// </summary>
        ///// <param name="conn">当前数据库连接对象</param>
        ///// <param name="reader">主要数据只读器</param>
        ///// <param name="config">当前映射配置</param>
        //public static Dictionary<string, object> ConvertDbFieldAsDictionary(DbConnection conn, DbDataReader reader, FieldConvertConfig config)
        //{
        //    Dictionary<string, object> objDict = GetReaderAsDictionary(reader);
        //    foreach (FieldMapping map in config.Mapping)
        //    {
        //        switch (map.Type)
        //        {
        //            case ExpressType.FieldExpression:
        //                if (objDict.ContainsKey(map.FieldName))
        //                {

        //                }
        //                break;
        //            case ExpressType.Define:
        //                break;
        //            case ExpressType.SqlParamQuery:
        //                break;
        //            case ExpressType.InvokeFunction:
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    return objDict;
        //}

    }

    /// <summary>
    /// 转换数据只读器为通用数据词典
    /// </summary>
    [APIExport]
    public delegate Dictionary<string, object> ReaderAsDictionary(DbDataReader reader);

}
