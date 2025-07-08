using Spectre.Console;

namespace Audio
{
    class Program
    {        
        static async Task Main(string[] args)
        {
            MusicData musicData = MusicData.LoadFromFile();

            List<string> options = ["Import music",
                "Play music",
                "Play playlist",
                "Create playlist",
                "Display playlists",
                "Edit playlist",
                "Delete music",
                "Delete playlist",
                "Exit"];

            while (true)
            {
                AnsiConsole.Write(
                    new FigletText("Audio Player")
                        .LeftJustified()
                        .Color(Color.SkyBlue1));

                Music.DisplayMusicList(musicData.ImportedMusic, "Imported music");

                string input = PromptWithSelection("Choose one of the options", options);

                switch (input)
                {
                    case "Import music":
                        musicData.ImportMusic();
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
                            Music.PlayMusic(musicData.ImportedMusic, index, false);
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
                            Music.PlayPlaylist(musicData.Playlists, selectPlaylist);
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
                            musicData.CreatePlaylist();
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
                        Music.DisplayPlaylists(musicData.Playlists);
                        Console.WriteLine("Press any key to return to the menu...");
                        Console.ReadKey();
                        Console.Clear();
                        break;
                    case "Edit playlist":
                        if (musicData.Playlists.Count > 0)
                        {
                            musicData.EditPlaylist();
                        }
                        else
                        {
                            Console.WriteLine("You haven't created any playlists yet. Press any key to continue");
                            Console.ReadKey();
                        }
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
    }
}