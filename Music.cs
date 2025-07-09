using NAudio.Wave;
using Spectre.Console;
using System.Diagnostics;

namespace Audio
{
    public class Music
    {        
        public static void PlayPlaylist(Dictionary<string, List<string>> playlists, string playlistName)
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

        public static void PlayMusic(List<string> musicList, int index, bool isPlaylist)
        {
            TimeSpan accumulatedPauseTime = TimeSpan.Zero;
            DateTime pauseStartTime = DateTime.MinValue;
            bool isStopped = false;
            bool isNext = false;

            while (index < musicList.Count)
            {
                string selectedMusic = Path.GetFileName(musicList[index]);
                Console.Clear();
                Console.WriteLine($"Playing music: {selectedMusic}");

                AudioFileReader audioFile = null;
                WaveOutEvent outputDevice = null;

                try
                {
                    audioFile = new AudioFileReader(musicList[index]);
                    outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFile);

                    bool naturalEnd = false;

                    outputDevice.PlaybackStopped += (sender, e) =>
                    {
                        if (audioFile != null && !isStopped)
                        {
                            naturalEnd = audioFile.CurrentTime >= audioFile.TotalTime - TimeSpan.FromMilliseconds(100);
                        }
                    };

                    outputDevice.Play();

                    TimeSpan totalTime = audioFile.TotalTime;
                    Stopwatch stopwatch = Stopwatch.StartNew();

                    _ = Task.Run(async () =>
                    {
                        if (isPlaylist == true)
                        {
                            while (outputDevice.PlaybackState != PlaybackState.Stopped)
                            {
                                Console.Clear();
                                Console.WriteLine($"Playing music: {selectedMusic}");
                                Console.WriteLine($"Current time: {audioFile.CurrentTime:hh\\:mm\\:ss} / Total time: {totalTime:hh\\:mm\\:ss}");
                                Console.WriteLine("Controls: [P] Pause | [S] Stop | [N] Next | [F] +10s | [R] -10s | [+/-] Volume");
                                Console.WriteLine($"Volume: {Math.Round(outputDevice.Volume * 100)}");

                                await Task.Delay(500);
                            }
                        }
                        else
                        {
                            while (outputDevice.PlaybackState != PlaybackState.Stopped)
                            {
                                Console.Clear();
                                Console.WriteLine($"Playing music: {selectedMusic}");
                                Console.WriteLine($"Current time: {audioFile.CurrentTime:hh\\:mm\\:ss} / Total time: {totalTime:hh\\:mm\\:ss}");
                                Console.WriteLine("Controls: [P] Pause | [S] Stop | [F] +10s | [R] -10s | [+/-] Volume");
                                Console.WriteLine($"Volume: {Math.Round(outputDevice.Volume * 100)}");

                                await Task.Delay(500);
                            }
                        }
                    });

                    bool isPlaying = true;
                    while (isPlaying)
                    {
                        if (audioFile.CurrentTime >= totalTime - TimeSpan.FromMilliseconds(100))
                        {
                            isPlaying = false;
                            naturalEnd = true;
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
                                case ConsoleKey.F:
                                    if (audioFile.CurrentTime + TimeSpan.FromSeconds(10) < audioFile.TotalTime)
                                    {
                                        audioFile.CurrentTime += TimeSpan.FromSeconds(10);
                                    }
                                    break;
                                case ConsoleKey.R:
                                    if (audioFile.CurrentTime - TimeSpan.FromSeconds(10) > TimeSpan.FromSeconds(0))
                                    {
                                        audioFile.CurrentTime -= TimeSpan.FromSeconds(10);
                                    }
                                    break;
                                case ConsoleKey.N:
                                    outputDevice.Stop();
                                    stopwatch.Stop();
                                    isPlaying = false;
                                    isStopped = true;
                                    isNext = true;
                                    break;
                            }
                        }
                    }

                    outputDevice.Stop();
                    if (isPlaylist)
                    {
                        if (isNext)
                        {
                            index++;
                            isStopped = false;
                        }
                        else
                        {
                            Console.WriteLine(isStopped
                            ? "Playlist has been stopped. Press any key to return to the main menu..."
                            : "Playlist has ended. Press any key to return to the main menu...");
                            Console.ReadKey();
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine(isStopped
                            ? "Music has been stopped. Press any key to return to the main menu..."
                            : "Music has ended. Press any key to return to the main menu...");
                        Console.ReadKey();
                        break;
                    }
                }
                finally
                {
                    outputDevice?.Dispose();
                    audioFile?.Dispose();
                }
            }

            if (isPlaylist && index >= musicList.Count)
            {
                Console.WriteLine("Playlist has finished. Press any key to return to the main menu...");
                Console.ReadKey();
            }

            Console.Clear();
        }
    }
}