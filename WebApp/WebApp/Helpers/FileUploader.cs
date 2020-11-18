using System.Linq;

namespace WebApp.Helpers
{
    public static class FileUploader
    {

        /// <summary>
        /// Converts the given filename to a system friendly filename.
        /// </summary>
        /// <param name="fileName">Filename to convert (do not include file path)</param>
        /// <returns>System friendly filename</returns>
        internal static string WindowsSafeFileName(string fileName)
        {
            return WindowsSafeFileName(fileName, '_');
        }

        /// <summary>
        /// Converts the given filename to a system friendly filename.
        /// </summary>
        /// <param name="fileName">Filename to convert (do not include file path)</param>
        /// <param name="replacementChar">Character that should replace invalid characters</param>
        /// <returns>System friendly filename</returns>
        /// <remarks> Replaces ' \ / : * ? " &lt; &gt; | and etc characters with the replacement character. </remarks>
        private static string WindowsSafeFileName(string fileName, char replacementChar)
        {

            char[] delimiters = { '\'', '\\', '/', ':', '*', '?', '"', '<', '>', '|', '!', '@', '#', '$', '%', '^', '&', '{', '}', '`', '=', '~', ',', ' ', '[', ']' };

            if (fileName.IndexOfAny(delimiters) >= 0)
            {
                fileName = delimiters.Aggregate(fileName, (current, t) => current.Replace(t, replacementChar));
            }

            return fileName;
        }
    }
}