using System.Collections.Generic;

namespace Arcanoid.Models;

public static class BonusTypeExtentions
{
    private static readonly Dictionary<BonusType, string> BonusSymbols = new Dictionary<BonusType, string>
    {
        { BonusType.IncreaseSpecialBallSpeed, "↑" },
        { BonusType.IncreasePlatformWidth,    "⇔" },
        { BonusType.ExtraPoints,              "＋" },
        { BonusType.ExtraAttempt,             "✚" },
        { BonusType.ExtraBall,                "◎" },
        { BonusType.DecreaseSpecialBallSpeed, "↓" },
        { BonusType.DecreasePlatformWidth,    "⇐" },
        { BonusType.MinusPoints,              "－" },
        { BonusType.MinusAttempt,             "✖" },
        { BonusType.Bonus10,                  "★" }
    };

    public static string GetSymbol(this BonusType bonusType)
    {
        return BonusSymbols[bonusType];
    }
}