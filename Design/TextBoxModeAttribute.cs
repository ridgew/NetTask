using System;

namespace NetTask.Design
{
    /// <summary>
    /// 文本的显示模型
    /// </summary>
    public class TextBoxModeAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TextBoxModeAttribute"/> class.
        /// </summary>
        public TextBoxModeAttribute()
            : base()
        {

        }

        /// <summary>
        /// 初始化一个 <see cref="TextBoxModeAttribute"/> class 实例。
        /// </summary>
        /// <param name="mode">显示模型</param>
        public TextBoxModeAttribute(BoxMode mode)
        {
            _mode = mode;
        }

        BoxMode _mode = BoxMode.SingleLine;
        /// <summary>
        /// 显示模型
        /// </summary>
        /// <returns></returns>
        public BoxMode GetMode()
        {
            return _mode;
        }

        /// <summary>
        /// 行跨度
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 列跨度
        /// </summary>
        public int Cols { get; set; }
    }

    /// <summary>
    /// 盒子模型
    /// </summary>
    public enum BoxMode 
    { 
        /// <summary>
        /// 单行
        /// </summary>
        SingleLine, 
        /// <summary>
        /// 多行
        /// </summary>
        MultiLine, 
        /// <summary>
        /// 密码
        /// </summary>
        Password 
    }
}
