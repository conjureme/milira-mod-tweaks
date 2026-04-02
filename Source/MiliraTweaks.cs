using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
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

        private static readonly HashSet<string> miliraFactionDefs = new HashSet<string>
        {
            "Milira_PlayerFaction",
            "Milira_ExpeditionPlayerFaction"
        };

        static MiliraTweaksMod()
        {
            var harmony = new Harmony("tyster.MiliraTweaks");
            harmony.PatchAll();

            var traitLockType = AccessTools.TypeByName("AriandelLibrary.GameComponent_AL_Polling");
            if (traitLockType == null)
            {
                Log.Warning("[MiliraTweaks] AriandelLibrary.GameComponent_AL_Polling not found - trait lock patch skipped");
                return;
            }

            var checkMethod = AccessTools.Method(traitLockType, "CheckAndRemoveTraits");
            if (checkMethod == null)
            {
                Log.Warning("[MiliraTweaks] CheckAndRemoveTraits not found on GameComponent_AL_Polling - trait lock patch skipped");
                return;
            }

            harmony.Patch(checkMethod, prefix: new HarmonyMethod(typeof(Patch_TraitLock_CheckAndRemoveTraits), nameof(Patch_TraitLock_CheckAndRemoveTraits.Prefix)));
        }

        public static bool IsWhitelisted(Pawn pawn)
        {
            return pawn?.kindDef != null && whitelistedKindDefs.Contains(pawn.kindDef.defName);
        }

        public static bool IsMiliraPlayerFaction()
        {
            return Faction.OfPlayer?.def != null && miliraFactionDefs.Contains(Faction.OfPlayer.def.defName);
        }
    }

    public class IncidentWorker_MiliraExpeditionaryJoin : IncidentWorker_GiveQuest
    {
        public static bool active = false;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
                return false;
            return MiliraTweaksMod.IsMiliraPlayerFaction();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            active = true;
            try
            {
                return base.TryExecuteWorker(parms);
            }
            finally
            {
                active = false;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
    public static class Patch_PawnGenerator_ForExpeditionaryJoin
    {
        static void Prefix(ref PawnGenerationRequest request)
        {
            if (!IncidentWorker_MiliraExpeditionaryJoin.active)
                return;

            PawnKindDef expeditionary = DefDatabase<PawnKindDef>.GetNamedSilentFail("Milira_Expeditionary");
            if (expeditionary != null)
                request.KindDef = expeditionary;
        }
    }

    public static class Patch_TraitLock_CheckAndRemoveTraits
    {
        public static bool Prefix(Pawn p)
        {
            if (MiliraTweaksMod.IsWhitelisted(p))
                return false;
            return true;
        }
    }
}
