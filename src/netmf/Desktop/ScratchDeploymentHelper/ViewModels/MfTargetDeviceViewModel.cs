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
using System.Threading.Tasks;
using System.Windows.Threading;

using PervasiveDigital.Scratch.DeploymentHelper.Common;
using PervasiveDigital.Scratch.DeploymentHelper.Models;
using PervasiveDigital.Scratch.Common;

namespace PervasiveDigital.Scratch.DeploymentHelper.ViewModels
{
    public class MfTargetDeviceViewModel : DeviceViewModel
    {
        private readonly ObservableCollection<FirmwareHost> _boardsSource = new ObservableCollection<FirmwareHost>();
        private readonly ObservableViewCollection<FirmwareHost, FirmwareHostViewModel> _boards;
        private readonly ObservableCollection<FirmwareImage> _imagesSource = new ObservableCollection<FirmwareImage>();
        private readonly ObservableViewCollection<FirmwareImage, FirmwareImageViewModel> _images;
        private bool _fInitialized = false;

        public MfTargetDeviceViewModel(Dispatcher disp)
            : base(disp)
        {
            _boards = new ObservableViewCollection<FirmwareHost, FirmwareHostViewModel>(disp);
            _images = new ObservableViewCollection<FirmwareImage, FirmwareImageViewModel>(disp);
        }

        public MfTargetDevice Source { get { return (MfTargetDevice)this.ViewSource; } }

        public bool IsFirmataInstalled { get { return this.Source.IsFirmataInstalled; } }

        public string FirmataAppName { get { return this.Source.FirmataAppName ?? ""; } }

        public ObservableViewCollection<FirmwareHost, FirmwareHostViewModel> Boards
        {
            get
            {
                InitializeCollections();
                return _boards;
            }
        }

        public ObservableViewCollection<FirmwareImage, FirmwareImageViewModel> FirmwareImages
        {
            get
            {
                InitializeCollections();
                return _images;
            }
        }

        private async void InitializeCollections()
        {
            if (!_fInitialized)
            {
                await _boards.Attach(_boardsSource);
                PopulateBoards();
                await _images.Attach(_imagesSource);
                PopulateImages();

                _fInitialized = true;
            }
        }

        public void PopulateBoards()
        {
            Guid originallySelected = Guid.Empty;
            if (this.SelectedFirmware != null)
                originallySelected = this.SelectedBoard.Id;

            var boards = this.Source.GetCandidateBoards();
            _boardsSource.Clear();
            _boardsSource.AddRange(boards);

            if (originallySelected == Guid.Empty)
                this.SelectedBoard = _boards.FirstOrDefault();
            else
                this.SelectedBoard = _boards.FirstOrDefault(x => x.Id == originallySelected);
        }

        public void PopulateImages()
        {
            if (this.SelectedBoard == null)
                return;

            Guid originallySelected = Guid.Empty;
            if (this.SelectedFirmware != null)
                originallySelected = this.SelectedFirmware.Id;

            var images = this.Source.GetCompatibleFirmwareImages(this.SelectedBoard.Id);
            _imagesSource.Clear();
            _imagesSource.AddRange(images);

            if (this.Source.FirmataAppVersion != null && !string.IsNullOrEmpty(this.Source.FirmataAppName))
            {
                foreach (var item in _images)
                {
                    item.IsInstalled = (item.ViewSource.AppName == FirmataAppName && item.ViewSource.AppVersion == this.Source.FirmataAppVersion);
                }
            }

            if (originallySelected == Guid.Empty)
            {
                if (_images.Any(x => x.IsInstalled))
                    this.SelectedFirmware = _images.FirstOrDefault(x => x.IsInstalled);
                else
                    this.SelectedFirmware = _images.FirstOrDefault();
            }
            else
                this.SelectedFirmware = _images.FirstOrDefault(x => x.Id == originallySelected);
        }

