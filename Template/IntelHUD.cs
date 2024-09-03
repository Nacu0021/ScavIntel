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
        public FLabel squadCount;

        public Vector2 pos;
        public Vector2 lastPos;

        // I love UI coding (CHANGE TO ARRAYS LATER)
        public float mapFade;
        public float cooldownFade;
        public float squadCountFade;
        public float scavengerDataFade;
        public float eliteDataFade;

        public int simulatedMapPress;
        public int cycleStartCounter;
        public int showCooldown;
        public int showSquadCount;
        public int showScavengerData;
        public int showEliteData;

        public int scavengerDataAnimation;
        public bool scavengerDataOnLast;
        public bool scavengerDataFromKill;

        public const int animationStart = 30;
        public const int animationLength = 6;
        public const float fadeSpeed = 0.2f;

        public IntelHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            lastPos = pos;

            scavengerData = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left, color = new Color(0.8f, 0.8f, 0.8f) };
            eliteData = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left, color = new Color(0.8f, 0.8f, 0.8f) };
            squadCooldown = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left, color = new Color(0.8f, 0.8f, 0.8f) };
            squadCount = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left, color = new Color(0.8f, 0.8f, 0.8f) };

            hud.fContainers[1].AddChild(scavengerData);
            hud.fContainers[1].AddChild(eliteData);
            hud.fContainers[1].AddChild(squadCooldown);
            hud.fContainers[1].AddChild(squadCount);

            UpdateInfo();
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
            squadCount.text = "Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString();

            if (cycleStartCounter > 0)
            {
                cycleStartCounter--;
                if (cycleStartCounter == 0)
                {
                    simulatedMapPress = 200;
                }
            }

            simulatedMapPress = Mathf.Max(0, simulatedMapPress - 1);
            showScavengerData = Mathf.Max(0, showScavengerData - 1);
            showEliteData = Mathf.Max(0, showEliteData - 1);
            showCooldown = Mathf.Max(0, showCooldown - 1);
            showSquadCount = Mathf.Max(0, showSquadCount - 1);

            if (hud.owner.MapInput.mp || simulatedMapPress > 0)
            {
                mapFade = Mathf.Min(1f, mapFade + fadeSpeed);
            }
            else mapFade = Mathf.Max(0f, mapFade - fadeSpeed);

            if (showScavengerData > 0)
            {
                scavengerDataFade = Mathf.Min(1f, scavengerDataFade + fadeSpeed);
                if (scavengerDataFade == 1f)
                {
                    scavengerDataAnimation = Mathf.Min(animationStart + animationLength, scavengerDataAnimation + 1);
                
                    float colorOverrideX = Mathf.InverseLerp(animationStart, animationStart + animationLength, scavengerDataAnimation);
                    float colorOverrideFac = -Mathf.Abs(Mathf.Pow(colorOverrideX * 2f - 1f, 2f)) + 1f;
                    scavengerData.color = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), Color.Lerp(Color.white, Color.red, 0.5f), colorOverrideFac);
                
                    if (scavengerDataAnimation > animationStart && scavengerDataOnLast)
                    {
                        scavengerData.text = string.Concat(
                            "Scavenger Counts (kills/kills max/total): ",
                            Hooks.GlobalInfo.killedScavs[0],
                            "/",
                            Hooks.GlobalInfo.killedScavs[0] + Hooks.GlobalInfo.availableScavs[0],
                            "/",
                            Hooks.GlobalInfo.allScavs[0]
                            );
                
                        // Play sound
                
                        scavengerDataOnLast = false;
                    }
                }
            }
            else
            {
                scavengerDataFade = Mathf.Max(0f, scavengerDataFade - fadeSpeed);
                scavengerDataAnimation = 0;
            }

            if (showEliteData > 0)
            {
                eliteDataFade = Mathf.Min(1f, eliteDataFade + fadeSpeed);
            }
            else eliteDataFade = Mathf.Max(0f, eliteDataFade - fadeSpeed);

            if (showCooldown > 0)
            {
                cooldownFade = Mathf.Min(1f, cooldownFade + fadeSpeed);
            }
            else cooldownFade = Mathf.Max(0f, cooldownFade - fadeSpeed);

            if (showSquadCount > 0)
            {
                squadCountFade = Mathf.Min(1f, squadCountFade + fadeSpeed);
            }
            else squadCountFade = Mathf.Max(0f, squadCountFade - fadeSpeed);
        }

        public override void Draw(float timeStacker)
        {
            Vector2 drawPos = DrawPos(timeStacker);

            scavengerData.SetPosition(drawPos - new Vector2(0f, 0f));
            eliteData.SetPosition(drawPos - new Vector2(0f, 30f));
            squadCooldown.SetPosition(drawPos - new Vector2(0f, 60f));
            squadCount.SetPosition(drawPos - new Vector2(0f, 90f));

            scavengerData.alpha = Mathf.Max(mapFade, scavengerDataFade);
            eliteData.alpha = Mathf.Max(mapFade, eliteDataFade);
            squadCooldown.alpha = Mathf.Max(mapFade, cooldownFade);
            squadCount.alpha = Mathf.Max(mapFade, squadCountFade);
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
            squadCount.RemoveFromContainer();

            Hooks.GlobalInfo.StatsUpdated -= UpdateInfo;
        }
    }
}
