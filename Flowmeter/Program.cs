using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using System.Data;
using кансоль = System.Console;

namespace Flowmeter;

partial class Program : object {
    readonly static string token = File.ReadAllText("TOKEN.txt");
    readonly static DiscordSocketClient client = new(new DiscordSocketConfig() {
        GatewayIntents = GatewayIntents.All,
        UseInteractionSnowflakeDate = false
    });
    readonly InteractionService interactionService = new(client.Rest);
    static Task Main() => new Program().MainAsync();
    // bot setup is done here
    async Task MainAsync() {
        await interactionService.AddModuleAsync<CommandModule>(null);
        client.Log += Log;
        client.Ready += Ready;
        client.SlashCommandExecuted += SlashCommandExecuted;
        client.SelectMenuExecuted += Client_SelectMenuExecuted;
        client.MessageReceived += Client_MessageReceived;
        client.ButtonExecuted += InteractionExecuted;
        interactionService.Log += Log;
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
        await Task.Delay(-1);
    }

    private async Task<Task> Client_SelectMenuExecuted(SocketMessageComponent cmd) {
        await interactionService.ExecuteCommandAsync(new InteractionContext(client, cmd, cmd.Channel), null);
        return Task.CompletedTask;
    }

    private async Task<Task> SlashCommandExecuted(SocketSlashCommand cmd) {
        await interactionService.ExecuteCommandAsync(new InteractionContext(client, cmd, cmd.Channel), null);
        return Task.CompletedTask;
    }

    private async Task<Task> InteractionExecuted(SocketMessageComponent component) {
        ulong guildID = ((SocketGuildChannel)component.Channel).Guild.Id;
        string h = component.Data.CustomId;
        string[] h_ = h.Split(";");
        if (h == "?") {
            await component.RespondAsync("what the fuck have you done", ephemeral: true);
        }
        else if (h_[0] == "UPDATELISTEMBED") {
            int page_to_go = int.Parse(h_[1]);
            string[] t = (from x in GetTags(guildID) select x.Split(";")[0]).ToArray();
            await component.UpdateAsync(m => {
                m.Embed = MakeListEmbed(page_to_go, t);
                m.Components = MakeListComponents("UPDATELISTEMBED", page_to_go, t);
            });
        }
        else {
            await component.RespondWithFileAsync("what.jpg");
            await Task.Delay(20 * 1000);
            await component.DeleteOriginalResponseAsync();
        }
        return Task.CompletedTask;
    }

    readonly static string[] games = [
        "Minecraft",
        "Half-Life 2",
        "Among Us",
        "CMMM+MM",
        "Crazy Machines 3",
        "Tetris",
        "Minesweeper",
        "Mindustry",
        "Worms Armageddon",
        "Baba is You",
        "Cell Machine Indev",
        "Geometry Dash",
        "VVVVVV",
        "Infinitode 2",
        "Minecraft Launcher",
        "Source Filmmaker",
        "Blender",
        "Code::Blocks 20.03",
        "wuggy games"
    ];

    private async Task<Task> Ready() {
        await interactionService.RegisterCommandsGloballyAsync();
        Data.StartTime = DateTime.Now;

        Random random = new();
        кансоль.WriteLine($"@{client.CurrentUser.Username}#{client.CurrentUser.Discriminator} is now ready");

        while (true) {
            await client.SetGameAsync(games[random.Next(games.Length)]);
            await Task.Delay(60 * 1000);
        }
        //return Task.CompletedTask;
    }

