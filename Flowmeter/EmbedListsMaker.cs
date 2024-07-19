using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Flowmeter {
    partial class Program {
        private static int p(string[] x) {
            return (x.Length + 9) / 10;
        }

        public static Embed MakeListEmbed(int page, string[] list) {
            long pages = p(list);
            string description = "";
            try {
                list = list[(10 * (page - 1))..];
                if (page != pages) list = list[..10];
            }
            catch (System.ArgumentOutOfRangeException) {
                list = [];
            }
            foreach (string str in list) {
                description += $"- {str}\n";
            }
            EmbedBuilder embed = new EmbedBuilder() {
                Title = $"Page {page}/{pages}",
                Description = description
            };
            return embed.Build();
        }

        public static Discord.MessageComponent MakeListComponents(string what, int page, string[] pages) {
            ActionRowBuilder components = new();

            void c(string s, int? id) => components.AddComponent(new ButtonBuilder(
                label: s,
                customId: id == null ? "?" : (what + ";" + id.ToString()),
                style: ButtonStyle.Secondary
            ).Build());

            int m = p(pages);
            if (2 < page && page <= m   ) c("⏪", 1);
            if (1 < page && page <= m   ) c("<", page - 1);
            if (0 < page && page < m    ) c(">", page + 1);
            if (0 < page && page < m - 1) c("⏩", m);

            if (!(0 < page && page <= m)) c("?", null);
            return new ComponentBuilder().AddRow(components).Build();
        }
    }
}
