﻿using SEC.Util;
using System.Text.RegularExpressions;

namespace SEC.Driver
{
    public class Fins : BaseDriver
    {
        /// <summary>
        /// PLC节点
        /// </summary>
        private byte? PLCNode = null;
        /// <summary>
        /// PC节点
        /// </summary>
        private byte? PCNode = null;
        /// <summary>
        /// 驱动状态
        /// </summary> 
        public override bool DriverState => base.DriverState && PLCNode != null && PCNode != null;

        /// <summary>
        /// 初始化欧姆龙Fins驱动
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="serverPort"></param>
        /// <param name="isPersistentConn"></param>
        public Fins(string communicationStr)
            : base(communicationStr)
        {
            Communication.HeadBytes = new byte[4] { 0x46, 0x49, 0x4E, 0x53 };
            Communication.DataLengthLocation = 4;
            Communication.DataLengthType = LengthTypeEnum.ReUint;
        }
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="tagGroup"></param>
        /// <param name="StationNumber"></param>
        /// <param name="TypeEnumtem"></param>
        /// <returns></returns>
        protected override byte[]? BatchReadCommand(TagGroup tagGroup, byte StationNumber, object TypeEnumtem)
        { 
            Tag firstTag = tagGroup.Tags.First();
            Tag lastTag = tagGroup.Tags.Last();
            if (tagGroup.IsBit)
            {
                tagGroup.Length = (ushort)((lastTag.Location - firstTag.Location) * 16 + lastTag.BitLocation - firstTag.BitLocation + 1);
            }
            return ((ushort)firstTag.Location).BatchReadCommand((AddressTypeEnum)TypeEnumtem, tagGroup.Length, firstTag.IsBit, StationNumber, PLCNode, PCNode, (byte)firstTag.BitLocation);
        }
        public override void Start(int cycle = 100)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (LogIn())
                    {
                        base.Start(cycle);
                        break;
                    }
                    Task.Delay(500);
                }
            });
        }
        /// <summary>
        /// 登陆PLC
        /// </summary>
        /// <returns></returns>
        private bool LogIn()
        {
            if (Communication.Send(FinsCommand.LogInCommand))
            {
                byte[]? bytes = Communication.Receive().GetLogIn();
                if (bytes != null)
                {
                    PCNode = bytes[3];
                    PLCNode = bytes[7];
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 读取最大长度
        /// </summary>
        public override int ReadMaxLenth => 960;
        /// <summary>
        /// tag点位地址解析
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        protected override Tag TagParsing(Tag tag)
        {
            string? addressType = Regex.Matches(tag.Address, "^[E][A-Z0-9]").FirstOrDefault()?.Value;
            addressType ??= Regex.Matches(tag.Address, "^[a-zA-Z]+").FirstOrDefault()?.Value;
            if (addressType != null && addressType.ToEnum(out AddressTypeEnum _AddressType))
            {
                tag.Type = _AddressType;
                string[] addressArr = tag.Address.Split('.');
                if (addressArr.Length > 1)
                {
                    tag.IsBit = true;
                    tag.BitLocation = addressArr[1].ToInt();
                }
                else if (tag.DataType == TagTypeEnum.Boole)
                {
                    tag.BitLocation = 0;
                    tag.IsBit = true;
                }
                tag.Location = (uint)addressArr[0].Remove(0, addressType.Length).ToInt();
            }
            return tag;
        }
        /// <summary>
        /// 发送并接收
        /// </summary>
        /// <param name="command"></param>
        /// <param name="headByte"></param>
        /// <returns></returns>
        public override byte[]? SendCommand(byte[] command)
        {
            var bytes = base.SendCommand(command).GetBody();
            if (bytes == null)
            {
                while (true)
                {
                    if (LogIn())
                    {
                        break;
                    }
                    Task.Delay(500);
                }
            }
            return bytes;
        }
        /// <summary>
        ///  写入数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <param name="isBit"></param>
        /// <returns></returns> 
        public override bool Write(Tag tag, object? value)
        { 
            if (tag.ClientAccess.Contains('W') && value != null)
            {
                byte[] command = ((ushort)tag.Location).BatchWriteCommand((AddressTypeEnum)tag.Type, tag.ObjectToBytes(value), tag.IsBit, PLCNode, PCNode,tag.StationNumber, (byte)tag.BitLocation);
                return SendCommand(command) != null;
            }
            return false;
        }
    }
}
