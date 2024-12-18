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
            while (true)
            {
                string path = @"C:\Users\Daniel\Desktop\Audio";
                string[] folderWithAudios = Directory.GetFileSystemEntries(path);

                for (int i = 0; i < folderWithAudios.Length; i++)
                {
                    var musicPanel = new Panel($"Index: {i}  Name of the music: {Path.GetFileName(folderWithAudios[i])}")
                    {
                        Border = BoxBorder.Rounded,
                        Width = 60,
                        Padding = new Padding(1, 0, 1, 0)
                    };
                    
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

                    break; 
                }


                Console.Write("Enter index of music, you want to play: ");
                if (!int.TryParse(Console.ReadLine(), out int input) || input < 0 || input >= folderWithAudios.Length)
                {
                    Console.WriteLine("Invalid index. Press any key to try again.");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }

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
}
