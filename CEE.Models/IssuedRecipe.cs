using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEE.Models
{
    /// <summary>
    /// 下发配方
    /// </summary>
    public class IssuedRecipe
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string? EqpID { get; set; }
        /// <summary>
        /// 授权ID
        /// </summary>
        public string? AuthorizeID { get; set; }
        /// <summary>
        /// 配方组
        /// </summary>
        public List<Parameter>? ParametersList { get; set; }
        /// <summary>
        /// 请求ID
        /// </summary>
        public string? ReqID { get; set; }
        /// <summary>
        /// 配方名称
        /// </summary>
        public string? RecipeName { get; set; }
        /// <summary>
        /// 配方ID
        /// </summary>
        public string? RecipeID { get; set; }
        /// <summary>
        /// 配方类型
        /// </summary>
        public string? RecipeType { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string? DataTime { get; set; }
    }
}
