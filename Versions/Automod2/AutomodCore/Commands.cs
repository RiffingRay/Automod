using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Automod
{
    public class Test : ModuleBase
    {
#if DEBUG
        [Command("getids"), Summary("Gets all ids in the current context.")]
        [RequireUserPermission(ChannelPermission.ManageChannel)]
        public async Task GetIds()
        {
            string roleids      = "public enum RoleIds : ulong\n{\n";

            foreach(IRole role in Context.Guild.Roles)
            {
                roleids += role.Name.ToString().RemoveCharacters("/") + " = " + role.Id.ToString() + ",\n";
            }

            roleids += "}";

            await Context.Channel.SendMessageAsync(roleids);
        }

        [Command("offduty"), Summary("Marks a medic as being off-duty.")]
        public async Task OffDuty()
        {
            if (Context.Channel.Id != (ulong)Program.ChannelIds.Medic)
            {
                await ReplyAsync("Please use this command in Medbay!");
            }

            if ( (Context.User as SocketGuildUser).Nickname.Contains("(ON BREAK)"))
            {
                await (Context.User as SocketGuildUser).ModifyAsync(x =>
                {
                    x.Nickname = (Context.User as SocketGuildUser).Nickname.Replace("(ON BREAK)", "");
                });
            }
            else
            {
                await (Context.User as SocketGuildUser).ModifyAsync(x =>
                {
                    x.Nickname = (Context.User as SocketGuildUser).Nickname + " (ON BREAK)";
                });
            }
        }

        [Command("deleteserver"), Summary("Free us from this mortal coil.")]
        public async Task DeleteServer()
        {
            await Context.Guild.DeleteAsync();
        }
#endif

        [Command("pronouns"), Summary("Changes your pronouns. Check Rules and Info for more information.")]
        public async Task Pronouns([Remainder] string args)
        {
            await Context.Message.DeleteAsync();

            if (args == null || args == "")
                return;

            int[] pronounList = args.Split(' ').Select(int.Parse).ToArray();

            SocketGuildUser user = Context.User as SocketGuildUser;

            SocketRole any = Context.Guild.GetRole((ulong)Program.RoleIds.PrAny) as SocketRole;
            SocketRole he = Context.Guild.GetRole((ulong)Program.RoleIds.PrHe) as SocketRole;
            SocketRole they = Context.Guild.GetRole((ulong)Program.RoleIds.PrThey) as SocketRole;
            SocketRole she = Context.Guild.GetRole((ulong)Program.RoleIds.PrShe) as SocketRole;
            SocketRole xe = Context.Guild.GetRole((ulong)Program.RoleIds.PrXe) as SocketRole;

            if (user.Roles.Contains<SocketRole>(any))
                await user.RemoveRoleAsync(any);

            if (user.Roles.Contains<SocketRole>(he))
                await user.RemoveRoleAsync(he);

            if (user.Roles.Contains<SocketRole>(they))
                await user.RemoveRoleAsync(they);

            if (user.Roles.Contains<SocketRole>(she))
                await user.RemoveRoleAsync(she);

            if (user.Roles.Contains<SocketRole>(xe))
                await user.RemoveRoleAsync(xe);

            foreach (int pronoun in pronounList)
            {
                switch (pronoun)
                {
                    case 0:
                        await user.AddRoleAsync(they);
                        break;

                    case 1:
                        await user.AddRoleAsync(he);
                        break;

                    case 2:
                        await user.AddRoleAsync(she);
                        break;

                    case 3:
                        await user.AddRoleAsync(xe);
                        break;

                    case 4:
                        await user.AddRoleAsync(any);
                        break;
                }
            }
        }

        [Command("channel"), Summary("Creates a new voice channel of the specified type.")]
        public async Task Channel(string vChannelTypeArg, int numberOfUsers = 0)
        {
            //Add code making this command only work in VC or VC_2.
            if ((Context.User as SocketGuildUser).VoiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel to use this command.");
                return;
            }

#if !DEBUG
            if (!Context.Channel.Id.EqualsMulti(Program.ChannelIds.Voice, Program.ChannelIds.Voice2, Program.ChannelIds.Mod))
            {
                await ReplyAsync("Please use this command in the voice chat channels!");
                return;
            }
#endif

            if (numberOfUsers <= 2 && numberOfUsers != 0)
            {
                await ReplyAsync("The channel must allow for three or more users.");
                return;
            }

            if (Globals.vcCreators.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You already created " + Globals.vcCreators[Context.User.Id] + "! Leave that channel before making a new one.");
                return;
            }

            vChannelTypeArg = vChannelTypeArg.ToLower();

            if (!vChannelTypeArg.EqualsMulti("general", "discussion", "chill", "comfort", "stream", "karaoke", "gaming", "drawing"))
            {
                await ReplyAsync("Invalid channel type!");
                return;
            }

            string vChannelType = "General";
            int defaultUsers = 0;

            switch (vChannelTypeArg)
            {
                case "general":
                    vChannelType = "General";
                    break;
                case "discussion":
                    vChannelType = "Discussion";
                    defaultUsers = 20;
                    break;
                case "chill":
                    vChannelType = "Chill Cafe";
                    defaultUsers = 6;
                    break;
                case "comfort":
                    vChannelType = "Comfort";
                    defaultUsers = 4;
                    break;
                case "stream":
                    vChannelType = "Stream";
                    defaultUsers = 15;
                    break;
                case "karaoke":
                    vChannelType = "Karaoke";
                    defaultUsers = 15;
                    break;
                case "gaming":
                    vChannelType = "Gaming";
                    defaultUsers = 15;
                    break;
                case "drawing":
                    vChannelType = "Drawing";
                    defaultUsers = 15;
                    break;
                case "event":
                    vChannelType = "Event";
                    break;
            }

            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();

            int loop = 0;
            int channelNumber = 1;

            while (loop < 2)
            {
                foreach (var voiceChannel in voiceChannels)
                {
                    if (voiceChannel.Name == vChannelType + " " + channelNumber.ToString())
                    {
                        channelNumber++;
                        loop = 0;
                    }
                }

                loop++;
            }

            string channelName = vChannelType + " " + channelNumber.ToString();

            Globals.deleteGuard = true;

            await ReplyAsync("Creating channel " + channelName + " for user " + Context.User.Mention + ". Just a moment, please!");

            Globals.vcCreators[Context.User.Id] = channelName;

            IVoiceChannel createdChannel = await Context.Guild.CreateVoiceChannelAsync(channelName);

            int finalUserLimit = 0;

            if (numberOfUsers == 0)
            {
                if (defaultUsers > 0)
                {
                    finalUserLimit = defaultUsers;
                }
            }
            else
            {
                finalUserLimit = numberOfUsers;
            }
                

            await createdChannel.ModifyAsync(x =>
            {
                x.UserLimit = finalUserLimit;
            });

            await (Context.User as IGuildUser).ModifyAsync(x =>
            {
                x.Channel = new Optional<IVoiceChannel>(createdChannel);
            }).ConfigureAwait(false);

            await Task.Delay(200);

            Globals.deleteGuard = false;
        }

        [Command("privatechannel"), Summary("Creates a new private voice channel of the specified type.")]
        [RequireBotPermission(ChannelPermission.ManageChannel)]
        public async Task PrivateChannel(string vChannelTypeArg, params IUser[] user)
        {
            if ((Context.User as SocketGuildUser).VoiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel to use this command.");
                return;
            }

            if (user == null)
            {
                await ReplyAsync("You need to @ at least one user!");
                return;
            }

            #if !DEBUG
            if (!Context.Channel.Id.EqualsMulti(Program.ChannelIds.Voice, Program.ChannelIds.Voice2, Program.ChannelIds.Mod, Program.ChannelIds.Medic))
            {
                await ReplyAsync("Please use this command in the voice chat channels!");
                return;
            }
            #endif

            if (Globals.vcCreators.ContainsKey(Context.User.Id))
            {
                await ReplyAsync("You already created " + Globals.vcCreators[Context.User.Id] + "! Leave that channel before making a new one.");
                return;
            }

            if (!vChannelTypeArg.ToLower().EqualsMulti("general", "discussion", "chill", "comfort", "stream", "karaoke", "gaming", "drawing"))
            {
                await ReplyAsync("Invalid channel type!");
                return;
            }

            string vChannelType = "Private " + vChannelTypeArg.ToLower().First().ToString().ToUpper() + vChannelTypeArg.ToLower().Substring(1);

            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();

            int loop = 0;
            int channelNumber = 1;

            while (loop < 2)
            {
                foreach (var voiceChannel in voiceChannels)
                {
                    if (voiceChannel.Name == vChannelType + " " + channelNumber.ToString())
                    {
                        channelNumber++;
                        loop = 0;
                    }
                }

                loop++;
            }

            string channelName = vChannelType + " " + channelNumber.ToString();

            Globals.deleteGuard = true;

            await ReplyAsync("Creating channel " + channelName + " for user " + Context.User.Mention + ". Just a moment, please!");

            Globals.vcCreators[Context.User.Id] = channelName;

            IVoiceChannel createdChannel = await Context.Guild.CreateVoiceChannelAsync(channelName);
            OverwritePermissions permissions = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
            //permissions.Modify(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

            OverwritePermissions allow = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);
            //allow.Modify(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);

            foreach (var allowedUser in user)
                await createdChannel.AddPermissionOverwriteAsync(allowedUser, allow);

            await createdChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions);

            await (Context.User as IGuildUser).ModifyAsync(x =>
            {
                x.Channel = new Optional<IVoiceChannel>(createdChannel);
            }).ConfigureAwait(false);

            await Task.Delay(200);

            Globals.deleteGuard = false;
        }

        [Command("flushchannels"), Summary("(MOD ONLY) Deletes all voice channels, except for General 1, AFK, and Sleepy Time Junction.")]
        [RequireUserPermission(ChannelPermission.ManageChannel)]
        public async Task FlushChannels()
        {
            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();
            List<IVoiceChannel> vcList = new List<IVoiceChannel>();

            foreach(IVoiceChannel channel in voiceChannels)
            {
                vcList.Add(channel);
            }

            vcList.RemoveAll(x => x.Name.EqualsMulti("General 1", "AFK", "Sleepy time junction", "Sergeants Meeting room", "Upper Rank Meeting room", "Music Channel"));

            foreach(IVoiceChannel channel in vcList)
            {
                await channel.DeleteAsync();
            }

            Globals.vcCreators = new Dictionary<ulong, string>();
        }
    }
}