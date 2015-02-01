using System;
using System.Linq;
using System.Collections.Generic;
using Fougerite;

public static class LevenshteinDistance
{
    /// http://www.dotnetperls.com/levenshtein
    /// <summary>
    /// Compute the distance between two strings.
    /// </summary>
    private static int Compute(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // Step 1
        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        // Step 2
        for (int i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (int j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Step 3
        for (int i = 1; i <= n; i++)
        {
            //Step 4
            for (int j = 1; j <= m; j++)
            {
                // Step 5
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }

    public static int Distance(this string s, string t)
    {
        char[] rm = new char[] { ' ', '"', '\'', '-' };
        return Compute(s.ToUpperInvariant().RemoveChars(rm), t.ToUpperInvariant().RemoveChars(rm));
    }
}

public static class BlueprintStringEx
{
    private static readonly string BP = "BP";
    private static readonly string BLUEPRINT = "Blueprint";

    public static bool HasBPTerm(this string itemName)
    {
        bool flag = false;
        foreach (string term in itemName.Split(new char[] { ' ' }))
        {
            flag = BP.Distance(term) == 0 ? true : false;
            if (flag)
                break;

            if (term.Length >= BLUEPRINT.Length)
                flag = BLUEPRINT.Distance(term) <= 1 ? true : false;
            else
                flag = BLUEPRINT.Substring(0, term.Length).Distance(term) <= 1 ? true : false;

            if (flag)
                break;
        }
        return flag;
    }

    public static string BaseItem(this string itemName)
    {
        ICollection<string> baseterms = new List<string>();
        foreach (string term in itemName.Split(new char[] { ' ' }))
        {
            if (BP.Distance(term) == 0)
                continue;

            if (term.Length >= BLUEPRINT.Length)
                if (BLUEPRINT.Distance(term) <= 1)
                    continue;
            else
                if (BLUEPRINT.Substring(0, term.Length).Distance(term) <= 1)
                    continue;

            baseterms.Add(term);
        }
        return string.Join(" ", baseterms.ToArray<string>());
    }

    public static string Blueprint(this string baseItemName)
    {
        string key = baseItemName.Replace("Research Kit 1", "Research Kit");
        if (BlueprintNames.ContainsKey(key))
            return string.Format("{0} {1}", key, BlueprintNames[key]);

        return baseItemName;
    }

    public static string MatchItemName(this string itemName)
    {
        string baseName = itemName.BaseItem();
        IEnumerable<string> queryName = from name in ItemNames
                                              group name by baseName.Distance(name) into match
                                              orderby match.Key ascending
                                              select match.FirstOrDefault();
        if (queryName.Count() != 1)
            Logger.LogDebug(string.Format("[MatchItemName] search={0} matches={1}", itemName, string.Join(", ", queryName.ToArray())));

        return itemName.HasBPTerm() ? queryName.FirstOrDefault().Blueprint() : queryName.FirstOrDefault();
    }

    private static readonly IEnumerable<string> ItemNames = new string[] { "556 Ammo", "9mm Ammo", "9mm Pistol", "Animal Fat", "Anti-Radiation Pills", "Armor Part 1", "Armor Part 2", "Armor Part 3", 
        "Armor Part 4", "Armor Part 5", "Armor Part 6", "Armor Part 7", "Arrow", "Bandage", "Bed", "Blood Draw Kit", "Blood", "Bolt Action Rifle", "Camp Fire", "Can of Beans", 
        "Can of Tuna", "Charcoal", "Chocolate Bar", "Cloth Boots", "Cloth Helmet", "Cloth Pants", "Cloth Vest", "Cloth", "Cooked Chicken Breast", "Empty 556 Casing",
        "Empty 9mm Casing", "Empty Shotgun Shell", "Explosive Charge", "Explosives", "F1 Grenade", "Flare", "Flashlight Mod", "Furnace", "Granola Bar", "Gunpowder", "HandCannon",
        "Handmade Lockpick", "Handmade Shell", "Hatchet", "Holo sight", "Hunting Bow", "Invisible Boots", "Invisible Helmet", "Invisible Pants", "Invisible Vest", "Kevlar Boots",
        "Kevlar Helmet", "Kevlar Pants", "Kevlar Vest", "Large Medkit", "Large Spike Wall", "Large Wood Storage", "Laser Sight", "Leather Boots", "Leather Helmet", "Leather Pants",
        "Leather Vest", "Leather", "Low Grade Fuel", "Low Quality Metal", "M4", "MP5A4", "Metal Ceiling", "Metal Door", "Metal Doorway", "Metal Foundation", "Metal Fragments",
        "Metal Ore", "Metal Pillar", "Metal Ramp", "Metal Stairs", "Metal Wall", "Metal Window Bars", "Metal Window", "P250", "Paper", "Pick Axe", "Pipe Shotgun", "Primed 556 Casing",
        "Primed 9mm Casing", "Primed Shotgun Shell", "Rad Suit Boots", "Rad Suit Helmet", "Rad Suit Pants", "Rad Suit Vest", "Raw Chicken Breast", "Recycle Kit 1", "Repair Bench", 
        "Research Kit 1", "Revolver", "Rock", "Shotgun Shells", "Shotgun", "Silencer", "Sleeping Bag", "Small Medkit", "Small Rations", "Small Stash", "Small Water Bottle", "Spike Wall", 
        "Stone Hatchet", "Stones", "Sulfur Ore", "Sulfur", "Supply Signal", "Torch", "Uber Hatchet", "Uber Hunting Bow", "Weapon Part 1", "Weapon Part 2", "Weapon Part 3", "Weapon Part 4",
        "Weapon Part 5", "Weapon Part 6", "Weapon Part 7", "Wood Barricade", "Wood Ceiling", "Wood Doorway", "Wood Foundation", "Wood Gate", "Wood Gateway", "Wood Pillar", "Wood Planks",
        "Wood Ramp", "Wood Shelter", "Wood Stairs", "Wood Storage Box", "Wood Wall", "Wood Window", "Wood", "Wooden Door", "Workbench"
    };

    private static readonly IDictionary<string, string> BlueprintNames = new Dictionary<string, string>()
    {
        { "556 Ammo", "Blueprint" }, { "9mm Ammo", "Blueprint" }, { "9mm Pistol", "Blueprint" }, { "Armor Part 1", "BP" }, { "Armor Part 2", "BP" }, { "Armor Part 3", "BP" }, { "Armor Part 4", "BP" },
        { "Armor Part 5", "BP" }, { "Armor Part 6", "BP" }, { "Armor Part 7", "BP" }, { "Arrow", "Blueprint" }, { "Bandage", "Blueprint" }, { "Bed", "Blueprint" }, { "Blood Draw Kit", "Blueprint" },
        { "Bolt Action Rifle", "Blueprint" }, { "Camp Fire", "Blueprint" }, { "Cloth Boots", "BP" }, { "Cloth Helmet", "BP" }, { "Cloth Pants", "BP" }, { "Cloth Vest", "BP" }, { "Empty Shotgun Shell", "Blueprint" },
        { "Explosive Charge", "Blueprint" }, { "Explosives", "Blueprint" }, { "F1 Grenade", "Blueprint" }, { "Flare", "Blueprint" }, { "Flashlight Mod", "Blueprint" }, { "Furnace", "Blueprint" },
        { "Gunpowder", "Blueprint" }, { "HandCannon", "Blueprint" }, { "Handmade Lockpick", "Blueprint" }, { "Handmade Shell", "Blueprint" }, { "Hatchet", "Blueprint" }, { "Holo sight", "Blueprint" },
        { "Hunting Bow", "Blueprint" }, { "Kevlar Boots", "BP" }, { "Kevlar Helmet", "BP" }, { "Kevlar Pants", "BP" }, { "Kevlar Vest", "BP" }, { "Large Medkit", "Blueprint" }, { "Large Spike Wall", "Blueprint" },
        { "Large Wood Storage", "Blueprint" }, { "Laser Sight", "Blueprint" }, { "Leather Boots", "BP" }, { "Leather Helmet", "BP" }, { "Leather Pants", "BP" }, { "Leather Vest", "BP" }, { "Low Grade Fuel", "Blueprint" },
        { "Low Quality Metal", "Blueprint" }, { "M4", "Blueprint" }, { "MP5A4", "Blueprint" }, { "Metal Ceiling", "" }, { "Metal Door", "Blueprint" }, { "Metal Doorway", "BP" }, { "Metal Foundation", "BP" },
        { "Metal Pillar", "BP" }, { "Metal Ramp", "BP" }, { "Metal Stairs", "BP" }, { "Metal Wall", "BP" }, { "Metal Window Bars", "Blueprint" }, { "Metal Window", "BP" }, { "P250", "Blueprint" },
        { "Paper", "Blueprint" }, { "Pick Axe", "Blueprint" }, { "Pipe Shotgun", "Blueprint" }, { "Primed 556 Casing", "Blueprint" }, { "Primed 9mm Casing", "Blueprint" }, { "Primed Shotgun Shell", "Blueprint" },
        { "Rad Suit Boots", "BP" }, { "Rad Suit Helmet", "BP" }, { "Rad Suit Pants", "BP" }, { "Rad Suit Vest", "BP" }, { "Repair Bench", "Blueprint" }, { "Research Kit", "Blueprint" }, { "Revolver", "Blueprint" },
        { "Shotgun Shells", "Blueprint" }, { "Shotgun", "Blueprint" }, { "Silencer", "Blueprint" }, { "Sleeping Bag", "Blueprint" }, { "Small Medkit", "Blueprint" }, { "Small Stash", "Blueprint" },
        { "Spike Wall", "Blueprint" }, { "Stone Hatchet", "Blueprint" }, { "Torch", "Blueprint" }, { "Weapon Part 1", "BP" }, { "Weapon Part 2", "BP" }, { "Weapon Part 3", "BP" }, { "Weapon Part 4", "BP" },
        { "Weapon Part 5", "BP" }, { "Weapon Part 6" , "BP" }, { "Weapon Part 7", "BP" }, { "Wood Barricade", "Blueprint" }, { "Wood Ceiling", "BP" }, { "Wood Doorway", "BP" }, { "Wood Foundation", "BP" },
        { "Wood Gate", "Blueprint" }, { "Wood Gateway", "Blueprint" }, { "Wood Pillar", "BP" }, { "Wood Planks", "Blueprint" }, { "Wood Ramp", "BP" }, { "Wood Shelter", "BP" }, { "Wood Stairs", "BP" },
        { "Wood Storage Box", "Blueprint" }, { "Wood Wall", "BP" }, { "Wood Window", "BP" }, { "Wooden Door", "Blueprint" }, { "Workbench", "Blueprint" }
    };
}
