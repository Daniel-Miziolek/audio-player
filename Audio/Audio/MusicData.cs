﻿using Spectre.Console;
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

        public MusicData DeleteMusic()
        {
            var nameOfTheMusic = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose music to delete")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more music)[/]")
                    .AddChoices(ImportedMusic)
            );

            if (ImportedMusic.Contains(nameOfTheMusic))
            {
                ImportedMusic.Remove(nameOfTheMusic);
                foreach (var playlist in Playlists.Values)
                {
                    playlist.Remove(nameOfTheMusic);
                }
                SaveToFile();
            }
            else
            {
                Console.WriteLine($"[Error] Could not find music with name: {nameOfTheMusic}");
            }
            return this;
        }

        public MusicData DeletePlaylist()
        {
            var keyOfThePlaylist = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose playlist to delete")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more playlists)[/]")
                    .AddChoices(Playlists.Select(key => key.Key))
            );

            if (Playlists.Select(key => key.Key).Contains(keyOfThePlaylist))
            {
                Playlists.Remove(keyOfThePlaylist);
                SaveToFile();
            }
            else
            {
                Console.WriteLine($"[Error] Could not find playlist with name: {keyOfThePlaylist}");
            }
            return this;
        }
    }

}