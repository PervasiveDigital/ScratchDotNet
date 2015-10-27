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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.DeploymentHelper.Views;
using PervasiveDigital.Scratch.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Properties;
using System.Collections.Specialized;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private RelayCommand _doneCommand;
        private readonly SettingsPage _view;

        public SettingsPageViewModel(SettingsPage view)
            : base(view.Dispatcher)
        {
            _view = view;
        }

        public bool UseOnlineUpdates
        {
            get { return Settings.Default.OnlineDataUpdates; }
            set 
            { 
                Settings.Default.OnlineDataUpdates = value;
                Settings.Default.Save();
                OnPropertyChanged(); 
            }
        }

        public bool CanChangeUpdateSetting
        {
            // Users cannot activate online updates if you are in classroom mode
            get { return !Settings.Default.ClassroomMode; }
        }

        public string ComPortText
        {
            get
            {
                string compositeString = "";
                if (Settings.Default.ScanCOMPorts!=null && Settings.Default.ScanCOMPorts.Count>0)
                {
                    var list = Settings.Default.ScanCOMPorts.Cast<string>().ToList();
                    compositeString = string.Join(", ", list);
                }
                return compositeString;
            }
            set
            {
                var list = new StringCollection();
                var tokens = value.Split(',');
                var regex = new Regex(@"^COM[\d]{1,3}?$",RegexOptions.IgnoreCase);

                foreach (var token in tokens)
                {
                    var candidate = token.Trim();
                    if (regex.IsMatch(candidate))
                        list.Add(candidate);
                }
                Settings.Default.ScanCOMPorts = list;
                Settings.Default.Save();
                OnPropertyChanged();
            }
        }

        public ICommand DoneCommand
        {
            get
            {
                if (_doneCommand == null)
                {
                    _doneCommand = new RelayCommand(DoneCommand_Executed);
                }
                return _doneCommand;
            }
        }

        private void DoneCommand_Executed(object obj)
        {
            if (_view.NavigationService.CanGoBack)
                _view.NavigationService.GoBack();
        }

    }
}