    private async Task<Task> Client_MessageReceived(SocketMessage message) {
        if (message.Channel is SocketDMChannel) return Task.CompletedTask;
        string msgl = message.Content.ToLower();
        ulong guildID = ((SocketGuildChannel)message.Channel).Guild.Id;
        string guildName = ((SocketGuildChannel)message.Channel).Guild.Name;

        if (!message.Author.IsBot || Data.botsToReplyTo.Contains(message.Author.Id)) {
            foreach (string tag in GetTags(guildID)) {
                string[] h = tag.Split(";");
                if (3 <= h.Length) {
                    string[] args = h.Length > 3 ? h.Skip(3).ToArray() : [];
                    if (Check(tag, message.Content)) {
                        string content = h[2];
                        // misder krabs soon i guess
                        if (args.Contains("react")) {
                            IEmote emojiToReact = Emoji.Parse("🔲");
                            if (Emote.TryParse(content, out Emote emote)) {
                                emojiToReact = emote;
                            }
                            else if (Emoji.TryParse(content, out Emoji emoji)) {
                                emojiToReact = emoji;
                            }
                            await message.AddReactionAsync(emojiToReact);
                        }
                        else {
                            await message.Channel.SendMessageAsync(content);
                            if (args.Contains("delete")) await message.DeleteAsync();
                        }
                    }
                }
            }

            if (msgl.StartsWith(Data.prefix)) {
                msgl = msgl[Data.prefix.Length..];

                if (msgl == "serialize yourself") {
                    if (message.Author.Id == 558979299177136164) {
                        await message.Channel.SendMessageAsync("nah bruvver thats not pythin");
                    }
                }

                if (msgl == "can i add tags") {
                    if (SkillIssued((IGuildUser)message.Author)) {
                        await message.ReplyAsync("No shitting Sherlocking 🔒\nHow to Sherlock your Shit Tutorial");
                    }
                    else {
                        await message.ReplyAsync("shore i guess");
                    }
                }
                else if (msgl.StartsWith("add tag ")) {
                    if (SkillIssued((IGuildUser)message.Author)) {
                        await message.Channel.SendMessageAsync("perms issue <:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882>");
                    }
                    else {
                        string rule = CheckRule(message.Content[(Data.prefix.Length + 8)..], guildID);
                        if (!rule.Contains(';')) {
                            await message.ReplyAsync(rule);
                        }
                        else {
                            AddLine(guildID, rule);
                            await message.ReplyAsync($"`{rule}` was added to **{guildName}**'s tags");
                        }
                    }
                }

                else if (msgl.StartsWith("update tag ")) {
                    if (SkillIssued((IGuildUser)message.Author)) {
                        await message.Channel.SendMessageAsync("perms issue <:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882>");
                    }
                    else {
                        string rule = CheckRule(message.Content[(Data.prefix.Length + 11)..], null);
                        if (!rule.Contains(';')) {
                            await message.ReplyAsync(rule);
                        }
                        else {
                            string[] ruleSplited = [.. rule.Split(";")];
                            string? oldRule = null;
                            foreach (string tag in GetTags(guildID)) {
                                if (tag.Split(";")[0] == ruleSplited[0]) {
                                    oldRule = tag;
                                    break;
                                }
                            }
                            if (oldRule != null) {
                                RemoveLine(guildID, oldRule);
                                AddLine(guildID, rule);
                                await message.ReplyAsync($"updated `{oldRule}` to `{rule}` on **{guildName}**");
                            }
                            else {
                                await message.ReplyAsync("cant find that rule");
                            }
                        }
                    }
                }

                else if (msgl.StartsWith("remove tag ")) {
                    if (SkillIssued((IGuildUser)message.Author)) {
                        await message.Channel.SendMessageAsync("perms issue <:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882><:pointlaugh:1128309108001484882>");
                    }
                    else {
                        string[] tags = GetTags(guildID);
                        string rule = message.Content[(Data.prefix.Length + 11)..];
                        foreach (string tag in tags) {
                            if (tag.Split(";")[0] == rule) {
                                rule = tag;
                                break;
                            }
                        }
                        if (tags.Contains(rule)) {
                            RemoveLine(guildID, rule);
                            await message.ReplyAsync($"`{rule}` was removed from **{guildName}**'s tags");
                        }
                        else {
                            await message.ReplyAsync($"`{rule}` is not an actual tag you silly");
                        }
                    }
                }

                else if (msgl == "list tags") {
                    string[] t = (from x in GetTags(guildID) select x.Split(";")[0]).ToArray();
                    await message.Channel.SendMessageAsync(
                        embed: MakeListEmbed(1, t),
                        components: MakeListComponents("UPDATELISTEMBED", 1, t)
                    );
                }
                else if (message.Content[Data.prefix.Length..] == "DO WHAT THE FUCK DO YOU WANT") {
                    int wuggy_number = (int)(new Random().NextDouble() * 1000000000) + 1;
                    string[] t = (from x in GetTags(guildID) select x.Split(";")[0]).ToArray();
                    await message.Channel.SendMessageAsync(
                        embed: MakeListEmbed(wuggy_number, t),
                        components: MakeListComponents("UPDATELISTEMBED", wuggy_number, t)
                    );
                }
            }
        }
        return Task.CompletedTask;
    }

