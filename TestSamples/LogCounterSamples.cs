#if TEST
using System;
using System.Data;
using System.Xml;
//using CommonLib;
using NetTask.Core;
using NetTask.Data;
using NetTask.Interface;
using NetTask.TaskImplement;

namespace NetTask.TestSamples
{
    public class LogCounterSamples
    {

        SqlNetTask GetLogCounter()
        {
            /*
select top 20 * from 访问分析 where 业务编号=17 order by 统计编号 desc
--delete from 访问分析 where 统计编号 >= 9936
 select top 20 * from 搜索统计 order by 统计编号 desc
--delete from 搜索统计 where 统计编号>=51
             */
            SqlNetTask sTask = new SqlNetTask
            {
                DbType = SqlDatabaseType.SQL2005,
                InnerKeyOrConnectionString = "DefaultDB",

                ExecSql = new SqlScript[] { 
                    new SqlScript { 
                        QueryType = SqlQueryType.Select, 
                        ResultType = SqlResultType.DataReader,

                         SqlParams = new SqlParameter[]{
                                new SqlParameter { DataType= DbType.Int64, Name = "业务编号", Value="17"  }
                            },

                        Text = @"declare @lastID bigint
select @lastID=Convert(bigint, (select top 1 分析标识 from [统计标记] 
where 任务标识 = 'SourceLogReader' + Convert(varchar(10), @业务编号)))

SELECT TOP 10 PV.[ID], 'N/A' as UCate, PV.[SOFTWARE_ID], PV.[SERVICE_ID], U.Device_ID, MB.M_Name, U.MSID, 
PV.[VISIT_TIME], U.First_Visit_Time, PV.[KEY_URL],PV.[REAL_URL] 
 FROM [LOG_PV] PV
	left join [SOFTWARE] U on U.SoftWare_ID=PV.SoftWare_ID
	inner join [GwsoftEase].[dbo].[GW_Device_Mobile] MB on MB.M_ID = U.Device_ID 
where PV.[SERVICE_ID]=@业务编号 and PV.id > @lastID order by PV.ID asc" 
                    }
                }
            };

            //设置接收器
            sTask.Receiver = new DataReceiverBridge { ImplementMode = SequentialMode.Instance };

            #region 添加DataReader处理
            DataReaderReceiver r = new DataReaderReceiver
            {
                Storage = new ClrDataStorage
                {
                    Direction = StoreDirection.Context,
                    Config = new TaskConfig
                        {
                            Items = new ConfigItem[] 
                                {
                                    new ConfigItem { Key="SplitStorage", Value = "true", RefType = "Dictionary<string,object>; NameValueCollection" },
                                    new ConfigItem { Key="StorageKeys", Value = "ID,SERVICE_ID,VISIT_TIME,UCate,M_Name,SOFTWARE_ID,MSID,REAL_URL",
                                        RefType = "日志编号,业务编号,访问时间,用户分类,手机型号" }
                                }
                        }
                }
            };
            sTask.Receiver.AddSubTask(r);
            #endregion

            #region 添加解析任务
            UrlParseTask u = new UrlParseTask
            {
                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[] 
                        { 
                            new ContextArgument { ContextName = "REAL_URL", ExchangeFlag = ArgumentExchange.Duplex, DelayClear=true,
                                Type = "string", Name = "NetTask.TaskImplement.UrlParseTask::URL" }
                        }
                    },
                    Items = new ConfigItem[] 
                    {
                        new ConfigItem { Key = "ResultItems", Value = "栏目|书籍|下载|订购|搜索" },
                        new ConfigItem { Key = "Pattern_栏目", Value = "{1}:/booklist\\.aspx\\?t=([^&]+)" },
                        new ConfigItem { Key = "Pattern_书籍", Value = "{1}:[?&]dtd=(\\d+)" },
                        new ConfigItem { Key = "Pattern_下载", Value = "{1}:/bookdown\\.aspx\\?dtd=(\\d+)" },
                        new ConfigItem { Key = "Pattern_订购", Value = "{1}:/Fee/success\\.aspx\\?resid=([^&]+)" },
                        new ConfigItem { Key = "Pattern_搜索", Value = "&keyword=([^&]+)" }
                    }
                }
            };
            sTask.Receiver.AddSubTask(u);
            #endregion

