using NotionDeadlineFairy.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionDeadlineFairy.Services
{
    public class SettingService
    {
        private static string SettingFilePath => Path.Combine(AppContext.BaseDirectory, "appsetting.json");
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private static readonly Lazy<SettingService> _instance =
            new Lazy<SettingService>(() => new SettingService());

        public static SettingService Instance => _instance.Value;

        public AppSetting Current { get; private set; } = AppSetting.CreateDefault();

        public SettingService()
        {
        }


        public void Load()
        {
            if (!File.Exists(SettingFilePath))
            {
                Current = AppSetting.CreateDefault();
                Save();
                return;
            }

            try
            {
                var json = File.ReadAllText(SettingFilePath);
                var loaded = JsonSerializer.Deserialize<AppSetting>(json, _jsonOptions);
                Current = loaded ?? AppSetting.CreateDefault();
            }
            catch
            {
                Current = AppSetting.CreateDefault();
                Save();
            }
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(SettingFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(Current, _jsonOptions);
            File.WriteAllText(SettingFilePath, json);
        }
    }

    public class AppSetting
    {
        public List<NotionConfig> DatabaseConfigs { get; set; } = new List<NotionConfig>();
        public WindowMode WindowMode { get; set; } = WindowMode.Topmost;
        public bool AutoStart { get; set; } = false;
        public int PollingIntervalSeconds { get; set; } = 300;
        public bool IsEditMode { get; set; } = false;
        public bool IsClickThrough { get; set; } = false;

        public static AppSetting CreateDefault()
        {
            return new AppSetting
            {
                DatabaseConfigs = new List<NotionConfig>(),
                WindowMode = WindowMode.Topmost,
                AutoStart = false,
                PollingIntervalSeconds = 300,
                IsEditMode = false,
                IsClickThrough = false,
            };
        }
    }
}
