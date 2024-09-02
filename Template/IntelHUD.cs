using UnityEngine;
using RWCustom;
using Mono.Cecil.Cil;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using HUD;

namespace ScavIntel
{
    public class IntelHUD : HudPart
    {
        public FLabel scavengerData;
        public FLabel eliteData;
        public FLabel squadCooldown;
        public FLabel squadNumber;
        public Vector2 pos;
        public Vector2 lastPos;
        public float mapFade;
        public int simulatedMapPress;

        public const float mapFadeSpeed = 0.2f;

        public IntelHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            lastPos = pos;

            scavengerData = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left };
            eliteData = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left };
            squadCooldown = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left };
            squadNumber = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left };

            hud.fContainers[1].AddChild(scavengerData);
            hud.fContainers[1].AddChild(eliteData);
            hud.fContainers[1].AddChild(squadCooldown);
            hud.fContainers[1].AddChild(squadNumber);

            UpdateInfo();
            Hooks.GlobalInfo.StatsUpdated += UpdateInfo;
        }

        public void UpdateInfo()
        {
            scavengerData.text = string.Concat(
                "Scavenger Counts (kills/kills max/total): ",
                Hooks.GlobalInfo.killedScavs[0],
                "/",
                Hooks.GlobalInfo.killedScavs[0] + Hooks.GlobalInfo.availableScavs[0],
                "/",
                Hooks.GlobalInfo.allScavs[0]
                );

            eliteData.text = string.Concat(
                "Elite Scavenger Counts (kills/kills max/total): ",
                Hooks.GlobalInfo.killedScavs[1],
                "/",
                Hooks.GlobalInfo.killedScavs[1] + Hooks.GlobalInfo.availableScavs[1],
                "/",
                Hooks.GlobalInfo.allScavs[1]
                );
        }

        public override void Update()
        {
            squadCooldown.text = "Squad cooldown: " + Hooks.GlobalInfo.squadCooldown.ToString() + "s";
            squadNumber.text = "Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString();

            simulatedMapPress = Mathf.Max(0, simulatedMapPress - 1);

            if (hud.owner.MapInput.mp || simulatedMapPress > 0)
            {
                mapFade = Mathf.Min(1f, mapFade + mapFadeSpeed);
            }
            else mapFade = Mathf.Max(0f, mapFade - mapFadeSpeed);
        }

        public override void Draw(float timeStacker)
        {
            Vector2 drawPos = DrawPos(timeStacker);

            scavengerData.SetPosition(drawPos - new Vector2(0f, 0f));
            eliteData.SetPosition(drawPos - new Vector2(0f, 30f));
            squadCooldown.SetPosition(drawPos - new Vector2(0f, 60f));
            squadNumber.SetPosition(drawPos - new Vector2(0f, 90f));

            float globalAlpha = mapFade;
            scavengerData.alpha = mapFade;
            eliteData.alpha = mapFade;
            squadCooldown.alpha = mapFade;
            squadNumber.alpha = mapFade;
        }

        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
        }

        public override void ClearSprites()
        {
            scavengerData.RemoveFromContainer();
            eliteData.RemoveFromContainer();
            squadCooldown.RemoveFromContainer();
            squadNumber.RemoveFromContainer();

            Hooks.GlobalInfo.StatsUpdated -= UpdateInfo;
        }
    }
}
