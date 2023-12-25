using CommunityToolkit.HighPerformance.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phobos.Core.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Phobos.Services
{
    internal class FileService : IFileService
    {
        private XamlRootService xamlRoot;

        public FileService(XamlRootService xamlRoot)
        {
            this.xamlRoot = xamlRoot;
        }

        public async Task SaveFileAsync(string name, string extension, byte[] data)
        {
            FileSavePicker picker = new();
            var hwnd = xamlRoot.Hwnd;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.SuggestedFileName = name;
            picker.FileTypeChoices.Add($"{extension} File", new List<string>() { extension });
            InitializeWithWindow.Initialize(picker, hwnd);
            StorageFile file = await picker.PickSaveFileAsync();
            if (file is not null)
            {
                CachedFileManager.DeferUpdates(file);
                using var stream = await file.OpenStreamForWriteAsync();
                await stream.WriteAsync(data, 0, data.Length);
                await CachedFileManager.CompleteUpdatesAsync(file);
            }
        }
    }
}
