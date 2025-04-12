using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace wizbotupdater.ViewModels
{
    /// <summary>
    /// Viewmodel for YoutubeDL bot dependency
    /// </summary>
    public class YtdlDepViewModel : DepViewModel
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public YtdlDepViewModel() : base("yt-dlp")
        {
        }

        #endregion

        #region Methods

        protected override async Task<DepState> InternalCheckAsync()
        {
            try
            {
                await Process.Start(new ProcessStartInfo()
                {
                    FileName = "yt-dlp",
                    Arguments = "-U",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                })!.WaitForExitAsync();

                return DepState.Installed;
            }
            catch
            {
                // it's just not installed if running it fails
                return DepState.NotInstalled;
            }
        }

        protected override async Task<bool> InternalInstallAsync()
        {
            var baseUrl = $"https://github.com/yt-dlp/yt-dlp/releases/latest/download/";

            var winDlLink = baseUrl + "yt-dlp.exe";
            var linuxDlLink = baseUrl + "yt-dlp";

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Delete the old yt-dlp directory, if it exists
                    var ytdlPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory)!, "yt-dlp");

                    if (Directory.Exists(ytdlPath))
                        Directory.Delete(ytdlPath, true);

                    // Recreate the directory for the new yt-dlp file
                    Directory.CreateDirectory(ytdlPath);

                    using var http = new HttpClient();
                    await using var stream = await http.GetStreamAsync(winDlLink);
                    await using var fs = new FileStream(Path.Combine(ytdlPath, "yt-dlp.exe"), FileMode.Create);

                    try
                    {
                        await stream.CopyToAsync(fs);
                        Environment.SetEnvironmentVariable("path",
                            Environment.GetEnvironmentVariable("path") + ";" + ytdlPath,
                            EnvironmentVariableTarget.User);
                    }
                    catch
                    {
                    }

                    return true;
                }
                else
                {
                    using var http = new HttpClient();
                    await using var stream = await http.GetStreamAsync(linuxDlLink);

                    // Ensure the directory exists
                    var targetDir = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "sbin"));
                    Directory.CreateDirectory(targetDir);

                    var ytdlpPath = Path.Combine(targetDir, "yt-dlp");
                    await using (var fs = new FileStream(ytdlpPath, FileMode.Create))
                    {
                        await stream.CopyToAsync(fs);
                    }

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = $"+x \"{ytdlpPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true
                        }
                    };

                    process.Start();
                    await process.WaitForExitAsync();

                    return true;
                }
            }
            catch (Exception)
            {
                // MessageBox.Show(ex.ToString(), "YoutubeDLP installation failed.");
                // todo: Add an error message box
            }

            return false;
        }

        #endregion
    }
}