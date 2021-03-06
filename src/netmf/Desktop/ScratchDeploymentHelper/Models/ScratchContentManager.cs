﻿using Microsoft.ApplicationInsights;
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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;
using System.Deployment.Application;
using System.Reflection;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class ScratchContentManager
    {
        private readonly TelemetryClient _tc;
        private string _root;

        public ScratchContentManager(TelemetryClient tc)
        {
            _tc = tc;
            EnsureDirectoryStructure();
        }

        public async Task UpdateScratchContent(IEnumerable<string> fileNames)
        {
            if (Settings.Default.ClassroomMode)
                UpdateScratchContentFromInstallation(fileNames);
            else
                await UpdateScratchContentFromInternet(fileNames);
        }

        private void UpdateScratchContentFromInstallation(IEnumerable<string> fileNames)
        {
            string sourcePath;

            if (ApplicationDeployment.IsNetworkDeployed)
                sourcePath = ApplicationDeployment.CurrentDeployment.DataDirectory;
            else
                sourcePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (var file in fileNames)
            {
                var sourceFilePath = Path.Combine(sourcePath, @"Assets\Installation", Constants.ScratchExtensionsPath, file);
                var destPath = Path.Combine(_root, file);

                var sourceLastWrite = File.GetLastWriteTimeUtc(sourceFilePath);
                var destLastWrite = File.GetLastWriteTimeUtc(destPath);

                if (destLastWrite < sourceLastWrite)
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);

                    File.Copy(sourceFilePath, destPath, true);
                }
            }
        }

        private async Task UpdateScratchContentFromInternet(IEnumerable<string> fileNames)
        {
            foreach (var file in fileNames)
            {
                try
                {
                    var destPath = Path.Combine(_root, file);
                    var lastWrite = File.GetLastWriteTimeUtc(destPath);

                    var source = Constants.S4NHost + Constants.ScratchExtensionsPath + file;

                    var req = (HttpWebRequest)WebRequest.Create(source);
                    req.IfModifiedSince = lastWrite;
                    using (var response = (HttpWebResponse)await req.GetResponseAsync())
                    {
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            var body = sr.ReadToEnd();
                            File.WriteAllText(destPath, body);
                        }
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Response != null)
                    {
                        var status = ((HttpWebResponse)wex.Response).StatusCode;
                        if (status == HttpStatusCode.NotModified)
                        {
                            // no big deal - we have the latest copy
                        }
                        else
                        {
                            // error - file not updated
                            _tc.TrackException(wex);
                        }
                    }
                    else
                    {
                        // error - file not updated
                        _tc.TrackException(wex);
                    }
                }
                catch (Exception ex)
                {
                    // log it, but keep going
                    _tc.TrackException(ex);
                }
            }
        }

        private void EnsureDirectoryStructure()
        {
            _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Scratch4.Net");
            if (!Directory.Exists(_root))
                Directory.CreateDirectory(_root);
        }
    }
}
