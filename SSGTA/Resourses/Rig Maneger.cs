using Photon.Realtime;
using Photon.Pun;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections.Generic;
using System;

namespace SSGTA.Resourses
{
    public class RigManager : BaseUnityPlugin
    {
        public static VRRig GetVRRigFromPlayer(Player p)
        {
            return GorillaGameManager.instance.FindPlayerVRRig(p);
        }

        public static VRRig GetRandomVRRig(bool includeSelf)
        {
            VRRig random = GorillaParent.instance.vrrigs[UnityEngine.Random.Range(0, GorillaParent.instance.vrrigs.Count - 1)];
            if (includeSelf)
            {
                return random;
            }
            else
            {
                if (random != GorillaTagger.Instance.offlineVRRig)
                {
                    return random;
                }
                else
                {
                    return GetRandomVRRig(includeSelf);
                }
            }
        }

        public static VRRig GetClosestVRRig()
        {
            float num = float.MaxValue;
            VRRig outRig = null;
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position) < num)
                {
                    num = Vector3.Distance(GorillaTagger.Instance.bodyCollider.transform.position, vrrig.transform.position);
                    outRig = vrrig;
                }
            }
            return outRig;
        }

        public static PhotonView GetPhotonViewFromVRRig(VRRig p)
        {
            return (PhotonView)Traverse.Create(p).Field("photonView").GetValue();
        }

        public static Photon.Realtime.Player GetRandomPlayer(bool includeSelf)
        {
            if (includeSelf)
            {
                return PhotonNetwork.PlayerList[UnityEngine.Random.Range(0, PhotonNetwork.PlayerList.Length - 1)];
            }
            else
            {
                return PhotonNetwork.PlayerListOthers[UnityEngine.Random.Range(0, PhotonNetwork.PlayerListOthers.Length - 1)];
            }
        }

        public static Photon.Realtime.Player GetPlayerFromVRRig(VRRig p)
        {
            return GetPhotonViewFromVRRig(p).Owner;
        }

        public static Photon.Realtime.Player GetPlayerFromID(string id)
        {
            Photon.Realtime.Player found = null;
            foreach (Photon.Realtime.Player target in PhotonNetwork.PlayerList)
            {
                if (target.UserId == id)
                {
                    found = target;
                    break;
                }
            }
            return found;
        }

        static Dictionary<string, string> datePool = new Dictionary<string, string> { };
        public static string CreationDate(VRRig rig)
        {
            string UserId = rig.Creator.UserId;

            if (datePool.ContainsKey(UserId))
                return datePool[UserId];
            else
            {
                datePool.Add(UserId, "LOADING");
                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = UserId }, delegate (GetAccountInfoResult result)
                {
                    string date = result.AccountInfo.Created.ToString("MMM dd, yyyy HH:mm").ToUpper();
                    datePool[UserId] = date;
                    rig.UpdateName();
                }, delegate { datePool[UserId] = "ERROR"; rig.UpdateName(); }, null, null);
                return "LOADING";
            }
        }

        public static string GetPlatform(VRRig rig)
        {
            string creationDateStr = CreationDate(rig);

            if (DateTime.TryParse(creationDateStr, out DateTime creationDate))
            {
                // Aperently there was a glith causing accounts created after 2022 to show as on standalone
                if (creationDate.Year <= 2022)
                    return "UNKNOWN";
            }

            string concatStringOfCosmeticsAllowed = rig.concatStringOfCosmeticsAllowed;

            if (concatStringOfCosmeticsAllowed.Contains("S. FIRST LOGIN"))
                return "STEAM";
            else if (concatStringOfCosmeticsAllowed.Contains("FIRST LOGIN") || rig.Creator.GetPlayerRef().CustomProperties.Count >= 2)
                return "PC";

            return "STANDALONE";
        }

        public static string GetFPS(VRRig rig)
        {
            Traverse fps = Traverse.Create(rig).Field("fps");

            if (fps != null)
                return fps.GetValue().ToString();

            return null;
        }
    }
}