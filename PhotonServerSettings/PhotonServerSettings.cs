using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Zorro.Core;
using UnityEngine.Networking;

namespace PhotonServerSettings
{
    [BepInPlugin("PhotonLAN.peak.PhotonServerSettings", "PEAK-PhotonServerSettings", "1.0.0.0")]
    public class PhotonServerSettings : BaseUnityPlugin
    {
        internal static ConfigEntry<string> PhotonServerAddress;
        internal static ConfigEntry<int> PhotonServerPort;
        internal static ConfigEntry<int> PhotonServerVersion;
        internal static ConfigEntry<string> PhotonConnectionProtocol;
        internal static ConfigEntry<string> PhotonAppIdRealtime;
        internal static ConfigEntry<string> PhotonAppIdVoice;

        private void Awake()
        {
            Logger.LogInfo("PEAK-PhotonServerSettings has loaded!");
            Logger.LogInfo("[PEAK-PhotonServerSettings] Based on REPO-PhotonServerSettings");
            Logger.LogInfo("[PEAK-PhotonServerSettings] Also based on NekogiriPeak by Kirigiri, made with <3 \nhttps://discord.gg/TBs8Te5nwn");

            // Initialize configuration
            PhotonAppIdRealtime = Config.Bind("Photon", "AppId Realtime", "", new ConfigDescription("Photon Realtime App ID"));
            PhotonAppIdVoice = Config.Bind("Photon", "AppId Voice", "", new ConfigDescription("Photon Voice App ID"));
            
            PhotonServerAddress = Config.Bind("Photon", "Server", "", new ConfigDescription("Photon Server Address"));
            PhotonServerPort = Config.Bind("Photon", "Server Port", 0, new ConfigDescription("Photon Server Port", new AcceptableValueRange<int>(0, 65535)));
            PhotonServerVersion = Config.Bind("Photon", "Server Version", 5, new ConfigDescription("Photon Server Version", new AcceptableValueRange<int>(4, 5)));
            
            PhotonConnectionProtocol = Config.Bind("Photon", "Protocol", "Udp", new ConfigDescription("Photon Protocol"));

            var harmony = new Harmony("PhotonLAN.peak.PEAK-PhotonServerSettings");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(CloudAPI), nameof(CloudAPI.CheckVersion))]
        public class CloudAPICheckVersionPatch
        {
            private static readonly DateTime startDate = new DateTime(2025, 6, 14);

            [HarmonyPrefix]
            public static bool Prefix(Action<LoginResponse> response)
            {
                Debug.Log("[PEAK-PhotonServerSettings] Patching CloudAPI.CheckVersion");

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
                        Message = "Thank you for playing PEAK! Pro tip, tapping SPRINT while climbing makes you do a LUNGE!"
                    };
                }

                response?.Invoke(loginResponse);

                return false;
            }
        }

        [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.ConnectUsingSettings), new[] { typeof(AppSettings), typeof(bool) })]
        public class PhotonNetworkPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(AppSettings appSettings, bool startInOfflineMode, ref bool __result)
            {
                Debug.Log("[PEAK-PhotonServerSettings] Overriding PhotonNetwork.ConnectUsingSettings");

                // If we should go to offline mode, don't apply any server settings
                if (startInOfflineMode || PhotonNetwork.OfflineMode)
                {
                    PhotonNetwork.OfflineMode = true;
                    __result = true;
                    return false;
                }
                // Apply server settings only if we're trying to connect to online mode
                if (!string.IsNullOrEmpty(PhotonServerAddress.Value))
                {
                    PhotonNetwork.PhotonServerSettings.AppSettings.Server = PhotonServerAddress.Value;
                    Debug.Log($"[PEAK-PhotonServerSettings] Changed Server Address: {PhotonNetwork.PhotonServerSettings.AppSettings.Server}");
                    
                    if (PhotonServerVersion.Value == 4)
                    {
                        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false;
                        PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV16;
                    }
                    
                    if (PhotonServerPort.Value > 0)
                    {
                        PhotonNetwork.PhotonServerSettings.AppSettings.Port = PhotonServerPort.Value;
                        Debug.Log($"[PEAK-PhotonServerSettings] Changed Server Port: {PhotonNetwork.PhotonServerSettings.AppSettings.Port}");
                    }
                }
                
                if (!string.IsNullOrEmpty(PhotonAppIdRealtime.Value))
                {
                    PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = PhotonAppIdRealtime.Value;
                    Debug.Log($"[PEAK-PhotonServerSettings] Changed AppIdRealtime: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime}");
                }
                
                if (!string.IsNullOrEmpty(PhotonAppIdVoice.Value))
                {
                    PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = PhotonAppIdVoice.Value;
                    Debug.Log($"[PEAK-PhotonServerSettings] Changed AppIdVoice: {PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice}");
                }
                // Continue with original method if we have server settings to apply
                __result = true;
                return true;
            }
        }

    }
}