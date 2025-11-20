using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorthStar4.Infrastructure
{
    //public static class HttpUtility
    //{
    //    public static NameValueCollection ParseQueryString(string query)
    //    {
    //        return ParseQueryString(query, Encoding.UTF8);
    //    }

    //    sealed class HttpQSCollection : NameValueCollection
    //    {
    //        public override string ToString()
    //        {
    //            int count = Count;
    //            if (count == 0)
    //                return "";
    //            StringBuilder sb = new StringBuilder();
    //            string[] keys = AllKeys;
    //            for (int i = 0; i < count; i++)
    //            {
    //                sb.AppendFormat("{0}={1}&", keys[i], UrlEncode(this[keys[i]]));
    //            }
    //            if (sb.Length > 0)
    //                sb.Length--;
    //            return sb.ToString();
    //        }
    //    }

    //    public static string UrlEncode(string str)
    //    {
    //        return UrlEncode(str, Encoding.UTF8);
    //    }

    //    public static string UrlEncode(string s, Encoding Enc)
    //    {
    //        if (s == null)
    //            return null;

    //        if (s == String.Empty)
    //            return String.Empty;

    //        bool needEncode = false;
    //        int len = s.Length;
    //        for (int i = 0; i < len; i++)
    //        {
    //            char c = s[i];
    //            if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z'))
    //            {
    //                if (HttpEncoder.NotEncoded(c))
    //                    continue;

    //                needEncode = true;
    //                break;
    //            }
    //        }

    //        if (!needEncode)
    //            return s;

    //        // avoided GetByteCount call
    //        byte[] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
    //        int realLen = Enc.GetBytes(s, 0, s.Length, bytes, 0);
    //        return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes, 0, realLen));
    //    }

    //    public static NameValueCollection ParseQueryString(string query, Encoding encoding)
    //    {
    //        if (query == null)
    //            throw new ArgumentNullException("query");
    //        if (encoding == null)
    //            throw new ArgumentNullException("encoding");
    //        if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
    //            return new HttpQSCollection();
    //        if (query[0] == '?')
    //            query = query.Substring(1);

    //        NameValueCollection result = new HttpQSCollection();
    //        ParseQueryString(query, encoding, result);
    //        return result;
    //    }

    //    internal static void ParseQueryString(string query, Encoding encoding, NameValueCollection result)
    //    {
    //        if (query.Length == 0)
    //            return;

    //        string decoded = HtmlDecode(query);
    //        int decodedLength = decoded.Length;
    //        int namePos = 0;
    //        bool first = true;
    //        while (namePos <= decodedLength)
    //        {
    //            int valuePos = -1, valueEnd = -1;
    //            for (int q = namePos; q < decodedLength; q++)
    //            {
    //                if (valuePos == -1 && decoded[q] == '=')
    //                {
    //                    valuePos = q + 1;
    //                }
    //                else if (decoded[q] == '&')
    //                {
    //                    valueEnd = q;
    //                    break;
    //                }
    //            }

    //            if (first)
    //            {
    //                first = false;
    //                if (decoded[namePos] == '?')
    //                    namePos++;
    //            }

    //            string name, value;
    //            if (valuePos == -1)
    //            {
    //                name = null;
    //                valuePos = namePos;
    //            }
    //            else
    //            {
    //                name = UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1), encoding);
    //            }
    //            if (valueEnd < 0)
    //            {
    //                namePos = -1;
    //                valueEnd = decoded.Length;
    //            }
    //            else
    //            {
    //                namePos = valueEnd + 1;
    //            }
    //            value = UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos), encoding);

    //            result.Add(name, value);
    //            if (namePos == -1)
    //                break;
    //        }
    //    }
    //}
}
