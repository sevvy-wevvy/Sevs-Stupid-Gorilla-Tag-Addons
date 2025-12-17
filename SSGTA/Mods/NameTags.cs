using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using GorillaLocomotion;
using SeveralBees;

namespace SSGTA.Mods
{
    public class NameTags : MonoBehaviour
    {
        public static NameTags Instance { get; private set; }

        private readonly Dictionary<string, GameObject> nameTags = new Dictionary<string, GameObject>();

        private void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            if (!SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Enabled").enabled)
            {
                ClearAll();
                return;
            }

            HashSet<string> aliveUsers = new HashSet<string>();

            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                if (string.IsNullOrEmpty(p.UserId)) continue;
                aliveUsers.Add(p.UserId);

                VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(p);
                if (rig == null || rig.isOfflineVRRig || rig.isMyPlayer)
                {
                    Remove(p.UserId);
                    continue;
                }

                if (!nameTags.TryGetValue(p.UserId, out GameObject tag) || tag == null)
                {
                    tag = new GameObject("Name Mod");
                    tag.transform.SetParent(rig.transform);
                    tag.AddComponent<TextMeshPro>();
                    nameTags[p.UserId] = tag;
                }

                TextMeshPro tmp = tag.GetComponent<TextMeshPro>();
                tmp.text = rig.OwningNetPlayer.NickName;

                if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Show Join Date").enabled)
                    tmp.text += "\nJoin Date: " + SSGTA.Resourses.RigManager.CreationDate(rig);

                if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Show Platform").enabled)
                    tmp.text += "\n" + SSGTA.Resourses.RigManager.GetPlatform(rig);

                if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Show FPS").enabled)
                    tmp.text += "\nFPS: " + SSGTA.Resourses.RigManager.GetFPS(rig);

                if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Show Special Cosmetics").enabled)
                    tmp.text += "\n" + UserInfo.Instance.CheckCosmetics(rig).Join("<color=white>, </color>");

                if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Show Mods").enabled)
                    tmp.text += "\n" + UserInfo.Instance.CheckMods(rig).Join("<color=white>, </color>");

                tmp.fontSize = 2f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = rig.playerColor;

                Transform t = tag.transform;
                t.position = rig.headConstraint.position + new Vector3(0f, 0.7f, 0f);
                t.LookAt(Camera.main.transform.position);
                t.Rotate(0f, 180f, 0f);
                t.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            Cleanup(aliveUsers);
        }

        private void Cleanup(HashSet<string> alive)
        {
            List<string> dead = new List<string>();

            foreach (var kvp in nameTags)
            {
                if (!alive.Contains(kvp.Key))
                {
                    if (kvp.Value != null) Destroy(kvp.Value);
                    dead.Add(kvp.Key);
                }
            }

            foreach (string id in dead) nameTags.Remove(id);
        }

        private void Remove(string userId)
        {
            if (!nameTags.TryGetValue(userId, out GameObject go)) return;
            if (go != null) Destroy(go);
            nameTags.Remove(userId);
        }

        private void ClearAll()
        {
            foreach (var kvp in nameTags)
                if (kvp.Value != null) Destroy(kvp.Value);

            nameTags.Clear();
        }
    }
}
