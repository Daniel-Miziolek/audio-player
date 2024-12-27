using System.Diagnostics;
using NAudio.Wave;
using Spectre.Console;

namespace Audio
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = @"C:\Users\Daniel\Desktop\Audio";
            List<string> folderWithAudios = Directory.GetFileSystemEntries(path).ToList();
            Dictionary<string, List<string>> playlist = new Dictionary<string, List<string>>();

            while (true)
            {                
                AnsiConsole.Write(
                    new Panel(
                        new Rows(
                            folderWithAudios
                                .Select((audio, index) =>
                                    new Panel($"Index: {index}  Name of the music: {Path.GetFileName(audio)}")
                                    {
                                        Border = BoxBorder.Rounded,
                                        Width = 60,
                                    })
                                .ToArray()
                        )
                    )
                    .Border(BoxBorder.Double)
                    .Header(" Imported Music ")
                    .Padding(1, 0, 1, 0)
                );

                Console.WriteLine("1. Import music");
                Console.WriteLine("2. Play music");
                Console.WriteLine("3. Create playlist");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ImportMusic(folderWithAudios);
                        break;
                    case "2":
                        Console.Write("Enter index of music you want to play: ");
                        if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= folderWithAudios.Count)
                        {
                            Console.WriteLine("Invalid index. Press any key to try again.");
                            Console.ReadKey();
                            Console.Clear();
                            continue;
                        }
                        PlayMusic(folderWithAudios, index);
                        break;
                    case "3":
                        CreatePlaylist(folderWithAudios, playlist);
                        break;
                    case "4":
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

        static void CreatePlaylist(List<string> folderWithAudios, Dictionary<string, List<string>> playLists)
        {
            Console.Write("Enter a name of playlist: ");
            string nameOfPlaylist = Console.ReadLine();
            AnsiConsole.MarkupLine($"[green]Create a playlist {nameOfPlaylist}[/]");
            List<string> musicToAdd = new List<string>();
            while (true)
            {
                var musicToAdd2 = AnsiConsole.Prompt<string>(
                    new SelectionPrompt<string>()
                        .Title("Choose musics to add")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more musics)[/]")
                        .AddChoices(folderWithAudios));
                musicToAdd.Add(musicToAdd2);
                AnsiConsole.WriteLine($"Music: {musicToAdd} added to your playlist: {nameOfPlaylist}!");
                Console.WriteLine("Are you want to add more music [y,n]");
                string yesOrNo = Console.ReadLine();
                if (yesOrNo == "y")
                {
                    
                }
                else if (yesOrNo == "n")
                {
                    break;
                }
                
            }

            Console.WriteLine($"Your created plalist has name: {nameOfPlaylist} and music: ");
            for (int i = 0; i < musicToAdd.Count; i++)
            {
                Console.WriteLine(musicToAdd[i]);
            }
            Console.ReadKey();
            Console.Clear();
        
        }

        static void ImportMusic(List<string> folderWithAudios)
        {
            string currentPath = @"C:\Users\Daniel";            

            while (true)
            {
                try
                {
                    var entries = Directory.GetFileSystemEntries(currentPath)
                        .Where(entry => Directory.Exists(entry) || IsAudioFile(entry)) 
                        .ToList();
         
                    Stack<string> pathHistory = new Stack<string>();

                    var selectedMusicOrFolder = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Choose music to import")
                            .PageSize(10) // Rozwiązanie w SpectreConsoleTest oraz problem z  page size!
                            .MoreChoicesText("[grey](Move up and down to reveal more folders and files)[/]")
                            .AddChoices(entries));                            

                    if (Directory.Exists(selectedMusicOrFolder))
                    {
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
                            AnsiConsole.MarkupLine($"[green]Add: {Path.GetFileName(selectedMusicOrFolder)}[/]");
                            Console.ReadKey();
                            Console.Clear();
                            break;
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }

        }

        static bool IsAudioFile(string path)
        {
            string[] allowedExtensions = { ".mp3", ".wav", ".flac", ".ogg" };
            return allowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        static void PlayMusic(List<string> folderWithAudios, int input)
        {

            string selectedMusic = Path.GetFileName(folderWithAudios[input]);
            Console.WriteLine($"Selected music: {selectedMusic}");

            using (var audioFile = new AudioFileReader(folderWithAudios[input]))
            using (var outputDevice = new WaveOutEvent())
            {
                Stopwatch stopwatch = new Stopwatch();
                outputDevice.Init(audioFile);
                outputDevice.Play();
                stopwatch.Start();
                TimeSpan totalTime = audioFile.TotalTime;

                string formattedTime = $"{totalTime.Hours:D2}:{totalTime.Minutes:D2}:{totalTime.Seconds:D2}";
                string formattedTimer = $"{stopwatch.Elapsed.Hours:D2}:{stopwatch.Elapsed.Minutes:D2}:{stopwatch.Elapsed.Seconds:D2}";

                bool isPlaying = true;

                _ = Task.Run(async () =>
                {
                    while (isPlaying)
                    {
                        Console.Clear();
                        Console.WriteLine($"Selected music: {selectedMusic}");
                        Console.WriteLine($"Actual time of music: {stopwatch.Elapsed:hh\\:mm\\:ss}  ---   Total time: {formattedTime}");
                        Console.WriteLine("[E] Pause | [Y] Stop");

                        if (audioFile.TotalTime <= stopwatch.Elapsed)
                        {
                            Console.WriteLine("Music has ended");
                            outputDevice.Stop();
                            stopwatch.Stop();
                            isPlaying = false;
                            break;
                        }

                        await Task.Delay(1000);
                    }
                });

                while (true)
                {
                    if (!isPlaying)
                    {
                        break;
                    }
                    else
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.E)
                        {
                            if (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                outputDevice.Pause();
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
                            break;
                        }
                    }
                }
            }

            Console.Clear();
        }
    }
}
