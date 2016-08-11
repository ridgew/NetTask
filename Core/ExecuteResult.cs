
namespace NetTask.Core
{
    /// <summary>
    /// Indicates the result of the Execute Method.
    /// </summary>
    public enum ExecuteResult
    {
        /// <summary>
        /// Indicates the Execute method was successful.
        /// </summary>
        Ok,

        /// <summary>
        /// Indicates the Execute method has failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates the Execute method encountered an exception.
        /// </summary>
        Exception,

        /// <summary>
        /// Indicates the Execute method was canceled.
        /// </summary>
        Canceled
    }

}
