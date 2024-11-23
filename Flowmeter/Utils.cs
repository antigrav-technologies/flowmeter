using Discord;
using System.Text.RegularExpressions;

namespace Flowmeter;

public static class Utils {
    public static bool SkillIssued(IGuildUser user) => !(user.GuildPermissions.Administrator || Data.TRUSTED_PEOPLE.Contains(user.Id));

    private static string GetFilePath(ulong id) {
        if (Data.COOL_SERVERS.Contains(id)) id = 1042064947867287643;
        string path = "tags";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path = Path.Join(path, $"{id}.txt");
        if (!File.Exists(path)) File.Create(path);
        return path;
    }

    public static string[] GetTags(ulong id) => File.ReadAllLines(GetFilePath(id));

    public static void SortTags(ulong id) {
        string[] lines = GetTags(id);
        Array.Sort(lines, StringComparer.OrdinalIgnoreCase);
        File.WriteAllLines(GetFilePath(id), lines);
    }
    
    public static void AddLine(ulong id, string line) => File.WriteAllLines(GetFilePath(id), [..GetTags(id), line]);

    public static void RemoveLine(ulong id, string line) {
        var tags = GetTags(id).ToList();
        tags.Remove(line);
        File.WriteAllLines(GetFilePath(id), [.. tags]);
    }

    public static bool Check(string rule, string msg) {
        if (rule.Count(x => x == ';') < 2) return false;
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

    public static string CheckRule(string rule, ulong? guildId) {
        string[] args = ["react", "delete"];
        string[] detectionTypes = ["=", "==", "default", "split", "startswith", "endswith", "regex", "REGEX"];
        string[] ruleSplited = rule.Split(";").Select(i => i.Trim()).ToArray();
        rule = string.Join(";", ruleSplited);
        int a = ruleSplited.Length;
        if (a is < 3 or > 4)
            return $"you need to type **3**-**4** arguments here but **{a}** was given";
        string keyword = ruleSplited[0];
        string detectionType = ruleSplited[1];
        string reply = ruleSplited[2];
        if (guildId != null && GetTags((ulong)guildId).Any(tag => tag.Split(";")[0] == keyword))
            return "silly you already have added that tag";
        if (keyword.Length > 125)
            return "keyword cant be longer than 125 symbols";
        if (reply.Length > 500)
            return "reply cant be longer than 500 symbols";
        if (ruleSplited.Length > 3 && ruleSplited.Skip(3).Any(x => !args.Contains(x)))
            return "unknown argument specified";
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
                _ = Regex.Match("", keyword);
            }
            catch (ArgumentException ex) {
                return $"invalid regex: {ex.Message}";
            }
        }
        return rule;
    }
}
