﻿using BepInEx;
using System.Security.Permissions;
using System.Security;
using UnityEngine;
using Fisobs.Core;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Template
{
    [BepInPlugin("nacu.template", "Template", "0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static BepInEx.Logging.ManualLogSource logger;

        public void OnEnable()
        {
            logger = Logger;
            On.RainWorld.OnModsInit += OnModsInit;
        }

        public void OnDisable()
        {
            logger = null;
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig.Invoke(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                //Futile.atlasManager.LoadAtlas("Atlases/template");
            }
        }
    }
}