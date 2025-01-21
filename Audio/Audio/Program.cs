using System.Diagnostics;
using NAudio.Wave;
using Spectre.Console;

namespace Audio
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<string> folderWithAudios = new();
            Dictionary<string, List<string>> playlists = new Dictionary<string, List<string>>();

            while (true)
            {
                DisplayMusicList(folderWithAudios, "Imported music");

                Console.WriteLine("1. Import music");
                Console.WriteLine("2. Play music");
                Console.WriteLine("3. Play playlist");
                Console.WriteLine("4. Create playlist");
                Console.WriteLine("5. Display playlists");
                Console.WriteLine("6. Exit");
                Console.Write("Enter your choice: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ImportMusic(folderWithAudios);
                        break;
                    case "2":
                        if (folderWithAudios.Count > 0)
                        {
                            var selectMusicToPlay = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("Choose music to play")
                                .PageSize(10)
                                .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                .AddChoices(folderWithAudios.Select(Path.GetFileName)));
                            int index = folderWithAudios.FindIndex(path => Path.GetFileName(path) == selectMusicToPlay);
                            PlayMusic(folderWithAudios, index, false);
                        }
                        else
                        {
                            Console.WriteLine("You haven't imported any music yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "3":
                        if (playlists.Count > 0)
                        {
                            var selectPlaylist = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Choose playlist to play")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                                    .AddChoices(playlists.Select(key => key.Key)));
                            PlayPlaylist(playlists, selectPlaylist);
                        }
                        else
                        {
                            Console.WriteLine("You haven't created any playlists yet. Press any key to continue");
                            Console.ReadKey();
                            Console.Clear();
                        }
                        break;
                    case "4":
                        CreatePlaylist(folderWithAudios, playlists);
                        break;
                    case "5":
                        Console.Clear();
                        DisplayPlaylists(playlists);
                        Console.WriteLine("Press any key to return to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case "6":
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
                    .Padding(1, 0, 1, 0));
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

        static void CreatePlaylist(List<string> folderWithAudios, Dictionary<string, List<string>> playlists)
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
                        .AddChoices(folderWithAudios.Select(Path.GetFileName)));

                string fullPath = folderWithAudios.FirstOrDefault(f => Path.GetFileName(f) == selectedMusic);
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

        static void ImportMusic(List<string> folderWithAudios)
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
                            .AddChoices(entries));



                    if (Directory.Exists(selectedMusicOrFolder))
                    {
                        pathHistory.Push(currentPath);
                        currentPath = selectedMusicOrFolder;
                    }
                    else if (File.Exists(selectedMusicOrFolder))
                    {
                        if (folderWithAudios.Contains(selectedMusicOrFolder))
                        {
                            AnsiConsole.MarkupLine($"[red]This music has already been imported: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }
                        else
                        {
                            folderWithAudios.Add(selectedMusicOrFolder);
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
                    .AddChoices(drives));
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

        static void PlayMusic(List<string> folderWithAudios, int index, bool isPlaylist)
        {
            TimeSpan accumulatedPauseTime = TimeSpan.Zero;
            DateTime pauseStartTime = DateTime.MinValue;
            bool isStopped = false;

            while (index < folderWithAudios.Count)
            {
                string selectedMusic = Path.GetFileName(folderWithAudios[index]);
                Console.Clear();
                Console.WriteLine($"Playing music: {selectedMusic}");

                using (var audioFile = new AudioFileReader(folderWithAudios[index]))
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

            if (isPlaylist && index >= folderWithAudios.Count)
            {
                Console.WriteLine("Playlist has finished. Press any key to return to the main menu...");
                Console.ReadKey();
            }

            Console.Clear();
        }

    }
}
