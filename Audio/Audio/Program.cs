using System.Diagnostics;
using System.Runtime.CompilerServices;
using Audio;
using NAudio.Wave;
using Spectre.Console;

namespace Audio
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MusicData musicData = MusicData.LoadFromFile();

            List<string> options = new List<string> { "Import music", "Play music", "Play playlist", "Create playlist", "Display playlists", "Delete music", "Delete playlist", "Exit" };

            while (true)
            {
                DisplayMusicList(musicData.ImportedMusic, "Imported music");

                string input = PromptWithSelection("Choose one of the options", options);

                switch (input)
                {
                    case "Import music":
                        ImportMusic(musicData.ImportedMusic);
                        musicData.SaveToFile();
                        break;
                    case "Play music":
                        if (musicData.ImportedMusic.Count > 0)
                        {
                            var selectMusicToPlay = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Choose music to play")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                    .AddChoices(musicData.ImportedMusic.Select(Path.GetFileName))
                            );
                            int index = musicData.ImportedMusic.FindIndex(path => Path.GetFileName(path) == selectMusicToPlay);
                            PlayMusic(musicData.ImportedMusic, index, false);
                        }
                        else
                        {
                            Console.WriteLine("You haven't imported any music yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "Play playlist":
                        if (musicData.Playlists.Count > 0)
                        {
                            var selectPlaylist = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Choose playlist to play")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                    .AddChoices(musicData.Playlists.Select(key => key.Key))
                            );
                            PlayPlaylist(musicData.Playlists, selectPlaylist);
                        }
                        else
                        {
                            Console.WriteLine("You haven't created any playlists yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "Create playlist":
                        if (musicData.ImportedMusic.Count > 0)
                        {
                            CreatePlaylist(musicData.ImportedMusic, musicData.Playlists);
                            musicData.SaveToFile();
                        }
                        else
                        {
                            Console.WriteLine("You haven't imported any music yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "Display playlists":
                        Console.Clear();
                        DisplayPlaylists(musicData.Playlists);
                        Console.WriteLine("Press any key to return to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case "Delete music":
                        if (musicData.ImportedMusic.Count > 0)
                        {
                            musicData.DeleteMusic();
                        }
                        else
                        {
                            Console.WriteLine("You haven't imported any music yet. Press any key to continue");
                            Console.ReadKey();
                        }
                        Console.Clear();
                        break;
                    case "Delete playlist":
                        if (musicData.Playlists.Count > 0)
                        {
                            musicData.DeletePlaylist();
                            Console.Clear();
                        }
                        else
                        {
                            Console.WriteLine("You haven't created any playlist yet. Press any key to continue");
                            Console.ReadKey();
                        }
                        Console.Clear();
                        break;
                    case "Exit":
                        bool x = AnsiConsole.Confirm("Are you sure you want to exit");
                        if (x == true) return;
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to try again");
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                }

            }
        }

        private static string PromptWithSelection(string title, IEnumerable<string> choices)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .AddChoices(choices)
            );
        }

        private static void DisplayMusicList(List<string> musicList, string header)
        {
            var table = new Table().Border(TableBorder.Heavy);

            if (musicList.Count > 0)
            {
                table.AddColumn("[yellow]Index[/]").AddColumn("[yellow]Music Name[/]");
                foreach (var (audio, index) in musicList.Select((audio, index) => (audio, index)))
                {
                    table.AddRow($"[blue]{index}[/]", $"[green]{Path.GetFileName(audio)}[/]");
                }
            }
            else
            {
                table.AddColumn("[red]You haven't imported any music yet[/]");
            }

            AnsiConsole.Write(
                new Panel(table)
                    .Header($"[yellow]{header}[/]")
                    .Border(BoxBorder.Heavy)
                    .Padding(1, 0, 1, 0)
            );
        }

        private static void PlayPlaylist(Dictionary<string, List<string>> playlists, string playlistName)
        {
            if (playlists.TryGetValue(playlistName, out List<string> playlistTracks))
            {
                Console.Clear();
                Console.WriteLine($"Playing playlist: {playlistName}");

                if (playlistTracks.Count == 0)
                {
                    Console.WriteLine("This playlist is empty. Press any key to return to the menu...");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                PlayMusic(playlistTracks, 0, true);
            }
            else
            {
                Console.WriteLine("Invalid playlist name. Press any key to try again...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static void CreatePlaylist(List<string> importedMusicList, Dictionary<string, List<string>> playlists)
        {
            if (importedMusicList.Count == 0)
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

            if (playlists.ContainsKey(playlistName))
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
                    .AddChoices(importedMusicList.Select(Path.GetFileName))
            );

            if (selectedTracks.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No tracks selected. Playlist creation canceled.[/]");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            var tracksToAdd = importedMusicList
                .Where(path => selectedTracks.Contains(Path.GetFileName(path)))
                .ToList();

            playlists[playlistName] = tracksToAdd;
            new MusicData { Playlists = playlists }.SaveToFile();

            AnsiConsole.MarkupLine($"[green]Playlist '{playlistName}' created successfully with {tracksToAdd.Count} tracks![/]");
            Console.ReadKey();
            Console.Clear();
        }

        private static void ImportMusic(List<string> importedMusicList)
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
                        if (!importedMusicList.Contains(selectedMusicOrFolder))
                        {
                            importedMusicList.Add(selectedMusicOrFolder);
                            AnsiConsole.MarkupLine($"[green]Added: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            new MusicData { ImportedMusic = importedMusicList }.SaveToFile();
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
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                }
            }
        }

        private static string ChooseDrive()
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

        private static bool IsAudioFile(string path)
        {
            string[] allowedExtensions = { ".mp3", ".wav", ".flac", ".ogg" };
            return allowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        private static void DisplayPlaylists(Dictionary<string, List<string>> playlists)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .Title("[yellow]Playlists[/]")
                .AddColumn("[blue]Playlist Name[/]")
                .AddColumn("[green]Tracks[/]");

            foreach (var playlist in playlists)
            {
                string tracks = playlist.Value.Count > 0
                    ? string.Join(", ", playlist.Value.Select(Path.GetFileName))
                    : "[grey](No tracks)[/]";

                table.AddRow($"[bold]{playlist.Key}[/]", tracks);
            }

            AnsiConsole.Write(table);
        }

        private static void ChangeVolume(WaveOutEvent outputDevice, bool increase)
        {
            float minVolume = 0.0F;
            float maxVolume = 1.0F;
            float volumeChange = 0.01F;

            float currentVolume = outputDevice.Volume;

            if (increase)
            {
                currentVolume = Math.Min(currentVolume + volumeChange, maxVolume);
            }
            else
            {
                currentVolume = Math.Max(currentVolume - volumeChange, minVolume);
            }

            outputDevice.Volume = currentVolume;
        }

        private static void PlayMusic(List<string> importedMusicList, int index, bool isPlaylist)
        {
            TimeSpan accumulatedPauseTime = TimeSpan.Zero;
            DateTime pauseStartTime = DateTime.MinValue;
            bool isStopped = false;

            while (index < importedMusicList.Count)
            {
                string selectedMusic = Path.GetFileName(importedMusicList[index]);
                Console.Clear();
                Console.WriteLine($"Playing music: {selectedMusic}");

                using (var audioFile = new AudioFileReader(importedMusicList[index]))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    TimeSpan totalTime = audioFile.TotalTime;
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    _ = Task.Run(async () =>
                    {
                        while (outputDevice.PlaybackState != PlaybackState.Stopped)
                        {
                            TimeSpan elapsedTime = stopwatch.Elapsed + accumulatedPauseTime;

                            Console.Clear();
                            Console.WriteLine($"Playing music: {selectedMusic}");
                            Console.WriteLine($"Current time: {elapsedTime:hh\\:mm\\:ss} / Total time: {totalTime:hh\\:mm\\:ss}");
                            Console.WriteLine("[P] Pause/Resume | [S] Stop | [+/-] Volume");
                            Console.WriteLine($"Volume: {Math.Round(outputDevice.Volume * 100)}");

                            await Task.Delay(500);
                        }
                    });

                    bool isPlaying = true;
                    while (isPlaying)
                    {
                        if (stopwatch.Elapsed + accumulatedPauseTime >= totalTime)
                        {
                            isPlaying = false;
                            break;
                        }

                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(true).Key;

                            switch (key)
                            {
                                case ConsoleKey.P:
                                    if (outputDevice.PlaybackState == PlaybackState.Playing)
                                    {
                                        outputDevice.Pause();
                                        pauseStartTime = DateTime.Now;
                                        stopwatch.Stop();
                                    }
                                    else
                                    {
                                        outputDevice.Play();
                                        stopwatch.Start();
                                    }
                                    break;
                                case ConsoleKey.S:
                                    outputDevice.Stop();
                                    stopwatch.Stop();
                                    isPlaying = false;
                                    isStopped = true;
                                    break;
                                case ConsoleKey.OemPlus:
                                    ChangeVolume(outputDevice, true);
                                    break;
                                case ConsoleKey.OemMinus:
                                    ChangeVolume(outputDevice, false);
                                    break;
                            }
                        }
                    }

                    outputDevice.Stop();
                }

                if (isPlaylist)
                {
                    index++;
                }
                else
                {
                    if (isStopped)
                    {
                        Console.WriteLine("Music has been stopped. Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Music has ended. Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    }
                }
            }

            if (isPlaylist && index >= importedMusicList.Count)
            {
                Console.WriteLine("Playlist has finished. Press any key to return to the main menu...");
                Console.ReadKey();
            }

            Console.Clear();
        }
    }
}