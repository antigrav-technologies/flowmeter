using Discord;
using static Flowmeter.Data;
using System.Text.RegularExpressions;

namespace Flowmeter;

public static class Utils {
    public static string GetVersion() {
        Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
        return $"build {version} ({buildDate})";
    }

    public static bool SkillIssued(IGuildUser user) => !(user.GuildPermissions.Administrator || Data.TRUSTED_PEOPLE.Contains(user.Id));

    public static string GetFolderPath(IEnumerable<string> args) {
        string folder = "";

        foreach (string f in args) {
            folder = Path.Combine(folder, f);
            if (Directory.Exists(folder)) continue;
            try {
                Directory.CreateDirectory(folder);
            }
            catch (IOException ex) {
                throw new IOException($"Failed to create folders: {ex.Message}", ex);
            }
        }

        return folder;
    }

    public static string GetFilePath(IList<string> args, string? createFile = null) {
        string path = Path.Combine(
            GetFolderPath(args.Take(args.Count - 1)),
            args.Last()
        );

        if (createFile != null && !Path.Exists(path)) {
            File.WriteAllText(path, createFile);
        }

        return path;
    }
    
    public static string GetTagsPath(ulong id) => GetFilePath([DATA_PATH, "tags", $"{Cool(id)}.txt"], "");

    public static string[] GetTags(ulong id) {
        return Cache.GetCache(id);
    }

    private static void WriteTags(ulong id, string[] tags) {
        Cache.SetCache(id, tags);
        File.WriteAllLines(GetTagsPath(id), tags);
    }
    
    public static void SortTags(ulong id) {
        string[] lines = GetTags(id);
        Array.Sort(lines, StringComparer.OrdinalIgnoreCase);
        WriteTags(id, lines);
    }
    
    public static void AddTag(ulong id, string line) => WriteTags(id, [..GetTags(id), line]);

    public static void RemoveTag(ulong id, string line) => WriteTags(id, GetTags(id).Where(x => x != line).ToArray());

    public static bool Check(string rule, string msg) {
        if (rule.Count(x => x == ';') < 2) return false;
        var msgl = msg.ToLower();
        var h = rule.Split(";");
        var keyword = h[0];
        var dp = h[1]; // detection type
        var keywordl = keyword.ToLower();
        return (dp == "default" && msgl.Contains(keywordl)) ||
               (dp == "=" && keywordl == msgl) ||
               (dp == "==" && keyword == msg) ||
               (dp == "split" && msgl.Split().Contains(keywordl)) ||
               (dp == "SPLIT" && msg.Split().Contains(keyword)) ||
               (dp == "startswith" && msgl.StartsWith(keywordl)) ||
               (dp == "STARTSWITH" && msg.StartsWith(keyword)) ||
               (dp == "endswith" && msgl.EndsWith(keywordl)) ||
               (dp == "endswith" && msg.EndsWith(keyword)) ||
               (dp == "regex" && Regex.IsMatch(msg, keyword, RegexOptions.IgnoreCase)) ||
               (dp == "REGEX" && Regex.IsMatch(msg, keyword));
    }

    public static bool CheckTag(string rule, ulong? guildId, bool update, out string result) {
        string[] ruleSplitted = rule.Split(";").Select(i => i.Trim()).ToArray();

        if (ruleSplitted.Length < 3) {
            result = $"you need to type **3**-**4** arguments here but **{ruleSplitted.Length}** was given";
            return false;
        }

        if (!update && guildId != null && GetTags((ulong)guildId).Any(tag => tag.Split(";")[0] == ruleSplitted[0])) {
            result = "silly you already have added that tag";
            return false;
        }

        if (ruleSplitted[0].Length > 125) {
            result = "keyword cant be longer than 125 symbols";
            return false;
        }

        if (ruleSplitted[2].Length > 500) {
            result = "reply cant be longer than 500 symbols";
            return false;
        }
        
        if (ruleSplitted.Length > 3 && ruleSplitted.Skip(3).Any(x => !ruleSplitted.Skip(3).Contains(x))) {
            result = "unknown argument specified";
            return false;
        }
        
        if (!DETECTION_TYPES.Contains(ruleSplitted[1])) {
            result = "incorrect detection type <:yeh:1183111141409435819>";
            return false;
        }
        
        if (rule.Contains('\n')) {
            result = "you cant word wrap";
            return false;
        }
        
        if ((ruleSplitted[1] == "split" || ruleSplitted[1] == "SPLIT") && ruleSplitted[0].Contains(' ')) {
            result = "you cant use spaces with **split** detection type";
            return false;
        }
        
        if (ruleSplitted.Any(x => x == "")) {
            result = "<:pangooin:1153354856032116808>";
            return false;
        }

        if (ruleSplitted.Length > 3 && ruleSplitted[3].Equals("react", StringComparison.CurrentCultureIgnoreCase) &&
            !(Emote.TryParse(ruleSplitted[2], out _) || Emoji.TryParse(ruleSplitted[2], out _))
           ) {
            result = "not an emoji";
            return false;
        }
        if (ruleSplitted[1] == "regex" || ruleSplitted[1] == "REGEX") {
            try {
                _ = Regex.Match("", ruleSplitted[0]);
            }
            catch (ArgumentException ex) {
                result = $"invalid regex: {ex.Message}";
                return false;
            }
        }

        result = string.Join(";", ruleSplitted);
        return true;
    }

    public static IEnumerable<ulong> ReadUlongData(string fp) => File.ReadAllLines(fp).Select(x => ulong.Parse(x.Split("//")[0].Trim()));
}