            #region 添加存储任务
            SqlNetTask s = new SqlNetTask
            {
                DbType = SqlDatabaseType.SQL2005,
                InnerKeyOrConnectionString = "DefaultDB",
                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[] { 
                        new ContextArgument { Name = "日志编号", Type = "long", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true },
                        new ContextArgument { Name = "业务编号", Type = "long", ExchangeFlag = ArgumentExchange.Duplex , DelayClear = true },
                        new ContextArgument { Name = "访问时间", Type = "long", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true  },
                        new ContextArgument { Name = "用户分类", Type = "string", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true  },
                        new ContextArgument { Name = "手机型号", Type = "string", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true  },

                        new ContextArgument { Name = "访问栏目", ContextName = "栏目", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "书籍标识", ContextName = "书籍", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "下载标识", ContextName = "下载", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "订购标识", ContextName = "订购", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "是否搜索", ContextName = "搜索", Type = "bool", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true  }
                    }
                    }
                },

                ExecSql = new SqlScript[] {
                    new SqlScript { 
                    QueryType = SqlQueryType.Insert, 
                    ResultType = SqlResultType.NoValue,
                    Disabled = true, //调试禁止写入
                    SqlParams = new SqlParameter[]{
                        new SqlParameter { DataType= DbType.Int64, Name = "日志编号" },
                        new SqlParameter { DataType= DbType.Int64, Name = "业务编号" },
                        new SqlParameter { DataType= DbType.Int64, Name = "访问时间" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "用户分类" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "手机型号" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "访问栏目" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "书籍标识" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "下载标识" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "订购标识" },
                        new SqlParameter { DataType= DbType.Boolean, Name = "是否搜索" } 
                    },
                    Text = @"insert into [访问分析]([日志编号],[业务编号],[访问时间],[用户分类], [手机型号], [访问栏目], [书籍标识], [下载标识], [订购标识], [是否搜索])
                        values(@日志编号, @业务编号, @访问时间, @用户分类, @手机型号, @访问栏目, @书籍标识, @下载标识, @订购标识, @是否搜索 )"
                    }
                }

            };
            sTask.Receiver.AddSubTask(s);
            #endregion

            #region 解析关键子到数据库[STOK]
            ConditionalConfig confCfg = new ConditionalConfig();
            confCfg.Executable = true;
            confCfg.AddItem(new ScopeItemCond { Key = "搜索" });
            TaskContainer kwt = new TaskContainer();
            //执行解析
            KeywordParseTask kpt = new KeywordParseTask
            {
                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[]  {
                            new ContextArgument { Name = "RawURL", ContextName = "REAL_URL", Type="string", Optional=false }
                        }
                    },
                    Items = new ConfigItem[] {
                        new ConfigItem { Key = "KeywordPattern", Value = "&keyword=([^&]+)", RefType = "string" },
                        new ConfigItem { Key = "Adapter", Value = "SimpleKeywordHandler",
                            RefType = typeof(KeywordParseTask).GetNoVersionTypeName() }
                    }
                }
            };

            kwt.Conditional = confCfg;
            kwt.AddSubTask(kpt);

            //执行存储
            SqlNetTask kwps = new SqlNetTask
            {
                DbType = SqlDatabaseType.SQL2005,
                InnerKeyOrConnectionString = "DefaultDB",
                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[] { 
                        new ContextArgument { Name = "日志编号", Type = "long", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true },
                        new ContextArgument { Name = "业务编号", Type = "long", ExchangeFlag = ArgumentExchange.Duplex, DelayClear = true  },
                        new ContextArgument { Name = "用户标识",  ContextName="SOFTWARE_ID",Type = "long", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "搜索时间", ContextName="访问时间", Type = "long", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "用户分类", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },

                        new ContextArgument { Name = "手机型号", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "手机号码", ContextName="MSID", Type = "string", ExchangeFlag = ArgumentExchange.Duplex },
                        new ContextArgument { Name = "关键字", Type = "string", ExchangeFlag = ArgumentExchange.Duplex, LastResult=true }
                    }
                    }
                },
                ExecSql = new SqlScript[] {
                    new SqlScript { 
                    QueryType = SqlQueryType.Insert,  ResultType = SqlResultType.NoValue,
                    Disabled = true, //调试禁止写入
                    SqlParams = new SqlParameter[]{
                        new SqlParameter { DataType= DbType.Int64, Name = "日志编号" },
                        new SqlParameter { DataType= DbType.Int64, Name = "业务编号" },
                        new SqlParameter { DataType= DbType.Int64, Name = "用户标识" },
                        new SqlParameter { DataType= DbType.Int64, Name = "搜索时间" },

                        new SqlParameter { DataType= DbType.AnsiString, Name = "用户分类" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "手机型号" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "手机号码" },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "关键字" } 
                    },
                    Text = @"insert into [搜索统计]([日志编号],[业务编号],[用户标识],[搜索时间],[用户分类],[手机型号],[手机号码],[关键字]) 
                        values(@日志编号,@业务编号,@用户标识,@搜索时间,@用户分类,@手机型号,@手机号码,@关键字)"
                    }
                }
            };
            kwt.AddSubTask(kwps);
            sTask.Receiver.AddSubTask(kwt);
            #endregion

            #region 写入执行点
            SqlNetTask m = new SqlNetTask
            {
                DbType = SqlDatabaseType.SQL2005,
                InnerKeyOrConnectionString = "DefaultDB",
                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[] { 
                        new ContextArgument { ContextName="日志编号", Name = "分析标识", Type = "long", ExchangeFlag = ArgumentExchange.Duplex}
                    }
                    }
                },

                ExecSql = new SqlScript[] {
                    new SqlScript { 
                    QueryType = SqlQueryType.Insert, 
                    ResultType = SqlResultType.NoValue,
                    Disabled = false, //调试禁止写入
                    SqlParams = new SqlParameter[]{
                        new SqlParameter { DataType= DbType.AnsiString, Name = "任务标识", Value="SourceLogReader17"  },
                        new SqlParameter { DataType= DbType.AnsiString, Name = "分析标识"} 
                    },
                    Text = @"  if exists(select top 1 * from [统计标记] where [任务标识] = @任务标识)
	Update [统计标记] set  [结束时间] = getdate(), [分析标识]= @分析标识, [执行状态]='OK'
             where [任务标识] = @任务标识
  else
    Insert into [统计标记]([任务标识] ,[开始时间] ,[结束时间] ,[分析标识] ,[执行状态]) 
             values(@任务标识, getdate(), getdate(), @分析标识, 'OK')"
                    }
                }

            };
            sTask.Receiver.AddSubTask(m);
            #endregion

            #region 写入进度
            SqlNetTask p = new SqlNetTask
            {
                DbType = SqlDatabaseType.SQL2005,
                InnerKeyOrConnectionString = "DefaultDB",

                Config = new TaskConfig
                {
                    ArgumentConfig = new StepArgument
                    {
                        Arguments = new ContextArgument[] { 
                        new ContextArgument { Name = "业务编号", Type = "long", ExchangeFlag = ArgumentExchange.Duplex}
                    }
                    }
                },

                ExecSql = new SqlScript[] {
                    new SqlScript { 
                    QueryType = SqlQueryType.Insert, 
                    ResultType = SqlResultType.NoValue,
                    Disabled = false, //调试禁止写入
                    SqlParams = new SqlParameter[]{
                        new SqlParameter { DataType= DbType.AnsiString, Name = "任务标识", Value="SP-17" },
                        new SqlParameter { DataType= DbType.Int64, Name = "业务编号" } 
                    },
                    Text = @"
declare @total decimal, @current decimal
declare @分析标识 nvarchar(200), @执行状态 nvarchar(200)

select @total=(SELECT count(*) FROM [LOG_PV] where [SERVICE_ID]=@业务编号),
@current=(SELECT count(*) FROM [访问分析] where 业务编号=@业务编号)

select @分析标识 = Convert(varchar(20), @current) + N'/' + Convert(varchar(20), @total),
@执行状态 = Convert(varchar(20), Convert(decimal(6,4),@current/@total)*100) + '%'

  if exists(select top 1 * from [统计标记] where [任务标识] = @任务标识)
	Update [统计标记] set  [结束时间] = getdate(), [分析标识]= @分析标识, [执行状态]=@执行状态
             where [任务标识] = @任务标识
  else
    Insert into [统计标记]([任务标识] ,[开始时间] ,[结束时间] ,[分析标识] ,[执行状态]) 
             values(@任务标识, getdate(), getdate(), @分析标识, @执行状态)"
                    }
                }

            };
            sTask.Receiver.AddSubTask(p);
            #endregion

            return sTask;
        }

        /// <summary>
        /// 主要测试
        /// </summary>
        public void LogCounterTest()
        {
            NetTaskScope context = NetTaskScope.Context;

            SqlNetTask sTask = GetLogCounter();
            sTask.GetXmlDoc(true).WriteIndentedContent(Console.Out);
            //sTask.RunInScope(context);

            Console.WriteLine(sTask.Status);
            if (sTask.Status == StepStatus.Exception)
            {
                Console.WriteLine(sTask.GetLastException());
            }

            Console.WriteLine(context.Path.GetTraceList(Environment.NewLine));
        }

        public void CircleContainerTaskTest()
        {
            NetTaskScope context = NetTaskScope.Context;

            CircleContainerTask cs = new CircleContainerTask()
            {
                BeCircleMode = CircleMode.While,
                CircleActionDelegate = "aaa",
                CircleInitialActionDelegate = "bbb",
                CircleIncrementActionDelegate = "ccc",
            };
            cs.AddSubTask(GetLogCounter());

            //cs.RunInScope(context);
            cs.GetXmlDoc(true).WriteIndentedContent(Console.Out);
        }


        public void ComplexTaskRead()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "\\test.ntml");

            TaskContainer t = xDoc.GetObject<TaskContainer>();
            Console.WriteLine(t);
        }

        public void TaskContainerTest()
        {
            NetTaskScope context = NetTaskScope.Context;

            TaskContainer t = new TaskContainer();
            ScopeSetTask set = new ScopeSetTask
            {
                Config = TaskConfig.CreateWithArgument(ContextArgument.CreateTransferArgument("test", "ok"))
            };

            ThreadSleepTask w = new ThreadSleepTask { WaitMilliseconds = "50ms" };

            w.GetXmlDoc(true).WriteIndentedContent(Console.Out);

            t.AddSubTask(w);
            t.AddSubTask(set);
            //
            t.GetXmlDoc(true).WriteIndentedContent(Console.Out);
            t.RunInScope(context);

            System.Diagnostics.Debug.Assert(context["test"].Equals("ok"));

        }

        public void UrlMappingTaskTest()
        {
            UrlMappingTask ufile = new UrlMappingTask { TaskUrl = @"D:\DevRoot\同步监控统计平台\Common\NetTask\bin\Debug\bookCounter.ntml" };
            ufile.GetXmlDoc(true).WriteIndentedContent(Console.Out);
        }

    }


    public class sEntityTest
    {
        public string[] testArray { get; set; }

        public void testXmlSeriliaze()
        {
            sEntityTest s = new sEntityTest { testArray = new string[] { "1", "2", "3" } };

            s.GetXmlDoc(true).WriteIndentedContent(Console.Out);
        }
    }

}
#endif