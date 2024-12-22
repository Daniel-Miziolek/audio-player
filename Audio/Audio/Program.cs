using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;
using System.Threading.Tasks;
using Spectre.Console;

namespace Audio
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string path = @"C:\Users\Daniel\Desktop\Audio";
            List<string> folderWithAudios = Directory.GetFileSystemEntries(path).ToList();

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
                        Console.WriteLine("Create playlist");
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

        static void ImportMusic(List<string> folderWithAudios)
        {
            string currentPath = @"C:\";
            Stack<string> pathHistory = new Stack<string>();

            string[] entries = Directory.GetFileSystemEntries(currentPath);
            int currentLine = 0;
            int topLine = 0;
            int windowHeight = Console.WindowHeight;

            Console.Clear();
            DisplayEntries(entries, currentLine, topLine, windowHeight);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (currentLine < entries.Length - 1)
                    {
                        currentLine++;
                        if (currentLine >= topLine + windowHeight)
                        {
                            topLine++;
                        }
                        DisplayEntries(entries, currentLine, topLine, windowHeight);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (currentLine > 0)
                    {
                        currentLine--;
                        if (currentLine < topLine)
                        {
                            topLine--;
                        }
                        DisplayEntries(entries, currentLine, topLine, windowHeight);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Enter)
                {
                    string path = entries[currentLine];
                    if (Directory.Exists(path))
                    {
                        pathHistory.Push(currentPath);
                        currentPath = path;
                        entries = Directory.GetFileSystemEntries(currentPath);
                        Console.Clear();
                        currentLine = 0;
                        topLine = 0;
                        DisplayEntries(entries, currentLine, topLine, windowHeight);
                    }
                    else if (File.Exists(path))
                    {
                        OpenFile(path, folderWithAudios);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (pathHistory.Count > 0)
                    {
                        currentPath = pathHistory.Pop();
                        entries = Directory.GetFileSystemEntries(currentPath);
                        Console.Clear();
                        currentLine = 0;
                        topLine = 0;
                        DisplayEntries(entries, currentLine, topLine, windowHeight);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.Clear();
                    break;
                }
            }
        }

        static void DisplayEntries(string[] entries, int currentLine, int topLine, int windowHeight)
        {
            Console.Clear();

            for (int i = topLine; i < Math.Min(topLine + windowHeight, entries.Length); i++)
            {
                bool accessDenied = false;

                if (Directory.Exists(entries[i]))
                {
                    try
                    {
                        Directory.GetFileSystemEntries(entries[i]);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        accessDenied = true;
                    }
                }

                if (i == currentLine)
                {
                    HighlightLine(i - topLine, entries[i], accessDenied);
                }
                else
                {
                    if (accessDenied)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.SetCursorPosition(0, i - topLine);
                    Console.WriteLine(entries[i]);
                    Console.ResetColor();
                }
            }
        }

        static void HighlightLine(int lineIndex, string entry, bool accessDenied)
        {
            Console.SetCursorPosition(0, lineIndex);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = accessDenied ? ConsoleColor.Red : ConsoleColor.White;
            Console.WriteLine(entry);
            Console.ResetColor();
        }

        static void OpenFile(string path, List<string> folderWithAudios)
        {
            if (!folderWithAudios.Contains(path))
            {
                folderWithAudios.Add(path);
                Console.WriteLine($"File {Path.GetFileName(path)} added to the list.");
            }
            else
            {
                Console.WriteLine($"File {Path.GetFileName(path)} is already in the list.");
            }
        }

        private static void PlayMusic(List<string> folderWithAudios, int input)
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

                //else if (audioFile.TotalTime <= stopwatch.Elapsed)
                //{
                //    Console.WriteLine("Music has ended");
                //    outputDevice.Stop();
                //    stopwatch.Stop();
                //    isPlaying = false;
                //    break;
                //}

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
