using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 定义域寻址路径
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Serializable]
    public class ScopePath
    {
        /// <summary>
        /// 获取或设置是否记录路径历史
        /// </summary>
        [XmlAttribute]
        public bool Recordable { get; set; }

        List<TypeExecuteCursor> TypeHistoryList = new List<TypeExecuteCursor>();

        Type lastEnterType = null;
        /// <summary>
        /// 进入操作类型
        /// </summary>
        public void EnterType(Type callerType, Type currentType)
        {
            //游标前进
            Cursor++;
            if (!Recordable) return;

            if (TypeHistoryList.Count > 0)
            {
                TypeExecuteCursor lastExec = TypeHistoryList[Cursor - 2];
                //Debug.WriteLine(string.Format("caller:{0}", lastExec.CallerType.FullName));
                if (!lastExec.CallerType.Equals(callerType))
                {
                    Depth++;
                }
                else
                {
                    if (callerType.Equals(currentType)) Depth++;
                }
            }
            else
            {
                if (currentType != lastEnterType) Depth++;
            }

            TypeHistoryList.Add(new TypeExecuteCursor { CallerType = callerType, ExecuteType = currentType, Depth = Depth });
            //Debug.WriteLine(string.Format("{1}进入类型：{0}, caller:{2}", currentType.FullName, new string('\t', Depth), callerType.FullName));

            lastEnterType = currentType;
        }

        /// <summary>
        /// 获取游标位置的记录
        /// </summary>
        /// <param name="cursor">游标位置</param>
        /// <returns></returns>
        public TypeExecuteCursor GetExecuteCursor(int cursor)
        {
            if (cursor >= 1 && TypeHistoryList.Count > 0)
            {
                return TypeHistoryList[cursor - 1];
            }
            else
            {
                return new TypeExecuteCursor();
            }
        }

        /// <summary>
        /// 获取特定游标位置的类型
        /// </summary>
        /// <param name="cursor">游标位置</param>
        /// <returns></returns>
        public Type GetCursorType(int cursor)
        {
            if (Cursor - 1 >= 0)
            {
                return TypeHistoryList[Cursor - 1].ExecuteType;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 退出操作类型
        /// </summary>
        /// <param name="currentType"></param>
        public void ExitType(Type currentType)
        {
            //游标后退
            Cursor--;
            if (!Recordable) return;

            TypeExecuteCursor tCursor = GetExecuteCursor(Cursor);
            //记录操作痕迹
            TypeHistoryList.Add(new TypeExecuteCursor
            {
                CallerType = tCursor.ExecuteType,
                ExecuteType = currentType,
                Depth = Depth,
                IsExit = true
            });

            if (Depth > 0)
            {
                //如果是当前运行域，则深度减小
                if (currentType == lastEnterType)
                {
                    Depth--;
                }
                else
                {
                    //BUG：容器类型的lastEnterType != currentType
                    if (currentType.Equals(typeof(TaskContainer)))
                        Depth--;
                }
            }

            //Debug.WriteLine(string.Format("{1}退出类型：{0}", currentType.FullName, new string('\t', Depth)));
            //更新当前作用域类型
            lastEnterType = tCursor.ExecuteType;
        }

        /// <summary>
        /// 获取当前路径的游标
        /// </summary>
        public int Cursor = 0;

        /// <summary>
        /// 深度
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (TypeHistoryList.Count >= Cursor)
            {
                List<string> pathList = new List<string>();
                for (int i = 1; i <= Cursor; i++)
                {
                    pathList.Add(TypeHistoryList[Cursor - i].ExecuteType.FullName);
                }
                return string.Join("/", pathList.ToArray());
            }
            return "/";
        }

        /// <summary>
        /// 获取历史路径
        /// </summary>
        /// <param name="separator">操作步骤分隔字符</param>
        /// <returns></returns>
        public string GetTraceList(string separator)
        {
            /*
            进入类型：NetTask.TaskImplement.SqlNetTask
                     * 
                进入类型：NetTask.Data.DataReceiverBridge
                     * 
                    进入类型：NetTask.Data.DataReaderReceiver
                        进入类型：NetTask.Core.ClrDataStorage
                        退出类型：NetTask.Core.ClrDataStorage
                    退出类型：NetTask.Data.DataReaderReceiver
                     * 
                    进入类型：NetTask.TaskImplement.UrlParseTask
                    退出类型：NetTask.TaskImplement.UrlParseTask
                     * 
                    进入类型：NetTask.TaskImplement.SqlNetTask
                    退出类型：NetTask.TaskImplement.SqlNetTask
                     * 
                退出类型：NetTask.Data.DataReceiverBridge
                     * 
            退出类型：NetTask.TaskImplement.SqlNetTask
            */
            string[] hPathArr = Array.ConvertAll<TypeExecuteCursor, string>(TypeHistoryList.ToArray(),
                c =>
                {
                    return string.Format("{1}[{3}]{2}类型：{0} [~{4}]", c.ExecuteType, new string('\t', c.Depth),
                        c.IsExit ? "退出" : "进入",
                        c.Depth,
                        c.CallerType);
                });
            return string.Join(separator, hPathArr);
        }


        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// 	<paramref name="obj"/> 参数为 null。
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj is ScopePath)
            {
                ScopePath right = (ScopePath)obj;
                return Recordable == right.Recordable
                    && TypeHistoryList.Count == right.TypeHistoryList.Count
                    && Cursor == right.Cursor
                    && this.ToString().Equals(right.ToString());
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// 没有任何寻址路径
        /// </summary>
        public static ScopePath Empty { get { return new ScopePath(); } }

    }

    /// <summary>
    /// 类型运行游标
    /// </summary>
    [Serializable]
    public struct TypeExecuteCursor
    {
        /// <summary>
        /// 是否是退出操作
        /// </summary>
        public bool IsExit { get; set; }

        /// <summary>
        /// 调用类型
        /// </summary>
        public Type CallerType { get; set; }

        /// <summary>
        /// 运行类型
        /// </summary>
        public Type ExecuteType { get; set; }

        /// <summary>
        /// 当前任务深度
        /// </summary>
        public int Depth { get; set; }
    }
}
