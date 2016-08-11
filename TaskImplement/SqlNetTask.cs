using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NetTask.Core;
using NetTask.Data;
using NetTask.Interface;
using System.Linq;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// SQL运行任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "sqlr", Description = "SQL数据库运行任务"), Serializable]
    [AcceptExtension(".ntml", ".ntSql")]
    public class SqlNetTask : XmlDefineTask
    {

        /// <summary>
        /// 初始化 <see cref="SqlNetTask"/> class.
        /// </summary>
        public SqlNetTask()
            : base()
        {
            #region 设置需要配置的项
            requreItemList.Add(new RequriedConfigItem
            {
                Key = "InnerKeyOrConnectionString",
                Optional = false,
                Description = "宿主内数据连接字符串键值或者指定连接字符串",
                Contract = IpcContract.ClrData,
                VerifyBy = typeof(string).FullName
            });
            #endregion
        }

        /// <summary>
        /// 宿主内数据连接字符串键值或者指定连接字符串
        /// </summary>
        [XmlAttribute]
        [DisplayName("连接字符串"), Description("宿主内数据连接字符串键值或者指定连接字符串")]
        public string InnerKeyOrConnectionString { get; set; }

        SqlDatabaseType dbType = SqlDatabaseType.SQL2005;
        /// <summary>
        /// 数据库类型
        /// </summary>
        [XmlAttribute]
        [DisplayName("数据库类型"), Description("任务运行的数据库类型")]
        public SqlDatabaseType DbType
        {
            get { return dbType; }
            set { dbType = value; }
        }

        SqlScript[] sqls = new SqlScript[0];
        /// <summary>
        /// 相关运行脚本
        /// </summary>
        [XmlArrayItem(ElementName = "sql")]
        [DisplayName("SQL脚本集"), Description("依次执行的SQL数据脚本")]
        public SqlScript[] ExecSql
        {
            get { return sqls; }
            set { sqls = value; }
        }

        /// <summary>
        /// 数据接收器
        /// </summary>
        [Description("接收改任务运行结果"), DisplayName("数据接收器")]
        public DataReceiverBridge Receiver { get; set; }

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            DbProviderFactory factory = System.Data.SqlClient.SqlClientFactory.Instance;
            if ((int)DbType < 20) factory = System.Data.OleDb.OleDbFactory.Instance;

            using (DbConnection conn = factory.CreateConnection())
            {
                try
                {
                    conn.ConnectionString = (InnerKeyOrConnectionString.IndexOf(';') != -1) ? InnerKeyOrConnectionString
                        : ConfigurationManager.ConnectionStrings[InnerKeyOrConnectionString].ConnectionString;
                }
                catch (Exception connEx)
                {
                    Status = StepStatus.Exception;
                    lasException = new System.Configuration.ConfigurationErrorsException("数据连接字符串" + InnerKeyOrConnectionString + "配置错误!", connEx);
                    return;
                }

                try
                {
                    conn.Open();
                    foreach (SqlScript sql in ExecSql)
                    {
                        if (!sql.Disabled)
                        {
                            execDbCommandInConnection(scope, factory, sql, conn);
                        }
                    }
                    conn.Close();
                    conn.Dispose();
                    Status = StepStatus.Finished;
                }
                catch (Exception ex)
                {
                    Status = StepStatus.Exception;
                    lasException = ex;
                }
            }
        }

        void execDbCommandInConnection(NetTaskScope scope, DbProviderFactory factory, SqlScript sql, DbConnection conn)
        {
            DbCommand cmd = factory.CreateCommand();
            cmd.Connection = conn;

            bool doApplyArgument = (Config != null && !Config.ArgumentConfig.IsEmpty());

            #region 处理参数
            foreach (SqlParameter p in sql.SqlParams)
            {
                //如果应用上下文有值，则设置参数的值
                if (doApplyArgument)
                {
                    ContextArgument arg = Config.ArgumentConfig.Arguments.FirstOrDefault<ContextArgument>(t => t.Name == p.Name);
                    if (arg != null)
                    {
                        p.RuntimeValue = arg.RuntimeValue;
                    }
                }
                p.BindWithDbCommand(cmd, scope);
            }
            #endregion

            cmd.CommandType = (sql.QueryType == SqlQueryType.StoredProcedure) ? CommandType.StoredProcedure : CommandType.Text;

            if (sql.ResultType == SqlResultType.NoValue)
            {
                if (sql.QueryType != SqlQueryType.MultiQuery)
                {
                    cmd.CommandText = sql.Text;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    using (StringReader sr = new StringReader(sql.Text))
                    {
                        #region 分段运行
                        StringBuilder sb = new StringBuilder();
                        string lineStr = null;
                        int sqlLen = 0;

                        while ((lineStr = sr.ReadLine()) != null)
                        {
                            if (lineStr.Trim().Equals("GO", StringComparison.InvariantCultureIgnoreCase))
                            {
                                cmd.CommandText = sb.ToString();
                                sqlLen = cmd.CommandText.Length;
                                cmd.ExecuteNonQuery();
                                sb.Remove(0, sqlLen);
                            }
                            else
                            {
                                sb.AppendLine(lineStr);
                            }
                        }

                        lineStr = sb.ToString().Trim();
                        sqlLen = lineStr.Length;
                        if (sqlLen > 0)
                        {
                            cmd.CommandText = lineStr;
                            cmd.ExecuteNonQuery();
                        }
                        #endregion
                        sr.Close();
                    }
                }
            }
            else
            {
                object objResult = null;
                cmd.CommandText = sql.Text;

                #region 有返回值需要处理
                if (sql.ResultType == SqlResultType.DataReader)
                {
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {

                    resultProcess:
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                processReceiverData(scope, reader);
                            }
                        }
                        if (reader.NextResult()) goto resultProcess;
                        reader.Close();
                    }
                }
                else
                {
                    if (sql.ResultType == SqlResultType.ScalarValue)
                    {
                        objResult = cmd.ExecuteScalar();
                    }
                    else if (sql.ResultType == SqlResultType.SingleRow || sql.ResultType == SqlResultType.DataTable)
                    {
                        #region 数据表或行
                        DataSet resultSet = new DataSet();
                        using (DbDataAdapter adp = factory.CreateDataAdapter())
                        {
                            adp.SelectCommand = cmd;
                            adp.Fill(resultSet);
                        }

                        if (sql.ResultType == SqlResultType.DataTable)
                        {
                            objResult = resultSet.Tables[0];
                        }
                        else
                        {
                            if (resultSet.Tables[0].Rows != null && resultSet.Tables[0].Rows.Count > 0)
                            {
                                objResult = resultSet.Tables[0].Rows[0];
                            }
                        }
                        #endregion
                    }

                    if (objResult != null)
                    {
                        processReceiverData(scope, objResult);
                    }
                }
                #endregion
            }

        }

        void processReceiverData(NetTaskScope scope, object sendDat)
        {
            scope.SetLastExecResult(sendDat);
            if (Receiver != null && Receiver.Accept(sendDat))
            {
                Receiver.RunInScope(scope);
            }
        }

    }
}
