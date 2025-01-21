# Audio Console Application

A simple console-based audio player allowing users to import, play, and manage playlists.

## Features
- Import and play audio files (.mp3, .wav, .flac, .ogg).
- Create and manage playlists.
- Pause, resume, or stop playback.

## Usage
1. **Import music** from directories.
2. **Play music**: Choose a single track or entire playlists.
3. **Create playlists** by selecting imported music.

## Requirements
- .NET Core or .NET 5+
- NAudio for audio playback
- Spectre.Console for UI

## Installation
Clone the repository:
```bash
git clone https://github.com/Daniel-Miziolek/audio-player.git
cd audio-player
dotnet restore
dotnet run
