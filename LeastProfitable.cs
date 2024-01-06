using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LeastProfitable
{
    [BepInPlugin(modGUID, modName, modSemVer)]
    public class LeastProfitable : BaseUnityPlugin
    {
        private const string modGUID = "evelritual.LeastProfitable";
        private const string modName = "Least Profitable";
        private const string modSemVer = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        private static LeastProfitable Instance;
        public static ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll();

            logger.LogInfo("LeastProfitable loaded!");
        }
    }
}

namespace LeastProfitable.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class profitPatch
    {
        [HarmonyPatch("WritePlayerNotes")]
        [HarmonyPostfix]
        private static void LeastProfitable(ref EndOfGameStats ___gameStats, ref int ___connectedPlayersAmount)
        {
            // No sense tracking if it's single player
            if (___connectedPlayersAmount <= 0)
            {
                return;
            }

            int minProfit = 999999;

            // Find min profit of all active players
            for (int i = 0; i < ___gameStats.allPlayerStats.Length; i++)
            {
                if (___gameStats.allPlayerStats[i].isActivePlayer && ___gameStats.allPlayerStats[i].profitable < minProfit)
                {
                    minProfit = ___gameStats.allPlayerStats[i].profitable;
                }
            }

            // Tag all players who tied for min profit
            for (int i = 0; i < ___gameStats.allPlayerStats.Length; i++)
            {
                if (___gameStats.allPlayerStats[i].isActivePlayer && ___gameStats.allPlayerStats[i].profitable == minProfit)
                {
                    ___gameStats.allPlayerStats[i].playerNotes.Add("Least profitable :(");
                }
            }
        }
    }
}
