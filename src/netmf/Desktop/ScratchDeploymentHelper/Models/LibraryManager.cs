using Microsoft.ApplicationInsights;
//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//-------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Ninject;

using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;
using System.Deployment.Application;
using System.Reflection;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class LibraryManager
    {
        private readonly TelemetryClient _tc;
        private string _s4nPath;
        private string _fwPath;
        private string _libPath;

        public LibraryManager(TelemetryClient tc)
        {
            _tc = tc;
            EnsureDirectoryStructure();
        }

        private void EnsureDirectoryStructure()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _s4nPath = Path.Combine(appDataPath, "ScratchForDotNet");
            if (!Directory.Exists(_s4nPath))
                Directory.CreateDirectory(_s4nPath);
            _fwPath = Path.Combine(_s4nPath, "firmware");
            if (!Directory.Exists(_fwPath))
                Directory.CreateDirectory(_fwPath);
            _libPath = Path.Combine(_s4nPath, "libraries");
            if (!Directory.Exists(_libPath))
                Directory.CreateDirectory(_libPath);
        }

        public string GetLibrary(Guid id, Action<string> mh)
        {
            if (Settings.Default.OnlineDataUpdates)
                return this.GetLibraryFromInternet(id, mh);
            else
                return this.GetLibraryFromInstallation(id, mh);
        }

        private string GetLibraryFromInstallation(Guid id, Action<string> mh)
        {
            try
            {
            var path = Path.Combine(_libPath, id.ToString("N"));
            bool fDownload = false;
            if (!Directory.Exists(path))
                fDownload = true;
            if (!fDownload)
                return path;

            var packageName = id.ToString("N") + ".zip";

            mh(string.Format("Unpacking {0} ...", packageName));

            string sourcePath;

            if (ApplicationDeployment.IsNetworkDeployed)
                sourcePath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            else
                sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var sourceFilePath = Path.Combine(sourcePath, @"Assets\Installation", Constants.FirmwarePath, packageName);

            ZipFile.ExtractToDirectory(sourceFilePath, _libPath);

            return path;
            }
            catch (Exception ex)
            {
                mh(string.Format("ERROR: failed to locate and extract the code library due to an exception : {0}", ex.Message));
                _tc.TrackException(ex, new Dictionary<string, string>() 
                    {
                        { "libraryId", id.ToString() }
                    });
                return null;
            }

        }

        private string GetLibraryFromInternet(Guid id, Action<string> mh)
        {
            try
            {
                var path = Path.Combine(_libPath, id.ToString("N"));
                bool fDownload = false;
                if (!Directory.Exists(path))
                    fDownload = true;
                if (!fDownload)
                    return path;

                var packageName = id.ToString("N") + ".zip";
                
                mh(string.Format("Downloading {0} ...", packageName));

                var uriString = Constants.S4NHost + Constants.FirmwarePath + packageName;
                var uri = new Uri(uriString);
                var tempFile = Path.GetTempFileName();
                var client = new WebClient();
                client.DownloadFile(uri, tempFile);

                // unzip where it belongs
                ZipFile.ExtractToDirectory(tempFile, _libPath);

                return path;
            }
            catch (Exception ex)
            {
                mh(string.Format("ERROR: failed to download or extract the code library due to an exception : {0}",ex.Message));
                _tc.TrackException(ex, new Dictionary<string,string>() 
                    {
                        { "libraryId", id.ToString() }
                    });
                return null;
            }
        }
    }
}
