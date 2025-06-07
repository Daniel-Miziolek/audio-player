using Spectre.Console;
using System.Text.Json;

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

            if (Playlists.ContainsKey(keyOfThePlaylist))
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

        public MusicData EditPlaylist()
        {
            if (Playlists.Count == 0)
            {
                Console.WriteLine("No playlists available to edit.");
                return this;
            }

            var playlistName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select playlist to edit")
                    .PageSize(10)
                    .AddChoices(Playlists.Keys)
            );

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"What do you want to do with playlist '{playlistName}'?")
                    .AddChoices(new[] { "Add tracks", "Remove tracks", "Cancel" })
            );

            if (action == "Cancel")
            {
                return this;
            }

            if (action == "Add tracks")
            {
                var availableTracks = ImportedMusic
                    .Where(track => !Playlists[playlistName].Contains(track))
                    .Select(Path.GetFileName)
                    .ToList();

                if (availableTracks.Count == 0)
                {
                    Console.WriteLine("No tracks available to add to this playlist.");
                    return this;
                }

                var tracksToAdd = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Select tracks to add")
                        .NotRequired()
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more tracks)[/]")
                        .InstructionsText("[grey](Press [blue]<space>[/] to toggle a track, [green]<enter>[/] to accept)[/]")
                        .AddChoices(availableTracks)
                );

                if (tracksToAdd.Count > 0)
                {
                    var fullPathsToAdd = ImportedMusic
                        .Where(path => tracksToAdd.Contains(Path.GetFileName(path)))
                        .ToList();

                    Playlists[playlistName].AddRange(fullPathsToAdd);
                    SaveToFile();
                    Console.WriteLine($"Added {tracksToAdd.Count} tracks to playlist '{playlistName}'.");
                }
            }
            else if (action == "Remove tracks")
            {
                if (Playlists[playlistName].Count == 0)
                {
                    Console.WriteLine("This playlist is empty. Nothing to remove.");
                    return this;
                }

                var tracksToRemove = AnsiConsole.Prompt(
                    new MultiSelectionPrompt<string>()
                        .Title("Select tracks to remove")
                        .NotRequired()
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more tracks)[/]")
                        .InstructionsText("[grey](Press [blue]<space>[/] to toggle a track, [green]<enter>[/] to accept)[/]")
                        .AddChoices(Playlists[playlistName].Select(Path.GetFileName))
                );

                if (tracksToRemove.Count > 0)
                {
                    Playlists[playlistName].RemoveAll(path => tracksToRemove.Contains(Path.GetFileName(path)));
                    SaveToFile();
                    Console.WriteLine($"Removed {tracksToRemove.Count} tracks from playlist '{playlistName}'.");
                }
            }

            return this;
        }
    }
}