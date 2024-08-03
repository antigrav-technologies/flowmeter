using Discord;

namespace Flowmeter {
    partial class Program {
        public static Embed MakeListEmbed(int page, string[] list) => new EmbedBuilder() {
            Title = $"Page {page}/{(list.Length + 9) / 10}",
            Description = string.Join("\n", from s in list.Skip((page - 1) * 10).Take(page == (list.Length + 9) / 10 ? list.Length % 10 : 10) select $"- {s}")
        }.Build();

        public static Discord.MessageComponent MakeListComponents(string what, int page, string[] pages) {
            ActionRowBuilder components = new();

            void c(string s, int? id) => components.AddComponent(new ButtonBuilder(
                label: s,
                customId: id == null ? "?" : (what + ";" + id.ToString()),
                style: ButtonStyle.Secondary
            ).Build());

            int m = (pages.Length + 9) / 10;
            if (2 < page && page <= m   ) c("⏪", 1);
            if (1 < page && page <= m   ) c("<", page - 1);
            if (0 < page && page < m    ) c(">", page + 1);
            if (0 < page && page < m - 1) c("⏩", m);

            if (!(0 < page && page <= m)) c("?", null);
            return new ComponentBuilder().AddRow(components).Build();
        }
    }
}
