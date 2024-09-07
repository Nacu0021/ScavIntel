using BepInEx;
using System.Security.Permissions;
using System.Security;
using UnityEngine;

#pragma warning disable CS0618
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace ScavIntel
{
    [BepInPlugin("nacu.scavintel", "Scav Intel", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool AppliedAlreadyDontDoItAgainPlease;
        internal static BepInEx.Logging.ManualLogSource logger;
        public static bool ShowStats = true;
        public static ScavIntelOptions optiones;

        public void OnEnable()
        {
            logger = Logger;
            optiones = new();
            On.RainWorld.OnModsInit += OnModsInit;
        }

        public void OnDisable()
        {
            logger = null;
        }

        public void Update()
        {
            if (Input.anyKeyDown && Input.GetKeyDown(optiones.StatsKeybind.Value))
            {
                ShowStats = !ShowStats;
            }
        }

        public static void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld raingame)
        {
            orig.Invoke(raingame);

            if (!AppliedAlreadyDontDoItAgainPlease)
            {
                AppliedAlreadyDontDoItAgainPlease = true;

                MachineConnector.SetRegisteredOI("nacu.scavintel", optiones);
                Hooks.Apply();
            }
        }
    }
}