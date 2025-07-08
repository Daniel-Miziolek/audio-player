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

        public void ImportMusic()
        {
            string currentPath = ChooseDrive();
            Stack<string> pathHistory = new Stack<string>();

            while (true)
            {
                try
                {
                    var entries = Directory.GetFileSystemEntries(currentPath)
                        .Where(entry => Directory.Exists(entry) || IsAudioFile(entry))
                        .ToList();

                    entries.Insert(0, "Go back");
                    entries.Insert(1, "Exit");

                    var selectedMusicOrFolder = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title($"Browsing: [blue]{currentPath}[/]")
                            .PageSize(10)
                            .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                            .AddChoices(entries)
                    );

                    if (Directory.Exists(selectedMusicOrFolder))
                    {
                        pathHistory.Push(currentPath);
                        currentPath = selectedMusicOrFolder;
                    }
                    else if (File.Exists(selectedMusicOrFolder))
                    {
                        if (!ImportedMusic.Contains(selectedMusicOrFolder))
                        {
                            ImportedMusic.Add(selectedMusicOrFolder);
                            AnsiConsole.MarkupLine($"[green]Added: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            SaveToFile();
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[red]This music has already been imported: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                        }
                    }
                    else if (selectedMusicOrFolder == "Go back")
                    {
                        if (pathHistory.Count > 0)
                        {
                            currentPath = pathHistory.Pop();
                        }
                        else
                        {
                            Console.Clear();
                            return;
                        }
                    }
                    else if (selectedMusicOrFolder == "Exit")
                    {
                        pathHistory.Clear();
                        Console.Clear();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }
            }
        }

        private string ChooseDrive()
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.Name)
                .ToList();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose a drive to start browsing:")
                    .PageSize(10)
                    .AddChoices(drives)
            );
        }

        private bool IsAudioFile(string path)
        {
            string[] allowedExtensions = { ".mp3", ".wav", ".flac", ".ogg" };
            return allowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        public void CreatePlaylist()
        {
            if (ImportedMusic.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No music imported yet. Please import music first.[/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            var playlistName = AnsiConsole.Ask<string>("Enter a name for the playlist:");
            if (string.IsNullOrWhiteSpace(playlistName))
            {
                AnsiConsole.MarkupLine("[red]Playlist name cannot be empty. Please try again.[/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            if (Playlists.ContainsKey(playlistName))
            {
                AnsiConsole.MarkupLine($"[red]A playlist with the name '{playlistName}' already exists. Please choose a different name.[/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            var selectedTracks = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select tracks to add to the playlist")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more tracks)[/]")
                    .InstructionsText("[grey](Press [blue]<space>[/] to toggle a track, [green]<enter>[/] to accept)[/]")
                    .AddChoices(ImportedMusic.Select(Path.GetFileName))
            );

            if (selectedTracks.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No tracks selected. Playlist creation canceled.[/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            var tracksToAdd = ImportedMusic
                .Where(path => selectedTracks.Contains(Path.GetFileName(path)))
                .ToList();

            Playlists[playlistName] = tracksToAdd;
            SaveToFile();

            AnsiConsole.MarkupLine($"[green]Playlist '{playlistName}' created successfully with {tracksToAdd.Count} tracks![/]");
            Console.ReadKey();
            Console.Clear();
        }

        public void DeleteMusic()
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
        }

        public void DeletePlaylist()
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
        }

        public void EditPlaylist()
        {
            if (Playlists.Count == 0)
            {
                Console.WriteLine("No playlists available to edit.");
                return;
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
                return;
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
                    return;
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
                    return;
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
        }
    }
}