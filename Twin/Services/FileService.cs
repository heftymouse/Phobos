using CommunityToolkit.HighPerformance.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twin.Core.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Twin.Services
{
    internal class FileService : IFileService
    {
        private static Dictionary<string, IList<string>> FileTypeAndExtension = new()
        {
            { "Gemini Document", new List<string>() { ".gmi" }},
            { "Plain Text", new List<string>() { ".txt" }},
            { "HTML Document", new List<string>() { ".html", ".htm" } }
        };

        private XamlRootService xamlRoot;

        public FileService(XamlRootService xamlRoot)
        {
            this.xamlRoot = xamlRoot;
        }

        public async Task SaveFileAsync(string name, string extension, byte[] data)
        {
            FileSavePicker picker = new();
            InitializeWithWindow.Initialize(picker, xamlRoot.Hwnd);
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.SuggestedFileName = name;
            var type = FileTypeAndExtension.Where((e) => e.Value.Contains(extension)).DefaultIfEmpty(new($"{extension} File", new List<string>() { extension }));
            picker.FileTypeChoices.Concat(type);
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
