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
        private readonly ObservableCollection<FirmwareImage> _imagesSource = new ObservableCollection<FirmwareImage>();
        private readonly ObservableViewCollection<FirmwareImage, FirmwareImageViewModel> _images;
        private bool _fInitialized = false;

        public MfTargetDeviceViewModel(Dispatcher disp)
            : base(disp)
        {
            _images = new ObservableViewCollection<FirmwareImage, FirmwareImageViewModel>(disp);
        }

        public MfTargetDevice Source { get { return (MfTargetDevice)this.ViewSource; } }

        public bool IsFirmataInstalled { get { return this.Source.IsFirmataInstalled; } }

        public string FirmataAppName { get { return this.Source.FirmataAppName ?? ""; } }

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
                await _images.Attach(_imagesSource);
                PopulateImages();

                _fInitialized = true;
            }
        }

        public void PopulateImages()
        {
            Guid originallySelected = Guid.Empty;
            if (this.SelectedFirmware!=null)
                originallySelected = this.SelectedFirmware.Id;

            var images = this.Source.GetCompatibleFirmwareImages(!this.ShowAllCompatibleFirmwareImages);
            _imagesSource.Clear();
            _imagesSource.AddRange(images);

            if (originallySelected == Guid.Empty)
                this.SelectedFirmware = _images.FirstOrDefault();
            else
                this.SelectedFirmware = _images.FirstOrDefault(x => x.Id == originallySelected);
        }

        private bool _showAllImages;
        public bool ShowAllCompatibleFirmwareImages
        {
            get { return _showAllImages; }
            set { SetProperty(ref _showAllImages, value); }
        }

        private FirmwareImageViewModel _selectedFirmware;
        public FirmwareImageViewModel SelectedFirmware
        {
            get { return _selectedFirmware; }
            set { SetProperty(ref _selectedFirmware, value); }
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
                PopulateImages();
            }
        }

        public void Deploy()
        {
            this.Source.Deploy();
        }
    }
}
