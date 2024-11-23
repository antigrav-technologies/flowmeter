using Discord;
using Discord.Rest;

namespace Flowmeter;

internal static class Data {
    public const string PREFIX = "hey flowmeter ";

    public static readonly ulong[] COOL_SERVERS = [
        1287684990041063445, // ctqa stnad
        1202574174946861076, // Чат против вонять подмышка бургер тайвань!
        854614974525472798,  // a silly server
        1183418786481700925, // this server is real fucked up
        1232247180480479282, // cube's warehouse
        1176906188139528223  // -x²
    ];
    public static readonly ulong[] BOTS_TO_REPLY_TO = [
        811569586675515433,  // ammeter
        1030817797921583236, // ICOSAHEDROOOOOOOO
        1204295911367512084  // abotmination amotbination
    ];
    public static readonly ulong[] TRUSTED_PEOPLE = [
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