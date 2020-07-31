using Discord;
using Discord.Commands;
using Ranka.Services;
using Ranka.Utils;
using System;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    /*
     * MusicModule uses Opus native libraries.
     * It's an independent implementation that doesn't require a dedicated server node such as lavalink to work.
     * Based on https://github.com/domnguyen/Discord-Bot/blob/master/src/Modules/AudioModule.cs
     */

    [Name("Music")]
    [Summary("Music module to interact with voice chat. Currently used to playback audio in a stream.")]
    public class MusicModule : RankaModule
    {
        // Private variables
        private readonly MidoService m_Service;

        // Dependencies are automatically injected via this constructor.
        // Remember to add an instance of the service.
        // to your IServiceCollection when you initialize your bot!
        public MusicModule(MidoService service)
        {
            m_Service = service ?? throw new ArgumentNullException(nameof(service), "No MidoService passed");
            m_Service.SetRankaModule(this); // Reference to this from the service.
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        //
        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        //
        // 'Avoid using long-running code in your modules wherever possible.
        // You should not be implementing very much logic into your modules,
        // instead, outsource to a service for that.'

        [Command("join", RunMode = RunMode.Async)]
        [Remarks("join")]
        [Summary("Joins the user's voice channel.")]
        public async Task JoinVoiceChannel()
        {
            if (m_Service.GetDelayAction()) return; // Stop multiple attempts to join too quickly.
            await m_Service.JoinAudioAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel).ConfigureAwait(false);

            // Start the autoplay service if enabled, but not yet started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel).ConfigureAwait(false);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("leave")]
        [Summary("Leaves the current voice channel.")]
        public async Task LeaveVoiceChannel()
        {
            await m_Service.LeaveAudioAsync(Context.Guild).ConfigureAwait(false);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("play <url/index>")]
        [Summary("Plays a song by url or local path.")]
        public async Task PlayVoiceChannel([Remainder][Summary("Link of a song")] string song)
        {
            var voiceState = Context.Client.CurrentUser as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await JoinVoiceChannel().ConfigureAwait(false);
            }

            // Play the audio. We check if audio is null when we attempt to play. This function is BLOCKING.
            await m_Service.ForcePlayAudioAsync(Context.Guild, Context.Channel, StringUtils.DiscordStringFormat(song)).ConfigureAwait(false);

            // Start the autoplay service if enabled, but not yet started.
            // Once force play is done, if auto play is enabled, we can resume the autoplay here.
            // We also write a counter to make sure this is the last play called, to avoid cascading auto plays.
            if (m_Service.GetNumPlaysCalled() == 0) await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel).ConfigureAwait(false);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Remarks("pause")]
        [Summary("Pauses the current song, if playing.")]
        public async Task PauseVoiceChannel()
        {
            m_Service.PauseAudio();
            await Task.Delay(0).ConfigureAwait(false); // Suppress async warrnings.
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Remarks("resume")]
        [Summary("Pauses the current song, if paused.")]
        public async Task ResumeVoiceChannel()
        {
            m_Service.ResumeAudio();
            await Task.Delay(0).ConfigureAwait(false); // Suppress async warrnings.
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Remarks("stop")]
        [Summary("Stops the current song, if playing or paused.")]
        public async Task StopVoiceChannel()
        {
            m_Service.StopAudio();
            await Task.Delay(0).ConfigureAwait(false); // Suppress async warrnings.
        }

        [Command("volume")]
        [Remarks("volume <num>")]
        [Summary("Changes the volume to [0 - 100].")]
        public async Task VolumeVoiceChannel([Summary("Volume percentage")] int volume)
        {
            m_Service.AdjustVolume((float)volume / 100.0f);
            await Task.Delay(0).ConfigureAwait(false); // Suppress async warrnings.
        }

        [Command("add", RunMode = RunMode.Async)]
        [Remarks("add <url/index>")]
        [Summary("Adds a song by url or local path to the playlist.")]
        public async Task AddVoiceChannel([Remainder][Summary("Link/search term")] string song)
        {
            // Add it to the playlist.
            await m_Service.PlaylistAddAsync(StringUtils.DiscordStringFormat(song)).ConfigureAwait(false);

            // Start the autoplay service if enabled, but not yet started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel).ConfigureAwait(false);
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("skip", "next")]
        [Remarks("skip")]
        [Summary("Skips the current song, if playing from the playlist.")]
        public async Task SkipVoiceChannel()
        {
            m_Service.PlaylistSkip();
            await Task.Delay(0).ConfigureAwait(false);
        }

        [Command("playlist", RunMode = RunMode.Async)]
        [Remarks("playlist")]
        [Summary("Shows what's currently in the playlist.")]
        public async Task PrintPlaylistVoiceChannel()
        {
            m_Service.PrintPlaylist();
            await Task.Delay(0).ConfigureAwait(false);
        }

        [Command("autoplay", RunMode = RunMode.Async)]
        [Remarks("autoplay <enable>")]
        [Summary("Starts the autoplay service on the current playlist.")]
        public async Task AutoPlayVoiceChannel([Summary("true/false")] bool enable)
        {
            m_Service.SetAutoPlay(enable);

            // Start the autoplay service if already on, but not started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel).ConfigureAwait(false);
        }

        [Command("whatsong", RunMode = RunMode.Async)]
        [Alias("np")]
        [Summary("What is currently playing")]
        public async Task WhatSongAsync()
        {
            m_Service.FetchMidoData();
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}