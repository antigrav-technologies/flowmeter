using Discord;

namespace Flowmeter;

internal static class EmbedMaker {
    public static Embed MakeEmbed(int page, string[] tags) => new EmbedBuilder() {
        Title = $"Page {page}/{(tags.Length + 9) / 10}",
        Description = string.Join('\n',
            tags.Skip((page - 1) * 10).Take(page == (tags.Length + 9) / 10 ? tags.Length % 10 : 10).Select(x => $"- {x.Split(";")[0]}")
        )
    }.Build();

    public static MessageComponent MakeComponents(string what, int page, int tagsLength) {
        ActionRowBuilder components = new();

        void C(string s, int? id) => components.AddComponent(new ButtonBuilder(
            label: s,
            customId: id == null ? "?" : $"{what};{id}",
            style: ButtonStyle.Secondary
        ).Build());

        int m = (tagsLength + 9) / 10;
        if (2 < page && page <= m   ) C("⏪", 1);
        if (1 < page && page <= m   ) C("<", page - 1);
        if (0 < page && page < m    ) C(">", page + 1);
        if (0 < page && page < m - 1) C("⏩", m);

        if (!(0 < page && page <= m)) C("?", null);
        return new ComponentBuilder().AddRow(components).Build();
    }
}
