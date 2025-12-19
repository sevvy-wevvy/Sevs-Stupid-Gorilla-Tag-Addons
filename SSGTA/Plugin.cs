using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using BepInEx;
using Debug = UnityEngine.Debug;
using System.Runtime.InteropServices.ComTypes;
using Valve.VR;
using UnityEngine.XR;
using static Mono.Security.X509.X520;
using Color = UnityEngine.Color;
using GorillaLocomotion;
using UnityEngine.InputSystem;
using SeveralBees;
using SSGTA.Mods;
using Photon.Pun;
using Photon.Realtime;
using System.Drawing;
using UnityEngine.Animations.Rigging;
using System.Reflection;

namespace SSGTA
{
    [BepInPlugin("com.sev.gorillatag.SSGTA", "SSGTA", "1.1.3")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInDependency("com.Sev.gorillatag.SeveralBees", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        internal string MainPageSbToken = "";
        internal string NameTagsPageSbToken = "";
        internal string UserDataPageSbToken = "";
        internal string SfteyPageSbToken = "";
        internal GameObject HandelrForShit = null;

        Dictionary<string, string> UserTabs = new Dictionary<string, string>();
        Dictionary<string, string> UserTabsMods = new Dictionary<string, string>();
        Dictionary<string, string> UserTabsCosmetics = new Dictionary<string, string>();

        private void Awake()
        {
            Instance = this;
            SeveralBees.Plugin.Instance.Startup.Add(InstanceStartThing);
        }

        public void InstanceStartThing()
        {
            HandelrForShit = new GameObject("SSGTA Mod Handler");
            HandelrForShit.AddComponent<Mods.NameTags>();
            HandelrForShit.AddComponent<Mods.UserInfo>();

            MainPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA</color>", true, "Main");
            NameTagsPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA || Name Tags</color>", false, MainPageSbToken);
            UserDataPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA || User Data</color>", false, MainPageSbToken);
            SfteyPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA || Saftey</color>", false, MainPageSbToken);

            Api.Instance.SetButtonInfo(MainPageSbToken, new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "Name Tags", isTogglable = false, method = () => Api.Instance.OpenMenu(NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "User Data", isTogglable = false, method = () => { Api.Instance.OpenMenu(UserDataPageSbToken); RefreshUserData(); } },
                new ModButtonInfo { buttonText = "Saftey", isTogglable = false, method = () => Api.Instance.OpenMenu(SfteyPageSbToken) },
            });

            Api.Instance.SetButtonInfo(NameTagsPageSbToken, new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "Enabled", isTogglable = true, enabled = GetEnabled("NameTags-Enabled", false), enableMethod = () => SaveEnabled("Enabled", "NameTags-Enabled", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Enabled", "NameTags-Enabled", NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "Show Join Date", isTogglable = true, enabled = GetEnabled("NameTags-ShowJoinDate", false), enableMethod = () => SaveEnabled("Show Join Date", "NameTags-ShowJoinDate", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Show Join Date", "NameTags-ShowJoinDate", NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "Show Platform", isTogglable = true, enabled = GetEnabled("NameTags-ShowPlatform", false), enableMethod = () => SaveEnabled("Show Platform", "NameTags-ShowPlatform", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Show Platform", "NameTags-ShowPlatform", NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "Show FPS", isTogglable = true, enabled = GetEnabled("NameTags-ShowFPS", false), enableMethod = () => SaveEnabled("Show FPS", "NameTags-ShowFPS", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Show FPS", "NameTags-ShowFPS", NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "Show Mods", isTogglable = true, enabled = GetEnabled("NameTags-ShowIllegalMods", false), enableMethod = () => SaveEnabled("Show Mods", "NameTags-ShowIllegalMods", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Show Mods", "NameTags-ShowIllegalMods", NameTagsPageSbToken) },
                new ModButtonInfo { buttonText = "Show Special Cosmetics", isTogglable = true, enabled = GetEnabled("NameTags-ShowSpecialCosmetics", false), enableMethod = () => SaveEnabled("Show Special Cosmetics", "NameTags-ShowSpecialCosmetics", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Show Special Cosmetics", "NameTags-ShowSpecialCosmetics", NameTagsPageSbToken) }
            });

            Api.Instance.SetButtonInfo(SfteyPageSbToken, new List<ModButtonInfo>
            {
                new ModButtonInfo { toolTip = "Makes it so all custon props except essentail one are null. Requires restart to fully turn off.", method = () => { PhotonNetwork.SetPlayerCustomProperties(null); NetworkSystem.Instance.SetMyTutorialComplete(); }, buttonText = "Clear Custom Props", isTogglable = true, enabled = GetEnabled("Saftey-ClearProps", false), enableMethod = () => SaveEnabled("Clear Custom Props", "Saftey-ClearProps", SfteyPageSbToken), disableMethod = () => SaveEnabled("Clear Custom Props", "Saftey-ClearProps", SfteyPageSbToken) },
                new ModButtonInfo { toolTip = "Makes it so all custon props except essentail one are null and enables all known props. Requires restart to fully turn off.", buttonText = "Spoof Custom Props", isTogglable = true, enabled = GetEnabled("Saftey-SpoofProps", false), enableMethod = () => SaveEnabled("Spoof Custom Props", "Saftey-SpoofProps", SfteyPageSbToken), disableMethod = () => { SaveEnabled("Spoof Custom Props", "Saftey-SpoofProps", SfteyPageSbToken); HasSpoofedProps = false; } },
                new ModButtonInfo { method = () => FpsSpoof(), toolTip = "Makes others see your fps as around 80. Requires restart to fully turn off.", buttonText = "Spoof FPS", isTogglable = true, enabled = GetEnabled("Saftey-SpoofFPS", false), enableMethod = () => SaveEnabled("Spoof FPS", "Saftey-SpoofFPS", SfteyPageSbToken), disableMethod = () => SaveEnabled("Spoof FPS", "Saftey-SpoofFPS", SfteyPageSbToken) },
            });
        }

        private bool HasSpoofedProps = false;
        public void Update()
        {
            if (!HasSpoofedProps && SeveralBees.Api.Instance.GrabButton(SfteyPageSbToken, "Spoof Custom Props").enabled && PhotonNetwork.IsConnected)
            {
                PhotonNetwork.SetPlayerCustomProperties(null);
                NetworkSystem.Instance.SetMyTutorialComplete();
                UserInfo.Instance.SpoofProps();
                HasSpoofedProps = true;
            }
        }

        void ClearUserData()
        {
            UserTabs.Clear();
            UserTabsMods.Clear();
            UserTabsCosmetics.Clear();
            Api.Instance.SetButtonInfo(UserDataPageSbToken, new List<ModButtonInfo>());
        }

        public void RefreshUserData()
        {
            UserTabs.Clear();
            UserTabsMods.Clear();
            UserTabsCosmetics.Clear();

            List<ModButtonInfo> mainButtons = new List<ModButtonInfo>
            {
                new ModButtonInfo { method = () => RefreshUserData(), buttonText = "<color=red>Refresh</color>", isTogglable = false }
            };

            foreach (Player p in PhotonNetwork.PlayerListOthers)
            {
                VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(p);
                if (rig == null || rig.isOfflineVRRig || rig.isMyPlayer) continue;

                string userId = rig.OwningNetPlayer.UserId;

                UserTabs[userId] = Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{rig.OwningNetPlayer.NickName}</color>", false, UserDataPageSbToken);
                UserTabsMods[userId] = Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{rig.OwningNetPlayer.NickName}</color> || Mods", false, UserTabs[userId]);
                UserTabsCosmetics[userId] = Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{rig.OwningNetPlayer.NickName}</color> || Cosmetics", false, UserTabs[userId]);

                mainButtons.Add(new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabs[userId]), buttonText = $"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>{rig.OwningNetPlayer.NickName}</color>", isTogglable = false });

                Api.Instance.SetButtonInfo(UserTabs[userId], new List<ModButtonInfo>
                {
                    new ModButtonInfo { buttonText = $"Name: {rig.OwningNetPlayer.NickName}", isTogglable = false },
                    new ModButtonInfo { buttonText = $"Join Date: {Resourses.RigManager.CreationDate(rig)}", isTogglable = false },
                    new ModButtonInfo { buttonText = $"Platform: {Resourses.RigManager.GetPlatform(rig)}", isTogglable = false },
                    //new ModButtonInfo { buttonText = $"FPS: {Resourses.RigManager.GetFPS(rig)}", isTogglable = false },
                    new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabsMods[userId]), buttonText = "<color=orange>Mods</color>", isTogglable = false },
                    new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabsCosmetics[userId]), buttonText = "<color=orange>Special Cosmetics</color>", isTogglable = false }
                });

                List<ModButtonInfo> modButtons = new List<ModButtonInfo>();
                foreach (string mod in Mods.UserInfo.Instance.CheckMods(rig))
                    modButtons.Add(new ModButtonInfo { buttonText = mod, isTogglable = false });

                Api.Instance.SetButtonInfo(UserTabsMods[userId], modButtons);

                List<ModButtonInfo> cosmeticButtons = new List<ModButtonInfo>();
                foreach (string cosmetic in Mods.UserInfo.Instance.CheckCosmetics(rig))
                    cosmeticButtons.Add(new ModButtonInfo { buttonText = cosmetic, isTogglable = false });

                Api.Instance.SetButtonInfo(UserTabsCosmetics[userId], cosmeticButtons);
            }

            Api.Instance.SetButtonInfo(UserDataPageSbToken, mainButtons);
        }

        internal void SaveEnabled(string ButtonName, string SaveName, string Token)
        {
            PlayerPrefs.SetInt("SSGTA-" + SaveName, (SeveralBees.Api.Instance.GrabButton(Token, ButtonName).enabled ? 1 : 0));
        }

        internal bool GetEnabled(string SaveName, bool Def = false)
        {
            return PlayerPrefs.GetInt("SSGTA-" + SaveName, Def ? 1 : 0) == 1;
        }

        private void FpsSpoof()
        {
            try
            {
                int fps = UnityEngine.Random.Range(75, 85);
                VRRig offlineVRRig = GorillaTagger.Instance.offlineVRRig;
                FieldInfo field = typeof(VRRig).GetField("fps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                field.SetValue(offlineVRRig, fps);
            }
            catch { }
        }
    }
}
