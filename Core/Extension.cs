using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
//using CommonLib;

namespace NetTask.Core
{
    /// <summary>
    /// 扩展方法使用
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 从配置静态方法创建符合特定委托类型的委托
        /// </summary>
        /// <typeparam name="TDelegate">委托类型</typeparam>
        /// <param name="staticCfgMethod">特定类型的静态方法描述，形如ClrServiceHost.Management.Communication::GetApplicationList, ClrServiceHost。</param>
        /// <returns></returns>
        public static TDelegate CreateFromConfig<TDelegate>(this string staticCfgMethod)
            where TDelegate : class
        {
            string pattern = "::([^,]+)";
            Match m = Regex.Match(staticCfgMethod, pattern, RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                throw new System.Configuration.ConfigurationErrorsException("服务端获取通信配置的委托方法(" + staticCfgMethod + ")配置错误！");
            }
            else
            {
                Type methodType = Type.GetType(staticCfgMethod.Replace(m.Value, string.Empty));
                MethodInfo method = methodType.GetMethod(m.Groups[1].Value, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                return Delegate.CreateDelegate(typeof(TDelegate), method, true) as TDelegate;
            }
        }

        /// <summary>
        /// 转换为委托字符串形式
        /// </summary>
        public static string ToDelegateString(this MemberInfo targetMethod)
        {
            Type targetType = targetMethod.ReflectedType;
            return string.Format("{0}::{1}, {2}", targetType, targetMethod.Name,
                Path.GetFileNameWithoutExtension(targetType.Assembly.Location) //targetType.Assembly.FullName.TrimAfter(",")
                );
        }

        /// <summary>
        /// 获取不包含版本的类型全称，形如：BizService.Interface.Services.LogService, BizService.Interface。
        /// </summary>
        /// <param name="instanceType">对象类型</param>
        /// <returns></returns>
        public static string GetNoVersionTypeName(this Type instanceType)
        {
            if (!instanceType.IsGenericType)
            {
                return string.Format("{0}, {1}",
                instanceType.FullName,
                Path.GetFileNameWithoutExtension(instanceType.Assembly.Location));
            }
            else
            {
                string rawFullName = instanceType.FullName;
                string baseTypeName = rawFullName.Substring(0, rawFullName.IndexOf('`'));
                return string.Format("{0}<{1}>, {2}",
                    baseTypeName,
                    string.Join(",", Array.ConvertAll<Type, string>(instanceType.GetGenericArguments(),
                    t => t.ToSimpleType())),
                Path.GetFileNameWithoutExtension(instanceType.Assembly.Location));
            }

        }

        /// <summary>
        /// 转换到特定数值类型
        /// </summary>
        public static TData To<TData>(this string rawString)
        {
            Converter<string, TData> gConvert = new Converter<string, TData>(s =>
            {
                if (rawString == null)
                {
                    return default(TData);
                }
                else
                {
                    return (TData)Convert.ChangeType(s, typeof(TData));
                }
            });
            return gConvert(rawString);
        }

        /// <summary>
        /// 无异常转换到特定数值类型
        /// </summary>
        public static TData As<TData>(this string rawString)
        {
            TData result = default(TData);
            try
            {
                result = To<TData>(rawString);
            }
            catch { }
            return result;
        }

        #region XMLNode扩展

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <param name="noNamespaceAttr">属性是否添加默认命名空间</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj, bool noNamespaceAttr)
        {
            if (pObj == null) { return null; }
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(pObj.GetType(), string.Empty);
                XmlTextWriter xtw = new XmlTextWriter(ms, Encoding.UTF8);
                if (noNamespaceAttr)
                {
                    XmlSerializerNamespaces xn = new XmlSerializerNamespaces();
                    xn.Add("", "");
                    xs.Serialize(xtw, pObj, xn);
                }
                else
                {
                    xs.Serialize(xtw, pObj);
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(Encoding.UTF8.GetString(ms.ToArray()).Trim());
                return xml;
            }

        }

        /// <summary>
        /// 获取对象序列化的XmlDocument版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static XmlDocument GetXmlDoc(this object pObj)
        {
            return GetXmlDoc(pObj, false);
        }

