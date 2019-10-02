using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordConsoleBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;
        
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _discord.MessageReceived += HandleCommandAsync;

        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var argPos = 0;
            if (!(msg.HasCharPrefix('!', ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))) return;

            var context = new SocketCommandContext(_discord, msg);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (!result.IsSuccess)
                Console.WriteLine(result.ErrorReason);
        }
    }
}
