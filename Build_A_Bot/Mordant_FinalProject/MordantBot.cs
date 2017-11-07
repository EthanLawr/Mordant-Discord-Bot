using System;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace mordant_FinalProject
{
    public class MordantBot
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        public static void Main(string[] args)
        => new MordantBot().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Setting the Window Sizes
            int originalWidth = Console.WindowWidth;
            int originalHeight = Console.WindowHeight;
            Console.SetWindowSize(originalWidth, originalHeight * 2);
            //Declaring variables.
            client = new DiscordSocketClient(new DiscordSocketConfig

            {
                LogLevel = LogSeverity.Verbose,
                WebSocketProvider = WS4NetProvider.Instance
            });


            string token = "TOKEN";
            commands = new CommandService();
            client.Log += Log;
            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();
            //Connect bot to Discord._
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            //Prevents bot from dying.
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            /* Determine if the message is a command, based on if it starts with '!'
            or a mention prefix */
            if (! message.HasMentionPrefix(client.CurrentUser, ref argPos)) return;
            // Create a Command Context
            var context = new SocketCommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                if(result.ErrorReason is "Unknown command.")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Mordant was mentioned without a command!");
                    Console.ResetColor();
                } else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                    Console.WriteLine("Error: " + result.ErrorReason);
                    Console.ResetColor();
                }

                
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }




        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }
    }

}