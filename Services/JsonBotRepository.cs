using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using upeko.Models;

namespace upeko.Services
{
    public class JsonBotRepository : IBotRepository
    {
        private readonly string _legacyBotsJsonPath;

        private readonly string _configFilePath;

        private ConfigModel _config = new();

        public JsonBotRepository()
        {
            var myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Get the application directory
            _configFilePath = Path.Combine(myDocumentsFolder, "upeko", "upeko.json");

            // Get the MyDocuments folder path
            var updaterFolder = Path.Combine(myDocumentsFolder, "NadekoBotUpdater");
            _legacyBotsJsonPath = Path.Combine(updaterFolder, "bots.json");

            // Initialize the config
            InitializeConfig();
        }

        private void InitializeConfig()
        {
            // Check if upeko.json exists in the application directory
            if (File.Exists(_configFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_configFilePath);
                    var config = JsonSerializer.Deserialize(json,
                        SourceJsonSerializer.Default.ConfigModel);
                    _config = config ?? new ConfigModel();

                    return;
                }
                catch (Exception)
                {
                    // todo: Show an error message box
                }
            }

            // Check if bots.json exists in the NadekoBotUpdater folder
            if (File.Exists(_legacyBotsJsonPath))
            {
                try
                {
                    var json = File.ReadAllText(_legacyBotsJsonPath);
                    var botModels = JsonSerializer.Deserialize(json, SourceJsonSerializer.Default.ListBotModel);

                    // Create a new config with the imported bots
                    _config = new ConfigModel
                    {
                        Bots = botModels ?? new List<BotModel>()
                    };

                    // Migrate legacy bot data structure
                    if (_config.Bots.Count > 0)
                    {
                        MigrateLegacyBotData(_config.Bots);
                    }

                    // Save the new config
                    SaveConfig();

                    return;
                }
                catch (Exception ex)
                {
                    // If there's an error reading the file, continue to the next option
                    // todo: Show an error message box
                    Console.WriteLine($"Error migrating legacy bots: {ex.Message}");
                }
            }

            // If no existing config is found, create a default one
            _config = new ConfigModel();
            SaveConfig();
        }

        private void MigrateLegacyBotData(List<BotModel> bots)
        {
            foreach (var bot in bots)
            {
                var botPath = bot.PathUri;
                if (string.IsNullOrWhiteSpace(botPath))
                    continue;

                if (!Directory.Exists(botPath))
                    continue;

                try
                {
                    // 1. Move system/creds.yml to system/data/creds.yml if it exists
                    var systemFolder = Path.Combine(botPath, "system");
                    var systemDataFolder = Path.Combine(systemFolder, "data");
                    var oldCredsPath = Path.Combine(systemFolder, "creds.yml");
                    var newCredsPath = Path.Combine(systemDataFolder, "creds.yml");

                    if (File.Exists(oldCredsPath) && Directory.Exists(systemDataFolder))
                    {
                        // Ensure target directory exists
                        Directory.CreateDirectory(Path.GetDirectoryName(newCredsPath)!);

                        // Move the file
                        if (!File.Exists(newCredsPath))
                        {
                            File.Move(oldCredsPath, newCredsPath);
                        }
                        else
                        {
                            // If file already exists at destination, make a backup and replace
                            var backupPath = newCredsPath + ".bak";
                            if (File.Exists(backupPath))
                                File.Delete(backupPath);

                            File.Move(newCredsPath, backupPath);
                            File.Move(oldCredsPath, newCredsPath);
                        }
                    }

                    // 2. Move system/data folder contents to the base folder
                    if (Directory.Exists(systemDataFolder))
                    {
                        // Create data folder in base directory if it doesn't exist
                        var baseDataFolder = Path.Combine(botPath, "data");
                        if (Directory.Exists(baseDataFolder))
                        {
                            Directory.Delete(baseDataFolder, true);
                        }

                        Directory.Move(systemDataFolder, baseDataFolder);
                    }

                    // 3. Remove all Windows shortcuts in the base folder
                    foreach (var file in Directory.GetFiles(botPath, "*.lnk"))
                    {
                        File.Delete(file);
                    }

                    // 4. Remove the system folder
                    if (Directory.Exists(systemFolder))
                    {
                        // Make sure we're done with all operations before deleting
                        Directory.Delete(systemFolder, true);
                    }

                    // 5. Mark migration as complete (optional: could set a flag in the bot model)
                    Console.WriteLine($"Successfully migrated legacy data for bot at {botPath}");
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other bots
                    Console.WriteLine($"Error migrating bot at {botPath}: {ex.Message}");
                }
            }
        }

        public ConfigModel GetConfig()
        {
            return _config;
        }

        public List<BotModel> GetBots()
        {
            return _config.Bots;
        }

        public void AddBot(BotModel bot)
        {
            if (bot.PathUri is null)
                return;

            if (!Directory.Exists(bot.PathUri))
                Directory.CreateDirectory(bot.PathUri);

            _config.Bots.Add(bot);
            SaveConfig();
        }

        public void UpdateBot(BotModel bot)
        {
            // Find the bot by Id and update it
            var existingBot = _config.Bots.FirstOrDefault(b => b.Guid == bot.Guid);
            if (existingBot != null)
            {
                var index = _config.Bots.IndexOf(existingBot);
                _config.Bots[index] = bot;
                SaveConfig();
            }
        }

        public void RemoveBot(BotModel bot)
        {
            _config.Bots.RemoveAll(b => b.Guid == bot.Guid);
            SaveConfig();
        }

        public void SaveConfig()
        {
            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(_configFilePath)!;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize and save the config
                var json = JsonSerializer.Serialize(_config, SourceJsonSerializer.Default.ConfigModel);
                File.WriteAllText(_configFilePath, json);
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}