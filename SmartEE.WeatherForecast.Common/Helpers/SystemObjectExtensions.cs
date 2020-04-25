using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace System
{
    public static partial class Extensions
    {
        public static string GetNumbers(this string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        public static bool IsNumeric(this object Expression)
        {
            if (Expression == null || Expression is DateTime)
                return false;

            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean)
                return true;

            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { } // just dismiss errors but return false
            return false;
        }

        public static Stream ToStream(this string @this)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(@this);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static bool IsNullType(this object obj)
        {
            return (obj == null || obj is DBNull);
        }

        public static bool IsSigned(this object obj)
        {
            return (obj is ValueType && (obj is Int32 || obj is Int64 || obj is Int16 || obj is IntPtr || obj is decimal || obj is SByte));
        }

        public static bool IsEmpty(this object obj)
        {
            return (!IsNullType(obj) && (
            (obj is String && ((string)obj).Length == 0) ||
            (obj is StringBuilder && ((StringBuilder)obj).Length == 0) ||
            (obj is ICollection && ((ICollection)obj).Count == 0) ||
            (obj is Array && ((Array)obj).Length == 0) ||
            (IsSigned(obj) && obj == (ValueType)(-1)) ||
            (obj is ValueType && obj == (ValueType)(0)) ||
            (obj is Guid && ((Guid)obj) == Guid.Empty)
            ));
        }

        //public static bool IsEmpty(this string obj)
        //{
        //    return string.IsNullOrEmpty(obj);
        //}

        public static bool IsEmpty(this decimal obj)
        {
            return (obj == 0);
        }

        public static bool IsEmpty(this int obj)
        {
            return (obj == 0);
        }

        public static bool IsEmpty(this DateTime obj)
        {
            return (obj == default(DateTime));
        }


        public static bool IsNullTypeOrEmpty(this object obj)
        {
            return (IsNullType(obj) || IsEmpty(obj));
        }

        public static int Bool2Int(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                if (Convert.ToBoolean(obj))
                    return 1;
                else
                    return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static bool IsTrue(this object obj)
        {
            if (obj == null)
                return false;
            try
            {
                return Convert.ToBoolean(obj);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Int32 Obj2Int32(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                Int32 intResult = 0;
                Int32.TryParse(obj.ToString(), out intResult);
                return intResult;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static Int64 Obj2Int64(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                return Convert.ToInt64(obj.Obj2Decimal());
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string Obj2String(this object obj)
        {
            if (obj == null)
                return null;
            try
            {
                return obj.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static decimal Obj2Decimal(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                decimal result = 0;
                decimal.TryParse(obj.ToString(), out result);
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static double Obj2Double(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                double result = 0;
                double.TryParse(obj.ToString(), out result);
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static float Obj2Float(this object obj)
        {
            if (obj == null)
                return 0;
            try
            {
                float result = 0;
                float.TryParse(obj.ToString(), out result);
                return result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// DateTime as UTV for UnixEpoch
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Per JWT spec:
        /// Gets the number of seconds from 1970-01-01T0:0:0Z as measured in UTC until the desired date/time.
        /// </summary>
        /// <param name="datetime">The DateTime to convert to seconds.</param>
        /// <remarks>if dateTimeUtc less than UnixEpoch, return 0</remarks>
        /// <returns>the number of seconds since Unix Epoch.</returns>
        public static long ToEpoch(this DateTime datetime)
        {
            DateTime dateTimeUtc = datetime;
            if (datetime.Kind != DateTimeKind.Utc)
            {
                dateTimeUtc = datetime.ToUniversalTime();
            }

            if (dateTimeUtc.ToUniversalTime() <= UnixEpoch)
            {
                return 0;
            }

            return (long)(dateTimeUtc - UnixEpoch).TotalSeconds;
        }

        public static DateTime FromUnixTime(this long unixTime)
        {
            return UnixEpoch.AddMilliseconds(unixTime);
        }

        public static DateTime ToEpoch(this int unixTimeStamp)
        {
            return ToEpoch((long)unixTimeStamp);
        }

        public static DateTime ToEpoch(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = UnixEpoch;
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static byte[] ReceiveAll(this Socket socket)
        {
            var buffer = new List<byte>();

            while (socket.Available > 0)
            {
                var currByte = new Byte[1];
                var byteCounter = socket.Receive(currByte, currByte.Length, SocketFlags.None);

                if (byteCounter.Equals(1))
                {
                    buffer.Add(currByte[0]);
                }
            }

            return buffer.ToArray();
        }
    }
}