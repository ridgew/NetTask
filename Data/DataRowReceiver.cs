using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// 数据行接收器
    /// </summary>
    public class DataRowReceiver : IDataReceiver
    {

        /// <summary>
        /// 是否能够接收当前数据
        /// </summary>
        /// <param name="objData">当前传输的数据</param>
        /// <returns></returns>
        public bool Accept(object objData)
        {
            return objData != null && objData.GetType().Equals(typeof(DataRow));
        }

    }
}
