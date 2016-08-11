using System;
using System.Data;
using System.Data.Common;
using System.Xml.Serialization;
//using CommonLib;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// SQL数据库脚本参数
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class SqlParameter
    {
        ParameterDirection pDirection = ParameterDirection.Input;
        /// <summary>
        /// 获取或设置一个值，该值指示参数是只可输入、只可输出、双向还是存储过程返回值参数。
        /// </summary>
        [XmlAttribute]
        public ParameterDirection Direction
        {
            get { return pDirection; }
            set { pDirection = value; }
        }

        /// <summary>
        /// 参数名称
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// 运行时值
        /// </summary>
        [XmlIgnore]
        public object RuntimeValue { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        [XmlAttribute]
        public string Value { get; set; }

        /// <summary>
        /// 字符参数值的格式化写入
        /// </summary>
        [XmlAttribute]
        public string Format { get; set; }

        /// <summary>
        /// 获取或设置列中数据的最大大小（以字节为单位）。
        /// </summary>
        [XmlAttribute]
        public int Size { get; set; }

        /// <summary>
        /// 参数数据类型
        /// </summary>
        [XmlAttribute]
        public DbType DataType { get; set; }

        /// <summary>
        /// 绑定到数据库操作命令
        /// </summary>
        /// <param name="cmd">相关命令</param>
        /// <param name="context">当前运行上下文</param>
        public void BindWithDbCommand(DbCommand cmd, NetTaskScope context)
        {
            DbParameter newP = cmd.CreateParameter();
            newP.ParameterName = this.Name;
            newP.Direction = this.Direction;
            newP.DbType = this.DataType;
            if (this.Size > 0)
                newP.Size = this.Size;

            if (RuntimeValue != null)
            {
                if (string.IsNullOrEmpty(Format))
                {
                    newP.Value = RuntimeValue;
                }
                else
                {
                    newP.Value = string.Format(Format, RuntimeValue);
                }
            }
            else
            {
                if (this.Value != null)
                {
                    //处理参数的值
                    object targetVal = this.Value;
                    #region 转参数值
                    switch (this.DataType)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.String:
                        case DbType.StringFixedLength:
                            break;

                        case DbType.Binary:
                            targetVal = this.Value.HexPatternStringToByteArray();
                            break;

                        case DbType.Boolean:
                            targetVal = this.Value.To<bool>();
                            break;
                        case DbType.Byte:
                            targetVal = this.Value.To<byte>();
                            break;
                        case DbType.Currency:
                            targetVal = this.Value.To<double>();
                            break;


                        case DbType.Time:
                        case DbType.Date:
                        case DbType.DateTime:
                        case DbType.DateTime2:
                        case DbType.DateTimeOffset:
                            targetVal = this.Value.To<DateTime>();
                            break;

                        case DbType.Decimal:
                            targetVal = this.Value.To<Decimal>();
                            break;
                        case DbType.Double:
                            targetVal = this.Value.To<double>();
                            break;
                        case DbType.Guid:
                            targetVal = new Guid(this.Value);
                            break;
                        case DbType.Int16:
                            targetVal = this.Value.To<Int16>();
                            break;
                        case DbType.Int32:
                            targetVal = this.Value.To<Int32>();
                            break;
                        case DbType.Int64:
                            targetVal = this.Value.To<Int64>();
                            break;
                        case DbType.Object:
                            break;
                        case DbType.SByte:
                            targetVal = this.Value.To<SByte>();
                            break;
                        case DbType.Single:
                            targetVal = this.Value.To<Single>();
                            break;
                        case DbType.UInt16:
                            targetVal = this.Value.To<UInt16>();
                            break;
                        case DbType.UInt32:
                            targetVal = this.Value.To<UInt32>();
                            break;
                        case DbType.UInt64:
                            targetVal = this.Value.To<UInt64>();
                            break;

                        case DbType.VarNumeric:
                            break;
                        case DbType.Xml:
                            break;

                        default:
                            break;
                    }
                    #endregion

                    if (RuntimeValue == null) RuntimeValue = targetVal;
                    newP.Value = targetVal;
                }
            }

            cmd.Parameters.Add(newP);
        }

    }
}
