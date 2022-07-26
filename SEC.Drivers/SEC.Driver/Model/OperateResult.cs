namespace SEC.Driver
{
    /// <summary>
    /// 操作结果的泛型类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperateResult<T>
    {
        /// <summary>
        /// 指示本次访问是否成功
        /// </summary>
        public bool IsSuccess { get; set; } 
        /// <summary>
        /// 具体的描述
        /// </summary>
        public string Message { get; set; }=string.Empty; 
        /// <summary>
        /// 具体的错误代码
        /// </summary>
        public int ErrorCode { get; set; } 
        /// <summary>
        /// 用户自定义的泛型数据
        /// </summary>
        public T? Content { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime TimeSpan { get; set; }= DateTime.UtcNow;
    } 
}
