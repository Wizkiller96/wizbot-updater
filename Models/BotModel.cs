using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using upeko.Services;

namespace upeko.Models
{
    public partial class BotModel
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = "New Bot";

        public Uri? IconUri { get; set; }

        public string? Version { get; set; }

        public Uri? PathUri { get; set; }
    }

    [JsonSerializable(typeof(BotModel))]
    [JsonSerializable(typeof(Guid))]
    [JsonSerializable(typeof(Uri))]
    [JsonSerializable(typeof(ConfigModel))]
    [JsonSerializable(typeof(ReleaseModel))]
    [JsonSerializable(typeof(ReleaseAsset))]
    [JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    )]
    public partial class SourceJsonSerializer : JsonSerializerContext;
}