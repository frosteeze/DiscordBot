using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordConsoleBot.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        
        public StartupService(DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            var discordToken = ConfigurationManager.AppSettings["DiscordToken"];

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
