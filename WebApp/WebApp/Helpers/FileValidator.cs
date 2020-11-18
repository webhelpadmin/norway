using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApp.Helpers
{
    internal class FileValidator
    {
        #region Properties & fields

        internal List<string> Errors { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for FileValidator
        /// </summary>
        /// <param name="errors">List for errors</param>
        internal FileValidator(List<string> errors)
        {
            Errors = errors;
        }

        #endregion

        /// <summary>
        /// Validate the file by Size, length name, extension
        /// </summary>
        /// <param name="file">File</param>
        internal void Validate(HttpPostedFileBase file)
        {
            if (file == null)
            {
                Errors.Add("The file is not provided.");
                return;
            }
            CheckContentLength(file.ContentLength);
            CheckSupportedSize(file.ContentLength);
            CheckFileNameLength(file.FileName);
            CheckFileExtension(file.FileName);
        }

        #region Private Methods

        private void CheckContentLength(int length)
        {
            bool result = length > 0;
            if (!result)
            {
                Errors.Add("The file is of 0 kb size.");
            }
        }

        private void CheckSupportedSize(int size)
        {
            var maxContentLength = CommonFunctions.GetMaxRequestLength();
            bool result = size < maxContentLength;
            if (!result)
            {
                Errors.Add($"Invalid file size. Please upload a file less than {maxContentLength / 1024} MB.");
            }
        }

        private void CheckFileNameLength(string fileName)
        {
            var maxFileNameLength = GetMaxFileNameLength(Constants.MaxFileNameLength);
            bool result = fileName.Length < maxFileNameLength;
            if (!result)
            {
                Errors.Add($"File name length should be less than {maxFileNameLength} symbols.");
            }
        }

        private void CheckFileExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            IList<string> allowedFileExtensions = GetSupportedFileTypes(Constants.AllowedFileExtensionsKey);
            if (!string.IsNullOrEmpty(extension))
            {
                if (allowedFileExtensions.Contains(extension.ToLower()))
                {
                    return;
                }
            }
            Errors.Add(("You have not selected an accepted file format to upload. Please upload: " +
                        string.Join(",", allowedFileExtensions.ToArray()) + "."));
        }

        private static IList<string> GetSupportedFileTypes(string settingsKey)
        {
            string supportedUploadingDocFileExtensions =
                CommonFunctions.GetApplicationSettingValue(settingsKey);
            return supportedUploadingDocFileExtensions.Split(',');
        }

        private static int GetMaxFileNameLength(string settingsKey)
        {

            int result = 150;
            string section = CommonFunctions.GetApplicationSettingValue(settingsKey);

            if (section != null)
            {
                int.TryParse(section, out result);
            }
            return result;
        }

        #endregion
    }
}
