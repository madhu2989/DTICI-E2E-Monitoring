using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Daimler.Providence.Service.BusinessLogic.Interfaces;
using Daimler.Providence.Service.Models;

namespace Daimler.Providence.Service.BusinessLogic
{
    /// <summary>
    /// Class which provides logic for exporting/importing Environments.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LicenseInformationManager : ILicenseInformationManager
    {
        #region Private Members 

        private const string Filename = "LicenseInformation.csv";

        #endregion

        #region Constructor

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public LicenseInformationManager(){}

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<List<LicenseInformation>> GetLicenseInformationAsync(CancellationToken token)
        {
            var licenseInformation = new List<LicenseInformation>();

            // Read data from the csv file
            var csvContent = await ReadCsvFileAsync(token).ConfigureAwait(false);
            foreach (var record in csvContent)
            {
                if (record.ContainsKey("Match type") && record["Match type"] == "Direct Dependency")
                {
                    var licenseInformation1 = new LicenseInformation
                    {
                        Package = record["Component name"],
                        Version = record["Channel versions"],
                        License = record["License names"]
                    };
                    licenseInformation.Add(licenseInformation1);
                }
            }
            return licenseInformation;
        }

        #endregion

        #region Private Methods

        private async Task<List<Dictionary<string, string>>> ReadCsvFileAsync(CancellationToken token)
        {
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase), $"Resources/{Filename}").Remove(0, 6);
            var lines = await File.ReadAllLinesAsync(path, token).ConfigureAwait(false);
            var csvDataArray = new List<string[]>();
            foreach (string line in lines)
            {
                csvDataArray.Add(line.Split(','));
            }

            var properties = lines[0].Split(',');
            var csvDataDictionary = new List<Dictionary<string, string>>();
            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    objResult.Add(properties[j], csvDataArray[i][j]);
                }
                csvDataDictionary.Add(objResult);
            }
            return csvDataDictionary;
        }

        #endregion
    }
}