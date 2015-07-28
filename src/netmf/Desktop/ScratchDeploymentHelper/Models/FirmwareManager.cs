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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

using Ninject;

using Newtonsoft.Json;

using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.Models
{
    public class FirmwareManager
    {
        private const string FirmwareDictionaryFile = "firmware-{0}.{1}.json";
        private static Version FirmwareVersion = new Version(1, 0);

        private readonly TelemetryClient _tc;
        private readonly LibraryManager _libmgr;
        private readonly ScratchContentManager _scm;

        private string _s4nPath;
        private string _fwPath;
        private string _fwDictPath;
        private string _fwDictFileName;

        private FirmwareDictionary _firmwareDictionary;
        private readonly AsyncLock _fwDictLock = new AsyncLock();

        public FirmwareManager(TelemetryClient tc, LibraryManager libmgr, ScratchContentManager scm)
        {
            _tc = tc;
            _libmgr = libmgr;
            _scm = scm;
        }

        public async Task Initialize()
        {
            await EnsureDirectoryStructure();
        }

        public FirmwareImage GetImage(Guid imageId)
        {
            var image = _firmwareDictionary.Images.FirstOrDefault(x => x.Id == imageId);
            return image;        
        }

        public IEnumerable<FirmwareHost> FindMatchingBoards(Version targetFrameworkVersion, string usbString, string clrBuildInfo, int oem, int sku)
        {
            // Start with usb string + OEM + SKU matches, then company matches, then targetframework
            var candidates = new List<Tuple<FirmwareHost, int>>();
            foreach (var board in _firmwareDictionary.Boards)
            {
                int score = 0;
                if (board.UsbName == usbString)
                    score += 4;
                if (clrBuildInfo.IndexOf(board.BuildInfoContains) != -1)
                    ++score;
                if (board.OEM == oem)
                {
                    ++score;
                    if (board.SKU == sku)
                        ++score;
                }
                candidates.Add(new Tuple<FirmwareHost, int>(board, score));
            }

            var sorted = candidates.OrderByDescending(x => x.Item2);
            return sorted.Select(tpl => tpl.Item1).ToList();
        }

        public IEnumerable<FirmwareImage> GetCompatibleImages(Guid hostId)
        {
            var result = new List<FirmwareImage>();
            var host = _firmwareDictionary.Boards.FirstOrDefault(x => x.Id == hostId);
            if (host!=null)
            {
                foreach (var id in host.CompatibleImages)
                {
                    var image = _firmwareDictionary.Images.FirstOrDefault(x => x.Id == id);
                    if (image != null)
                        result.Add(image);
                }
            }
            return result;
        }

        public ReadOnlyCollection<FirmwareImage> Images
        {
            get
            {
                if (_firmwareDictionary == null)
                    return null;
                else
                    return new ReadOnlyCollection<FirmwareImage>(_firmwareDictionary.Images);
            }
        }

        public async Task<byte[]> GetImageForBoard(Guid id)
        {
            var board = _firmwareDictionary.Boards.FirstOrDefault(x => x.Id == id);
            if (board == null)
                return null;
            var path = Path.Combine(_s4nPath, "images", board.ProductImageName);

            try
            {
                if (File.Exists(path))
                {
                    var result = File.ReadAllBytes(path);
                    if (result.Length > 0)
                        return result;
                }
            }
            catch
            {
                // ignore exceptions here and try to read from the net again.
            }

            try
            {
                // Read from the network
                var networkPath = Constants.S4NHost + Constants.ImagePath + board.ProductImageName;
                var buffer = new byte[4096];
                var req = (HttpWebRequest)WebRequest.Create(networkPath);
                using (var response = (HttpWebResponse)await req.GetResponseAsync())
                {
                    var output = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                    using (var stream = response.GetResponseStream())
                    {
                        int count = 0;
                        do
                        {
                            count = await stream.ReadAsync(buffer, 0, buffer.Length);
                            if (count > 0)
                                output.Write(buffer, 0, count);
                        } while (count > 0);
                    }
                    output.Close();
                }
                return File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                _tc.TrackException(ex, new Dictionary<string, string>() 
                    {
                        { "boardId", id.ToString() }
                    });
                return null;
            }
        }

        private async Task EnsureDirectoryStructure()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _s4nPath = Path.Combine(appDataPath, "ScratchForDotNet");
            if (!Directory.Exists(_s4nPath))
                Directory.CreateDirectory(_s4nPath);
            _fwPath = Path.Combine(_s4nPath, "firmware");
            if (!Directory.Exists(_fwPath))
                Directory.CreateDirectory(_fwPath);
            var imagePath = Path.Combine(_s4nPath, "images");
            if (!Directory.Exists(imagePath))
                Directory.CreateDirectory(imagePath);
            await ReadFirmwareDictionary(false);
            if (_firmwareDictionary != null)
                await UpdateDownloadableAssets();
        }

        public async Task UpdateFirmwareDictionary()
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
                    // We've never retrieved the file - go get a copy unconditionally
                    // Use a temp file because DownloadFile will create a zero-length file if there is an error retrieving the web content
                    var tempFile = Path.GetTempFileName();
                    var client = new WebClient();
                    client.DownloadFile(uri, tempFile);
                    File.Copy(tempFile, destPath, true);
                    await ReadFirmwareDictionary(true);
                }
                catch (Exception ex)
                {
                    _tc.TrackException(ex);
                    _firmwareDictionary = null;
                }
            }
            else
            {
                try
                {
                    // The file exists locally - get it only if it is newer on the server
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
                    await ReadFirmwareDictionary(true);
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
                    _tc.TrackException(ex);
                }
                if (_firmwareDictionary == null || _firmwareDictionary.Boards.Count == 0)
                    throw new Exception("Unable to load firmware dictionary. Program cannot start");
            }
        }

        public async Task<List<Tuple<FirmwareAssembly, byte[]>>> GetAssembliesForImage(Guid id, Action<string> mh)
        {
            if (_firmwareDictionary == null)
            {
                mh("ERROR: No database of firmware images is available. Connect to the internet and restart this program to download firmware.");
                return null;
            }

            var image = _firmwareDictionary.Images.FirstOrDefault(x => x.Id == id);
            if (image == null)
            {
                mh("ERROR: Firmware image not found");
                _tc.TrackEvent("dictMissingImage", new Dictionary<string, string>()
                        {
                            { "imageId", id.ToString() },
                        });
                return null;
            }

            var result = new List<Tuple<FirmwareAssembly, byte[]>>();

            foreach (var assemId in image.RequiredAssemblies)
            {
                var assemblyInfo = _firmwareDictionary.Assemblies.FirstOrDefault(x => x.Id == assemId);
                if (assemblyInfo==null)
                {
                    mh(string.Format("The firmware database is corrupt. Missing assembly record {0}", assemId.ToString()));
                    _tc.TrackEvent("fwmgrDictMissingAssem", new Dictionary<string, string>()
                        {
                            { "imageId", id.ToString() },
                            { "assemId", id.ToString() },
                        });
                    return null;
                }
                var libPath = _libmgr.GetLibrary(assemblyInfo.LibraryId, mh);
                if (string.IsNullOrEmpty(libPath))
                {
                    mh("ERROR: Failed to download library");
                    return null;
                }

                byte[] data = null;
                try
                {
                    var path = Path.Combine(
                        libPath,
                        assemblyInfo.IsLittleEndian ? "le" : "be",
                        assemblyInfo.Filename);
                    data = File.ReadAllBytes(path);
                }
                catch (Exception ex)
                {
                    mh(string.Format("ERROR: Exception while reading assembly for deployment : {0}", ex.Message));
                    _tc.TrackException(ex, new Dictionary<string, string>()
                        {
                            { "imageId", id.ToString() },
                            { "assemId", assemId.ToString() },
                        });
                    return null;
                }
                if (data!=null)
                {
                    result.Add(Tuple.Create(assemblyInfo,data));
                }
                else
                {
                    _tc.TrackEvent("fwmgrZeroLengthAssem", new Dictionary<string, string>()
                        {
                            { "imageId", id.ToString() },
                            { "assemId", id.ToString() },
                        });
                    return null;
                }
            }

            return result;
        }

        private async Task ReadFirmwareDictionary(bool fDownloadScratchDocuments)
        {
            using (var releaser = await _fwDictLock.LockAsync())
            {
                try
                {
                    var content = File.ReadAllText(GetFirmwareDictionaryPath());

                    _firmwareDictionary = JsonConvert.DeserializeObject<FirmwareDictionary>(content);

                    if (fDownloadScratchDocuments)
                        await UpdateDownloadableAssets();
                }
                catch (FileNotFoundException fnfex)
                {
                    _firmwareDictionary = null;
                }
                catch (Exception ex)
                {
                    _firmwareDictionary = null;
                    _tc.TrackException(ex);
                }
            }
        }

        private async Task UpdateDownloadableAssets()
        {
            var files = new HashSet<string>();
            foreach (var item in _firmwareDictionary.Images)
            {
                if (!string.IsNullOrEmpty(item.ScratchExtension) &&
                    !files.Contains(item.ScratchExtension))
                {
                    files.Add(item.ScratchExtension);
                }
            }
            await _scm.UpdateScratchContent(files);

            var xmgr = App.Kernel.Get<ExtensionManager>();
            await xmgr.UpdatePlugins();
        }

        private string GetFirmwareDictionaryFileName()
        {
            if (string.IsNullOrEmpty(_fwDictFileName))
            {
                _fwDictFileName = string.Format(FirmwareDictionaryFile, FirmwareVersion.Major, FirmwareVersion.Minor);
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
