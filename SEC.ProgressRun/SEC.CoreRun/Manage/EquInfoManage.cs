using SEC.CommUtil;
using SEC.Models.Driver;
using SEC.Util;
using System.Collections.Concurrent;

namespace SEC.CoreRun.Manage
{
    public static class EquInfoManage
    {
         
        /// <summary>
        /// 设备对应配置和管道服务
        /// </summary>
        private static readonly ConcurrentDictionary<string, EquConfig> _EquConfigs = new();

        public static ConcurrentDictionary<string, EquConfig>  EquConfigs => _EquConfigs;
        /// <summary>
        /// 添加配置服务
        /// </summary>
        /// <param name="equConfig"></param>
        /// <param name="pipelineServer"></param>
        /// <returns></returns>
        public static void Add(EquConfig equConfig)
        {
            _EquConfigs.TryAdd(equConfig.EQU, equConfig); 
        }
        /// <summary>
        /// 根据Tag名称获取对应Tag点
        /// </summary>
        /// <param name="pipelineServer"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static Tag? GetTag(string equ, string TagName)
        {
            _EquConfigs.TryGetValue(equ, out EquConfig? equConfig);
            return equConfig?.Tags.FirstOrDefault(p => p.TagName == TagName);
        }
        /// <summary>
        /// 根据Tag名称获取对应Tag点
        /// </summary>
        /// <param name="pipelineServer"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static List<Tag>? GetTag(string equ, List<string> TagNames)
        {
            _EquConfigs.TryGetValue(equ, out EquConfig? equConfig);
            return equConfig?.Tags.FindAll(p => TagNames.Contains(p.TagName));
        }
        /// <summary>
        /// 更新点位
        /// </summary>
        /// <param name="pipelineServer"></param>
        /// <param name="tag"></param>
        public static bool UpdateTag(string equ, Tag tag)
        {
            bool flag = false;
            Tag? _tag = GetTag(equ, tag.TagName);
            if (_tag != null && _tag.Value != tag.Value)
            {
                _tag.Value = tag.Value;
                flag = true;
            }
            return flag;
        }
    }
}
