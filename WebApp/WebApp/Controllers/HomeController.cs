using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApp.Helpers;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        #region Fields & properties

        /// <summary>
        /// List of errors for validation of file
        /// </summary>
        internal List<string> Errors { get; private set; } = new List<string>();

        /// <summary>
        /// Answer whether provided file is valid.
        /// </summary>
        internal bool IsValid => Errors.Count == 0;

        #endregion

        #region Actions

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.MaximumFileSize = CommonFunctions.GetMaxRequestLength();
            ViewBag.AllowedFileExtensions = CommonFunctions.GetApplicationSettingValue(Constants.AllowedFileExtensionsKey);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            try
            {
                ValidateFile(file);

                if (IsValid)
                {
                    var filePath = SaveFile(file);
                    var model = ProcessFile(filePath);

                    if (IsValid)
                    {
                        var chartModel = new ChartModel
                        {
                            Categories = model.Aggregate(
                                    new StringBuilder(),
                                    (builder, item) =>
                                        builder.AppendFormat("\'{0}\',", item.BeginDate.Date.ToShortDateString()))
                                .ToString()
                                .TrimEnd(','),
                            Data = model.Aggregate(
                                    new StringBuilder(),
                                    (builder, item) =>
                                        builder.AppendFormat(CultureInfo.InvariantCulture, "{0},", item.Percent))
                                .ToString()
                                .TrimEnd(',')
                        };
                        return View("Chart", chartModel);
                    }
                }
                ViewBag.MaximumFileSize = CommonFunctions.GetMaxRequestLength();
                ViewBag.AllowedFileExtensions = CommonFunctions.GetApplicationSettingValue(Constants.AllowedFileExtensionsKey);
                ViewBag.Message = ErrorsToString();
                return View("Index");
            }
            catch (Exception exception)
            {
                Errors.Add("File upload failed!!");
                Errors.Add(exception.Message);
                ViewBag.Message = ErrorsToString();
                return View("Index");
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Aggregate errors into string
        /// </summary>
        /// <returns>Errors</returns>
        private string ErrorsToString()
        {
            var errMsg = Errors.Aggregate(new StringBuilder(), (builder, item) => builder.AppendFormat("{0},", item));
            return errMsg.ToString().TrimEnd(',');
        }

        /// <summary>
        /// Validate the provided file
        /// </summary>
        /// <param name="file">File</param>
        private void ValidateFile(HttpPostedFileBase file)
        {
            var fileValidator = new FileValidator(Errors);
            fileValidator.Validate(file);
        }

        /// <summary>
        /// Save the file on the server
        /// </summary>
        /// <param name="file">File</param>
        /// <returns>Path to the file on the server</returns>
        private string SaveFile(HttpPostedFileBase file)
        {
            var fileName = Path.GetFileName(file.FileName);

            var folderPath = Server.MapPath(CommonFunctions.GetApplicationSettingValue(Constants.UploadsFolderPath));
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var safeFileName = FileUploader.WindowsSafeFileName(fileName);
            var uniqueFileName = DateTime.UtcNow.Ticks + safeFileName;
            var path = Path.Combine(folderPath, uniqueFileName);

            file.SaveAs(path);
            return path;
        }

        /// <summary>
        /// Process the uploaded file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>List of Percent Models</returns>
        private List<PercentModel> ProcessFile(string filePath)
        {
            var fp = new FileProcessor(Errors);
            var dalModels = fp.ProcessFile(filePath);
            return fp.ProcessCSVModels(dalModels);
        }

        #endregion
    }
}