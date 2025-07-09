using Spectre.Console;

namespace audio_player
{
    public class Display
    {
        public static void DisplayMusicList(List<string> musicList, string header)
        {
            var table = new Table().Border(TableBorder.Heavy);

            if (musicList.Count > 0)
            {
                table.AddColumn("[SkyBlue1]Index[/]").AddColumn("[SkyBlue1]Music Name[/]");
                foreach (var (audio, index) in musicList.Select((audio, index) => (audio, index)))
                {
                    table.AddRow($"[blue]{index}[/]", $"[white]{Path.GetFileName(audio)}[/]");
                }
            }
            else
            {
                table.AddColumn("[red]You haven't imported any music yet[/]");
            }

            AnsiConsole.Write(
                new Panel(table)
                    .Header($"[SkyBlue1]{header}[/]")
                    .Border(BoxBorder.Heavy)
                    .Padding(1, 0, 1, 0)
            );
        }

        public static void DisplayPlaylists(Dictionary<string, List<string>> playlists)
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
    }
}
