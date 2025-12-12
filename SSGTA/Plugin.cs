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
using System.Drawing;
using UnityEngine.Animations.Rigging;

namespace SSGTA
{
    [BepInPlugin("com.sev.gorillatag.SSGTA", "SSGTA", "1.0.0")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInDependency("com.Sev.gorillatag.SeveralBees", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[SSGTA] Plugin awake");
            Instance = this;
            UnityEngine.Debug.Log("[SSGTA] Plugin Instance Set");
        }

        /*-------------------------------------------------------------------------------------------------------------------*/

        internal string MainPageSbToken = "";
        internal string NameTagsPageSbToken = "";
        internal string UserDataPageSbToken = "";
        internal GameObject HandelrForShit = null;
        public void Start()
        {
            HandelrForShit = new GameObject("SSGTA Mod Handler");
            HandelrForShit.AddComponent<Mods.NameTags>();
            HandelrForShit.AddComponent<Mods.UserInfo>();

            MainPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA</color>", true, "Main");
            NameTagsPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA || Name Tags</color>", false, MainPageSbToken);
            UserDataPageSbToken = Api.Instance.GenerateToken("<color=purple>SSGTA || User Data</color>", false, MainPageSbToken);

            Api.Instance.SetButtonInfo(MainPageSbToken, new List<ModButtonInfo>
            {
                new ModButtonInfo { buttonText = "Name Tags", isTogglable = false, method = () => Api.Instance.OpenMenu(NameTagsPageSbToken), toolTip = "Opens the SSGTA Name Tags menu." },
                new ModButtonInfo { buttonText = "User Data", isTogglable = false, method = () => { Api.Instance.OpenMenu(UserDataPageSbToken); RefreshUserData(); }, toolTip = "Opens the SSGTA User Data menu." },
            });

            Api.Instance.SetButtonInfo(NameTagsPageSbToken, new List<ModButtonInfo>
            {
                new ModButtonInfo { enabled = GetEnabled("NameTags-Enabled", false), enableMethod = () => SaveEnabled("Enabled", "NameTags-Enabled", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Enabled", "NameTags-Enabled", NameTagsPageSbToken), buttonText = "Enabled", isTogglable = true, toolTip = "Toggles the name tags." },
                new ModButtonInfo { buttonText = "Alot Of Data", isTogglable = true, enabled = GetEnabled("NameTags-AlotOfData", true), enableMethod = () => SaveEnabled("Alot Of Data", "NameTags-AlotOfData", NameTagsPageSbToken), disableMethod = () => SaveEnabled("Alot Of Data", "NameTags-AlotOfData", NameTagsPageSbToken), toolTip = "Toggles the if the nametags show a bunch of info." },
            });
        }
        public void Update()
        {

        }

        Dictionary<string, string> UserTabs = new Dictionary<string, string> { };
        Dictionary<string, string> UserTabsMods = new Dictionary<string, string> { };
        Dictionary<string, string> UserTabsCosmetics = new Dictionary<string, string> { };
        public void RefreshUserData()
        {
            List<ModButtonInfo> ButtonsAndShit = new List<ModButtonInfo>();
            ButtonsAndShit.Add(new ModButtonInfo { buttonText = $"<color=red>Refresh</color>", isTogglable = false, toolTip = $"Refreshes/Reloads all user data tables." });
            foreach (Photon.Realtime.Player ssss in PhotonNetwork.PlayerListOthers)
            {
                VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(ssss);
                if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                {
                    if (!UserTabs.ContainsKey(rig.OwningNetPlayer.UserId))
                    {
                        UserTabs.Add(rig.OwningNetPlayer.UserId, Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>" + rig.OwningNetPlayer.NickName + "</color>", true, UserDataPageSbToken));
                    }

                    ButtonsAndShit.Add(new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabs[rig.OwningNetPlayer.UserId]), buttonText = $"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>" + rig.OwningNetPlayer.NickName + "</color>", isTogglable = false, toolTip = $"Opens the menu for {rig.OwningNetPlayer.NickName}." });

                    if (!UserTabsMods.ContainsKey(rig.OwningNetPlayer.UserId))
                    {
                        UserTabsMods.Add(rig.OwningNetPlayer.UserId, Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>" + rig.OwningNetPlayer.NickName + "</color> || Mods", true, UserTabs[rig.OwningNetPlayer.UserId]));
                    }
                    if (!UserTabsCosmetics.ContainsKey(rig.OwningNetPlayer.UserId))
                    {
                        UserTabsCosmetics.Add(rig.OwningNetPlayer.UserId, Api.Instance.GenerateToken($"<color=#{ColorUtility.ToHtmlStringRGB(rig.playerColor)}>" + rig.OwningNetPlayer.NickName + "</color> || Cosmetics", true, UserTabs[rig.OwningNetPlayer.UserId]));
                    }

                    List<ModButtonInfo> ButtonsAndShitNew = new List<ModButtonInfo>();

                    ButtonsAndShitNew.Add(new ModButtonInfo { buttonText = $"Name: {rig.OwningNetPlayer.NickName}", isTogglable = false, toolTip = $"The Name of {rig.OwningNetPlayer.NickName}." });
                    ButtonsAndShitNew.Add(new ModButtonInfo { buttonText = $"Join Date: {Resourses.RigManager.CreationDate(rig)}", isTogglable = false, toolTip = $"The join date of {rig.OwningNetPlayer.NickName}." });
                    ButtonsAndShitNew.Add(new ModButtonInfo { buttonText = $"Platform: {Resourses.RigManager.GetPlatform(rig)}", isTogglable = false, toolTip = $"The platform of {rig.OwningNetPlayer.NickName}." });
                    ButtonsAndShitNew.Add(new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabsMods[rig.OwningNetPlayer.UserId]), buttonText = $"<color=orange>Mods</color>", isTogglable = false, toolTip = $"Enter the users mods." });
                    ButtonsAndShitNew.Add(new ModButtonInfo { method = () => Api.Instance.OpenMenu(UserTabsCosmetics[rig.OwningNetPlayer.UserId]), buttonText = $"<color=orange>Special Cosmetics</color>", isTogglable = false, toolTip = $"Enter the users special cosmetics." });

                    Api.Instance.SetButtonInfo(UserTabs[rig.OwningNetPlayer.UserId], ButtonsAndShitNew);

                    List<string> ModsList = Mods.UserInfo.Instance.CheckMods(rig);
                    List<ModButtonInfo> ButtonsAndShitMods = new List<ModButtonInfo>();
                    foreach (string mod in ModsList)
                    {
                        ButtonsAndShitMods.Add(new ModButtonInfo { buttonText = mod, isTogglable = false, toolTip = $"A mod that {rig.OwningNetPlayer.NickName} has." });
                    }

                    Api.Instance.SetButtonInfo(UserTabsMods[rig.OwningNetPlayer.UserId], ButtonsAndShitMods);

                    List<string> CosmeticsList = Mods.UserInfo.Instance.CheckCosmetics(rig);
                    List<ModButtonInfo> ButtonsAndShitCosmetics = new List<ModButtonInfo>();

                    foreach (string cosmetic in CosmeticsList)
                    {
                        ButtonsAndShitCosmetics.Add(new ModButtonInfo { buttonText = cosmetic, isTogglable = false, toolTip = $"A cosmetic that {rig.OwningNetPlayer.NickName} has." });
                    }
                    Api.Instance.SetButtonInfo(UserTabsCosmetics[rig.OwningNetPlayer.UserId], ButtonsAndShitCosmetics);
                }
                Api.Instance.SetButtonInfo(UserDataPageSbToken, ButtonsAndShit);
            }
        }

        internal void SaveEnabled(string ButtonName, string SaveName, string Token)
        {
            PlayerPrefs.SetInt("SSGTA-" + SaveName, (SeveralBees.Api.Instance.GrabButton(Token, ButtonName).enabled ? 1 : 0));
        }

        internal bool GetEnabled(string SaveName, bool Def = false)
        {
            return (PlayerPrefs.GetInt("SSGTA-" + SaveName, (Def ? 1 : 0)) == 1 ? true : false);
        }
    }
}
