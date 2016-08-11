#if TEST
using System;
using System.Diagnostics;
using NetTask.Core;
using NetTask.TaskImplement;

namespace NetTask.TestSamples
{
    class UnitTest
    {

        public void TestTemp()
        {
            /*
<?xml version="1.0" encoding="utf-8"?>
<ExecWithTimeoutTask Disabled="false">
  <Config>
    <ArgumentConfig>
      <arg Name="IsTimeOut" Type="bool" Optional="false" LastResult="false" ExchangeFlag="Duplex" DelayClear="true" />
      <arg Name="Arguments" Type="string" Optional="false" LastResult="false" ExchangeFlag="Duplex" DelayClear="false" />
      <arg Name="TimeoutSeconds" Type="int" Value="20" Optional="false" LastResult="false" ExchangeFlag="Duplex" DelayClear="false" />
      <arg Name="Output" Type="string" Optional="false" LastResult="false" ExchangeFlag="Duplex" DelayClear="true" />
    </ArgumentConfig>
    <Items>
      <ConfigItem Key="StaticmethodOrFilePath" Value="c:\a.bat" RefType="string" />
    </Items>
  </Config>
</ExecWithTimeoutTask>
             */

            ExecWithTimeoutTask ext = new ExecWithTimeoutTask();
            ext.Config = new TaskConfig
            {
                Items = new ConfigItem[] {
                    new ConfigItem { Key = "StaticmethodOrFilePath", Value = @"CabArchive.exe", RefType = "string" }
                },
                ArgumentConfig = new StepArgument
                {
                    Arguments = new ContextArgument[] {
                        new ContextArgument { Name = "IsTimeOut", DelayClear = true, Type = "bool", Optional = false },
                        new ContextArgument { Name = "Arguments", Type = "string", Optional = true },
                        new ContextArgument { Name = "TimeoutSeconds", Type = "int", Optional = true, Value = "20" },
                        new ContextArgument { Name = "Output", DelayClear = true, Type = "string", Optional = false, LastResult = true },
                    }
                }
            };

            //ext.GetXmlDoc(true).WriteIndentedContent(Console.Out);

            ext.RunInScope(NetTaskScope.Context);
        }


        public void processTest()
        {
            using (Process proc = new Process())
            {

                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = "cmd.exe";
                pi.UseShellExecute = false;
                pi.RedirectStandardInput = true;
                pi.RedirectStandardOutput = true;
                pi.RedirectStandardError = true;
                pi.CreateNoWindow = true;
                proc.StartInfo = pi;
                proc.Start();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                proc.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    sb.Append(e.Data);

                });

                proc.StandardInput.WriteLine("dir");
                proc.StandardInput.WriteLine("exit");

                string result = proc.StandardOutput.ReadToEnd().Replace("\r", "");

                Console.WriteLine(sb.ToString());
                proc.Close();

                Console.WriteLine(result);
            }
        }
    }
}
#endif