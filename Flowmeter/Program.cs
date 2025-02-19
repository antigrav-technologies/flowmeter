using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using кансоль = System.Console;
using static Flowmeter.Utils;
[assembly: System.Reflection.AssemblyVersion("1.0.*")]

namespace Flowmeter;

internal class Program {
    private static readonly string TOKEN = File.ReadAllText(GetFilePath([AppDomain.CurrentDomain.BaseDirectory, "TOKEN.txt"]));

    private static readonly DiscordSocketClient CLIENT = new(new DiscordSocketConfig() {
        GatewayIntents = GatewayIntents.All,
        UseInteractionSnowflakeDate = false
    });

    private readonly InteractionService interactionService = new(CLIENT.Rest);

    public static Task Main() => new Program().MainAsync();
    // bot setup is done here
    private async Task MainAsync() {
        await interactionService.AddModuleAsync<CommandModule>(null);
        CLIENT.Log += Log;
        CLIENT.Ready += Ready;
        CLIENT.SlashCommandExecuted += SlashCommandExecuted;
        CLIENT.SelectMenuExecuted += Client_SelectMenuExecuted;
        CLIENT.MessageReceived += Client_MessageReceived;
        CLIENT.ButtonExecuted += InteractionExecuted;
        interactionService.Log += Log;
        await CLIENT.LoginAsync(TokenType.Bot, TOKEN);
        await CLIENT.StartAsync();
        await Task.Delay(-1);
    }

    private async Task<Task> Client_SelectMenuExecuted(SocketMessageComponent cmd) {
        await interactionService.ExecuteCommandAsync(new InteractionContext(CLIENT, cmd, cmd.Channel), null);
        return Task.CompletedTask;
    }

    private async Task<Task> SlashCommandExecuted(SocketSlashCommand cmd) {
        await interactionService.ExecuteCommandAsync(new InteractionContext(CLIENT, cmd, cmd.Channel), null);
        return Task.CompletedTask;
    }

    private async Task<Task> InteractionExecuted(SocketMessageComponent component) {
        ulong guildId = ((SocketGuildChannel)component.Channel).Guild.Id;
        string[] h = component.Data.CustomId.Split(";");
        if (h[0] == "?") {
            await component.RespondAsync("what the fuck have you done", ephemeral: true);
        }
        else if (h[0] == "UPDATELISTEMBED") {
            int pageToGo = int.Parse(h[1]);
            string[] tags = GetTags(guildId);
            await component.UpdateAsync(m => {
                m.Embed = EmbedMaker.MakeEmbed(pageToGo, tags);
                m.Components = EmbedMaker.MakeComponents("UPDATELISTEMBED", pageToGo, tags.Length);
            });
        }
        else {
            await component.RespondWithFileAsync("what.jpg");
            await Task.Delay(20 * 1000);
            await component.DeleteOriginalResponseAsync();
        }
        return Task.CompletedTask;
    }

    private async Task<Task> Ready() {
        await interactionService.RegisterCommandsGloballyAsync();
        
        кансоль.WriteLine($"@{CLIENT.CurrentUser.Username}#{CLIENT.CurrentUser.Discriminator} is now ready");

        while (true) {
            await CLIENT.SetGameAsync(Data.GAMES[Data.RANDOM.Next(Data.GAMES.Length)]);
            await Task.Delay(60 * 1000);
        }
        //return Task.CompletedTask;
    }

