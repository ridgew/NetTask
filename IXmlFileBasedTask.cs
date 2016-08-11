using System;

namespace NetTask.Interface
{
    /// <summary>
    /// 基于XML文档配置的相关任务
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface IXmlFileBasedTask : INetTask
    {
        /// <summary>
        /// 从特定文件路径绑定任务
        /// </summary>
        /// <param name="fileUrl">xml文件路径</param>
        void BindFromXmlFile(string fileUrl);

    }

}
