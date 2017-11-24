using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Automod
{
    public static class Extensions
    {
        public static bool ContainsMulti(this string ToCheck, params string[] CheckForThese)
        {
            foreach (string CurrentlyChecked in CheckForThese)
            {
                if (ToCheck.Contains(CurrentlyChecked))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsMulti(this string ToCheck, params string[] IsString)
        {
            foreach (string BeingChecked in IsString)
            {
                if (BeingChecked == ToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsMulti(this ulong ToCheck, params ulong[] IsString)
        {
            foreach (ulong BeingChecked in IsString)
            {
                if (BeingChecked == ToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool EqualsMulti(this ulong ToCheck, params Program.ChannelIds[] CheckForThese)
        {
            foreach (Program.ChannelIds BeingChecked in CheckForThese)
            {
                if ((ulong)BeingChecked == ToCheck)
                {
                    return true;
                }
            }

            return false;
        }

        public static string OnlyChars(this string ToReplace, string onlyTheseChars)
        {
            string toReturn = "";

            char[] baseString = ToReplace.ToCharArray();

            char[] onlyThese = onlyTheseChars.ToCharArray();

            foreach (char checking in onlyThese)
            {
                if (baseString.Contains(checking))
                {
                    toReturn += checking.ToString();
#if DEBUG
                    Console.WriteLine("Adding character: " + checking.ToString());
#endif
                }
            }

            Console.WriteLine("Returning string: " + toReturn);

            return toReturn;
        }

        public static string RemoveCharacters(this string ToCheck, string CharsToRemove)
        {
            string BeingChecked = ToCheck;
            Char[] RemoveChars = CharsToRemove.ToCharArray();

            foreach (Char CurrentlyRemoved in RemoveChars)
            {
                BeingChecked = BeingChecked.Replace(CurrentlyRemoved.ToString(), "");
            }

            return BeingChecked;
        }

        public static bool IsAllUpper(this string input)
        {
            if (input.Length < 7)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && Char.IsLower(input[i]))
                        return false;
                }
                return true;
            }

            else
            {
                int numberOfUppers = 0;

                for (int i = 0; i < input.Length; i++)
                {
                    if (Char.IsLetter(input[i]) && Char.IsUpper(input[i]))
                        numberOfUppers++;
                }

                if (numberOfUppers >= input.Length - 3)
                    return true;
                else
                    return false;
            }
        }

        public static int CheckRules(this SocketMessage message)
        {
            if (message.Author.IsBot)
                return 20;

            string messageRecieved = message.Content.ToLower();
            string capsCheck = message.Content.RemoveCharacters("*_1234567890[]{}!@#$%^&*()\\|~`;:'\",.<>?/ +=-~`|");
            string emojiCheck = new string(messageRecieved.ToCharArray().Distinct().ToArray());

            SocketRole trainee = (message.Author as SocketGuildUser).Guild.GetRole( (ulong)Program.RoleIds.Trainees );

            if (emojiCheck.Length == 1)
                return 1;

            if (emojiCheck.Length == 3 || emojiCheck.Length == 2)
                if (emojiCheck.ContainsMulti(":", ";", ")", "(", "^", "xd", "xp"))
                    return 1;

            if (messageRecieved == "( ͡° ͜ʖ ͡°)")
                return 1;

            if (messageRecieved.ContainsMulti(" rape ", " rape,", " rape.", "suicide", "kill myself", "kill himself", "kill herself") && !message.Channel.Id.EqualsMulti(Program.ChannelIds.Theory, Program.ChannelIds.Advice, Program.ChannelIds.Vent))
                return 2;

            if (messageRecieved.ContainsMulti("retard", "faggot", "nigga", "nigger", "fuckboy", "fuckboi", "fuccboi", "fuccboy", "secrettestslurstring") && !message.Channel.Id.EqualsMulti(Program.ChannelIds.Advice, Program.ChannelIds.Vent))
                return 3;

            if ((message.Author as SocketGuildUser).Roles.Contains<SocketRole>(trainee))
            {
                if (messageRecieved.RemoveCharacters(" ").ContainsMulti(".com", ".net", ".io", ".me"))
                    return 6;
            }

            if (messageRecieved.RemoveCharacters(" ").ContainsMulti("pornhub.com", "picarto.tv", "youtube.com", "youtu.be", "twitch.tv", "hitbox.tv", "tumblr.com", "twitter.com") && !message.Channel.Id.EqualsMulti(Program.ChannelIds.Theory, Program.ChannelIds.Chill, Program.ChannelIds.Fanart, Program.ChannelIds.Fanfic, Program.ChannelIds.Art, Program.ChannelIds.Audition, Program.ChannelIds.Meme, Program.ChannelIds.Unrelated, Program.ChannelIds.Recommend, Program.ChannelIds.Announce, Program.ChannelIds.Voice, Program.ChannelIds.Voice2, Program.ChannelIds.Commands, Program.ChannelIds.Streams))
                if (!(message.Author as SocketGuildUser).GuildPermissions.KickMembers)
                    return 6;

            if (message.Content != "")
                if (capsCheck != "" && capsCheck.IsAllUpper())
                    return 7;

            if (messageRecieved.ContainsMulti("discord.gg", "discordapp.com/invite/"))
                if (!(message.Author as SocketGuildUser).GuildPermissions.KickMembers)
                    return 8;

            if (messageRecieved.ToLower().Length > 3)
                if (messageRecieved.ToLower().ToCharArray()[0] == '/' && messageRecieved.ToLower().ToCharArray()[1] == 'r' && !message.Channel.Id.EqualsMulti(Program.ChannelIds.Commands, Program.ChannelIds.Mod))
                    return 9;

            if (messageRecieved == "!shib" && !message.Channel.Id.EqualsMulti(Program.ChannelIds.Animal, Program.ChannelIds.Chill, Program.ChannelIds.Vent, Program.ChannelIds.Advice))
                return 10;

            return 20;
        }
    }
}