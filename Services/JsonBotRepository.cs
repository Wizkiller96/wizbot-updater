using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using upeko.Models;

namespace upeko.Services
{
    public class JsonBotRepository : IBotRepository
    {
        // Static, reusable JsonSerializerOptions instances
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };


        private readonly string _legacyBotsJsonPath;

        private readonly string _configFilePath;
        private readonly string _myDocumentsFolder;

        private ConfigModel _config = new();

        public JsonBotRepository()
        {
            _myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Get the application directory
            _configFilePath = Path.Combine(_myDocumentsFolder, "upeko.json");

            // Get the MyDocuments folder path
            var updaterFolder = Path.Combine(_myDocumentsFolder, "NadekoBotUpdater");
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
                    var config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonSerializerOptions);
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
                    var botModels = JsonSerializer.Deserialize<List<BotModel>>(json, _jsonSerializerOptions);

                    // Create a new config with the imported bots
                    _config = new ConfigModel
                    {
                        Bots = botModels ?? new List<BotModel>()
                    };

                    // Save the new config
                    SaveConfig();

                    return;
                }
                catch (Exception)
                {
                    // If there's an error reading the file, continue to the next option
                    // todo: Show an error message box
                }
            }

            // If no existing config is found, create a default one
            _config = new ConfigModel();
            SaveConfig();
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

            if (!Directory.Exists(bot.PathUri!.LocalPath))
                Directory.CreateDirectory(bot.PathUri!.LocalPath);

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
                var json = JsonSerializer.Serialize(_config, _jsonSerializerOptions);
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