using System;
using System.Collections.Generic;
using System.IO;

namespace upeko.Models
{
    public class ConfigModel
    {
        public List<BotModel> Bots { get; set; } = new List<BotModel>();

        public string DefaultBotsFolder { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "upeko",
            "bots");
    }
}