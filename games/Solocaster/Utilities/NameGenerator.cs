using System;
using Solocaster.Character;

namespace Solocaster.Utilities;

public static class NameGenerator
{
    private static readonly Random _random = new();

    private static readonly string[] MalePrefixes =
    {
        "Gor", "Thar", "Mor", "Kael", "Vor", "Drak", "Bran", "Arn", "Fen", "Grim",
        "Hal", "Jor", "Kern", "Lor", "Mag", "Nar", "Orm", "Rath", "Sven", "Tor"
    };

    private static readonly string[] FemalePrefixes =
    {
        "Ael", "Bri", "Cyr", "Ela", "Fae", "Gwen", "Ivy", "Kira", "Lyr", "Mira",
        "Nyx", "Ora", "Ria", "Sera", "Tia", "Val", "Wren", "Yara", "Zara", "Luna"
    };

    private static readonly string[] MaleSuffixes =
    {
        "ian", "ius", "ak", "en", "or", "us", "ax", "rim", "don", "ric",
        "mar", "gar", "dan", "ven", "ron", "thos", "mir", "nar", "zan", "dur"
    };

    private static readonly string[] FemaleSuffixes =
    {
        "ara", "ena", "ia", "wyn", "is", "elle", "ina", "ora", "ess", "ani",
        "lia", "rie", "dra", "tha", "va", "na", "ra", "sa", "la", "ryn"
    };

    public static string Generate(Sex sex)
    {
        var prefixes = sex == Sex.Male ? MalePrefixes : FemalePrefixes;
        var suffixes = sex == Sex.Male ? MaleSuffixes : FemaleSuffixes;

        var prefix = prefixes[_random.Next(prefixes.Length)];
        var suffix = suffixes[_random.Next(suffixes.Length)];

        return prefix + suffix;
    }
}
