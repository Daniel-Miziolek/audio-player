using System.Diagnostics;
using NAudio.Wave;
using Spectre.Console;

namespace Audio
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<string> importedMusicList = new();
            Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();
            List<string> options = new List<string> { "Import music", "Play music", "Play playlist", "Create playlist", "Display playlists", "Exit" };

            while (true)
            {
                DisplayMusicList(importedMusicList, "Imported music");

                string input = PromptWithSelection("Choose one of the options", options);

                switch (input)
                {
                    case "Import music":
                        ImportMusic(importedMusicList);
                        break;
                    case "Play music":
                        if (importedMusicList.Count > 0)
                        {
                            var selectMusicToPlay = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Choose music to play")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                    .AddChoices(importedMusicList.Select(Path.GetFileName))
                            );
                            int index = importedMusicList.FindIndex(path => Path.GetFileName(path) == selectMusicToPlay);
                            PlayMusic(importedMusicList, index, false);
                        }
                        else
                        {
                            Console.WriteLine("You haven't imported any music yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "Play playlist":
                        if (playlists.Count > 0)
                        {
                            var selectPlaylist = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Choose playlist to play")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                    .AddChoices(playlists.Select(key => key.Key))
                            );
                            PlayPlaylist(playlists, selectPlaylist);
                        }
                        else
                        {
                            Console.WriteLine("You haven't created any playlists yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "Create playlist":
                        if (importedMusicList.Count > 0)
                        {
                            CreatePlaylist(importedMusicList, playlists);
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
                        DisplayPlaylists(playlists);
                        Console.WriteLine("Press any key to return to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case "Exit":
                        Console.WriteLine("Are you sure you want to exit? [y,n]");
                        string yesOrNo = Console.ReadLine();
                        if (yesOrNo.ToLower() == "y") return;
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

        static string PromptWithSelection(string title, IEnumerable<string> choices)
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .AddChoices(choices)
            );
        }

        static void DisplayMusicList(List<string> musicList, string header)
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


        static void PlayPlaylist(Dictionary<string, List<string>> playlists, string playlistName)
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

        static void CreatePlaylist(List<string> importedMusicList, Dictionary<string, List<string>> playlists)
        {
            Console.Write("Enter a name for the playlist: ");
            string nameOfPlaylist = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nameOfPlaylist) || playlists.ContainsKey(nameOfPlaylist))
            {
                Console.WriteLine("Invalid or duplicate playlist name. Press any key to try again.");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            List<string> musicToAdd = new List<string>();

            while (true)
            {
                var selectedMusic = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose music to add to the playlist")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more music)[/]")
                        .AddChoices(importedMusicList.Select(Path.GetFileName)
                    )
                );

                string fullPath = importedMusicList.FirstOrDefault(f => Path.GetFileName(f) == selectedMusic);
                if (fullPath != null && !musicToAdd.Contains(fullPath))
                {
                    musicToAdd.Add(fullPath);
                    AnsiConsole.MarkupLine($"[green]{selectedMusic} added to playlist {nameOfPlaylist}.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{selectedMusic} is already in the playlist.[/]");
                }

                bool addMore = AnsiConsole.Confirm("Do you want to add more music?");
                if (!addMore) break;
            }

            playlists[nameOfPlaylist] = musicToAdd;
            Console.WriteLine($"Playlist '{nameOfPlaylist}' created successfully.");
            Console.ReadKey();
            Console.Clear();
        }

        static void ImportMusic(List<string> importedMusicList)
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
                        if (importedMusicList.Contains(selectedMusicOrFolder))
                        {
                            AnsiConsole.MarkupLine($"[red]This music has already been imported: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }
                        else
                        {
                            importedMusicList.Add(selectedMusicOrFolder);
                            AnsiConsole.MarkupLine($"[green]Added: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            Console.Clear();
                            break;
                        }
                    }
                    else if (selectedMusicOrFolder == "Go back")
                    {
                        if (pathHistory.Count > 0)
                        {
                            currentPath = pathHistory.Pop();
                            continue;
                        }
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Access denided: {ex.Message}[/]");
                }
                catch (IOException ex)
                {
                    AnsiConsole.MarkupLine($"[red]I/O error: {ex.Message}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                }
            }
        }

        static string ChooseDrive()
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

        static bool IsAudioFile(string path)
        {
            string[] allowedExtensions = { ".mp3", ".wav", ".flac", ".ogg" };
            return allowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        static void DisplayPlaylists(Dictionary<string, List<string>> playlists)
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

        static void PlayMusic(List<string> importedMusicList, int index, bool isPlaylist)
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
                            Console.WriteLine("[E] Pause/Resume | [Y] Stop");

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
                            var key = Console.ReadKey(true);

                            if (key.Key == ConsoleKey.E)
                            {
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
                            }
                            else if (key.Key == ConsoleKey.Y)
                            {
                                outputDevice.Stop();
                                stopwatch.Stop();
                                isPlaying = false;
                                isStopped = true;
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
                    if (isStopped == true)
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
