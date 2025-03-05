using System;
using System.Runtime.InteropServices;

namespace upeko;

public static class PlatformSpecific
{
    public static string GetNameOfFileExplorer()
    {
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "explorer.exe",
            PlatformID.Unix => "xdg-open",
            PlatformID.MacOSX => "open",
            _ => throw new PlatformNotSupportedException("Unsupported platform: " + Environment.OSVersion.Platform)
        };
    }

    public static string GetExecutableName()
    {
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "NadekoBot.exe",
            PlatformID.Unix => "NadekoBot",
            PlatformID.MacOSX => "NadekoBot",
            _ => throw new PlatformNotSupportedException("Unsupported platform: " + Environment.OSVersion.Platform)
        };
    }

    /// <summary>
    /// Gets the OS identifier for the download URL.
    /// </summary>
    public static string GetOS()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "win";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "osx";
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";
        
        throw new PlatformNotSupportedException("Unsupported OS platform");
    }

    /// <summary>
    /// Gets the architecture identifier for the download URL.
    /// </summary>
    public static string GetArchitecture()
        => RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.ProcessArchitecture}")
        };
}