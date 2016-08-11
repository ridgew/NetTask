#if TEST
using System;
using System.Diagnostics;
using NetTask.Core;
using NetTask.Interface;
//using CommonLib;
using NetTask.TaskImplement;

namespace NetTask.TestSamples
{
    public class CircleTaskTest
    {

        public void TempLineTest()
        {
            Console.WriteLine(int.MaxValue);
        }

        public void CondCompareTest()
        {
            //NetTask.Core.NetTaskScope:LastResult
            NetTaskScope context = NetTaskScope.Context;
            context["test"] = 0;

            CircleContainerTask cs = new CircleContainerTask
            {
                BeCircleMode = CircleMode.StepIncrement,
                CircleActionDelegate = typeof(SimpleCounterTask).GetMethod("StaticCounterTest").ToDelegateString()
            };

            ConditionalConfig confCfg = new ConditionalConfig { Executable = true };
            //confCfg.AddItem(new ScopeItemCond { Key = "搜索", Logic = LogicExpression.None });
            confCfg.AddItem(new ScopeItemCompare<int> { Key = "test", Logic = LogicExpression.None, Value = 5, Mode = CompareMode.NotEqual });

            cs.Conditional = confCfg;

            /*
<?xml version="1.0" encoding="utf-8"?>
<CircleContainerTask ImplementMode="Instance" BeCircleMode="While" CircleActionDelegate="NetTask.TestSamples.SimpleCounterTask::StaticCounterTest, NetTask">
  <ConditionalConfig Executable="True">
    <ScopeItemCompareOfInt32 Logic="None" Key="test" Mode="Lower" Value="5" type="NetTask.Core.ScopeItemCompare`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], NetTask.Interface" />
 </ConditionalConfig>
             */

            //cs.GetXmlDoc(true).WriteIndentedContent(Console.Out);
            cs.RunInScope(context);

        }

        public void CondMathCompareTest()
        {
            //NetTask.Core.NetTaskScope:LastResult
            NetTaskScope context = NetTaskScope.Context;
            context["test"] = 0;

            CircleContainerTask cs = new CircleContainerTask
            {
                BeCircleMode = CircleMode.DoWhile
                //,CircleActionDelegate = typeof(SimpleCounterTask).GetMethod("StaticCounterTest").ToDelegateString()
            };

            SimpleCounterTask sc = new SimpleCounterTask();
            cs.AddSubTask(sc);

            ConditionalConfig confCfg = new ConditionalConfig { Executable = true };
            //confCfg.AddItem(new ScopeItemMatch<int>
            //{
            //    Key = "test",
            //    RefreshScope = true,
            //    Logic = LogicExpression.None,
            //    Mode = CompareMode.NotEqual,
            //    Operation = MathOperation.Add,
            //    DesiredValue = 10,
            //    RightValue = "5"
            //});

            confCfg.AddItem(ScopeItemMatch<float>.CreateStepCounterInScope(context, "test", 0f, "1.5", 5.0f, CompareMode.Lower, MathOperation.SelfAdd));
            /*
             <?xml version="1.0" encoding="utf-8"?>
                <CircleContainerTask ImplementMode="Instance" BeCircleMode="DoWhile">
                  <ConditionalConfig Executable="True">
                    <ScopeItemMatchOfSingle Key="test" Operation="SelfAdd" RefreshScope="true" Mode="Lower" DesiredValue="5" RightValue="1.5" Logic="None" type="NetTask.Core.ScopeItemMatch&lt;float&gt;, NetTask.Interface" />
                  </ConditionalConfig>
                  <SimpleCounterTask Disabled="false" type="NetTask.TestSamples.SimpleCounterTask, NetTask" />
              </CircleContainerTask>
             */

            cs.Conditional = confCfg;

            /*
 <?xml version="1.0" encoding="utf-8"?>
<CircleContainerTask ImplementMode="Instance" BeCircleMode="StepIncrement" CircleActionDelegate="NetTask.TestSamples.SimpleCounterTask::StaticCounterTest, NetTask">
  <ConditionalConfig Executable="True">
    <ScopeItemMatchOfInt32 Key="test" Operation="Add" Mode="NotEqual" DesiredValue="10" RightValue="5" Logic="None" type="NetTask.Core.ScopeItemMatch&lt;int&gt;, NetTask.Interface" />
  </ConditionalConfig>
             */
            cs.GetXmlDoc(true).WriteIndentedContent(Console.Out);
            cs.RunInScope(context);

        }

