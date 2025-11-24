using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Theming
{
    public record ThemePalette(
        string Key,
        string Name,
        string Description,
        string PrimaryHex,
        string SecondaryHex);

    public static class ThemePalettes
    {
        public static readonly IReadOnlyList<ThemePalette> All = new[]
        {
            new ThemePalette(
                "royal",
                "Royal Blue",
                "Azul royal com grafite noturno.",
                "#1d4ed8",
                "#0f172a"),
            new ThemePalette(
                "emerald",
                "Emerald",
                "Verde esmeralda com cinza aço.",
                "#059669",
                "#0b2535"),
            new ThemePalette(
                "sunset",
                "Sunset",
                "Laranja queimado com roxo profundo.",
                "#f97316",
                "#311b4f"),
            new ThemePalette(
                "ocean",
                "Ocean",
                "Azul petróleo com azul marinho.",
                "#0284c7",
                "#082f49"),
            new ThemePalette(
                "berry",
                "Berry",
                "Roxo intenso com azul escuro.",
                "#7c3aed",
                "#1b1b3a"),
            new ThemePalette(
                "graphite",
                "Graphite",
                "Cinza grafite com destaque azul.",
                "#0ea5e9",
                "#1f2937")
        };

        public static ThemePalette Default => All[0];

        public static ThemePalette FromKey(string? key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                var found = All.FirstOrDefault(p =>
                    string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    return found;
                }
            }
            return Default;
        }

        public static ThemePalette FromColors(string? primary, string? secondary)
        {
            if (!string.IsNullOrWhiteSpace(primary) && !string.IsNullOrWhiteSpace(secondary))
            {
                var found = All.FirstOrDefault(p =>
                    string.Equals(p.PrimaryHex, primary, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.SecondaryHex, secondary, StringComparison.OrdinalIgnoreCase));
                if (found != null)
                {
                    return found;
                }
            }
            return Default;
        }
    }
}
