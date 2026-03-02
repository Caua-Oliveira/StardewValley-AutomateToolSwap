using HarmonyLib;
using StardewValley;
using StardewValley.Tools;

namespace OnlyWhenItCounts;

[HarmonyPatch(typeof(FishingRod), nameof(FishingRod.DoFunction))]
public static class FishingRodPatch
{
    private static float preStamina = -1f;

    public static void Prefix(FishingRod __instance, Farmer who)
    {
        if (!ModEntry.Config.Enabled) return;

        Farmer player = who ?? Game1.player;

        // This exactly matches the logic for when a cast drains stamina
        if (player.IsLocalPlayer &&
            !__instance.isFishing && !__instance.castedButBobberStillInAir &&
            !__instance.pullingOutOfWater && !__instance.isNibbling &&
            !__instance.hit && !__instance.showingTreasure)
        {
            preStamina = player.stamina;
        }
        else
        {
            preStamina = -1f;
        }
    }

    public static void Postfix(FishingRod __instance, Farmer who)
    {
        if (!ModEntry.Config.Enabled || preStamina == -1f) return;

        Farmer player = who ?? Game1.player;

        if (player.IsLocalPlayer)
        {
            // If isFishing is true, the bobber successfully landed in water.
            // If it is false, the bobber hit land.
            if (!__instance.isFishing)
            {
                player.stamina = preStamina;
            }
        }

        preStamina = -1f;
    }
}