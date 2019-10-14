using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cresij_Control_Manager
{  
    /// <summary>
    /// 转换类
    /// </summary>
    public class Transformation
    {

        /// <summary>
            /// 汉字转换为Unicode编码
            /// </summary>
            /// <param name="str">要编码的汉字字符串</param>
            /// <returns>Unicode编码的的字符串</returns>
        public static string ToUnicode(string str)
        {
            byte[] bts = Encoding.Unicode.GetBytes(str);
            string r = "";
            for (int i = 0; i < bts.Length; i += 2) r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') 
                    + bts[i].ToString("x").PadLeft(2, '0');
            return r;
        }

        public static string ToUnicode(string str, bool hasSpace)
        {
            byte[] textbuf = Encoding.Default.GetBytes(str);
            string textAscii = string.Empty;//用来存储转换过后的ASCII码
            for (int i = 0; i < textbuf.Length; i++)
            {
                textAscii += " " + textbuf[i].ToString("X");
            }
            return textAscii;
        }
        /// <summary>
            /// 将Unicode编码转换为汉字字符串
            /// </summary>
            /// <param name="str">Unicode编码字符串</param>
            /// <returns>汉字字符串</returns>
        public static string ToGB2312(string str)
        {
            string r = "";
            MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled 
                | RegexOptions.IgnoreCase);
            byte[] bts = new byte[2];
            foreach (Match m in mc)
            {
                bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                r += Encoding.Unicode.GetString(bts);
            }
            return r;
        }

        //字符转ASCII码：
        public static int Asc(string character)
        {
            if (character.Length == 1)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                int intAsciiCode = (int)asciiEncoding.GetBytes(character)[0];
                return (intAsciiCode);
            }
            else
            {
                throw new Exception("Character is not valid.");
            }

        }

        /// <summary>
        /// IP地址返回成AscIII型式
        /// </summary>
        /// <param name="_IP"></param>
        /// <returns></returns>
        public static string AsciiIP(string _IP)
        {
            string _result = "";
            string[] _r = new string[30];
            for (int i = 0; i < _r.Length; i++) _r[i] = "00";
            for (int i = 0; i < _IP.Length; i++)
            {
                _r[i] = IntToHex(Asc(_IP.Substring(i, 1)));
            }
            for (int j = 0; j < _r.Length; j++)
            {
                _result += " " + _r[j];
            }
            return _result.Trim();
        }

        /// <summary> 
        /// 累加校验和 
        /// </summary> 
        /// <param name="memorySpage">需要校验的数据</param> 
        /// <returns>返回校验和结果</returns> 
        public static string FillSum(string Hexstr)
        {
            byte[] memorySpage = strToToHexByte(Hexstr);
            int num = 0;
            for (int i = 0; i < memorySpage.Length; i++)
            {
                num = (num + memorySpage[i]) % 0xffff;
            }
            //实际上num 这里已经是结果了，如果只是取int 可以直接返回了 
            memorySpage = BitConverter.GetBytes(num);
            //返回累加校验和 
            num = BitConverter.ToInt16(new byte[] { memorySpage[0], memorySpage[1] }, 0);
            return IntToHex(num % 256);
        }

        /// <summary>
        /// 十进制转十六进制
        /// </summary>
        /// <param name="_Value">十进制数</param>
        /// <returns></returns>
        public static string IntToHex(int _Value)
        {
            return _Value.ToString("X2");
        }
        /// <summary>
        /// 两个字节的返回值
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        public static string IntToHex(int _Value, bool HasSpace)
        {
            string _temp = IntToHex(_Value);
            switch (_temp.Length)
            {
                case 1:
                    _temp = "000" + _temp;
                    break;
                case 2:
                    _temp = "00" + _temp;
                    break;
                case 3:
                    _temp = "0" + _temp;
                    break;
            }

            string _result = "";
            for (int i = 0; i < _temp.Length; i++)
            {
                if (i % 2 == 1) _result += _temp[i] + " ";
                else
                    _result += _temp[i];
            }
            return _result;
        }

        public static string IntToHexReturnThree(int _Value, bool HasSpace)
        {
            string _temp = IntToHex(_Value);
            switch (_temp.Length)
            {
                case 1:
                    _temp = "00000" + _temp;
                    break;
                case 2:
                    _temp = "0000" + _temp;
                    break;
                case 3:
                    _temp = "000" + _temp;
                    break;
                case 4:
                    _temp = "00" + _temp;
                    break;
                case 5:
                    _temp = "0" + _temp;
                    break;
            }

            string _result = "";
            for (int i = 0; i < _temp.Length; i++)
            {
                if (i % 2 == 1) _result += _temp[i] + " ";
                else
                    _result += _temp[i];
            }
            return _result;
        }

        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString">16进制字符串</param>
        /// <returns>字节数组</returns>
        public static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// IP地址转换成十六进制
        /// </summary>
        /// <param name="_IP"></param>
        /// <returns></returns>
        public static string IPToHex(string _IP)
        {
            string[] _P = _IP.Split(' ');
            string t = "";
            for (var i = _P.Length - 1; i >= 0; i--)
            {
                if (t == "")
                    t = IntToHex(Convert.ToInt16(_P[i]));
                else
                    t += " " + IntToHex(Convert.ToInt16(_P[i]));
            }
            return t;
        }

        /// <summary>
        /// 整型转十六进制,取低两位
        /// </summary>
        /// <param name="_Value"></param>
        /// <returns></returns>
        public static string IntToLowHex(int _Value)
        {
            string _result = _Value.ToString("X2");

            if (_result.Length == 1)
                _result = "0" + _result;
            else
                _result.Substring(0, 2);
            return _result.Trim();
        }

        private static string BinToHexAndFour(string _Bin)
        {
            string _Result = "";

            switch (_Bin)
            {
                case "0000":
                    _Result = "0";
                    break;
                case "0001":
                    _Result = "1";
                    break;
                case "0010":
                    _Result = "2";
                    break;
                case "0011":
                    _Result = "3";
                    break;
                case "0100":
                    _Result = "4";
                    break;
                case "0101":
                    _Result = "5";
                    break;
                case "0110":
                    _Result = "6";
                    break;
                case "0111":
                    _Result = "7";
                    break;
                case "1000":
                    _Result = "8";
                    break;
                case "1001":
                    _Result = "9";
                    break;
                case "1010":
                    _Result = "A";
                    break;
                case "1011":
                    _Result = "B";
                    break;
                case "1100":
                    _Result = "C";
                    break;
                case "1101":
                    _Result = "D";
                    break;
                case "1110":
                    _Result = "E";
                    break;
                case "1111":
                    _Result = "F";
                    break;
            }
            return _Result;
        }
        /// <summary>
        /// 二进制转十六进制
        /// </summary>
        /// <param name="_Bin"></param>
        /// <returns></returns>
        public static string BinToHex(string _Bin)
        {
            string _temp = "";
            if (_Bin.Length == 4)
            {
                _temp = BinToHexAndFour(_Bin);
                if (_temp.Length == 1) _temp = "0" + _temp;
            }
            else
            {
                _temp = BinToHexAndFour(_Bin.Substring(0, 4)) + BinToHexAndFour(_Bin.Substring(4, 4));
                if (_temp.Length == 3) _temp = "0" + _temp;
            }

            return _temp;
        }

        /// <summary>
        /// 十六进制转二进制
        /// </summary>
        /// <param name="_Hex">输入两位十六进制</param>
        /// <returns>返回八位二进制数</returns>
        public static string HexToBin(string _Hex)
        {
            int tem = Convert.ToInt32(_Hex, 16);
            string _result = Convert.ToString(tem, 2);
            int L = _result.Length;
            for (int i = 0; i < 8 - L; i++)
                _result = "0" + _result;
            return _result;
        }

        public static int HextoInt(string Hexstr)
        {
            if (Hexstr == "")
                return 0;
            else
                return int.Parse(Hexstr.Replace(" ", ""), NumberStyles.HexNumber);
        }

        public static string HexToStr(string Hexstr)
        {
            int HexValue = HextoInt(Hexstr);
            return Chr(HexValue);
        }

        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                ASCIIEncoding asciiEncoding = new ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }

        /// <summary>
        /// 根据输入的字符串和协议头判断中间的字节数,返回值为真是表示合法字符串，为否为不完整返回值
        /// </summary>
        /// <param name="Hexstr"></param>
        /// <param name="Head"></param>
        /// <returns></returns>
        public static ResultHex ResultCheck(string Hexstr, string Head)
        {
            string _Hex = Hexstr;
            ResultHex rh = new ResultHex();
            const string EndBytes = "FF FF FF FF A0 A1 A2 A3";
            int _Heads, _Ends = -1;
            _Heads = Hexstr.IndexOf(Head);
            if (_Heads >= 0)
            {
                _Hex = _Hex.Substring(_Heads, _Hex.Length - _Heads);
                _Heads = 0;
            }

            _Ends = _Hex.IndexOf(EndBytes);
            if (_Heads == 0 && _Ends >= 0 && (_Ends - _Heads >= 0))
            {
                rh.isAvail = true;
                int HeadLen = Head.Length;
                rh.AvailHex = _Hex.Substring(HeadLen, _Ends - HeadLen).Trim();
            }
            else
                rh.AvailHex = "";
            rh.HexLen = rh.AvailHex.Length;
            return rh;
        }

        /// <summary>
        ///串口信息转换
        /// </summary>
        /// <param name="_Hex">字符串</param>
        /// <returns></returns>
        public static string SerioPortNameToHex(string _Hex, SerioParam _HexType)
        {
            string _result = "";
            switch (_Hex)
            {
                case "01":
                    if (_HexType == SerioParam.BaudRate) _result = "110";
                    if (_HexType == SerioParam.DataBits) _result = "5";
                    if (_HexType == SerioParam.StopBits) _result = "1位";
                    if (_HexType == SerioParam.Parity) _result = "NONE";
                    break;
                case "02":
                    if (_HexType == SerioParam.BaudRate) _result = "300";
                    if (_HexType == SerioParam.DataBits) _result = "6";
                    if (_HexType == SerioParam.StopBits) _result = "1.5位";
                    if (_HexType == SerioParam.Parity) _result = "ODD";
                    break;
                case "03":
                    if (_HexType == SerioParam.BaudRate) _result = "600";
                    if (_HexType == SerioParam.DataBits) _result = "7";
                    if (_HexType == SerioParam.StopBits) _result = "2位";
                    if (_HexType == SerioParam.Parity) _result = "EVEN";
                    break;
                case "04":
                    if (_HexType == SerioParam.BaudRate) _result = "1200";
                    if (_HexType == SerioParam.DataBits) _result = "8";
                    if (_HexType == SerioParam.StopBits) _result = "0.5位";
                    if (_HexType == SerioParam.Parity) _result = "MARK";
                    break;
                case "05":
                    if (_HexType == SerioParam.BaudRate) _result = "2400";
                    if (_HexType == SerioParam.DataBits) _result = "9";
                    if (_HexType == SerioParam.Parity) _result = "SPACE";
                    break;
                case "06":
                    if (_HexType == SerioParam.BaudRate) _result = "4800";
                    break;
                case "07":
                    if (_HexType == SerioParam.BaudRate) _result = "9600";
                    break;
                case "08":
                    if (_HexType == SerioParam.BaudRate) _result = "14400";
                    break;
                case "09":
                    if (_HexType == SerioParam.BaudRate) _result = "19200";
                    break;
                case "0A":
                    if (_HexType == SerioParam.BaudRate) _result = "38400";
                    break;
                case "0B":
                    if (_HexType == SerioParam.BaudRate) _result = "56000";
                    break;
                case "0C":
                    if (_HexType == SerioParam.BaudRate) _result = "56700";
                    break;
                case "0D":
                    if (_HexType == SerioParam.BaudRate) _result = "115200";
                    break;
                case "0E":
                    if (_HexType == SerioParam.BaudRate) _result = "128000";
                    break;
                case "0F":
                    if (_HexType == SerioParam.BaudRate) _result = "256000";
                    break;
                case "FF":
                    _result = "未设置";
                    break;

            }
            return _result;
        }

        /// <summary>
        /// 根据ID号返回串口的名称
        /// </summary>
        /// <param name="SerioID"></param>
        /// <returns></returns>
        public static string SerioIDToName(string SerioID)
        {
            string _result = "";
            switch (SerioID)
            {
                case "01":
                    _result = "COM1";
                    break;
                case "02":
                    _result = "COM2";
                    break;
                case "03":
                    _result = "COM3";
                    break;
                case "04":
                    _result = "COM4";
                    break;
                case "05":
                    _result = "485-1";
                    break;
                case "06":
                    _result = "485-2";
                    break;
                case "FF":
                    _result = "未指定";
                    break;
            }
            return _result;
        }
        /// <summary>
        /// 根据设备的ID号返回设备类型
        /// </summary>
        /// <param name="_MID"></param>
        /// <returns></returns>
        public static string MachineIDToType(string _MID)
        {
            string _result = "";
            switch (_MID)
            {
                case "01":
                    _result = "照明设备";
                    break;
                case "02":
                    _result = "视频设备";
                    break;
                case "03":
                    _result = "电脑设备";
                    break;
                case "04":
                    _result = "语音设备";
                    break;
                case "05":
                    _result = "门禁设备";
                    break;
                case "FF":
                    _result = "未指定";
                    break;
            }
            return _result;
        }

        public static byte[] HexStringToByteArray(string s)
        {
            if (s.Length == 0)
                throw new Exception("将16进制字符串转换成字节数组时出错，错误信息：被转换的字符串长度为0。");
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }
    }
}
