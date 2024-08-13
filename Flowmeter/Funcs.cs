using Discord;
using System.Data;
using System.Text.RegularExpressions;

namespace Flowmeter;

partial class Program {

    public static ulong[] trustedPeople = [
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
        1204295911367512084   // abotmination amotbination
    ];

    public static bool SkillIssued(IGuildUser user) => !(user.GuildPermissions.Administrator || trustedPeople.Contains(user.Id));

    public static string GetFilePath(ulong? id) {
        string path = "D:\\flowmeter data\\tags";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        path = Path.Join(path, id.ToString() + ".txt");
        if (!File.Exists(path))
            File.Create(path);
        return path;
    }

    public static string[] GetTags(ulong? id) => File.ReadAllLines(GetFilePath(id));

    public static void AddLine(ulong id, string line) => File.AppendAllLines(GetFilePath(id), [line]);

    public static void RemoveLine(ulong id, string line) {
        var tags = GetTags(id).ToList();
        tags.Remove(line);
        File.WriteAllLines(GetFilePath(id), [.. tags]);
    }

    public static bool Check(string rule, string msg) {
        var msgl = msg.ToLower();
        var h = rule.Split(";");
        var keyword = h[0];
        var dp = h[1]; // detection type
        var keywordl = keyword.ToLower();
        return (
            (dp == "default" && msgl.Contains(keywordl)) ||
            (dp == "=" && keywordl == msgl) ||
            (dp == "==" && keyword == msg) ||
            (dp == "split" && msgl.Split().Contains(keywordl)) ||
            (dp == "startswith" && msg.StartsWith(keywordl)) ||
            (dp == "endswith" && msg.EndsWith(keywordl)) ||
            (dp == "regex" && Regex.IsMatch(msgl, keyword, RegexOptions.IgnoreCase)) ||
            (dp == "REGEX" && Regex.IsMatch(msg, keyword))
        );
    }

    public static string CheckRule(string rule, ulong? guildID) {
        string[] ruleSplited = (from i in rule.Split(";") select i.Trim()).ToArray();
        rule = string.Join(";", ruleSplited);
        int a = ruleSplited.Length;
        if (3 > a || a > 4)
            return $"you need to type **3**-**4** arguments here but **{a}** was given";
        string keyword = ruleSplited[0];
        string detectionType = ruleSplited[1];
        string reply = ruleSplited[2];
        if (guildID != null && GetTags(guildID).Any(tag => tag.Split(";")[0] == keyword))
            return "silly you already have added that tag";
        if (keyword.Length > 125)
            return "keyword cant be longer than 125 symbols";
        if (reply.Length > 500)
            return "reply cant be longer than 500 symbols";
        if (ruleSplited.Length > 3 && ruleSplited[3].Length > 30)
            return "why in the world you need reply type longer than 30 symbols";
        string[] detectionTypes = ["=", "==", "default", "split", "startswith", "endswith", "regex", "REGEX"];
        if (!detectionTypes.Contains(detectionType))
            return "incorrect detection type <:yeh:1183111141409435819>";
        if (rule.Contains('\n'))
            return "you cant word wrap";
        if (detectionType == "split" && keyword.Contains(' '))
            return "you cant use spaces with **split** detection type!";
        if (keyword == "" || reply == "")
            return "<:pangooin:1153354856032116808>";
        if (ruleSplited.Length > 3 && ruleSplited[3].Equals("react", StringComparison.CurrentCultureIgnoreCase) && !(Emote.TryParse(reply, out _) || Emoji.TryParse(reply, out _)))
            return "neither emoji id or unicode emoji";
        if (detectionType == "regex") {
            try {
                Regex.Match("", keyword);
            }
            catch (ArgumentException ex) {
                return $"invalid regex: {ex.Message}";
            }
        }
        return rule;
    }
}
