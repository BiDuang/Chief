using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Chief.Models;
using I18N.Avalonia.Interface;
using Newtonsoft.Json;
using Splat;

namespace Chief.Utils;

public class Cache
{
    public static readonly string CacheDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string ChiefDir = Path.Combine(CacheDir, "Chief");

    public static async Task<bool> ChangeCacheDir(string destDir, bool moveExistData = true)
    {
        if (destDir == ChiefDir) return true;
        try
        {
            Directory.CreateDirectory(destDir);
            if (moveExistData)
                foreach (var file in Directory.EnumerateFiles(ChiefDir, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetDirectoryName(
                        Path.GetRelativePath(ChiefDir, file)) ?? string.Empty;
                    Directory.CreateDirectory(Path.Combine(destDir, relativePath));
                    await using var sourceStream = File.Open(file, FileMode.Open);
                    await using var destStream =
                        File.Create(Path.Combine(destDir, relativePath, Path.GetFileName(file)));
                    await sourceStream.CopyToAsync(destStream);
                }
        }
        catch (Exception e)
        {
            //TODO: Exception handling
            Debug.WriteLine(e);
            return false;
        }


        return true;
    }

    public class Config
    {
        [JsonIgnore] public static readonly string ConfigDir = Path.Combine(ChiefDir, "Config.json");
        [JsonIgnore] public static Config? Instance;

        private Config()
        {
            if (!File.Exists(ConfigDir))
            {
                AppLanguage = Thread.CurrentThread.CurrentCulture.Name;
            }
            else
                try
                {
                    var json = File.ReadAllText(ConfigDir);
                    JsonConvert.PopulateObject(json, this);
                }
                catch
                {
                    //TODO: Exception handling
                }
        }

        [JsonProperty("CacheDir")] private static string AppCacheDir => ChiefDir;

        [JsonProperty("Theme")] public ThemeMode AppTheme { get; set; } = ThemeMode.SyncWithSystem;

        [JsonProperty("Language")]
        public string AppLanguage
        {
            get => Locator.Current.GetService<ILocalizer>()!.Language.Name;
            set => Locator.Current.GetService<ILocalizer>()!.Language = new CultureInfo(value);
        }

        [JsonProperty("Animation")] public double AnimationSpeed { get; set; } = 500;

        public CultureInfo GetAppCultureInfo() => new(AppLanguage);
        public static void LoadConfig() => Instance = new Config();

        public bool Save()
        {
            if (!Directory.Exists(ChiefDir))
                Directory.CreateDirectory(ChiefDir);
            try
            {
                var json = JsonConvert.SerializeObject(this);
                File.WriteAllText(ConfigDir, json);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}