    private Task Log(LogMessage msg) {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }
}

internal static class Data {
    public static DateTime StartTime = DateTime.MinValue;
    public static string prefix = "hey flowmeter ";
    public static readonly List<ulong> CoolServers = [
        1178285875608698951, // ctqa stnad
        1202574174946861076, // h++
        854614974525472798,  // a silly server
        1183418786481700925, // this server is real fucked up
        1232247180480479282  // cube's junkyard
    ];
    public readonly static ulong[] botsToReplyTo = [
        811569586675515433,  // ammeter
        1030817797921583236, // ICOSAHEDROOOOOOOO
        1204295911367512084  // abotmination amotbination
    ];
    public readonly static ulong[] trustedPeople = [
        558979299177136164,   // tema5002
        903650492754845728,   // slinx92
        986132157967761408,   // slinx93
        1163914091270787125,  // dtpls20
        801078409076670494,   // hexahedron1
        1143072932596305932,  // kesslon1632
        710621353128099901,   // rech2020
        712639066373619754,   // aflyde
        1186681736936050691,  // ammeter.
        1122540181984120924,  // voltmeter2
        1172796751216906351,  // aperturesanity
        811569586675515433,   // ammeter
        1030817797921583236,  // ICOSAHEDROOOOOOOO
        1204295911367512084,  // abotmination amotbination
        1056952213056004118   // lampadaire
    ];
    public static Random Random = new();
    public static async Task<RestMessage> ReplyAsync(this IMessage msg,
        string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null, AllowedMentions? allowedMentions = null, MessageComponent? components = null, ISticker[]? stickers = null, Embed[]? embeds = null, MessageFlags flags = MessageFlags.None) {
        return (RestMessage)await msg.Channel.SendMessageAsync(text, isTTS, embed, options, allowedMentions, new MessageReference(msg.Id), components, stickers, embeds, flags);
    }
}

internal class CommandModule : InteractionModuleBase {
    public InteractionService? Service {get; set;}

    [SlashCommand("ping", "get ping")]
    public async Task Ping() {
        await RespondAsync(((DiscordSocketClient)Context.Client).Latency + "ms");
    }

    [SlashCommand("help", "get help")]
    public async Task Help() {
        await RespondAsync(embed: new EmbedBuilder() {
            Title = "Flowmeter",
            Description = $@"bot made by tema5002

> Say *{Data.prefix}add tag keyword;detection_type;reply;args* to **add new tag**
> Say *{Data.prefix}update tag keyword;detection_type;reply;args* to **update already existing tag**
> Say *{Data.prefix}remove tag keyword* to **remove tag**
> Say *{Data.prefix}list tags* to **list existing tags on this server**
> Commands Arguments
> - **keyword** - keyword which triggers the reply
> - **detection_type**:
> - - **default** - triggers if **keyword** in message content (not case sensitive)
> - - **split** - i have no clue how do i explain but it uses python `.split()`
> - - **=** - match
> - - **==** - exact match (it means case sensitive)
> - - **startswith** - triggers when **reply** starts with **keyword**
> - - **endswith** - triggers when **reply** ends with **keyword**
> - - **REGEX**/**regex** - case sensetive/not case sensetive regular expression
> - **reply** - a reply
> - **args**
> - - react - if this argument is specified then **reply** must be emoji
> - - delete - deletes the message
[support server](https://discord.gg/kCStS6pYqr) (kind of) | [source code](https://github.com/tema5002/flowmeter)",
            Color = new Color((uint)Data.Random.Next(0x1000000))
        }.Build());
    }

    [SlashCommand("check", "check by which tags message was triggered")]
    public async Task Check(string h) {
        string[] TAGS = (from x in Program.GetTags(((SocketGuildChannel)Context.Channel).Guild.Id)
                         where 3 <= x.Count(c => c == ';') && x.Count(c => c == ';') <= 4 && Program.Check(x, h)
                         select x).ToArray();
        if (TAGS.Length > 0) {
            await RespondAsync(
                embed: new EmbedBuilder() {
                    Title = $"This was triggered by {TAGS.Length} tags",
                    Description = string.Join("\n", from x in TAGS select $"- {x}")
                }.Build()
            );
        }
        else {
            await RespondAsync("null <:fluent_bug:1203623430948130866>");
        }
    }
}
