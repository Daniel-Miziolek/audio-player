using System.Text.Json;
using System.Text.Json.Serialization;

namespace Audio
{
    public class MusicData
    {
        public List<string> ImportedMusic { get; set; } = new();
        public Dictionary<string, List<string>> Playlists { get; set; } = new();

        private static string FilePath => "music_data.json";

        public void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, options));
        }

        public static MusicData LoadFromFile()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<MusicData>(json) ?? new MusicData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Could not load data: {ex.Message}");
                }
            }
            return new MusicData();
        }
    }

}
