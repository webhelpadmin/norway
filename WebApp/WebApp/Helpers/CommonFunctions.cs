using System.Configuration;
using System.Web.Configuration;

namespace WebApp.Helpers
{
    internal class CommonFunctions
    {
        /// <summary>
        /// Get the value of the specified key from the application settings section in web.config.
        /// </summary>
        /// <param name="key">The key whose value is to be retrieved.</param>
        /// <returns></returns>
        internal static string GetApplicationSettingValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Get maximum request length specified in the web.config or default value
        /// </summary>
        /// <returns>integer value of MaxRequestLength size</returns>
        internal static int GetMaxRequestLength()
        {
            int result = 25 * 1024 * 1024;
            if (ConfigurationManager.GetSection("system.web/httpRuntime") is HttpRuntimeSection section)
            {
                result = section.MaxRequestLength * 1024 - 5 * 1024 * 1024;
            }
            return result;
        }

    }
}