using Discord;
using Discord.Commands;
using GoogleTranslateFreeApi;
using Ranka.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ranka.Modules
{
    // I went out of ideas.
    [Name("Fun")]
    [Summary("Fun pointless commands")]
    public class FunModule : RankaModule
    {
        [Command("sex")]
        [Summary("...")]
        public async Task SexAsync()
        {
            string assetsDir = AppDomain.CurrentDomain.BaseDirectory + "assets";
            string[] replies =
            {
                "...",
                "why.",
                "no sex!",
                "no sex!!!!",
                "I find it hard to believe why my programmer gave me a command named this. ~(>_<。)＼",
                "I said no!",
                "why, did, you, command, me, that?",
                "jesus you must be lonely.",
                "could you stop?",
                "fuck.",
                $"{assetsDir}/ranka_really.JPEG",
                $"{assetsDir}/ranka_really2.JPEG",
                $"{assetsDir}/ranka_deadinside.JPEG"
            };

            var randomReply = replies[new Random().Next(replies.Length)];

            if (randomReply.EndsWith(".JPEG"))
                await RankaFileUploadAsync(randomReply).ConfigureAwait(false);
            else
                await RankaReplyAsync(randomReply).ConfigureAwait(false);
        }

        [Command("translate")]
        [Summary("Translates a sentence to English")]
        public async Task TranslateAsync([Remainder][Summary("The sentence")] string sentence)
        {
            var translator = new GoogleTranslator();

            Language from = Language.Auto;
            Language to = Language.English;

            TranslationResult result = await translator.TranslateAsync(sentence, from, to).ConfigureAwait(false);

            double confidence = Math.Round(double.Parse(result.LanguageDetections[0].Confidence.ToString()) * 100, 2);

            EmbedBuilder eb = new EmbedBuilder
            {
                Title = "Translation",
                Description = $"Sentence: {result.OriginalText}\nLanguage detected: {result.LanguageDetections[0].Language.FullName} ({confidence}% confident)",
                Color = Color.Green
            };

            eb.AddField("In English", result.MergedTranslation);
            if (result.OriginalTextTranscription != null) eb.AddField("Transcription", result.OriginalTextTranscription);
            if (result.Synonyms != null) eb.AddField("Synonyms", result.Synonyms.ToString());

            eb.WithFooter(footer =>
            {
                footer.Text = "Google Translate";
            });

            await RankaReplyAsync(eb).ConfigureAwait(false);
        }

        [Command("widen", RunMode = RunMode.Async)]
        [Summary("Widen an image")]
        public async Task WidenAsync()
        {
            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            string img = await RankaMessageGrabber.GetLastImage(Context).ConfigureAwait(false);

            var input = RankaFileHandler.DownloadImage(img);

            try
            {
                using Process imageMagick = new Process();
                imageMagick.StartInfo.UseShellExecute = false;
                imageMagick.StartInfo.FileName = "convert";
                imageMagick.StartInfo.Arguments = $"\"{input}\" -resize \"2000x500!\" -quality 100 \"{rootdir}/output.png\"";
                imageMagick.StartInfo.CreateNoWindow = true;
                imageMagick.Start();
                imageMagick.WaitForExit();

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Command("widen", RunMode = RunMode.Async)]
        [Summary("Widen someone")]
        public async Task WidenAsync([Remainder][Summary("That someone")] IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user), "No user mentioned");

            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);
            string avatar = user.GetAvatarUrl();

            var input = RankaFileHandler.DownloadImage(avatar);

            try
            {
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -resize \"2000x500!\" -quality 100 \"{rootdir}/output.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Command("fuckitup", RunMode = RunMode.Async)]
        [Summary("Fucks up an image")]
        public async Task FuckUpAsync()
        {
            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);
            string img = await RankaMessageGrabber.GetLastImage(Context).ConfigureAwait(false);

            var input = RankaFileHandler.DownloadImage(img);

            try
            {
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -liquid-rescale \"500x500\" -implode 0.25 -quality 100 \"{rootdir}/output.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Command("cubeitup", RunMode = RunMode.Async)]
        [Summary("Cubes up an image")]
        public async Task CubeUpAsync()
        {
            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            string img = await RankaMessageGrabber.GetLastImage(Context).ConfigureAwait(false);

            var input = RankaFileHandler.DownloadImage(img);

            try
            {
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -resize \"256x256\" \"{rootdir}/top.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -resize \"256x256\" \"{rootdir}/left.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -resize \"256x256\" \"{rootdir}/right.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{rootdir}/top.png\" -resize 260x301! -alpha set -background none -shear 0x30 -rotate -60 -gravity center -crop 960x301+0+0 \"{rootdir}/top_shear.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{rootdir}/left.png\" -resize 260x301! -alpha set -background none -shear 0x30 \"{rootdir}/left_shear.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{rootdir}/right.png\" -resize 260x301! -alpha set -background none -shear 0x-30 \"{rootdir}/right_shear.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "bash";
                    imageMagick.StartInfo.Arguments = $"-c \"convert ./image/left_shear.png ./image/right_shear.png +append \\( ./image/top_shear.png -repage +0-149 \\) -background none -layers merge +repage -resize 80% ./image/output.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Command("write", RunMode = RunMode.Async)]
        [Summary("Write something and turn it into an image")]
        public async Task MakeTextAsync([Remainder][Summary("Your quote")] string quote)
        {
            if (quote == null) throw new ArgumentNullException(nameof(quote), "Nothings written");

            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            try
            {
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"-background white -fill black -font /usr/share/fonts/truetype/freefont/FreeSansBoldOblique.ttf -pointsize 72 -size 480x -gravity Center caption:\"{quote}\" \"{rootdir}/output.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Command("writeimg", RunMode = RunMode.Async)]
        [Summary("Write something and turn it into an image from last image")]
        public async Task MakeTextImgAsync([Remainder][Summary("Your quote")] string quote)
        {
            if (quote == null) throw new ArgumentNullException(nameof(quote), "Nothings written");

            string rootdir = $"{AppDomain.CurrentDomain.BaseDirectory}image";
            if (!Directory.Exists(rootdir)) Directory.CreateDirectory(rootdir);

            string img = await RankaMessageGrabber.GetLastImage(Context).ConfigureAwait(false);

            var input = RankaFileHandler.DownloadImage(img);

            try
            {
                await Task.Run(() =>
                {
                    using Process imageMagick = new Process();
                    imageMagick.StartInfo.UseShellExecute = false;
                    imageMagick.StartInfo.FileName = "convert";
                    imageMagick.StartInfo.Arguments = $"\"{input}\" -fill black -font /usr/share/fonts/truetype/freefont/FreeSansBoldOblique.ttf -pointsize 64 -size 480x -gravity Center -annotate +0+4 \"{quote}\" \"{rootdir}/output.png\"";
                    imageMagick.StartInfo.CreateNoWindow = true;
                    imageMagick.Start();
                    imageMagick.WaitForExit();
                }).ConfigureAwait(false);

                await RankaFileUploadAsync($"{rootdir}/output.png").ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}