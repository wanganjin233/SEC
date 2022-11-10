using Newtonsoft.Json.Linq;
using SEC.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SEC.Driver
{
    public static class Tools
    {
        /// <summary>
        /// COM1,9600,8,无,1
        /// </summary>
        /// <param name="connectionStr"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static ICommunication? ConnectionResolution(string? connectionStr)
        {
            if (string.IsNullOrEmpty(connectionStr))
            {
                return null;
            }
            else if (Regex.Matches(connectionStr, "COM[0-9]+,[0-9]+,[0-9],.+,.+").Any())
            {
                string[] connectionStrSplit = connectionStr.Split(',');
                return new SerialPort(connectionStrSplit[0],
                                      connectionStrSplit[1].ToInt(),
                                      connectionStrSplit[2].ToInt(),
                                      connectionStrSplit[3],
                                      connectionStrSplit[4]);
            }
            else if (Regex.Matches(connectionStr, "[0-9]+[.][0-9]+[.][0-9]+[.][0-9]+[:][0-9]+").Any())
            {
                return new SocketClient(connectionStr);
            }
            throw new Exception("未找到合适的连接");
        }
        public static byte[] ObjectToBytes(this Tag tag, object value)
        { 
            byte[] bytes = tag.DataType switch
            {
                TagTypeEnum.Boole => BitConverter.GetBytes(Convert.ToBoolean(value.ToString())),
                TagTypeEnum.Ushort => BitConverter.GetBytes(Convert.ToUInt16(value.ToString())),
                TagTypeEnum.Short => BitConverter.GetBytes(Convert.ToInt16(value.ToString())),
                TagTypeEnum.Uint => BitConverter.GetBytes(Convert.ToUInt32(value.ToString())),
                TagTypeEnum.Int => BitConverter.GetBytes(Convert.ToInt32(value.ToString())),
                TagTypeEnum.Ulong => BitConverter.GetBytes(Convert.ToUInt64(value.ToString())),
                TagTypeEnum.Long => BitConverter.GetBytes(Convert.ToInt64(value.ToString())),
                TagTypeEnum.Float => BitConverter.GetBytes(Convert.ToSingle(value.ToString())),
                TagTypeEnum.Double => BitConverter.GetBytes(Convert.ToDouble(value.ToString())),
                TagTypeEnum.String => Encoding.GetEncoding(tag.Coding).GetBytes((value.ToString() ?? string.Empty).PadRight(tag.DataLength, '\0')),
                _ => throw new NotImplementedException("无法找到合适的转换")
            };
            return bytes.DataSequence(tag.Sort); 
        }
        public static object ObjectMatching(this Tag tag, object value)
        {
            return tag.DataType switch
            {
                TagTypeEnum.Boole =>    Convert.ToBoolean(value.ToString()),
                TagTypeEnum.Ushort =>   Convert.ToUInt16(value.ToString()),
                TagTypeEnum.Short =>    Convert.ToInt16(value.ToString()),
                TagTypeEnum.Uint =>  Convert.ToUInt32(value.ToString()),
                TagTypeEnum.Int =>   Convert.ToInt32(value.ToString()),
                TagTypeEnum.Ulong => Convert.ToUInt64(value.ToString()),
                TagTypeEnum.Long =>  Convert.ToInt64(value.ToString()),
                TagTypeEnum.Float => Convert.ToSingle(value.ToString()),
                TagTypeEnum.Double =>Convert.ToDouble(value.ToString()),
                TagTypeEnum.String => value.ToString()??string.Empty,
                _ => throw new NotImplementedException("无法找到合适的转换")
            }; 
        }
        /// <summary>
        /// byte转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="length"></param>
        /// <param name="translator"></param>
        /// <returns></returns>
        public static OperateResult<T[]> Translator<T>(this byte[]? datas, ushort length, Func<byte[], int, T> translator)
        {
            try
            {
                if (datas != null && datas.Any() && datas.Length / length > 0)
                {
                    datas = datas.Take(datas.Length - datas.Length % length).ToArray();
                    T[] result = new T[datas.Length / length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = translator(datas, i * length);
                    }
                    return new OperateResult<T[]> { IsSuccess = true, Content = result };
                }
                else
                {
                    return new OperateResult<T[]> { IsSuccess = false, Message = "读取数据为空" };
                }
            }
            catch (Exception e)
            {
                return new OperateResult<T[]> { IsSuccess = false, Message = $"读取数据发生异常{e.Message}" };
            }
        }
        /// <summary>
        /// 字符串类型转换
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="strLength"></param>
        /// <param name="length"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static OperateResult<string[]> Translator(this byte[]? datas, ushort strLength, ushort length, Encoding encoding)
        {
            try
            {
                if (datas != null && length > 0)
                {
                    string[] result = new string[length];
                    for (int i = 0; i < length; i++)
                    {
                        result[i] = encoding.GetString(datas, i * strLength * 2, strLength * 2);
                    }
                    return new OperateResult<string[]> { IsSuccess = true, Content = result };
                }
                else
                {
                    return new OperateResult<string[]> { IsSuccess = false, Message = "读取数据为空" };
                }
            }
            catch (Exception e)
            {
                return new OperateResult<string[]> { IsSuccess = false, Message = $"读取数据发生异常{e.Message}" };
            }
        }
    }
}
