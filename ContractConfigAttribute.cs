using System;
using NetTask.Core;

namespace NetTask.Interface
{
    /// <summary>
    /// 约束配置
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    [AttributeUsage(AttributeTargets.All)]
    public class ContractConfigAttribute : Attribute
    {
        /// <summary>
        /// 初始化一个 <see cref="ContractConfigAttribute"/> class 实例。
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="contractType">Type of the contract.</param>
        public ContractConfigAttribute(IpcContract category, Type contractType)
        {
            _verifyType = contractType;
            _category = category;
        }

        Type _verifyType = typeof(void);
        IpcContract _category = IpcContract.ClrData;

        /// <summary>
        /// 验证范围
        /// </summary>
        public IpcContract Category { get { return _category; } }

        /// <summary>
        /// 验证CLR强类型
        /// </summary>
        public Type VerifyType { get { return _verifyType; } }
    }
}
