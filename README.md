# WizBot Installer

Cross-platform created to simplify the installation of WizBot(s).
Built with .NET 8 and Avalonia UI.


## Download

Download the latest pre-built binaries for your operating system from the [RELEASES](https://github.com/Wizkiller96/wizbot-updater/releases/latest) page.

---

## Features

- **Multiple bots**: Install and manage multiple WizBot instances from one interface
- **Updates**: Check for and download the latest WizBot versions
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Themes**: Support for light and dark themes

## Usage Guide

### First Launch

1. Start WizBot Updater application
2. Click the **+** button to create a new bot 
3. Click "Update" to download the latest version
4. Once downloaded, click the **Note** icon above RUN to open the creds
5. Fill in your bot credentials - [guide here](https://docs.wizbot.cc/creds-guide)
6. Click "Start" to run your new bot

### Dependencies

At the top of the window, you will see `ffmpeg` and `yt-dlp` buttons. Click them to download the dependencies for your bot if you wish to use music features.

# Development

If you want to build WizBot Updater from source, follow these steps:

#### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed on your system

### Build from Source

#### Clone the repository
```bash
git clone https://github.com/Wizkiller96/wizbot-updater.git
```

#### Build the application
```bash
cd wizbot-updater
dotnet build
```

#### Run the application
```bash
dotnet run
```


### Requirements

- .NET 8 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code with C# extensions

### Project Structure

- `/Models`: Data models
- `/ViewModels`: MVVM view models, most of the logic is here (contrary to MVVM pattern)
- `/Views`: Avalonia UI views
- `/Services`: Application services including UpdateChecker

## Troubleshooting

Join our [discord](https://wizbot.cc/discord and ask for help in the `#help` channel.

## License

GPLv3 - See the [LICENSE](LICENSE.md) file for details.

## Links

- [WizBot](https://wizbot.cc/) - The Discord bot that this manager is built for

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.