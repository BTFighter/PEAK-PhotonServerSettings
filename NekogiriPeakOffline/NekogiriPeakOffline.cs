using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Zorro.Core;
using UnityEngine.Networking;

namespace NekogiriMod
{
    [BepInPlugin("kirigiri.peak.nekogirioffline", "NekogiriPeakOffline", "1.0.0.0")]
    public class NekogiriMod : BaseUnityPlugin
    {
        private void Awake()
        {
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

            Logger.LogInfo("Made with <3 By Kirigiri \nhttps://discord.gg/TBs8Te5nwn");
        }

        [HarmonyPatch(typeof(CloudAPI), nameof(CloudAPI.CheckVersion))]
        public class CloudAPICheckVersionPatch
        {
            private static readonly DateTime startDate = new DateTime(2025, 6, 14);

            [HarmonyPrefix]
            public static bool Prefix(Action<LoginResponse> response)
            {
                Debug.Log("[NekogiriPeak] Patching CloudAPI.CheckVersion");

                BuildVersion buildVersion = new BuildVersion(Application.version);

                DateTime now = DateTime.Now;
                DateTime midnight = now.Date.AddDays(1);
                TimeSpan timeUntilMidnight = midnight - now;

                int daysSinceStart = (now.Date - startDate).Days;
                int levelIndex = Mathf.Max(1, daysSinceStart + 1);

                LoginResponse loginResponse;

                if (buildVersion.BuildName == "beta")
                {
                    loginResponse = new LoginResponse
                    {
                        VersionOkay = true,
                        HoursUntilLevel = timeUntilMidnight.Hours,
                        MinutesUntilLevel = timeUntilMidnight.Minutes,
                        SecondsUntilLevel = timeUntilMidnight.Seconds,
                        LevelIndex = levelIndex,
                        Message = "Thanks for testing the PEAK beta. Watch out for bugs! (the current beta is the same as the live game, check back later for a new beta!)"
                    };
                }
                else
                {
                    loginResponse = new LoginResponse
                    {
                        VersionOkay = true,
                        HoursUntilLevel = timeUntilMidnight.Hours,
                        MinutesUntilLevel = timeUntilMidnight.Minutes,
                        SecondsUntilLevel = timeUntilMidnight.Seconds,
                        LevelIndex = levelIndex,
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
                Debug.Log("[NekogiriPeak] Patching NetworkConnector.ConnectToPhoton");

                PhotonNetwork.OfflineMode = true;
                BuildVersion version = new BuildVersion(Application.version);
                PhotonNetwork.AutomaticallySyncScene = true;
                PhotonNetwork.GameVersion = version.ToString();
                PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = version.ToMatchmaking();

                var method = typeof(NetworkConnector).GetMethod("PrepareSteamAuthTicket", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] {
                new Action(() =>
                {
                    PhotonNetwork.ConnectUsingSettings();
                    Debug.Log("Photon Start " + PhotonNetwork.NetworkClientState.ToString() +
                              " using app version: " + version.ToMatchmaking());
                })
            });
                }
                else
                {
                    Debug.LogError("[NekogiriPeak] Failed to find PrepareSteamAuthTicket via reflection");
                }

                return false;
            }
        }
        [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.ConnectUsingSettings), new[] { typeof(AppSettings), typeof(bool) })]
        public class PhotonNetworkPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(AppSettings appSettings, bool startInOfflineMode, ref bool __result)
            {
                Debug.Log("[NekogiriPeak] Overriding PhotonNetwork.ConnectUsingSettings");

                if (startInOfflineMode || PhotonNetwork.OfflineMode)
                {
                    PhotonNetwork.OfflineMode = true;
                    Debug.LogWarning("Kirigiri disabled the online mode, going offline !");
                    __result = true;
                    return false;
                }

                __result = false;
                return false;
            }
        }
    }
}