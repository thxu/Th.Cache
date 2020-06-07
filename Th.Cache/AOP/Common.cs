using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using ArxOne.MrAdvice.Advice;
using Newtonsoft.Json;

namespace Th.Cache.AOP
{
    internal static class Common
    {
        /// <summary>
        /// 获取缓存key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context">函数执行上下文</param>
        /// <returns>key</returns>
        internal static string GetKey(this string key, MethodAdviceContext context)
        {
            string prefix = "Th_AutoCache_";
            if (string.IsNullOrWhiteSpace(key))
            {
                var defaultKey = GetDefaultKey(context);
                if (string.IsNullOrWhiteSpace(defaultKey))
                {
                    return string.Empty;
                }
                return prefix + defaultKey;
            }

            List<string> paramNames = GetParamNameOfKey(key);

            if (paramNames == null || !paramNames.Any())
            {
                return prefix + key;
            }

            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                foreach (string paramName in paramNames)
                {
                    var val = SerializeObject(GetComplexVal(paramName, context));
                    dic.Add(paramName, val);
                }

                string keyTmp = key;
                foreach (KeyValuePair<string, string> keyValuePair in dic)
                {
                    keyTmp = keyTmp.Replace("{" + keyValuePair.Key + "}", keyValuePair.Value);
                }
                return prefix + ToMd5(keyTmp);
            }
            catch (Exception)
            {
                return prefix + GetDefaultKey(context);
            }
        }

        /// <summary>
        /// 获取默认key
        /// </summary>
        /// <param name="context">函数调用上下文</param>
        /// <returns>输入默认key</returns>
        internal static string GetDefaultKey(this MethodAdviceContext context)
        {
            try
            {
                List<string> argList = new List<string>();
                IList<object> arguments = context.Arguments;
                ParameterInfo[] parameters = context.TargetMethod.GetParameters();
                if (parameters.Length <= 0)
                {
                    return string.Empty;
                }
                for (int i = 0; arguments != null && i < arguments.Count; i++)
                {
                    var md5Str = ToMd5(ToJson(arguments[i]));
                    argList.Add(md5Str);
                }

                if (argList.Count <= 0)
                {
                    return string.Empty;
                }
                return ToMd5(string.Join("", argList));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 从参数中解析key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static List<string> GetParamNameOfKey(this string key)
        {
            List<string> res = new List<string>();
            try
            {
                StringBuilder paramTmp = new StringBuilder();
                bool flg = false;
                foreach (char ch in key)
                {
                    if (ch == '{' || ch == '}')
                    {
                        if (ch == '{')
                        {
                            flg = true;
                            paramTmp.Clear();
                            continue;
                        }

                        if (ch == '}')
                        {
                            flg = false;
                            res.Add(paramTmp.ToString());
                        }
                    }

                    if (flg)
                    {
                        paramTmp.Append(ch);
                    }
                }
            }
            catch (Exception)
            {
                return res;
            }

            return res;
        }

        /// <summary>
        /// 获取参数中的key值
        /// </summary>
        /// <param name="param">参数</param>
        /// <param name="context">函数执行上下文</param>
        /// <returns>参数值</returns>
        internal static object GetComplexVal(this string param, MethodAdviceContext context)
        {
            var paramTmps = param.Split('.');
            ParameterInfo[] parameters = context.TargetMethod.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Name == paramTmps[0])
                {
                    Type ts = context.Arguments[i].GetType();

                    object obj = DeepCopy(context.Arguments[i], ts);
                    for (int j = 1; j < paramTmps.Length; j++)
                    {
                        var val = obj.GetType().GetProperty(paramTmps[j])?.GetValue(obj, null);
                        if (val != null) obj = DeepCopy(val, val.GetType());
                    }

                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// object序列化JSON字符串扩展方法
        /// </summary>
        /// <param name="obj">object及其子类对象</param>
        /// <returns>JSON字符串</returns>
        internal static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            try
            {
                JsonSerializerSettings settting = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss", Formatting = Formatting.Indented, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                return JsonConvert.SerializeObject(obj, settting);
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 加密32
        /// </summary>
        /// <param name="source">数据</param>
        /// <returns>密文</returns>
        internal static string ToMd5(this string source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
            StringBuilder sb = new StringBuilder(32);
            foreach (byte item in t)
            {
                sb.Append(item.ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// XML序列化方式深复制
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="type">对象类型</param>
        /// <returns>复制对象</returns>
        internal static object DeepCopy(this object obj, Type type)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                XmlSerializer xml = new XmlSerializer(type);
                xml.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                retval = xml.Deserialize(ms);
                ms.Close();
            }

            return retval;
        }

        /// <summary>
        /// JSON序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>JSON字符串</returns>
        internal static string SerializeObject(this object obj)
        {
            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" };
                return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
