using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LumenWorks.Framework.IO.Csv;
using WebApp.Models;

namespace WebApp.Helpers
{
    internal class FileProcessor
    {
        #region Properties & fields

        internal List<string> Errors { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for FileProcessor
        /// </summary>
        /// <param name="errors">List for errors</param>
        internal FileProcessor(List<string> errors)
        {
            Errors = errors;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Read & validate the uploaded file
        /// </summary>
        /// <param name="filePath">file path on the server</param>
        /// <returns>List of CSVModels</returns>
        internal List<CSVModel> ProcessFile(string filePath)
        {
            var isDelimiterParsed = char.TryParse(CommonFunctions.GetApplicationSettingValue(Constants.Delimiter), out var result);
            var delimiter = isDelimiterParsed ? result : ';';
            var records = new List<CSVModel>();
            using (var stream = new StreamReader(filePath))
            using (var csv = new CsvReader(stream, true, delimiter))
            {
                var fieldCount = csv.FieldCount;
                var headers = csv.GetFieldHeaders();
                ValidateHeadersAndColumns(fieldCount, headers);
                var i = 0;
                while (csv.ReadNextRecord())
                {
                    var item = new CSVModel
                    {
                        BeginDate = csv[i, headers[0]],
                        IRR_ACB = csv[i, headers[1]],
                        Id = ++i
                    };
                    records.Add(item);
                }
            }

            return records;
        }

        /// <summary>
        /// Process CSVModels
        /// </summary>
        /// <param name="records"></param>
        /// <returns>List of Percent Models</returns>
        internal List<PercentModel> ProcessCSVModels(List<CSVModel> records)
        {
            var listOfPercents = new List<PercentModel>();
            var specificDateFormat = CommonFunctions.GetApplicationSettingValue(Constants.DateFormat);
            var dateFormat = !string.IsNullOrEmpty(specificDateFormat) ? specificDateFormat : "dd.MM.yyyy";
            var culture = CultureInfo.InvariantCulture;
            var dateStyle = DateTimeStyles.None;
            var doubleStyle = NumberStyles.Any;
            foreach (var record in records)
            {
                var isValidRecord = true;
                var item = new PercentModel
                {
                    Id = record.Id
                };
                var isDateParsed = DateTime.TryParseExact(record.BeginDate, dateFormat, culture, dateStyle, out var beginDate);

                if (isDateParsed)
                {
                    item.BeginDate = beginDate;
                }
                else
                {
                    isValidRecord = false;
                    Errors.Add($"Incorrect date {record.BeginDate} on row {record.Id}");
                }

                var isValueParsed = double.TryParse(record.IRR_ACB, doubleStyle, culture, out var irrResult);
                if (isValueParsed)
                {
                    item.IRR_ACB = irrResult;
                }
                else
                {
                    isValidRecord = false;
                    Errors.Add($"Incorrect value {record.IRR_ACB} on row {record.Id}");
                }

                if (isValidRecord)
                {
                    listOfPercents.Add(item);
                }
            }
            listOfPercents.Sort();

            ValidateForDuplicationOfDate(listOfPercents);

            CalculatePercents(listOfPercents);

            return listOfPercents;
        }

        #endregion

        #region Private methods

        private void ValidateHeadersAndColumns(int fieldCount, string[] headers)
        {
            if (fieldCount != 2)
            {
                Errors.Add("The file contains more than 2 columns.");
            }
            else if (!string.Equals(headers[0], CommonFunctions.GetApplicationSettingValue(Constants.Header0),
                StringComparison.InvariantCultureIgnoreCase))
            {
                Errors.Add(
                    $"The first header is not equal to {CommonFunctions.GetApplicationSettingValue(Constants.Header0)}");
            }
            else if (!string.Equals(headers[1], CommonFunctions.GetApplicationSettingValue(Constants.Header1),
                StringComparison.InvariantCultureIgnoreCase))
            {
                Errors.Add(
                    $"The second header is not equal to {CommonFunctions.GetApplicationSettingValue(Constants.Header1)}");
            }
        }

        private void ValidateForDuplicationOfDate(List<PercentModel> listOfPercents)
        {
            var duplications = listOfPercents.GroupBy(x => new { x.BeginDate })
                .Where(x => x.Skip(1).Any()).ToList();
            if (duplications.Any())
            {
                foreach (var duplication in duplications)
                {
                    Errors.Add("The list contains duplicates for BeginDate column " +
                               duplication.Key.BeginDate.ToShortDateString());
                }
            }
        }

        private static void CalculatePercents(List<PercentModel> listOfPercents)
        {
            var k = 1.0;
            foreach (var item in listOfPercents)
            {
                item.Percent = (k * (1 + item.IRR_ACB / 100) - 1) * 100;
                k *= 1 + item.IRR_ACB / 100;
            }
        }

        #endregion
    }
}