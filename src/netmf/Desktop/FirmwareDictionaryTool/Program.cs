using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PervasiveDigital.Scratch.Common;
using System.IO;

namespace FirmwareDictionaryTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var MicrosoftSpot431LibraryId = Guid.Parse("e98b4d8f-6f47-4663-bbae-bbe6fc8a019f");
            var ghiSdkLibraryId = Guid.Parse("9dea3dc3-6e24-45c4-9d1f-79cc35020e34");
            var brainPad431LibraryId = Guid.Parse("26571886-8ec7-4d55-9d43-c418545f2169");

            var dict = new FirmwareDictionary();

            //
            // Firmware Images
            //
            var brainPadImage = new FirmwareImage()
            {
                Id = Guid.Parse("b335f011-7604-4984-9418-33c9ce00d3ae"),
                Name = "BrainPad",
                AppName = "BrainPadFirmataApp",
                AppVersion = new Version(1, 0, 0, 0),
                Description = "This firmware unlocks all of the features of the GHI Electronics BrainPad",
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
                ConfigurationExtension = null,
                ImageCreatedBy = "Pervasive Digital LLC",
                ImageSupportUrl = "mailto:support@pervasive.digital",
            };
            dict.Images.Add(brainPadImage);

            //
            // Assemblies
            //
            var assm = new FirmwareAssembly()
            {
                Id = Guid.Parse("99d20fa1-895e-49b6-9a4f-45e4e08ff106"),
                Filename = "Microsoft.SPOT.Graphics.pe",
                IsLittleEndian = true,
                LibraryId = MicrosoftSpot431LibraryId,
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
            };
            dict.Assemblies.Add(assm);
            brainPadImage.RequiredAssemblies.Add(assm.Id);

            assm = new FirmwareAssembly()
            {
                Id = Guid.Parse("a39662ef-143e-4cb4-9f75-22f0ea1b404b"),
                Filename = "Microsoft.SPOT.Hardware.USB.pe",
                IsLittleEndian = true,
                LibraryId = MicrosoftSpot431LibraryId,
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
            };
            dict.Assemblies.Add(assm);
            brainPadImage.RequiredAssemblies.Add(assm.Id);

            assm = new FirmwareAssembly()
            {
                Id = Guid.Parse("302fe463-f90c-4a03-9554-e8af4903f516"),
                Filename = "GHI.Hardware.pe",
                IsLittleEndian = true,
                LibraryId = ghiSdkLibraryId,
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
            };
            dict.Assemblies.Add(assm);
            brainPadImage.RequiredAssemblies.Add(assm.Id);

            assm = new FirmwareAssembly()
            {
                Id = Guid.Parse("62239dee-07d4-4447-997f-fa3808e250bd"),
                Filename = "FirmataRuntime.pe",
                IsLittleEndian = true,
                LibraryId = brainPad431LibraryId,
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
            };
            dict.Assemblies.Add(assm);
            brainPadImage.RequiredAssemblies.Add(assm.Id);

            assm = new FirmwareAssembly()
            {
                Id = Guid.Parse("5b61839a-bc8b-4328-800b-e3372a7262e2"),
                Filename = "BrainPadFirmataApp.pe",
                IsLittleEndian = true,
                LibraryId = brainPad431LibraryId,
                TargetFrameworkVersion = new Version(4, 3, 1, 0),
            };
            dict.Assemblies.Add(assm);
            brainPadImage.RequiredAssemblies.Add(assm.Id);

            //
            // Boards
            //

            var board = new FirmwareHost()
            {
                Id = Guid.Parse("6a9bdb56-8005-428d-9f29-8c425d9614b0"),
                Name = "BrainPad",
                ProductImageName = "BrainPad.jpg",
                Manufacturer = "GHI Electronics",
                Description = "Hardware for STEM education",
                UsbName = "G30_G30",
                BuildInfoContains = "GHI Electronics",
                OEM = 0xff,
                SKU = 0xffff,
            };
            board.CompatibleImages.Add(brainPadImage.Id);
            dict.Boards.Add(board);

            var content = JsonConvert.SerializeObject(dict, Formatting.Indented);
            File.WriteAllText("dict.json", content);
        }
    }
}
