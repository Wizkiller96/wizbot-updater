using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpCompress.Readers;

namespace wizbotupdater.ViewModels
{
    /// <summary>
    /// Viewmodel for FFMPEG bot dependency
    /// </summary>
    public class FfmpegDepViewModel : DepViewModel
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public FfmpegDepViewModel() : base("ffmpeg")
        {
        }

        #endregion

        #region Methods

        protected override async Task<DepState> InternalCheckAsync()
        {
            await Task.Yield();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var p = Process.Start(psi);
                try
                {
                    p?.Kill();
                }
                catch
                {
                }

                return DepState.Installed;
            }
            catch
            {
                return DepState.NotInstalled;
            }
        }

        protected override async Task<bool> InternalInstallAsync()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const string ffmpegWindowsBuild =
                        "https://github.com/GyanD/codexffmpeg/releases/download/6.1.1/ffmpeg-6.1.1-essentials_build.zip";
                    using (var http = new HttpClient())
                    await using (var stream = await http.GetStreamAsync(ffmpegWindowsBuild))
                    await using (var fs = new FileStream("./ffmpeg.zip", FileMode.Create))
                    {
                        await stream.CopyToAsync(fs);
                    }

                    var ffmpegPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory)!, "ffmpeg");

                    if(Directory.Exists(ffmpegPath))
                        Directory.Delete(ffmpegPath);
                    
                    Directory.CreateDirectory(ffmpegPath);
                    ZipFile.ExtractToDirectory("./ffmpeg.zip", ffmpegPath);

                    try
                    {
                        Environment.SetEnvironmentVariable("path",
                            Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User) + ";" +
                            ffmpegPath +
                            @"\ffmpeg-6.1.1-essentials_build\bin",
                            EnvironmentVariableTarget.User);
                        File.Delete("./ffmpeg.zip");
                    }
                    catch
                    {
                        
                    }

                    return true;
                }
                else
                {
                    // switch on  processor architecture
                    var arch = (RuntimeInformation.ProcessArchitecture) switch
                    {
                        Architecture.X64 => "amd64",
                        Architecture.Arm64 => "arm64",
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var staticUrl = $"https://www.johnvansickle.com/ffmpeg/old-releases/" +
                                    $"ffmpeg-6.0.1-{arch}-static.tar.xz";
                    
                    // Ensure the directory exists
                    var targetDir = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "sbin"));
                    Directory.CreateDirectory(targetDir);
                    
                    // Download the archive
                    using var http = new HttpClient();
                    await using var stream = await http.GetStreamAsync(staticUrl);
                    using var reader = ReaderFactory.Open(stream);
                    var cnt = 0;
                    while (reader.MoveToNextEntry())
                    {
                        if (!reader.Entry.IsDirectory)
                        {
                            var fileName = Path.GetFileName(reader.Entry.Key);
                            if (fileName is "ffmpeg" or "ffprobe")
                            {
                                var targetPath = Path.Combine(targetDir, fileName);
                                reader.WriteEntryToFile(targetPath);
                                cnt++;
                                
                                // Set executable permission right after extracting each file
                                var process = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "chmod",
                                        Arguments = $"+x \"{targetPath}\"",
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    }
                                };
                                
                                process.Start();
                                await process.WaitForExitAsync();
                                
                                if (cnt >= 2)
                                    break;
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                // MessageBox.Show(ex.ToString(), "Ffmpeg download or install failed.");
                return false;
            }
        }

        #endregion
    }
}