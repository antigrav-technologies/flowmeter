using Discord;
using Discord.Rest;
using static Flowmeter.Utils;

namespace Flowmeter;

internal static class Data {
    public const string PREFIX = "hey flowmeter ";
    public const ulong SLINX_ATTIC = 1042064947867287643;
    public const ulong TEMA5002 = 558979299177136164;
    public const ulong RECHIE2020 = 710621353128099901;
    public static readonly string DATA_PATH = GetFolderPath(["data"]);
    
    public static readonly string[] TAG_ARGUMENTS = [
        "react",
        "delete"
    ];
    public static readonly string[] DETECTION_TYPES = [
        "=",
        "==",
        "default",
        "DEFAULT",
        "split",
        "SPLIT",
        "startswith",
        "STARTSWITH",
        "endswith",
        "ENDSWITH",
        "regex",
        "REGEX"
    ];

    public static IEnumerable<ulong> COOL_SERVERS = ReadUlongData(GetFilePath([DATA_PATH, "cool_servers.txt"], ""));
    
    public static IEnumerable<ulong> BOTS_TO_REPLY_TO = ReadUlongData(GetFilePath([DATA_PATH, "bots_to_reply_to.txt"], ""));
    
    public static IEnumerable<ulong> TRUSTED_PEOPLE = ReadUlongData(GetFilePath([DATA_PATH, "trusted_people.txt"], ""));
    public static readonly Random RANDOM = new();
    public static async Task<RestMessage> ReplyAsync(this IMessage msg,
        string? text = null, bool isTts = false, Embed? embed = null, RequestOptions? options = null, AllowedMentions? allowedMentions = null, MessageComponent? components = null, ISticker[]? stickers = null, Embed[]? embeds = null, MessageFlags flags = MessageFlags.None) {
        return (RestMessage)await msg.Channel.SendMessageAsync(text, isTts, embed, options, allowedMentions, new MessageReference(msg.Id), components, stickers, embeds, flags);
    }
    public static readonly string[] GAMES = [
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
}
