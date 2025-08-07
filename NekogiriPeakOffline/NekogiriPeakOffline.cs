using System;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace NekogiriMod
{
    [BepInPlugin("kirigiri.peak.nekogirioffline", "NekogiriPeakOffline", "1.0.0.0")]
    public class NekogiriMod : BaseUnityPlugin
    {
        private void Awake()
        {
            // Set up plugin logging
            Logger.LogInfo(@"
 ██ ▄█▀ ██▓ ██▀███   ██▓  ▄████  ██▓ ██▀███   ██▓
 ██▄█▒ ▓██▒▓██ ▒ ██▒▓██▒ ██▒ ▀█▒▓██▒▓██ ▒ ██▒▓██▒
▓███▄░ ▒██▒▓██ ░▄█ ▒▒██▒▒██░▄▄▄░▒██▒▓██ ░▄█ ▒▒██▒
▓██ █▄ ░██░▒██▀▀█▄  ░██░░▓█  ██▓░██░▒██▀▀█▄  ░██░
▒██▒ █▄░██░░██▓ ▒██▒░██░░▒▓███▀▒░██░░██▓ ▒██▒░██░
▒ ▒▒ ▓▒░▓  ░ ▒▓ ░▒▓░░▓   ░▒   ▒ ░▓  ░ ▒▓ ░▒▓░░▓  
░ ░▒ ▒░ ▒ ░  ░▒ ░ ▒░ ▒ ░  ░   ░  ▒ ░  ░▒ ░ ▒░ ▒ ░
░ ░░ ░  ▒ ░  ░░   ░  ▒ ░░ ░   ░  ▒ ░  ░░   ░  ▒ ░
░  ░    ░     ░      ░        ░  ░     ░      ░  
                                                 
");
            Logger.LogInfo("NekogiriPeakOffline has loaded!");

            var harmony = new Harmony("kirigiri.peak.nekogirioffline");
            harmony.PatchAll();

            // Optionally log that the patch has been applied
            Logger.LogInfo("Made with <3 By Kirigiri \nhttps://discord.gg/TBs8Te5nwn");
        }

        [HarmonyPatch(typeof(CloudAPI), nameof(CloudAPI.CheckVersion))]
        public class CloudAPICheckVersionPatch
        {
            private static int? _cachedLevelIndex = null;

            [HarmonyPrefix]
            public static bool Prefix(Action<LoginResponse> response)
            {
                Debug.Log("[NekogiriPeak] Patching CloudAPI.CheckVersion");

                BuildVersion buildVersion = new BuildVersion(Application.version);

                if (_cachedLevelIndex == null)
                {
                    System.Random rng = new System.Random();
                    _cachedLevelIndex = rng.Next(0, int.MaxValue);
                }

                LoginResponse loginResponse;

                // Assuming you have access to buildVersion object here
                if (buildVersion.BuildName == "beta")
                {
                    loginResponse = new LoginResponse
                    {
                        VersionOkay = true,
                        HoursUntilLevel = 24,
                        MinutesUntilLevel = 0,
                        SecondsUntilLevel = 0,
                        LevelIndex = _cachedLevelIndex.Value,
                        Message = "Thanks for testing the PEAK beta. Watch out for bugs! (the current beta is the same as the live game, check back later for a new beta!)"
                    };
                }
                else
                {
                    loginResponse = new LoginResponse
                    {
                        VersionOkay = true,
                        HoursUntilLevel = 24,
                        MinutesUntilLevel = 0,
                        SecondsUntilLevel = 0,
                        LevelIndex = _cachedLevelIndex.Value,
                        Message = "why did the chicken cross the caldera?"
                    };
                }

                response?.Invoke(loginResponse);
                return false;
            }
        }

        [HarmonyPatch(typeof(NetworkConnector), nameof(NetworkConnector.ConnectToPhoton))]
        public class NetworkConnectorPatch
        {
            [HarmonyPrefix]
            public static bool Prefix()
            {

                PhotonNetwork.OfflineMode = true;
                return false;
            }
        }
    }
}
