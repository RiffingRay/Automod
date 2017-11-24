using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Automod.Extensions;

namespace Automod.Commands
{
    public class Channels : ModuleBase
    {
        //DeleteGuard is to prevent channels from being deleted as soon as they're created due to being empty. It might be removable, it's a holdover from D.Net 0.9.
        [Command("channel"), Summary("Creates a new temporary voice channel.")]
        public async Task PublicChannel(string name, int users = 0)
        {
            //Add code making this command only work in VC or VC_2.
            if ((Context.User as SocketGuildUser).VoiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel to use this command.");
                return;
            }

            if (users <= 2 && users != 0)
            {
                await ReplyAsync("The channel must allow for three or more users.");
                return;
            }

            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();

            string channelName = voiceChannels.FindVoiceChannelName(name.ToLower(), false);

            if (channelName == "invalid")
            {
                await ReplyAsync("Channel type is invalid! Check Rules and Info for a full list.");
                return;
            }

            Globals.deleteGuard = true;

            await ReplyAsync("Creating channel " + name + " for user " + Context.User.Mention + ". Just a moment, please!");

            IVoiceChannel createdChannel = await Context.Guild.CreateVoiceChannelAsync(channelName);

            if (users != 0)
            {
                await createdChannel.ModifyAsync(x =>
                {
                    x.UserLimit = users;
                });
            }

            await (Context.User as IGuildUser).ModifyAsync(x =>
            {
                x.Channel = new Optional<IVoiceChannel>(createdChannel);
            }).ConfigureAwait(false);

            await Task.Delay(200);

            Globals.deleteGuard = false;
        }

        [Command("privatechannel"), Summary("Creates a new temporary private voice channel.")]
        [RequireBotPermission(ChannelPermission.ManageChannel)]
        public async Task PrivateChannel(string name, params IUser[] users)
        {
            if ((Context.User as SocketGuildUser).VoiceChannel == null)
            {
                await ReplyAsync("You must be in a voice channel to use this command.");
                return;
            }

            if (users == null)
            {
                await ReplyAsync("You need to @ at least one user!");
                return;
            }

            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();

            string channelName = voiceChannels.FindVoiceChannelName(name.ToLower(), true);

            Globals.deleteGuard = true;

            await ReplyAsync("Creating channel " + name + " for user " + Context.User.Mention + ". Just a moment, please!");

            IVoiceChannel createdChannel = await Context.Guild.CreateVoiceChannelAsync(name);

            //Is there REALLY no better way to do this?!
            OverwritePermissions permissions = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);

            OverwritePermissions allow = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);

            await createdChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, permissions);

            foreach (var allowedUser in users)
                await createdChannel.AddPermissionOverwriteAsync(allowedUser, allow);

            await (Context.User as IGuildUser).ModifyAsync(x =>
            {
                x.Channel = new Optional<IVoiceChannel>(createdChannel);
            }).ConfigureAwait(false);

            await Task.Delay(200);

            Globals.deleteGuard = false;
        }

        [Command("flushchannels"), Summary("(MOD ONLY) Deletes all voice channels, except for General 1, AFK, the meeting rooms, and Sleepy Time Junction.")]
        [RequireUserPermission(ChannelPermission.ManageChannel)]
        public async Task FlushChannels()
        {
            IVoiceChannel[] voiceChannels = (await Context.Guild.GetVoiceChannelsAsync().ConfigureAwait(false)).ToArray();
            List<IVoiceChannel> vcList = new List<IVoiceChannel>();

            foreach (IVoiceChannel channel in voiceChannels)
            {
                vcList.Add(channel);
            }

            vcList.RemoveAll(x => x.Name.EqualsMulti("General 1", "AFK", "Jukebox", "Sergeants Meeting room", "Upper Rank Meeting room"));

            foreach (IVoiceChannel channel in vcList)
            {
                await channel.DeleteAsync();
            }
        }
    }
}
