using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Automod.Ids;
using Automod.Extensions;

namespace Automod
{
    public class Program
    {
        private readonly DiscordSocketClient _client;
        private IServiceProvider _services;
        private readonly IServiceCollection _map    = new ServiceCollection();
        private readonly CommandService _commands   = new CommandService();
        private Random rand                         = new Random();

        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 50,
            });
        }

        private static Task Logger(LogMessage message)
        {
            var cc = Console.ForegroundColor;
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        private async Task MainAsync()
        {
            _client.Log += Logger;

            _client.MessageReceived += CheckRules;

            _client.MessageDeleted += OnDelete;

            _client.UserLeft += UserLeft;

            _client.UserJoined += UserJoined;

            _client.UserBanned += UserBanned;

            _client.UserVoiceStateUpdated += ChannelChanged;

            await InitCommands();

            string token;

#if DEBUG
            //token = "put token here";
            Console.WriteLine("Automod running in debug mode.");
#else
            //token = "put token here";
            Console.WriteLine("Automod running in release mode.");
#endif

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(5000);

            await _client.SetGameAsync("Punish the Users (2017)");

            await Task.Delay(-1);
        }

        private async Task InitCommands()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _services = _map.BuildServiceProvider();

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;
            if (msg.Channel.Id != Channels.Commands) return;

            int pos = 0;

            if (msg.HasCharPrefix('&', ref pos))
            {
                var context = new SocketCommandContext(_client, msg);

                var result = await _commands.ExecuteAsync(context, pos, _services);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private async Task CheckRules(SocketMessage message)
        {
            //Prevents Maymay from talking outside of approved channels.
            if (message.Author.Id == 235849760253280257 && !message.Channel.Id.EqualsMulti(Channels.Animal, Channels.Chill, Channels.Vent, Channels.Advice))
                await message.DeleteAsync();

            //Bots are exempt from the rules.
            if (message.Author.IsBot)
                return;

            //Mod and medic chats are also exempt from the rules.
            if (message.Channel.Id == Channels.Mod || message.Channel.Id == Channels.Medic)
                return;

            int ruleBroken = message.CheckRules();

            if (ruleBroken == 20)
                return;

            string ruleBrokenDescription = "ERROR";

            switch (ruleBroken)
            {
                case 1:
                    ruleBrokenDescription = "No single-character/emote messages.";
                    break;
                case 2:
                    ruleBrokenDescription = "No discussion of suicide or rape outside of the designated channels #scream_and_shout or #comfort_and_love.";
                    break;
                case 3:
                    ruleBrokenDescription = "No hate speech or use of slurs.";
                    break;
                case 4:
                    //Unused.
                    ruleBrokenDescription = "No use of Bones' old names.";
                    break;
                case 5:
                    //Unused.
                    ruleBrokenDescription = "No trolling.";
                    break;
                case 6:
                    ruleBrokenDescription = "No links outside of designated channels; check Rules and Info for more information.";
                    break;
                case 7:
                    ruleBrokenDescription = "No ALLCAPS messages. Messages that are mostly ALLCAPS but have one or two lowercase letters still violate this rule.";
                    break;
                case 8:
                    ruleBrokenDescription = "No linking to other Discord servers without permission. Check Rules and Info for more information.";
                    break;
                case 9:
                    ruleBrokenDescription = "No using Lady Luck outside of the designated channel, #lady_luck.";
                    break;
                case 10:
                    ruleBrokenDescription = "No using Maymay outside of designated channels: Cute Animals, Chill Chatter, Comfort and Love, and Scream and Shout.";
                    break;
                default:
                    //I've maintained Automod since around March or so, and I've never had this one come up.
                    ruleBrokenDescription = "Fatal error! Report this to the developer immediately.";
                    break;
            }

            await message.DeleteAsync();

            IMessage scolding = await message.Channel.SendMessageAsync("Violation of Rule " + ruleBroken.ToString() + ": " + ruleBrokenDescription);

            await Task.Delay(5000);

            await scolding.DeleteAsync();

            //Don't add violations of rules 1 and 7 to the log, it makes it fucking impossible to see actual rulebreaks.
            if (ruleBroken == 1 || ruleBroken == 7)
                return;

            ITextChannel logChannel = (message.Author as SocketGuildUser).Guild.GetChannel(Channels.Log) as ITextChannel;

            await logChannel.SendMessageAsync("User " + message.Author.Mention + " (ID: " + message.Author.Id.ToString() + ") " +
                " broke rule " + ruleBroken.ToString() +
                " in channel " + message.Channel.Name +
                ".\nTime: " + message.Timestamp +
                ".\nMessage content:\n\" " + message.Content + " \".");
        }

        //This feels a little Big Brother-ish to me, but oh well.
        private async Task OnDelete(Cacheable<IMessage, ulong> messageCache, ISocketMessageChannel channel)
        {
            await messageCache.GetOrDownloadAsync();

            IMessage message = messageCache.Value;
            string messageRecieved = message.Content.ToLower();

            if (message.Author.IsBot)
                return;

            if ((message as SocketMessage).CheckRules() != 20)
                return;

            //So we spy on everyone except the people in actual power?
            if ((message.Author as SocketGuildUser).GuildPermissions.Administrator)
                return;

            //You have no clue how cluttered the logs were until this line was added.
            if (message.Content.ContainsMulti("&pronouns", "%play", "%queue", "!shib", "&channel"))
                return;

            ITextChannel logChannel = (message.Author as SocketGuildUser).Guild.GetChannel(Channels.Log) as ITextChannel;

            if (message.Content == "")
            {
                await logChannel.SendMessageAsync(message.Attachments.First().ProxyUrl);
                return;
            }

            await logChannel.SendMessageAsync("Message posted by user " + message.Author.Mention +
                " in channel " + message.Channel.Name +
                " has been deleted. Contents: \n\" " + message.Content + " \"");
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            //User ID for iwantpieican, one of the server's corporals, who has to leave the server constantly because of family issues.
            if (user.Id == 209422051721609217)
                return;

            ITextChannel logChannel = user.Guild.GetChannel(Channels.Join) as ITextChannel;

            await logChannel.SendMessageAsync("User " + user.Mention + " (ID: " + user.Id + ")" + " left the server.");
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            switch (user.Id)
            {
                //User ID for iwantpieican, one of the server's corporals, who has to leave the server constantly because of family issues.
                case 209422051721609217:
                    await user.AddRoleAsync(user.Guild.GetRole(Roles.Corporals));
                    break;

                default:
                    string[] MessagesArray =
                    {
                            "Welcome, <USER>! I hope you enjoy neverending rides!",
                            "Hello, <USER>! I hope you enjoy neverending rides!",
                            "Hello, <USER>! Enjoy your stay!",
                            "Have a nice day, <USER>!",
                            "Welcome to the Skeleton Cavern, <USER>!",
                            "Have a nice night, <USER>!",
                            "Please keep your arms and legs inside the vehicle at all times, <USER>, and remember: The ride never ends!",
                            "Hello and welcome to Kentucky Fried Chicken, home of the Kentucky Fried Chicken, can I take your order, <USER>?"
                        };

                    string GreetingsMessage = MessagesArray[rand.Next(MessagesArray.Length)].Replace("<USER>", user.Mention);
                    GreetingsMessage += " Be sure to read #rules_and_info, and feel free to dump an introduction in #introductions.";

                    ITextChannel joinChannel = user.Guild.GetChannel(Channels.Join) as ITextChannel;
                    ITextChannel greetingChannel = user.Guild.GetChannel(Channels.Greetings) as ITextChannel;

                    await joinChannel.SendMessageAsync("User " + user.Mention + " (ID: " + user.Id.ToString() + ") joined the server at " + user.JoinedAt + ".");
                    await greetingChannel.SendMessageAsync(GreetingsMessage);
                    await user.AddRoleAsync(user.Guild.GetRole(Roles.Trainees));
                    break;
            }
        }

        private async Task UserBanned(SocketUser user, SocketGuild guild)
        {
            ITextChannel logChannel = guild.GetChannel(Channels.Join) as ITextChannel;

            await logChannel.SendMessageAsync("User " + user.Mention + " (ID: " + user.Id + ")" + " has been banned.");
        }

        private async Task ChannelChanged(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel == after.VoiceChannel)
                return;

            if (Globals.deleteGuard)
                return;

            if (before.VoiceChannel.Users.IsNullOrEmpty())
                await before.VoiceChannel.DeleteAsync();
        }
    }
}