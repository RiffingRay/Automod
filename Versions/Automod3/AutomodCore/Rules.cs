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
    public static class Rules
    {
        public static int CheckRules(this SocketMessage message)
        {
            //Bots are exempt from the rules.
            if (message.Author.IsBot)
                return 20;

            string messageRecieved = message.Content.ToLower();

            //C# treats non-alphabetical chars as being uppercase, so a message like "300+" would be seen as allcaps. This removes as many non-alphabetical chars as I could, but I probaby missed a whole bunch.
            string capsCheck = message.Content.RemoveCharacters("*_1234567890[]{}!@#$%^&*()\\|~`;:'\",.<>?/ +=-~`|");

            //Remove duplicates, so that messages like ":)))))))))))))))" are still caught as single-icon messages.
            string emojiCheck = new string(messageRecieved.ToCharArray().Distinct().ToArray());

            //This is used because rule 6 is more strict on trainees than it is on other users.
            SocketRole trainee = (message.Author as SocketGuildUser).Guild.GetRole(Roles.Trainees);

            if (emojiCheck.Length == 1)
                return 1;

            if (emojiCheck.Length == 3 || emojiCheck.Length == 2)
                if (emojiCheck.ContainsMulti(":", ";", ")", "(", "^", "xd", "xp", ":3"))
                    return 1;

            if (messageRecieved == "( ͡° ͜ʖ ͡°)")
                return 1;

            if (messageRecieved.ContainsMulti(" rape ", " rape,", " rape.", "suicide", "kill myself", "kill himself", "kill herself") && !message.Channel.Id.EqualsMulti(Channels.Theory, Channels.Advice, Channels.Vent))
                return 2;

            //secretslurteststring was used in private, I didn't feel comfortable typing slurs, even if it was in complete private and only to make sure that this rule worked.
            //Shorter form of "faggot" is not checked because I was afraid of it giving a lot of false-positives.
            if (messageRecieved.ContainsMulti("retard", "faggot", "nigga", "nigger", "fuckboy", "fuckboi", "fuccboi", "fuccboy", "secrettestslurstring") && !message.Channel.Id.EqualsMulti(Channels.Advice, Channels.Vent))
                return 3;

            //More strict on trainees so people who just joined can't exploit the limited amount of checking the rules actually do.
            if ((message.Author as SocketGuildUser).Roles.Contains<SocketRole>(trainee))
            {
                if (messageRecieved.RemoveCharacters(" ").ContainsMulti(".com", ".tv", ".gov", ".net", ".io", ".me"))
                    return 6;
            }

            //I fucking hate this rule, look at how many exceptions there are!
            //The more strict version for trainees isn't used here because I was afraid of it causing false positives. Use it if you feel it's necessary.
            //Captains and above can post links anywhere.
            if (messageRecieved.RemoveCharacters(" ").ContainsMulti("pornhub.com", "picarto.tv", "youtube.com", "youtu.be", "twitch.tv", "hitbox.tv", "tumblr.com", "twitter.com") && !message.Channel.Id.EqualsMulti(Channels.Theory, Channels.Chill, Channels.Fanart, Channels.Fanfic, Channels.Art, Channels.Audition, Channels.Meme, Channels.Unrelated, Channels.Recommend, Channels.Announce, Channels.Voice, Channels.Voice2, Channels.Commands, Channels.Streams))
                if (!(message.Author as SocketGuildUser).GuildPermissions.KickMembers)
                    return 6;

            if (message.Content != "")
                if (capsCheck != "" && capsCheck.IsAllUpper())
                    return 7;

            //Consider merging this with rule 6.
            if (messageRecieved.ContainsMulti("discord.gg", "discordapp.com/invite/"))
                if (!(message.Author as SocketGuildUser).GuildPermissions.KickMembers)
                    return 8;

            //Had to do this crap because Lady Luck's devs decided to use / as their command prefix, and I couldn't block everything that started with / because of /me and /shrug
            if (messageRecieved.ToLower().Length > 3)
                if (messageRecieved.ToLower().ToCharArray()[0] == '/' && messageRecieved.ToLower().ToCharArray()[1] == 'r' && !message.Channel.Id.EqualsMulti(Channels.Commands, Channels.Mod))
                    return 9;

            //This and rule 9 can be merged into a more general "No bots outside of the proper channels" rule.
            if (messageRecieved == "!shib" && !message.Channel.Id.EqualsMulti(Channels.Animal, Channels.Chill, Channels.Vent, Channels.Advice))
                return 10;

            return 20;
        }
    }
}
