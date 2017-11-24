using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Automod
{
    public class Program
    {
#if DEBUG
        public enum ChannelIds : ulong
        {
            Log = 301455883336941569,
            Join = 000000000000000000,
            Mod = 327522383852470282,
            Medic = 000000000000000000,
            Rules = 000000000000000000,
            VInfo = 000000000000000000,
            Announce = 000000000000000000,
            Streams = 337399140768153621,
            Greetings = 000000000000000000,
            Introduce = 000000000000000000,
            General = 301454585858490378,
            Chill = 000000000000000000,
            Theory = 000000000000000000,
            Animal = 000000000000000000,
            Advice = 000000000000000000,
            Vent = 000000000000000000,
            Voice = 000000000000000000,
            Voice2 = 000000000000000000,
            Audition = 000000000000000000,
            Fanart = 000000000000000000,
            Art = 000000000000000000,
            Question = 000000000000000000,
            Fanfic = 000000000000000000,
            Suggest = 000000000000000000,
            Meme = 000000000000000000,
            Unrelated = 000000000000000000,
            Recommend = 000000000000000000,
            Commands = 000000000000000000,
            Office = 000000000000000000
        }
        public enum RoleIds : ulong
        {
            Sergeants = 303771377796775937,
            Donators = 000000000000000000,
            Captains = 303771404950831105,
            Robots = 302964385737998336,
            Corporals = 303771418611679234,
            Privates = 303771435866914819,
            Trainees = 303771458184937472,
            PrThey = 303771490858303488,
            PrShe = 303771490824880129,
            PrHe = 303771475804946432,
            PrXe = 303771562455072768,
            PrAny = 321711348394491916,
            Everyone = 327570844530507777,
        }
#else 
        public enum ChannelIds : ulong
        {
            Log = 279359181566377984,
            Join = 295239389720870913,
            Mod = 279700881128292352,
            Medic = 325132935630028810,
            Rules = 279381020212592640,
            VInfo = 279413622671933440,
            Announce = 279400628214824960,
            Streams = 337399140768153621,
            Greetings = 279369804391907329,
            Introduce = 279700987197784065,
            General = 209963524389076994,
            Chill = 279617422154137600,
            Theory = 279718665639952404,
            Animal = 315461993307832321,
            Advice = 279406315657494538,
            Vent = 279567743563464714,
            Voice = 279399322473463809,
            Voice2 = 321471137177665538,
            Audition = 279396399978774529,
            Fanart = 279411261123330050,
            Art = 317137354458660864,
            Question = 279408483852484608,
            Fanfic = 279404837563531264,
            Suggest = 279409471162155008,
            Meme = 279420626769543180,
            Unrelated = 279423209462300682,
            Recommend = 279567607357505537,
            Commands = 303933891151134720,
            Office = 307468540573777921
        }

        public enum RoleIds : ulong
        {
            TheGeneral = 278347563453644800,
            Sergeants = 212287761632329728,
            Robots = 221985683902955522,
            Captains = 215380959460196352,
            Donators = 315031701044723714,
            Corporals = 229948553269739520,
            Privates = 218810975854395392,
            Trainees = 218811470232813568,
            PrShe = 300452410269106182,
            PrXe = 300452467714293760,
            PrHe = 300452431190556672,
            PrAny = 300455974001704961,
            PrThey = 300452448437534720,
            Everyone = 326753855394283521,
        }
#endif

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

            _client.UserVoiceStateUpdated += ChannelChanged;

            await InitCommands();

            string token;

#if DEBUG
            token = "Mjg4MzQwNDE0MDAyMzY0NDE3.C_lXxQ.1qKk1SYwGhWj8FDJZXlVkYCD-YE";
            Console.WriteLine("Automod running in debug mode.");
#else
            token = "MjgxMzMyMzA3NjYxNjg0NzM4.C_lXrQ.kqbrLwWOl8862fYzfdEc8epYYwQ";
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
            // Repeat this for all the service classes
            // and other dependencies that your commands might need.

            // Either search the program and add all Module classes that can be found:
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>();

            _services = _map.BuildServiceProvider();

            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            int pos = 0;

            if (msg.HasCharPrefix('&', ref pos))
            {
                // Create a Command Context
                var context = new SocketCommandContext(_client, msg);

                // Execute the command. (result does not indicate a return value, 
                // rather an object stating if the command executed succesfully).
                var result = await _commands.ExecuteAsync(context, pos, _services);

                // Uncomment the following lines if you want the bot
                // to send a message if it failed (not advised for most situations).
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    await msg.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        #region Rules
        private async Task CheckRules(SocketMessage message)
        {
            if (message.Author.Id == 235849760253280257 && !message.Channel.Id.EqualsMulti(ChannelIds.Animal, ChannelIds.Chill, ChannelIds.Vent, ChannelIds.Advice))
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Author.IsBot)
                return;

            if (message.Channel.Id == (ulong)ChannelIds.Mod || message.Channel.Id == (ulong)ChannelIds.Medic)
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
                    ruleBrokenDescription = "No use of Bones' old names.";
                    break;
                case 5:
                    ruleBrokenDescription = "No trolling.";
                    break;
                case 6:
                    ruleBrokenDescription = "No advertising or linking to streams or videos outside of #unrelated or #shitpost_memes.";
                    break;
                case 7:
                    ruleBrokenDescription = "No ALLCAPS messages.";
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
                    ruleBrokenDescription = "Fatal error! Report this to Sergeant Ray immediately.";
                    break;
            }

            await message.DeleteAsync();

            IMessage scolding = await message.Channel.SendMessageAsync("Violation of Rule " + ruleBroken.ToString() + ": " + ruleBrokenDescription);

            if (scolding == null)
            {
                Console.WriteLine("Bot is not obtaining message properly; scolding is null.");
            }

            await Task.Delay(2000);

            await scolding.DeleteAsync();

            if (ruleBroken == 1 || ruleBroken == 7)
                return;

            ITextChannel logChannel = (message.Author as SocketGuildUser).Guild.GetChannel((ulong)ChannelIds.Log) as ITextChannel;

            await logChannel.SendMessageAsync("User " + message.Author.Mention + " (ID: " + message.Author.Id.ToString() + ") " +
                " broke rule " + ruleBroken.ToString() +
                " in channel " + message.Channel.Name +
                ".\nTime: " + message.Timestamp +
                ".\nMessage content:\n\" " + message.Content + " \".");
        }
        #endregion Rules

        private async Task OnDelete(Cacheable<IMessage, ulong> messageCache, ISocketMessageChannel channel)
        {
            await messageCache.GetOrDownloadAsync();

            IMessage message = messageCache.Value;
            string messageRecieved = message.Content.ToLower();

            if (message.Author.IsBot)
                return;

            if ((message as SocketMessage).CheckRules() != 20)
                return;

            if (message.Author.Id == 288340414002364417 || message.Author.Id == 281332307661684738)
                return;

            if ((message.Author as SocketGuildUser).GuildPermissions.Administrator)
                return;

            if (message.Content == "!shib")
                return;

            if (message.Content.ContainsMulti("&pronouns", "%play", "%queue"))
                return;

            if (message.Content == "")
                return;

            if (message.Content.Contains("&channel") && message.Channel.Id.EqualsMulti(ChannelIds.Voice, ChannelIds.Voice2))
                return;

            ITextChannel logChannel = (message.Author as SocketGuildUser).Guild.GetChannel((ulong)ChannelIds.Log) as ITextChannel;

            await logChannel.SendMessageAsync("Message deleted by user " + message.Author.Mention +
                " in channel " + message.Channel.Name +
                ". Contents: \n\" " + message.Content + " \"");
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            if (user.Id == 209422051721609217)
                return;

            ITextChannel logChannel = user.Guild.GetChannel((ulong)ChannelIds.Join) as ITextChannel;

            await logChannel.SendMessageAsync("User " + user.Mention + " (ID: " + user.Id + ")" + " left the server.");
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            switch (user.Id)
            {
                case 209422051721609217:
                    await user.AddRoleAsync(user.Guild.GetRole((ulong)Program.RoleIds.Corporals));
                    break;

                case 291426189128630275:
                    await user.Guild.AddBanAsync(user);
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

                    ITextChannel joinChannel = user.Guild.GetChannel((ulong)ChannelIds.Join) as ITextChannel;
                    ITextChannel greetingChannel = user.Guild.GetChannel((ulong)ChannelIds.Greetings) as ITextChannel;

                    await joinChannel.SendMessageAsync("User " + user.Mention + " (ID: " + user.Id.ToString() + ") joined the server at " + user.JoinedAt + ".");
                    await greetingChannel.SendMessageAsync(GreetingsMessage);
                    await user.AddRoleAsync(user.Guild.GetRole((ulong)Program.RoleIds.Trainees));
                    break;
            }
        }

        private async Task ChannelChanged(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (before.VoiceChannel == after.VoiceChannel)
                return;

            if (Globals.deleteGuard)
                return;

            if (!Globals.vcCreators.ContainsKey(user.Id))
                return;

            if (before.VoiceChannel.Name != Globals.vcCreators[user.Id])
                return;

            await before.VoiceChannel.DeleteAsync();
            Globals.vcCreators.Remove(user.Id);
        }
    }
}