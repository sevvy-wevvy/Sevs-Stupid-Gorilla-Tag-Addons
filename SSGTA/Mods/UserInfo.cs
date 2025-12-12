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
using Photon.Pun;

namespace SSGTA.Mods
{
    public class NameTags : MonoBehaviour
    {
        public static NameTags Instance { get; private set; }

        private void Awake()
        {
            UnityEngine.Debug.Log("[SSGTA] NameTags awake");
            Instance = this;
            UnityEngine.Debug.Log("[SSGTA] NameTags Instance Set");
        }

        /*-------------------------------------------------------------------------------------------------------------------*/

        public void Update()
        {
            if (!SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Enabled").enabled) return;

            foreach (Photon.Realtime.Player ssss in PhotonNetwork.PlayerListOthers)
            {
                VRRig rig = GorillaGameManager.instance.FindPlayerVRRig(ssss);
                if (!rig.isOfflineVRRig && !rig.isMyPlayer)
                {
                    GameObject nameTag = rig.transform.Find("Name Mod")?.gameObject;

                    nameTag = new GameObject("Name Mod");

                    TextMeshPro textMesh = nameTag.AddComponent<TextMeshPro>();
                    if (!SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Alot Of Data").enabled) textMesh.text = rig.OwningNetPlayer.NickName;
                    else
                    {
                        textMesh.text = rig.OwningNetPlayer.NickName + "\nJoin Date: " + SSGTA.Resourses.RigManager.CreationDate(rig) + "\n" + SSGTA.Resourses.RigManager.GetPlatform(rig) + "\nFPS: " + SSGTA.Resourses.RigManager.GetFPS(rig);
                    }
                    textMesh.fontSize = 2f;
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.color = rig.playerColor;

                    nameTag.transform.SetParent(rig.transform);

                    UnityEngine.Object.Destroy(nameTag, Time.deltaTime);

                    Transform nameTagTransform = nameTag.transform;
                    nameTagTransform.position = rig.headConstraint.position + new Vector3(0f, 0.7f, 0f);
                    if (SeveralBees.Api.Instance.GrabButton(Plugin.Instance.NameTagsPageSbToken, "Alot Of Data").enabled) nameTagTransform.position += new Vector3(0f, 0.2f, 0f);
                    nameTagTransform.LookAt(Camera.main.transform.position);
                    nameTagTransform.Rotate(0f, 180f, 0f);

                    nameTag.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
        }
    }
}