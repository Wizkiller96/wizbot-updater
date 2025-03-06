using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace upeko.Services
{
    public interface IDialogService
    {
        Task<string?> ShowFolderPickerAsync(string title, string? initialDirectory = null);
        Task<string?> ShowImagePickerAsync(string title, string? initialDirectory = null);
    }

    public class DialogService : IDialogService
    {
        public async Task<string?> ShowFolderPickerAsync(string title, string? initialDirectory = null)
        {
            var topLevel = TopLevel.GetTopLevel(App.MainWindow);
            if (topLevel == null)
                return null;
                
            var folderPickerOptions = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            };
            
            if (!string.IsNullOrEmpty(initialDirectory))
            {
                folderPickerOptions.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
            }
            
            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(folderPickerOptions);
            
            if (result.Count > 0)
            {
                return result[0].Path.LocalPath;
            }
            
            return null;
        }

        public async Task<string?> ShowImagePickerAsync(string title, string? initialDirectory = null)
        {
            var topLevel = TopLevel.GetTopLevel(App.MainWindow);
            if (topLevel == null)
                return null;
                
            var filePickerOptions = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Image Files")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif" },
                        MimeTypes = new[] { "image/png", "image/jpeg", "image/gif" }
                    }
                }
            };
            
            if (!string.IsNullOrEmpty(initialDirectory))
            {
                filePickerOptions.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(initialDirectory);
            }
            
            var result = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);
            
            if (result.Count > 0)
            {
                return result[0].Path.LocalPath;
            }
            
            return null;
        }
    }
}
