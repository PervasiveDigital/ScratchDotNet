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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.Common
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
