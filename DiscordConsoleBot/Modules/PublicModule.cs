using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordConsoleBot.Types;
using Newtonsoft.Json.Linq;

namespace DiscordConsoleBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        [Command("i")]
        [Alias("I")]
        [Summary("Gets an image from Google Image Search")]
        public async Task Say([Remainder] string input)
        {
            await ReplyAsync(GetGoogleImageResult(input));
        }

        [Command("emote")]
        [Alias("Emote")]
        [Summary("GetsEmojis")]
        public async Task Emote([Remainder] string input)
        {
            var directory = Directory.GetCurrentDirectory() + "\\Assets\\";
            if (input == "list")
            {
                var emoList = "";
                foreach (var emo in Directory.GetFiles(directory, "*.png").Select(Path.GetFileName).ToArray())
                {
                    emoList = emoList + " " + emo.Replace(".png", "") + " ";
                }
                await ReplyAsync("List of emotes are: \n" + emoList);
            }
            await Context.Channel.SendFileAsync(directory + input + ".png");
        }

        [Command("tag")]
        [Alias("Tag")]
        [Summary("Tag yourself with whatever tag is available")]
        public async Task Tag([Remainder] string tagName)
        {
            var user = Context.User;
            var role = Context.Guild.Roles.Where(x => x.Name.ToLower().StartsWith(tagName.ToLower())).OrderBy(x => x.Name).First();

            try
            {
                await (user as IGuildUser).AddRoleAsync(role);
                await ReplyAsync("Tag " + role.Name + " added! To remove this tag, just say !removetag tagname");

            }
            catch (Exception e)
            {
                await ReplyAsync("Either that role is not available or you're not supposed to have that role!");
            }
        }

        [Command("tagremove")]
        [Alias("Tagremove", "removetag", "Removetag")]
        [Summary("Remove tags that you currently have")]
        public async Task Tagremove([Remainder] string tagName)
        {
            var user = Context.User;
            var role = Context.Guild.Roles.Where(x => x.Name.ToLower().StartsWith(tagName.ToLower())).OrderBy(x => x.Name).First();

            try
            {
                await (user as IGuildUser).RemoveRoleAsync(role);
                await ReplyAsync("Tag " + role.Name + " removed! To add the tag back, just say !tag tagname");
            }
            catch (Exception e)
            {
                await ReplyAsync("Either that role is not available or you don't have permission!");
            }
        }

        [Command("roll")]
        [Alias("dice", "d", "r")]
        [Summary("Rolls a dice following dice notation logic")]
        public async Task RollDice(string diceNotation)
        {
            var resolver = DiceResolver.FromDiceNotation(diceNotation);
            if(resolver == null)
            {
                await ReplyAsync("Invalid dice notation");
                return;
            }

            resolver.Roll();
            await ReplyAsync(resolver.LastResult);
        }


        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        private string GetGoogleImageResult(string arg)
        {
            var parameter = WebUtility.UrlEncode(arg);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var keys = new[] { ConfigurationManager.AppSettings["GoogleKey1"],
                                   ConfigurationManager.AppSettings["GoogleKey2"],
                                   ConfigurationManager.AppSettings["GoogleKey3"] };

                foreach (var key in keys)
                {
                    var urlParameters = $"?key={key}&amp&cx=002562538147271710657:8gkjvuo8weg&q=" + parameter + "&searchType=image";
                    var response = client.GetAsync(urlParameters).Result;
                    if (!response.IsSuccessStatusCode) continue;
                    dynamic data = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    return data.items[0].link;
                }
                return "Failover quota maxed";
            }
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}