        /// <summary>
        /// 从已序列化数据(XmlDocument)中获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="xmlDoc">已序列化的文档对象</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(this XmlDocument xmlDoc)
        {
            if (xmlDoc == null) { return default(T); }
            XmlNodeReader xmlReader = new XmlNodeReader(xmlDoc.DocumentElement);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// 输出带缩进格式的XML文档
        /// </summary>
        /// <param name="xDoc">XML文档对象</param>
        /// <param name="writer">文本输出器</param>
        public static void WriteIndentedContent(this XmlDocument xDoc, TextWriter writer)
        {
            XmlTextWriter xWriter = new XmlTextWriter(writer);
            xWriter.Formatting = Formatting.Indented;
            xDoc.WriteContentTo(xWriter);
        }

        /// <summary>
        /// 从节点返回为原始对象
        /// </summary>
        /// <typeparam name="TResult">对象类型</typeparam>
        /// <param name="xmlNode">序列化节点实例</param>
        public static TResult GetAsObject<TResult>(this XmlNode xmlNode)
        {
            XmlNodeReader xmlReader = new XmlNodeReader(xmlNode);
            XmlSerializer serializer = new XmlSerializer(typeof(TResult), new XmlRootAttribute(xmlNode.Name));
            return (TResult)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// 安全转换节点属性为特定类型的值
        /// </summary>
        /// <typeparam name="TValue">返回属性数据类型</typeparam>
        /// <param name="node">当前节点</param>
        /// <param name="attrName">节点属性</param>
        /// <param name="default">如不存在，则设置的默认值。</param>
        /// <returns></returns>
        public static TValue Attr<TValue>(this XmlNode node, string attrName, TValue @default)
        {
            XmlAttribute nodeAttr = node.Attributes[attrName];
            if (nodeAttr == null)
            {
                return @default;
            }
            else
            {
                return nodeAttr.Value.As<TValue>();
            }
        }

        /// <summary>
        /// 安全转换节点属性为特定类型的值
        /// </summary>
        public static TValue Attr<TValue>(this XmlNode node, string attrName)
        {
            return Attr<TValue>(node, attrName, default(TValue));
        }

        /// <summary>
        /// 在满足不为默认值的情况下添加节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <param name="attrVal">属性值，当值为null值移除该节点属性</param>
        /// <param name="ignoreFun">判断方法</param>
        /// <returns></returns>
        public static XmlNode IgnoreDefaultValue(this XmlNode node, string attrName, object attrVal, Func<bool> ignoreFun)
        {
            if (!ignoreFun())
            {
                return WithAttr(node, attrName, attrVal);
            }
            else
            {
                return node;
            }
        }

        /// <summary>
        /// 附加或设置节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <param name="attrVal">属性值，当值为null值移除该节点属性</param>
        /// <returns>应用了该属性的节点</returns>
        public static XmlNode WithAttr(this XmlNode node, string attrName, object attrVal)
        {
            XmlAttribute attr = node.Attributes[attrName];
            if (attrVal != null && attr == null)
            {
                attr = node.OwnerDocument.CreateAttribute(attrName);
                node.Attributes.Append(attr);
            }

            if (attrVal == null)
            {
                if (attr != null)
                    node.Attributes.Remove(attr);
            }
            else
            {
                attr.Value = attrVal.ToString();
            }

            return node;
        }

        /// <summary>
        /// 删除节点属性
        /// </summary>
        /// <param name="node">当前需要设置的节点</param>
        /// <param name="attrName">属性名称</param>
        /// <returns>删除了该属性的节点</returns>
        public static XmlNode RemoveAttr(this XmlNode node, string attrName)
        {
            XmlAttribute attr = node.Attributes[attrName];
            if (attr != null) node.Attributes.Remove(attr);
            return node;
        }

        /// <summary>
        /// 创建当前节点的子节点
        /// </summary>
        public static XmlNode CrateChildElement(this XmlNode parentNode, string nodeName)
        {
            XmlNode xNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, nodeName, null);
            parentNode.AppendChild(xNode);
            return xNode;
        }

        #endregion

        /// <summary>
        /// 转换字符串为字符串数组
        /// </summary>
        /// <param name="expression">字符串，使用固定字符", ; |"分隔或使用[]标记。</param>
        /// <param name="charArr">分隔字符数组</param>
        /// <returns></returns>
        public static string[] AsStringArray(this string expression, params char[] charArr)
        {
            if (expression.StartsWith("[") && expression.EndsWith("]"))
            {
                string str2Process = expression.Substring(1, expression.Length - 2);
                return SplitString(str2Process, ",", "\'", true);
            }
            else
            {
                if (charArr != null && charArr.Length > 0)
                {
                    return expression.Split(charArr, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    return expression.Split(new char[] { ',', ';', '|' },
                        StringSplitOptions.RemoveEmptyEntries);
                }
            }
        }

        /// <summary>
        /// 分隔特定的字符串为有效字符参数数组
        /// <para>
        /// http://www.codeproject.com/KB/dotnet/TextQualifyingSplit.aspx
        /// Split("This is an ""example."".Cool!", ".", "\"", true)
        /// </para>
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="delimiter">分隔字符串</param>
        /// <param name="qualifier">包含引用字符串</param>
        /// <param name="ignoreCase">是否区不区分大小写</param>
        /// <returns></returns>
        public static string[] SplitString(this string expression, string delimiter, string qualifier, bool ignoreCase)
        {
            bool _QualifierState = false;
            int innerStart = 0;

            Func<string, string, string> trimItem = (s, q) =>
            {
                if (!string.IsNullOrEmpty(q))
                {
                    if (s.StartsWith(q))
                        s = s.Substring(qualifier.Length);

                    if (s.EndsWith(q))
                        s = s.Substring(0, s.Length - q.Length);
                }
                return s;
            };

            List<string> resList = new List<string>();
            string crtItem = null;
            for (int cursor = 0; cursor < expression.Length - 1; cursor++)
            {
                if ((qualifier != null)
                 && (string.Compare(expression.Substring(cursor, qualifier.Length), qualifier, ignoreCase) == 0))
                {
                    _QualifierState = !(_QualifierState);
                }
                else if (!(_QualifierState)
                      && (delimiter != null)
                      && (string.Compare(expression.Substring(cursor, delimiter.Length), delimiter, ignoreCase) == 0))
                {
                    crtItem = expression.Substring(innerStart, cursor - innerStart);
                    crtItem = trimItem(crtItem, qualifier);
                    resList.Add(crtItem);
                    innerStart = cursor + 1;
                }
            }

            if (innerStart < expression.Length)
            {
                crtItem = expression.Substring(innerStart, expression.Length - innerStart);
                int addEmptyCount = 0;
                while (delimiter != null && crtItem.EndsWith(delimiter))
                {
                    crtItem = crtItem.Substring(0, crtItem.Length - delimiter.Length);
                    addEmptyCount++;
                }
                crtItem = trimItem(crtItem, qualifier);
                resList.Add(crtItem);
                for (int i = 0; i < addEmptyCount; i++)
                {
                    resList.Add(String.Empty);
                }
            }
            return resList.ToArray();
        }

        /// <summary>
        /// 二进制序列的16进制视图形式（16字节换行）
        /// </summary>
        public static string ByteArrayToHexString(this byte[] tBinBytes)
        {
            string draftStr = System.Text.RegularExpressions.Regex.Replace(BitConverter.ToString(tBinBytes),
            "([A-z0-9]{2}\\-){16}",
            m =>
            {
                return m.Value.Replace("-", " ") + Environment.NewLine;
            });
            return draftStr.Replace("-", " ");
        }

        /// <summary>
        /// 从原始16进制字符还原到字节序列
        /// </summary>
        public static byte[] HexPatternStringToByteArray(this string hexrawStr)
        {
            if (string.IsNullOrEmpty(hexrawStr))
            {
                return new byte[0];
            }

            string trueRaw = hexrawStr.Replace(" ", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("-", "")
                .Replace("\t", "").Trim();

            int totalLen = trueRaw.Length;
            if (totalLen % 2 != 0)
            {
                throw new InvalidCastException("hex string size invalid.");
            }
            else
            {
                byte[] rawBin = new byte[totalLen / 2];
                for (int i = 0; i < totalLen; i = i + 2)
                {
                    rawBin[i / 2] = Convert.ToByte(int.Parse(trueRaw.Substring(i, 2),
                        System.Globalization.NumberStyles.AllowHexSpecifier));
                }
                return rawBin;
            }
        }

        #region 枚举扩展

        /// <summary>
        /// Includes an enumerated type and returns the new value
        /// </summary>
        public static T Include<T>(this Enum value, T append)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(append, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) | (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Removes an enumerated type and returns the new value
        /// </summary>
        public static T Remove<T>(this Enum value, T remove)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(remove, type);
            if (parsed.Signed is long)
            {
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
            }

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>
        /// Checks if an enumerated type contains a value
        /// </summary>
        public static bool Has<T>(this Enum value, T check)
        {
            Type type = value.GetType();

            //determine the values
            object result = value;
            _Value parsed = new _Value(check, type);
            if (parsed.Signed is long)
            {
                return (Convert.ToInt64(value) & (long)parsed.Signed) == (long)parsed.Signed;
            }
            else if (parsed.Unsigned is ulong)
            {
                return (Convert.ToUInt64(value) & (ulong)parsed.Unsigned) == (ulong)parsed.Unsigned;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an enumerated type is missing a value
        /// </summary>
        public static bool Missing<T>(this Enum obj, T value)
        {
            return !Has<T>(obj, value);
        }

        #region Helper Classes

        //class to simplfy narrowing values between
        //a ulong and long since either value should
        //cover any lesser value
        private class _Value
        {

            //cached comparisons for tye to use
            private static Type _UInt64 = typeof(ulong);
            private static Type _UInt32 = typeof(long);

            public long? Signed;
            public ulong? Unsigned;

            public _Value(object value, Type type)
            {

                //make sure it is even an enum to work with
                if (!type.IsEnum)
                {
                    throw new ArgumentException("Value provided is not an enumerated type!");
                }

                //then check for the enumerated value
                Type compare = Enum.GetUnderlyingType(type);

                //if this is an unsigned long then the only
                //value that can hold it would be a ulong
                if (compare.Equals(_Value._UInt32) || compare.Equals(_Value._UInt64))
                {
                    this.Unsigned = Convert.ToUInt64(value);
                }
                //otherwise, a long should cover anything else
                else
                {
                    this.Signed = Convert.ToInt64(value);
                }

            }

        }

        #endregion

        #endregion

    }
}
