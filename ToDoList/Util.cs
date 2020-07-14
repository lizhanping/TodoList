/*
*---------------------------------
*|		All rights reserved.
*|		author: lizhanping
*|		version:1.0
*|		File: Util.cs
*|		Summary: 
*|		Date: 2020/3/3 13:29:57
*---------------------------------
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace ToDoList
{
    /// <summary>
    /// 帮助类
    /// </summary>
    public class Util
    {
        /// <summary>
        /// 获取key值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetAppSetting(string key)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                    return config.AppSettings.Settings[key].Value;
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 设置Key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetAppSetting(string key,string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                    config.AppSettings.Settings[key].Value = value.ToString();
                else
                    config.AppSettings.Settings.Add(key, value.ToString());
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch
            {
                return;
            }
        }
    }
}
