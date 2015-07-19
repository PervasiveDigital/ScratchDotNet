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
using System.Diagnostics.Tracing;
using System.IO;
using System.Text;

namespace PervasiveDigital.Scratch.DeploymentHelper.Common
{
    class Logger : EventSource, ILogger
    {
        public Logger()
        {
        }

        [NonEvent]
        public void Debug(string message, string file = "", string member = "", int line = 0)
        {
            if (!string.IsNullOrEmpty(file))
                file = Path.GetFileName(file);
            debug(file, member, line, message);
        }

        [NonEvent]
        public void Info(string message, string file = "", string member = "", int line = 0)
        {
            if (!string.IsNullOrEmpty(file))
                file = Path.GetFileName(file);
            info(file, member, line, message);
        }

        [NonEvent]
        public void Warning(string message, string file = "", string member = "", int line = 0)
        {
            if (!string.IsNullOrEmpty(file))
                file = Path.GetFileName(file);
            warning(file, member, line, message);
        }

        [NonEvent]
        public void Error(string message, string file = "", string member = "", int line = 0)
        {
            if (!string.IsNullOrEmpty(file))
                file = Path.GetFileName(file);
            error(file, member, line, message);
        }

        [NonEvent]
        public void Critical(string message, string file = "", string member = "", int line = 0)
        {
            if (!string.IsNullOrEmpty(file))
                file = Path.GetFileName(file);
            critical(file, member, line, message);
        }

        //
        //  The actual events
        //     These have to be implemented in this fashion because if they appear in the interface, then they are marked
        //     as 'virtual' and they get ignored as Event methods.
        //
        [Event(1, Level = EventLevel.Verbose, Opcode = EventOpcode.Info, Task = EventTask.None)]
        public void debug(string file, string member, int line, string message)
        {
            if (IsEnabled())
                this.WriteEvent(1, file, line, member, message);
        }

        [Event(2, Level = EventLevel.Informational, Opcode = EventOpcode.Info, Task = EventTask.None)]
        public void info(string file, string member, int line, string message)
        {
            if (IsEnabled())
                this.WriteEvent(2, file, line, member, message);
        }

        [Event(3, Level = EventLevel.Warning, Opcode = EventOpcode.Info, Task = EventTask.None)]
        public void warning(string file, string member, int line, string message)
        {
            if (IsEnabled())
                this.WriteEvent(3, file, line, member, message);
        }

        [Event(4, Level = EventLevel.Error, Opcode = EventOpcode.Info, Task = EventTask.None)]
        public void error(string file, string member, int line, string message)
        {
            if (IsEnabled())
                this.WriteEvent(4, file, line, member, message);
        }

        [Event(5, Level = EventLevel.Critical, Opcode = EventOpcode.Info, Task = EventTask.None)]
        public void critical(string file, string member, int line, string message)
        {
            if (IsEnabled())
                this.WriteEvent(5, file, line, member, message);
        }
    }
}