        public void MustEqualCircleTest()
        {
            CircleContainerTask cs = new CircleContainerTask
            {
                BeCircleMode = CircleMode.StepIncrement,
                CircleActionDelegate = typeof(SimpleCounterTask).GetMethod("StaticCounterTest").ToDelegateString()
            };

            ConditionalConfig confCfg = new ConditionalConfig { Executable = true };
            confCfg.AddItem(new ScopeItemCompare<string> { Key = "NetTask.Core.NetTaskScope:LastResult", Logic = LogicExpression.None, Value = "100%", Mode = CompareMode.NotEqual });
            cs.Conditional = confCfg;
            cs.GetXmlDoc(true).WriteIndentedContent(Console.Out);
        }

        public void GetTypeTest()
        {
            Type t = Type.GetType("NetTask.Core.ScopeItemCompare`1, NetTask.Interface");
            t = t.MakeGenericType(typeof(int));

            Type t2 = TypeCache.GetRuntimeType("NetTask.Core.ScopeItemCompare<int>, NetTask.Interface");
            Debug.Assert(t.Equals(t2));

        }


        public void UrlMappingTaskTest()
        {
            NetTaskScope context = NetTaskScope.Context;

            UrlMappingTask t = new UrlMappingTask { TaskUrl = "file:///D:/DevRoot/同步监控统计平台/Common/NetTask/bin/Debug/bookcounter12n.ntml" };
            t.RunInScope(context);
        }

        public void EnumeratorCircleTaskTest()
        {
            CircleContainerTask ct = new CircleContainerTask()
            {
                Config = new TaskConfig
                {
                    Items = new ConfigItem[] {
                        new ConfigItem { Key = "p1", Value = "12", RefType = "int" },
                        new ConfigItem { Key = "p2", Value = "15", RefType = "int" },
                        new ConfigItem { Key = "p3", Value = "22", RefType = "int" }
                    }
                },
                BeCircleMode = CircleMode.Enumerator
            };

            ct.AddSubTask(new SimpleCounterTask());
            ct.AddSubTask(ScopeDumpTask.CreateConsoleDumpTask());

            /* 
    <TaskConfig>
    <ArgumentConfig />
    <Items>
      <ConfigItem Key="p1" Value="12" RefType="int" />
      <ConfigItem Key="p2" Value="15" RefType="int" />
      <ConfigItem Key="p3" Value="22" RefType="int" />
    </Items>
  </TaskConfig>*/
            //ct.GetXmlDoc(true).WriteIndentedContent(Console.Out);

            NetTaskScope sc = new NetTaskScope();
            ct.RunInScope(sc);


        }

    }

    public class SimpleCounterTask : XmlDefineTask
    {

        string scopeKey = "test";

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (!scope.ContainsKey(scopeKey))
            {
                scope[scopeKey] = 0;
            }

            Debug.WriteLine(scope[scopeKey]);
            //System.Threading.Thread.Sleep(2000);
        }

        public static void StaticCounterTest(NetTaskScope scope, INetTask currentStep)
        {
            if (!scope.ContainsKey("test"))
            {
                scope["test"] = 1;
            }
            else
            {
                scope["test"] = (int)scope["test"] + 1;
            }

            Console.WriteLine("第{0}次运行..", scope["test"]);
        }
    }
}
#endif