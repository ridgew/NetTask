using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTask.Core;
using System.IO;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 文件系统操作任务
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "fileio", Description = "文件系统操作任务")]
    public class FileIOTask : XmlDefineTask
    {
        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            File.Move("", "");
            File.Delete("");
            File.Copy("", "", true);
        }
    }
}
