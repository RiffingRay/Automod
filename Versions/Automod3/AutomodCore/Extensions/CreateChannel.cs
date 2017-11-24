using System.Linq;
using Discord;

namespace Automod.Extensions
{
    //It's named CreateChannel because when this was first being written, I planned to also put a method in here to simplify the channel-making process.
    //Decided against that though, because my poor understanding of awaits gave me hell while trying to do so.
    public static class CreateChannel
    {
        /// <summary>
        /// Generates the next name in the sequence.
        /// </summary>
        /// <param name="voiceChannels">Array of voice channels currently in the server.</param>
        /// <param name="name">The type of voice channel being created.</param>
        /// <param name="isPrivate">Whether or not the channel is private.</param>
        /// <returns>The next voice channel name in the sequence.</returns>
        public static string FindVoiceChannelName(this IVoiceChannel[] voiceChannels, string name, bool isPrivate)
        {
            if (!name.EqualsMulti("general", "discussion", "chill", "comfort", "stream", "karaoke", "gaming", "drawing"))
                return "invalid";

            int loop = 0;
            int channelNumber = 1;

            string channelName = name.First().ToString().ToUpper() + name.Substring(1);

            //Ensures that the loop runs at least twice before breaking. C# doesn't really have an easy way of restarting a foreach loop, so this was necessary.
            while (loop < 2)
            {
                foreach (var voiceChannel in voiceChannels)
                {
                    if (voiceChannel.Name == channelName + " " + channelNumber.ToString())
                    {
                        channelNumber++;
                        loop = 0;
                    }
                }

                loop++;
            }

            channelName += " " + channelNumber.ToString();

            channelName = channelName.First().ToString().ToUpper() + channelName.Substring(1);

            if (isPrivate)
                channelName = "Private" + channelName;

            return channelName;
        }
    }
}
