using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class FirmwareManager
    {
        private const string S4NHost = "http://www.scratch4.net/";
        private const string FirmwarePath = "content/firmware/";
        private const string FirmwareDictionaryFile = "firmware.json";

        private string _s4nPath;
        private string _fwPath;

        private FirmwareDictionary _firmwareDictionary;
        private object _fwDictLock = new object();

        public FirmwareManager()
        {
            EnsureDirectoryStructure();
            Task.Run(() => UpdateFirmwareDictionary());
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
            var destPath = Path.Combine(_fwPath, FirmwareDictionaryFile);
            var version = typeof(FirmwareManager).Assembly.GetName().Version;
            var versionString = version.Major.ToString() + "." + version.Minor;
            var uriString = S4NHost + FirmwarePath + versionString + "/" + FirmwareDictionaryFile;
            var uri = new Uri(uriString);

            var lastWrite = File.GetLastWriteTime(destPath);
            if (lastWrite.Year < 2014)
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
                    var fwDictPath = Path.Combine(_fwPath, FirmwareDictionaryFile);
                    var content = File.ReadAllText(fwDictPath);

                    _firmwareDictionary = JsonConvert.DeserializeObject<FirmwareDictionary>(content);
                }
                catch (FileNotFoundException fnfex)
                {
                    _firmwareDictionary = null;
                }
            }
        }
    }
}
