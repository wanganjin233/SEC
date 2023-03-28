using SEC.Interface.Interactive;
using SEC.Models.Interactive;
using SEC.Util;
using System.Collections.Concurrent;
using System.Reflection;

namespace SEC.CommUtil
{

    public class InstanceMethodInfo
    {
        /// <summary>
        /// 实列
        /// </summary>
        public object? Instance { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public MethodInfo? MethodInfo { get; set; }
    }

    public class ControllerManage
    {
        /// <summary>
        /// 方法字典
        /// </summary>
        public ConcurrentDictionary<string, InstanceMethodInfo> Routes = new();
        /// <summary>
        /// 初始化控制器
        /// </summary>
        /// <param name="objects">参数</param>
        /// <exception cref="Exception">未找到构造函数参数</exception>
        public ControllerManage(params object[] objects)
        {
            //遍历程序集所有控制器实列
            foreach (var type in Assembly.GetCallingAssembly().GetTypes().Where(p => p.GetInterface(typeof(ICommController).Name) != null))
            {
                List<object> parameters = new();
                //获取构造函数参数
                var InterfaceParameters = type.GetConstructors().FirstOrDefault()?.GetParameters();
                if (InterfaceParameters != null)
                {
                    //遍历构造函数参数
                    foreach (var InterfaceParameter in InterfaceParameters)
                    {
                        //匹配传入参数
                        var parameter = objects.FirstOrDefault(p => p.GetType().IsAssignableTo(InterfaceParameter.ParameterType));
                        if (parameter != null)
                        {
                            parameters.Add(parameter);
                        }
                        else
                        {
                            throw new Exception("无类型需要参数");
                        }
                    }
                }
                //创建实例
                object? instance = Activator.CreateInstance(type, parameters.ToArray(), null);
                //添加路由方法
                foreach (var methodInfo in type.GetMethods(BindingFlags.Public|BindingFlags.Instance).Where(p => p.DeclaringType == type))
                {
                    Routes.TryAdd($"{type.Name}.{methodInfo.Name}".Replace(".", "/").ToLower(), new InstanceMethodInfo { Instance = instance, MethodInfo = methodInfo });
                }
            }
        }
        /// <summary>
        /// 方法调用
        /// </summary>
        /// <param name="Identity">标识</param>
        /// <param name="operateResult">接收内容</param>
        /// <param name="objects">参数</param>
        /// <returns></returns>
        public object? MethodInfoInvoke(OperateResult<object> operateResult, params object[] objects)
        {
            //根据路由获取实列方法
            Routes.TryGetValue(operateResult.Router.ToLower(), out var instance);
            if (instance?.MethodInfo != null)
            {
                //获取方法参数0
                var parameters = instance.MethodInfo.GetParameters();
                //序列化内容
                string? ContentJson = operateResult.Content?.ToJson();
                List<object> parameterObjs = new();
                //设置方法的参数
                foreach (var parameter in parameters)
                {
                    object? parameterObj = objects.FirstOrDefault(p => p.GetType().IsAssignableTo(parameter.ParameterType)); 
                    parameterObj ??= ContentJson?.ToObject(parameter.ParameterType); 
                    if (parameterObj != null)
                    {
                        parameterObjs.Add(parameterObj);
                    }
                }
                //调用方法
                return instance.MethodInfo.Invoke(instance.Instance, parameterObjs.ToArray());
            }
            return null;
        }
    }
}
