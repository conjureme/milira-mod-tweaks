using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace MiliraTweaks
{
    [StaticConstructorOnStartup]
    public static class MiliraTweaksMod
    {
        private static readonly HashSet<string> whitelistedKindDefs = new HashSet<string>
        {
            "Milira_Expeditionary"
        };

        static MiliraTweaksMod()
        {
            new Harmony("tyster.MiliraTweaks").PatchAll();
        }

        public static bool IsWhitelisted(Pawn pawn)
        {
            return pawn?.kindDef != null && whitelistedKindDefs.Contains(pawn.kindDef.defName);
        }
    }

    [HarmonyPatch]
    public static class Patch_TraitLock_CheckAndRemoveTraits
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("AriandelLibrary.GameComponent_AL_TraitLock");
            return AccessTools.Method(type, "CheckAndRemoveTraits");
        }

        static bool Prefix(Pawn p)
        {
            if (MiliraTweaksMod.IsWhitelisted(p))
                return false;
            return true;
        }
    }
}
