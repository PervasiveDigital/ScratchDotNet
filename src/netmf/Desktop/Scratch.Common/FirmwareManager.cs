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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class FirmwareManager
    {
        private const string FirmwareDictionaryFile = "firmware-{0}.{1}.json";

        private readonly LibraryManager _libmgr;

        private string _s4nPath;
        private string _fwPath;
        private string _fwDictPath;
        private string _fwDictFileName;

        private FirmwareDictionary _firmwareDictionary;
        private object _fwDictLock = new object();

        public FirmwareManager(LibraryManager libmgr)
        {
            _libmgr = libmgr;
            EnsureDirectoryStructure();
            Task.Run(() => UpdateFirmwareDictionary());
        }

        public IEnumerable<FirmwareImage> FindCompatibleImages(bool bestMatchesOnly, Version targetFrameworkVersion, string usbString, int oem, int sku)
        {
            // Start with usb string + OEM + SKU matches, then company matches, then targetframework
            var candidates = new List<Tuple<FirmwareImage,int>>();
            foreach (var image in _firmwareDictionary.Images)
            {
                int score = 0;
                // if this doesn't match, then nothing else matters
                if (image.TargetFrameworkVersion != targetFrameworkVersion)
                    continue;
                if (image.TargetDeviceUsbName == usbString)
                    ++score;
                if (image.OEM == oem)
                {
                    ++score;
                    if (image.SKU == sku)
                        ++score;
                }
                if (!bestMatchesOnly || score > 0)
                    candidates.Add(new Tuple<FirmwareImage,int>(image,score));
            }

            var sorted = candidates.OrderByDescending(x => x.Item2);
            return sorted.Select(tpl => tpl.Item1).ToList();
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
            ReadFirmwareDictionary();
        }

        private async void UpdateFirmwareDictionary()
        {
            var destPath = GetFirmwareDictionaryPath();
            var uriString = Constants.S4NHost + Constants.FirmwarePath + GetFirmwareDictionaryFileName();
            var uri = new Uri(uriString);

            var lastWrite = File.GetLastWriteTime(destPath);
            // Missing files use a year of 1601 instead of DateTime.MinValue. Treat anything
            //   before 2015 as a missing file, because unless I start time traveling, that is
            //   an invalid file.
            if (lastWrite.Year < 2015)
            {
                try
                {
                    // We've never retrieved the file - got get a copy unconditionally
                    // Use a temp file because DownloadFile will create a zero-length file if there is an error retrieving the web content
                    var tempFile = Path.GetTempFileName();
                    var client = new WebClient();
                    client.DownloadFile(uri, tempFile);
                    File.Copy(tempFile, destPath, true);
                    ReadFirmwareDictionary();
                }
                catch (WebException wex)
                {
                    _firmwareDictionary = null;
                }
            }
            else
            {
                try
                {
                    // The file exists locally - get it only if it is newer on the server
                    var buffer = new byte[1024];
                    var comparisonTime = lastWrite.ToUniversalTime();
                    var req = (HttpWebRequest)WebRequest.Create(uri);
                    req.IfModifiedSince = comparisonTime;
                    using (var response = (HttpWebResponse)await req.GetResponseAsync())
                    {
                        using (var sr = new StreamReader(response.GetResponseStream()))
                        {
                            var body = sr.ReadToEnd();
                            File.WriteAllText(destPath, body);
                        }
                    }
                    ReadFirmwareDictionary();
                }
                catch (WebException wex)
                {
                    if (wex.Response!=null)
                    {
                        var status = ((HttpWebResponse)wex.Response).StatusCode;
                        if (status == HttpStatusCode.NotModified)
                        {
                            // no big deal - we have the latest copy
                        }
                        else
                        {
                            // error - file not updated
                        }
                    }
                    else
                    {
                        // error - file not updated
                    }
                }
            }
        }

        private void ReadFirmwareDictionary()
        {
            lock (_fwDictLock)
            {
                try
                {
                    var content = File.ReadAllText(GetFirmwareDictionaryPath());

                    _firmwareDictionary = JsonConvert.DeserializeObject<FirmwareDictionary>(content);
                }
                catch (FileNotFoundException fnfex)
                {
                    _firmwareDictionary = null;
                }
            }
        }

        private string GetFirmwareDictionaryFileName()
        {
            if (string.IsNullOrEmpty(_fwDictFileName))
            {
                var version = typeof(FirmwareManager).Assembly.GetName().Version;
                _fwDictFileName = string.Format(FirmwareDictionaryFile, version.Major, version.Minor);
            }
            return _fwDictFileName;
        }

        private string GetFirmwareDictionaryPath()
        {
            if (string.IsNullOrEmpty(_fwDictPath))
            {
                var filename = GetFirmwareDictionaryFileName();
                _fwDictPath = Path.Combine(_fwPath, filename);
            }
            return _fwDictPath;
        }
    }
}
