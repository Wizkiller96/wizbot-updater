using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using SharpCompress.Compressors.Xz;
using SharpCompress.Readers;
using ZstdSharp;

namespace NadekoUpdater.Common
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
        private FfmpegDepViewModel() : base("FFMPEG")
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
                    p.Kill();
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

                    var ffmpegPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "ffmpeg");

                    Directory.CreateDirectory(ffmpegPath);
                    ZipFile.ExtractToDirectory("./ffmpeg.zip", ffmpegPath);

                    Environment.SetEnvironmentVariable("path",
                        Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User) + ";" + ffmpegPath +
                        @"\nightly\bin",
                        EnvironmentVariableTarget.User);
                    File.Delete("./ffmpeg.zip");

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
                    var xzFile = "~/.local/sbin/";

                    using var http = new HttpClient();
                    await using var stream = await http.GetStreamAsync(staticUrl);
                    using var reader = ReaderFactory.Open(stream);
                    var cnt = 0;
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.Key is "ffmpeg" or "ffprobe")
                        {
                            await using var fs = new FileStream(Path.Combine(xzFile, reader.Entry.Key),
                                FileMode.Create);

                            if (++cnt == 2)
                                break;
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