    private async Task<Task> Client_MessageReceived(SocketMessage message) {
        if (message.Channel is SocketDMChannel) return Task.CompletedTask;
        string msgl = message.Content.ToLower();
        ulong guildId = ((SocketGuildChannel)message.Channel).Guild.Id;
        string guildName = ((SocketGuildChannel)message.Channel).Guild.Name;

        if (message.Author.IsBot && !Data.BOTS_TO_REPLY_TO.Contains(message.Author.Id)) return Task.CompletedTask;
        
        foreach (string tag in GetTags(guildId)) {
            string[] h = tag.Split(";");
            if (3 > h.Length) continue;
            
            string[] args = h.Length > 3 ? h.Skip(3).ToArray() : [];
            if (!Check(tag, message.Content)) continue;
            
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

        if (!msgl.StartsWith(Data.PREFIX)) return Task.CompletedTask;
        
        msgl = msgl[Data.PREFIX.Length..];

        if (msgl == "serialize yourself") {
            if (message.Author.Id == 558979299177136164) {
                await message.Channel.SendMessageAsync("nah bruvver thats not pythin");
            }
        }

        else if (msgl == "can i add tags") {
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
                if (!CheckTag(message.Content[(Data.PREFIX.Length + 8)..], guildId, out string tag)) {
                    await message.ReplyAsync(tag);
                }
                else {
                    AddTag(guildId, tag);
                    await message.ReplyAsync($"`{tag}` was added to **{guildName}**'s tags");
                }
            }
        }

        else if (msgl.StartsWith("update tag ")) {
            if (SkillIssued((IGuildUser)message.Author)) {
                await message.Channel.SendMessageAsync("Shut the Pup Up or You Get No Balls");
            }
            else {
                if (!CheckTag(message.Content[(Data.PREFIX.Length + 8)..], guildId, out string tag)) {
                    await message.ReplyAsync(tag);
                }
                else {
                    string? oldRule = GetTags(guildId).FirstOrDefault(t => t.Split(";")[0] == tag.Split(";")[0]);
                    if (oldRule != null) {
                        RemoveTag(guildId, oldRule);
                        AddTag(guildId, tag);
                        await message.ReplyAsync($"updated `{oldRule}` to `{tag}` on **{guildName}**");
                    }
                    else {
                        await message.ReplyAsync("cant find that rule");
                    }
                }
            }
        }

        else if (msgl.StartsWith("remove tag ")) {
            if (SkillIssued((IGuildUser)message.Author)) {
                await message.Channel.SendMessageAsync("SORRY MARI2 BUT I CANNOT FULFIL THIS REQUEST BECAUSE I AM AROACE 🔥🔥🔥🔥🔥🗣️🗣️🗣️🗣️🗣️🗣️🗣️🗣️🗣️🗣️🗣️");
            }
            else {
                string[] tags = GetTags(guildId);
                string rule = message.Content[(Data.PREFIX.Length + 11)..];
                foreach (string tag in tags) {
                    if (tag.Split(";")[0] != rule) continue;
                    rule = tag;
                    break;
                }
                if (tags.Contains(rule)) {
                    RemoveTag(guildId, rule);
                    await message.ReplyAsync($"`{rule}` was removed from **{guildName}**'s tags");
                }
                else {
                    await message.ReplyAsync($"`{rule}` is not an actual tag you silly");
                }
            }
        }

        else if (msgl == "list tags") {
            var tags = GetTags(guildId);
            await message.Channel.SendMessageAsync(
                embed: EmbedMaker.MakeEmbed(1, tags),
                components: EmbedMaker.MakeComponents("UPDATELISTEMBED", 1, tags.Length)
            );
        }
 
        else if (msgl == "sort tags") {
            if (SkillIssued((IGuildUser)message.Author)) {
                await message.Channel.SendMessageAsync("why dont you sort some bitches");
            }
            else {
                SortTags(guildId);
                await message.Channel.SendMessageAsync("h o u s e r 🦡😬");
            }
        }
        
        else if (msgl == "download wuggy games") {
            if (message.Author.Id == Data.RECHIE2020) {
                await message.Channel.SendMessageAsync("i am gonna touch you in a sexual way if you will not shut the fu");
            }
            else if (message.Author.Id != Data.TEMA5002) {
                await message.Channel.SendMessageAsync("молче мистер петух");
            }
            else {
                Data.COOL_SERVERS = ReadUlongData(GetFilePath([Data.DATA_PATH, "cool_servers.txt"], ""));
                Data.BOTS_TO_REPLY_TO = ReadUlongData(GetFilePath([Data.DATA_PATH, "bots_to_reply_to.txt"], ""));
                Data.TRUSTED_PEOPLE = ReadUlongData(GetFilePath([Data.DATA_PATH, "trusted_people.txt"], ""));
                await message.Channel.SendMessageAsync("🦈💀 Ah, looks like we're off to a great start! What's your take on the intricacies of шкилы and their supposed connection to субботу утром? 😏");
            }
        }
        
        else if (message.Content[Data.PREFIX.Length..] == "DO WHAT THE FUCK DO YOU WANT") {
            var tags = GetTags(guildId);
            int wuggyNumber = (int)(Data.RANDOM.NextDouble() * 1000000000) + 1;
            await message.Channel.SendMessageAsync(
                embed: EmbedMaker.MakeEmbed(wuggyNumber, tags),
                components: EmbedMaker.MakeComponents("UPDATELISTEMBED", wuggyNumber, tags.Length)
            );
        }
        return Task.CompletedTask;
    }

    private static Task Log(LogMessage msg) {
        кансоль.WriteLine(msg);
        return Task.CompletedTask;
    }
}

internal class CommandModule : InteractionModuleBase {
    public InteractionService? Service {get; set;}

    [SlashCommand("ping", "get ping")]
    public async Task PingSlashCommand() {
        await RespondAsync(((DiscordSocketClient)Context.Client).Latency + "ms");
    }

    [SlashCommand("help", "get help")]
    public async Task HelpSlashCommand() {
        await RespondAsync(embed: new EmbedBuilder() {
            Title = "Flowmeter",
            Description = $@"bot made by tema5002

> Say *{Data.PREFIX}add tag keyword;detection_type;reply;args* to **add new tag**
> Say *{Data.PREFIX}update tag keyword;detection_type;reply;args* to **update already existing tag**
> Say *{Data.PREFIX}remove tag keyword* to **remove tag**
> Say *{Data.PREFIX}list tags* to **list existing tags on this server**
> Commands Arguments
> - **keyword** - keyword which triggers the reply
> - **detection_type** (if uppercase its case sensetive except for =):
> - - **default** - triggers if **keyword** in message content (not case sensitive)
> - - **split** - triggers only to whole words surrounded by spaces or end of string
> - - **=** - match
> - - **==** - exact match (case sensitive)
> - - **startswith** - triggers when **reply** starts with **keyword**
> - - **endswith** - triggers when **reply** ends with **keyword**
> - - **regex** - regular expression
> - **reply** - a reply
> - **args**
> - - react - if this argument is specified then **reply** must be an emoji
> - - delete - deletes the message
[source code](https://github.com/antigrav-technologies/flowmeter)",
            Color = new Color((uint)Data.RANDOM.Next(0x1000000)),
            Footer = new() { Text = GetVersion() }
        }.Build());
    }

    [SlashCommand("check", "check by which tags message was triggered")]
    public async Task CheckSlashCommand(string h) {
        var tags = GetTags(((SocketGuildChannel)Context.Channel).Guild.Id).Where(x => Check(x, h)).ToList();
        if (tags.Any())
            await RespondAsync(embed: new EmbedBuilder() {
                Title = $"This was triggered by {tags.Count()} tags",
                Description = string.Join('\n', tags.Select(x => $"- {x}"))
            }.Build());
        else await RespondAsync("null <:fluent_bug:1203623430948130866>");
    }
}
