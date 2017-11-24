using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Automod.Ids;
using Automod.Extensions;

namespace Automod.Commands
{
    public class Pronouns : ModuleBase
    {
        [Command("pronouns"), Summary("Changes your pronouns. Check Rules and Info for more information.")]
        public async Task SetPronouns([Remainder] string args)
        {
            await Context.Message.DeleteAsync();

            if (args == null || args == "")
                return;

            int[] pronounList = args.Split(' ').Select(int.Parse).ToArray();

            SocketGuildUser user = Context.User as SocketGuildUser;

            SocketRole any = Context.Guild.GetRole(Roles.PrAny) as SocketRole;
            SocketRole he = Context.Guild.GetRole(Roles.PrHe) as SocketRole;
            SocketRole they = Context.Guild.GetRole(Roles.PrThey) as SocketRole;
            SocketRole she = Context.Guild.GetRole(Roles.PrShe) as SocketRole;
            SocketRole xe = Context.Guild.GetRole(Roles.PrXe) as SocketRole;

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
    }
}