        public string ScratchExtensionName
        {
            get
            {
                if (this.SelectedBoard != null && this.SelectedFirmware != null && this.SelectedFirmware.IsInstalled &&
                    !string.IsNullOrEmpty(this.SelectedFirmware.ScratchExtensionName))
                    return this.SelectedFirmware.ScratchExtensionName;
                return null;
            }
        }
        public bool ConfigurationIsAvailable
        {
            get
            {
                if (this.SelectedBoard != null && this.SelectedFirmware != null && this.SelectedFirmware.IsInstalled &&
                    !string.IsNullOrEmpty(this.SelectedFirmware.ConfigurationExtensionSource))
                    return true;
                return false;
            }
        }

        public string SupportUrl
        {
            get
            {
                if (this.SelectedBoard != null)
                    return this.SelectedBoard.SupportUrl;
                else
                    return "";
            }
        }

        public string SupportText
        {
            get
            {
                if (this.SelectedBoard != null)
                    return this.SelectedBoard.SupportText;
                else
                    return null;
            }
        }

        public bool BoardSupportIsAvailable
        {
            get { return !(string.IsNullOrEmpty(this.SupportText) || string.IsNullOrEmpty(this.SupportUrl)); }
        }

        public string FwSupportUrl
        {
            get
            {
                if (this.SelectedFirmware != null)
                    return this.SelectedFirmware.SupportUrl;
                else
                    return "";
            }
        }

        public string FwSupportText
        {
            get
            {
                if (this.SelectedFirmware != null)
                    return this.SelectedFirmware.SupportText;
                else
                    return null;
            }
        }

        public bool FirmwareSupportIsAvailable
        {
            get { return !(string.IsNullOrEmpty(this.SupportText) || string.IsNullOrEmpty(this.SupportUrl)); }
        }

        private FirmwareHostViewModel _selectedBoard;
        public FirmwareHostViewModel SelectedBoard
        {
            get { return _selectedBoard; }
            set 
            { 
                SetProperty(ref _selectedBoard, value);
                OnPropertyChanged("SupportUrl");
                OnPropertyChanged("SupportText");
                OnPropertyChanged("BoardSupportIsAvailable");
                OnPropertyChanged("ScratchExtensionName");
            }
        }

        private FirmwareImageViewModel _selectedFirmware;
        public FirmwareImageViewModel SelectedFirmware
        {
            get { return _selectedFirmware; }
            set 
            { 
                SetProperty(ref _selectedFirmware, value);
                OnPropertyChanged("FwSupportUrl");
                OnPropertyChanged("FwSupportText");
                OnPropertyChanged("FirmwareSupportIsAvailable");
                OnPropertyChanged("ScratchExtensionName");
            }
        }

        public string FirmataAppVersion 
        { 
            get 
            {
                if (this.Source.FirmataAppVersion != null)
                    return this.Source.FirmataAppVersion.ToString();
                else
                    return "";
            }
        }

        //public bool IsImageRecognized
        //{
        //    get
        //    {

        //    }
        //}

        //public string ImageCreatedBy
        //{
        //    get
        //    {

        //    }
        //}

        //public string ImageSupportUrl
        //{
        //    get
        //    {

        //    }
        //}

        protected override void OnViewSourceChanged()
        {
            base.OnViewSourceChanged();
            this.Source.PropertyChanged += Source_PropertyChanged;
        }

        void Source_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
            if (e.PropertyName=="IsFirmataInstalled")
            {
                // This property only changes when we succeed in reading device info or after a deployment
                //   so use it to trigger a refresh of the list of images.
                PopulateBoards();
                PopulateImages();
            }
        }

        public async void Deploy()
        {
            if (this.SelectedFirmware != null)
            {
                App.ShowDeploymentLogWindow();
                await this.Source.Deploy(this.SelectedBoard.Id, this.SelectedFirmware.ViewSource.Id, App.AppendToLogWindow);
            }
        }
    }
}
