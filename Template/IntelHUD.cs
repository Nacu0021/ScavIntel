using UnityEngine;
using RWCustom;
using HUD;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScavIntel
{
    public class IntelHUD : HudPart
    {
        public Vector2 pos;
        public Vector2 lastPos;

        public float mapFade;

        public int simulatedMapPress;
        public int cycleStartCounter;

        public FLabel[] data;
        public FLabel[] dataShadows;
        public float[] dataFades;
        public int[] dataShows;
        public int[] dataAnimations;
        public bool[] dataOnLast;

        public const int animationStart = 40;
        public const int animationLength = 8;
        public const float fadeSpeed = 0.2f;

        public FSprite[] scavPointerLines;
        public Vector2 shouldPointAt;
        public Vector2 pointAt;
        public Vector2 lastPointAt;
        public float pointerFade;
        public bool showPointer;
        public float pointerRotation;
        public float lastPointerRotation;
        public float pointer3DWidth = 3f;
        public float pointerWidth = 6f;
        public float pointerRotationAdd;
        public Color goalPointerColor;

        public int lastScavInRoomCount;
        public List<ScavStatInfo> scavStats;

        public IntelHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            lastPos = pos;

            data = new FLabel[4];
            dataShadows = new FLabel[4];
            dataFades = new float[4];
            dataShows = new int[4];
            dataAnimations = new int[4];
            dataOnLast = new bool[4];

            for (int i = 0; i < 4; i++)
            {
                data[i] = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left };
                dataShadows[i] = new FLabel(Custom.GetDisplayFont(), "") { alignment = FLabelAlignment.Left, color = new Color(0.01f, 0.01f, 0.01f) };
                hud.fContainers[1].AddChild(dataShadows[i]);
                hud.fContainers[1].AddChild(data[i]);
            }

            UpdateInfo();

            //scavPointer = new FSprite[2];
            //for (int i = 0; i < 2; i++)
            //{
            //    scavPointer[i] = new FSprite("keyArrowA") { shader = Custom.rainWorld.Shaders["Hologram"], color = RainWorld.SaturatedGold };
            //    hud.fContainers[1].AddChild(scavPointer[i]);
            //}
            scavPointerLines = new FSprite[9];
            for (int i = 0; i < 9; i++)
            {
                scavPointerLines[i] = new FSprite("pixel") { shader = Custom.rainWorld.Shaders["Hologram"], color = Color.white }; //3d cursor color
                hud.fContainers[1].AddChild(scavPointerLines[i]);
            }

            pointerRotationAdd = 1f;
            goalPointerColor = Color.white;

            scavStats = [];
        }

        public void UpdateInfo()
        {
            bool twoActive = (Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowTotal.Value) ||
                             (Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowKillsMax.Value) ||
                             (Plugin.optiones.ShowTotal.Value && Plugin.optiones.ShowKillsMax.Value);
            bool allActive = Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowTotal.Value && Plugin.optiones.ShowKillsMax.Value;

            data[0].text = string.Concat(
                "Scavenger Counts (",
                Plugin.optiones.ShowKills.Value ? "kills" : "",
                twoActive ? "/" : "",
                Plugin.optiones.ShowKillsMax.Value ? "kills max" : "",
                allActive ? "/" : "",
                Plugin.optiones.ShowTotal.Value ? "total" : "",
                "): ",
                Plugin.optiones.ShowKills.Value ? Hooks.GlobalInfo.killedScavs[0] : "",
                twoActive ? "/" : "",
                Plugin.optiones.ShowKillsMax.Value ? Hooks.GlobalInfo.availableScavs[0] + Hooks.GlobalInfo.killedScavs[0] : "",
                allActive ? "/" : "",
                Plugin.optiones.ShowTotal.Value ? Hooks.GlobalInfo.allScavs[0] : ""
                );

            data[1].text = string.Concat(
                "Elite Scavenger Counts (",
                Plugin.optiones.ShowKills.Value ? "kills" : "",
                twoActive ? "/" : "",
                Plugin.optiones.ShowKillsMax.Value ? "kills max" : "",
                allActive ? "/" : "",
                Plugin.optiones.ShowTotal.Value ? "total" : "",
                "): ",
                Plugin.optiones.ShowKills.Value ? Hooks.GlobalInfo.killedScavs[1] : "",
                twoActive ? "/" : "",
                Plugin.optiones.ShowKillsMax.Value ? Hooks.GlobalInfo.availableScavs[1] + Hooks.GlobalInfo.killedScavs[1] : "",
                allActive ? "/" : "",
                Plugin.optiones.ShowTotal.Value ? Hooks.GlobalInfo.allScavs[1] : ""
                );
            data[3].text = Plugin.optiones.ShowSquadCount.Value ? ("Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString()) : "";
        }

        public int ScoreOfPointScav(Player player, Scavenger scav)
        {
            int score = Custom.ManhattanDistance(player.abstractCreature.pos, scav.abstractCreature.pos);

            if (scav.room == null) return score;

            if (!scav.room.VisualContact(player.abstractCreature.pos, scav.abstractCreature.pos)) score *= 3;
            score += (int)(20f * (1f - Mathf.Abs(Vector2.Dot(Custom.DirVec(scav.room.MiddleOfTile(player.abstractCreature.pos), scav.room.MiddleOfTile(scav.abstractCreature.pos)), Vector2.right))));

            return score;
        }

        public override void Update()
        {
            //Player player = null;
            //if (hud.owner is Player p && p.room != null && !p.dead)
            //{
            //    p = player;
            //}

            #region ScavengerData
            data[2].text = "Squad cooldown: " + Hooks.GlobalInfo.squadCooldown.ToString() + "s";

            if (cycleStartCounter > 0)
            {
                cycleStartCounter--;
                if (cycleStartCounter == 0)
                {
                    simulatedMapPress = 200;
                }
            }

            simulatedMapPress = Mathf.Max(0, simulatedMapPress - 1);

            for (int i = 0; i < 4; i++)
            {
                dataShows[i] = Mathf.Max(0, dataShows[i] - 1);
            }

            if (hud.owner.MapInput.mp || simulatedMapPress > 0)
            {
                mapFade = Mathf.Min(1f, mapFade + fadeSpeed);
            }
            else mapFade = Mathf.Max(0f, mapFade - fadeSpeed);

            for (int i = 0; i < 4; i++)
            {
                if (dataShows[i] > 0)
                {
                    dataFades[i] = Mathf.Min(1f, dataFades[i] + fadeSpeed);
                    if (dataFades[i] == 1f && i != 2 && dataOnLast[i])
                    {
                        dataAnimations[i] = Mathf.Min(animationStart + animationLength, dataAnimations[i] + 1);

                        float colorOverrideX = Mathf.InverseLerp(animationStart, animationStart + animationLength, dataAnimations[i]);
                        float colorOverrideFac = -Mathf.Abs(Mathf.Pow(colorOverrideX * 2f - 1f, 2f)) + 1f;
                        data[i].color = Color.Lerp(Color.white, RainWorld.SaturatedGold, colorOverrideFac * 0.9f); // Change flash color if needed

                        if (dataAnimations[i] > animationStart)
                        {
                            bool twoActive = (Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowTotal.Value) ||
                                             (Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowKillsMax.Value) ||
                                             (Plugin.optiones.ShowTotal.Value && Plugin.optiones.ShowKillsMax.Value);
                            bool allActive = Plugin.optiones.ShowKills.Value && Plugin.optiones.ShowTotal.Value && Plugin.optiones.ShowKillsMax.Value;
                            switch (i)
                            {
                                case 0:
                                    data[i].text = string.Concat(
                                        "Scavenger Counts (",
                                        Plugin.optiones.ShowKills.Value ? "kills" : "",
                                        twoActive ? "/" : "",
                                        Plugin.optiones.ShowKillsMax.Value ? "kills max" : "",
                                        allActive ? "/" : "",
                                        Plugin.optiones.ShowTotal.Value ? "total" : "",
                                        "): ",
                                        Plugin.optiones.ShowKills.Value ? Hooks.GlobalInfo.killedScavs[0] : "",
                                        twoActive ? "/" : "",
                                        Plugin.optiones.ShowKillsMax.Value ? Hooks.GlobalInfo.availableScavs[0] + Hooks.GlobalInfo.killedScavs[0] : "",
                                        allActive ? "/" : "",
                                        Plugin.optiones.ShowTotal.Value ? Hooks.GlobalInfo.allScavs[0] : ""
                                        );
                                    break;
                                case 1:
                                    data[i].text = string.Concat(
                                        "Elite Scavenger Counts (",
                                        Plugin.optiones.ShowKills.Value ? "kills" : "",
                                        twoActive ? "/" : "",
                                        Plugin.optiones.ShowKillsMax.Value ? "kills max" : "",
                                        allActive ? "/" : "",
                                        Plugin.optiones.ShowTotal.Value ? "total" : "",
                                        "): ",
                                        Plugin.optiones.ShowKills.Value ? Hooks.GlobalInfo.killedScavs[1] : "",
                                        twoActive ? "/" : "",
                                        Plugin.optiones.ShowKillsMax.Value ? Hooks.GlobalInfo.availableScavs[1] + Hooks.GlobalInfo.killedScavs[1] : "",
                                        allActive ? "/" : "",
                                        Plugin.optiones.ShowTotal.Value ? Hooks.GlobalInfo.allScavs[1] : ""
                                        );
                                    break;
                                case 3:
                                    data[i].text = Plugin.optiones.ShowSquadCount.Value ? ("Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString()) : "";
                                    break;
                            }
                        }

                        if (dataAnimations[i] > animationLength + animationStart) dataOnLast[i] = false;
                    }
                }
                else
                {
                    dataFades[i] = Mathf.Max(0f, dataFades[i] - fadeSpeed);
                    dataAnimations[i] = 0;
                }
                dataShadows[i].text = data[i].text;
            }
            #endregion

            #region ScavengerPointerAndAlsoStats

            showPointer = false;
            if (hud.owner is Player player)
            {
                Room room = player.abstractCreature.world.game.cameras[0].room;
                if (room != null)
                {
                    Scavenger pointCreature = null;
                    List<Scavenger> stattedScavs = [];
                    if (Plugin.ShowStats || Plugin.optiones.ShowPointer.Value)
                    {
                        foreach (var crit in room.abstractRoom.creatures)
                        {
                            if (!crit.state.dead &&
                                (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) &&
                                crit.realizedCreature != null && !crit.realizedCreature.inShortcut)
                            {
                                if (Plugin.optiones.ShowPointer.Value)
                                {
                                    if (pointCreature == null || ScoreOfPointScav(player, crit.realizedCreature as Scavenger) < ScoreOfPointScav(player, pointCreature))
                                    {
                                        pointCreature = crit.realizedCreature as Scavenger;
                                    }
                                }

                                if (Plugin.ShowStats) stattedScavs.Add(crit.realizedCreature as Scavenger);
                            }
                        }
                    }

                    if (pointCreature != null)
                    {
                        shouldPointAt = Custom.DirVec(player.mainBodyChunk.pos, pointCreature.mainBodyChunk.pos);
                    }
                    showPointer = pointCreature != null && !player.inShortcut;

                    if (stattedScavs.Count > 0)
                    {
                        foreach (Scavenger scag in stattedScavs)
                        {
                            if (!scavStats.Any(x => x.followScav == scag))
                            {
                                ScavStatInfo info = new ScavStatInfo(scag, room.abstractRoom.index);
                                scavStats.Add(info);
                                hud.fContainers[1].AddChild(info.statShadow);
                                hud.fContainers[1].AddChild(info.stats);
                                hud.fContainers[1].AddChild(info.statsArrow);
                            }
                        }
                    }

                    foreach (var stat in scavStats)
                    {
                        if (!Plugin.ShowStats || room.abstractRoom.index != stat.origRoom)
                        {
                            stat.requestRemove = true;
                            stat.deathFade = 0f;
                        };

                        stat.Update();
                        if (stat.followScav != null && pointCreature != null && stat.followScav == pointCreature)
                        {
                            goalPointerColor = GetScavLevelColor(GetScavLevel(pointCreature));
                        }

                        if (stat.requestRemove && stat.deathFade == 0f) stat.RemoveSprites();
                    }
                    scavStats.RemoveAll(x => x.requestRemove == true && x.deathFade == 0f);
                }
            }
            lastPointAt = pointAt;
            pointAt = Vector3.Slerp(pointAt, shouldPointAt, 0.3f);

            if (showPointer)
            {
                pointerFade = Mathf.Min(1f, pointerFade + fadeSpeed * 0.1f);
            }
            else pointerFade = Mathf.Max(0f, pointerFade - fadeSpeed * 0.1f);

            lastPointerRotation = pointerRotation;
            pointerRotation += 1f / Mathf.Lerp(120f, 40f, UnityEngine.Random.value) * pointerRotationAdd;//Mathf.Lerp(-0.005f, 0.005f, UnityEngine.Random.value);
            if (pointerRotation > 1f)
            {
                pointerRotationAdd = -1f;
            }
            else if (pointerRotation < -1f)
            {
                pointerRotationAdd = 1f;
            }
            #endregion
        }

        public override void Draw(float timeStacker)
        {
            Vector2 drawPos = DrawPos(timeStacker);

            for (int i = 0; i < 4; i++)
            {
                data[i].SetPosition(drawPos + new Vector2(-5f, 0f - 30f * i));
                data[i].alpha = Mathf.Max(mapFade, dataFades[i]);
                dataShadows[i].SetPosition(data[i].GetPosition() + new Vector2(2f, -2f));
                dataShadows[i].alpha = Mathf.Max(mapFade, dataFades[i]);
            }

            data[2].isVisible = Plugin.optiones.ShowSquadCooldown.Value;
            dataShadows[2].isVisible = Plugin.optiones.ShowSquadCooldown.Value;
            data[3].isVisible = Plugin.optiones.ShowSquadCount.Value;
            dataShadows[3].isVisible = Plugin.optiones.ShowSquadCount.Value;

            // This is literally the worst thing ive ever written but it looks cool and works so idc
            if (hud.owner is Player player)
            {
                Room room = player.abstractCreature.world.game.cameras[0].room;
                if (room != null)
                {
                    float rotationFac = Mathf.Lerp(lastPointerRotation, pointerRotation, timeStacker);//Mathf.Sin(Mathf.Lerp(lastPointerRotation, pointerRotation, timeStacker) * Mathf.PI / 2f);
                    float rotationSign = Mathf.Sign(rotationFac);
                    Vector2 pointerDir = Vector3.Slerp(lastPointAt, pointAt, timeStacker);
                    Vector2 playerPos = Vector2.Lerp(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, timeStacker);
                    Vector2 perp = Custom.PerpendicularVector(pointerDir);
                    Vector2 backPoint = playerPos + pointerDir * 20f;
                    Vector2 frontPoint = playerPos + pointerDir * 34f;
                    Vector2 rightPoint = backPoint + perp * pointerWidth * (1f * rotationSign - rotationFac);
                    Vector2 leftPoint = backPoint - perp * pointerWidth * (1f * rotationSign - rotationFac);
                    Vector2 rightFac = perp * pointer3DWidth * rotationFac;
                    float gruh = 14f;
                    float grug = Mathf.Sqrt(gruh * gruh + pointerWidth * pointerWidth);

                    scavPointerLines[3].SetPosition(backPoint + rightFac - room.game.cameras[0].pos);
                    scavPointerLines[3].scaleX = pointerWidth * 2f * (1f * rotationSign - rotationFac);
                    scavPointerLines[3].rotation = Custom.VecToDeg(pointerDir);

                    scavPointerLines[4].SetPosition(Vector2.Lerp(rightPoint + rightFac, frontPoint + rightFac * 0.5f, 0.5f) - room.game.cameras[0].pos);
                    scavPointerLines[4].scaleY = grug;
                    scavPointerLines[4].rotation = Custom.VecToDeg(Custom.DirVec(rightPoint + rightFac, frontPoint + rightFac * 0.5f));

                    scavPointerLines[5].SetPosition(Vector2.Lerp(leftPoint + rightFac, frontPoint + rightFac * 0.5f, 0.5f) - room.game.cameras[0].pos);
                    scavPointerLines[5].scaleY = grug;
                    scavPointerLines[5].rotation = Custom.VecToDeg(Custom.DirVec(leftPoint + rightFac, frontPoint + rightFac * 0.5f));

                    scavPointerLines[6].SetPosition(backPoint - rightFac - room.game.cameras[0].pos);
                    scavPointerLines[6].scaleX = pointerWidth * 2f * (1f * rotationSign - rotationFac);
                    scavPointerLines[6].rotation = Custom.VecToDeg(pointerDir);

                    scavPointerLines[7].SetPosition(Vector2.Lerp(rightPoint - rightFac, frontPoint - rightFac * 0.5f, 0.5f) - room.game.cameras[0].pos);
                    scavPointerLines[7].scaleY = grug;
                    scavPointerLines[7].rotation = Custom.VecToDeg(Custom.DirVec(rightPoint - rightFac, frontPoint - rightFac * 0.5f));

                    scavPointerLines[8].SetPosition(Vector2.Lerp(leftPoint - rightFac, frontPoint - rightFac * 0.5f, 0.5f) - room.game.cameras[0].pos);
                    scavPointerLines[8].scaleY = grug;
                    scavPointerLines[8].rotation = Custom.VecToDeg(Custom.DirVec(leftPoint - rightFac, frontPoint - rightFac * 0.5f));

                    for (int i = 0; i < 2; i++)
                    {
                        scavPointerLines[i + 1].SetPosition(backPoint + perp * pointer3DWidth * (i == 0 ? 1 : -1) * (1f * rotationSign - rotationFac) - room.game.cameras[0].pos);
                        scavPointerLines[i + 1].scaleX = pointer3DWidth * 2f * rotationFac;
                        scavPointerLines[i + 1].rotation = Custom.VecToDeg(pointerDir);
                    }
                    scavPointerLines[0].SetPosition(frontPoint - room.game.cameras[0].pos);
                    scavPointerLines[0].scaleX = pointer3DWidth * rotationFac;
                    scavPointerLines[0].rotation = Custom.VecToDeg(pointerDir);

                    foreach (var stat in scavStats)
                    {
                        stat.Draw(timeStacker, room.game.cameras[0].pos);
                    }
                }
            }
            for (int i = 0; i < 9; i++)
            {
                scavPointerLines[i].alpha = pointerFade;
                scavPointerLines[i].isVisible = scavPointerLines[i].alpha > 0f;
                scavPointerLines[i].color = Color.Lerp(scavPointerLines[i].color, goalPointerColor, 0.2f);
            }
        }

        public Vector2 DrawPos(float timeStacker)
        {
            Vector2 add = Vector2.zero;
            if (hud.parts.Any(x => x is ExpeditionHUD))
            {
                add += new Vector2(0f, -160f);
            }
            if (hud.owner.GetOwnerType() != HUD.HUD.OwnerType.Player)
            {
                add += new Vector2(0f, -40f);
            }
            return Vector2.Lerp(lastPos, pos, timeStacker) + add;
        }

        public override void ClearSprites()
        {
            for (int i = 0; i < 4; i++)
            {
                data[i].RemoveFromContainer();
                dataShadows[i].RemoveFromContainer();
            }
            //scavPointer[0].RemoveFromContainer();
            //scavPointer[1].RemoveFromContainer();
            for (int i = 0; i < 9; i++)
            {
                scavPointerLines[i].RemoveFromContainer();
            }

            foreach (var stat in scavStats)
            {
                stat.RemoveSprites();
            }
        }

        public static int GetScavLevel(Scavenger scav)
        {
            return Mathf.RoundToInt(100f * (scav.blockingSkill * 0.15f + scav.dodgeSkill * 0.15f + scav.meleeSkill * 0.15f + scav.midRangeSkill * 0.15f + scav.reactionSkill * 0.4f));
        }

        public static Color GetScavLevelColor(int level)
        {
            float blueFac = Mathf.InverseLerp(50, 0, level);
            float greenFac = Mathf.InverseLerp(80, 50, level);
            return new Color(1f, greenFac, blueFac);
        }

        public class ScavStatInfo
        {
            public Scavenger followScav;
            public int origRoom;
            public FLabel stats;
            public FLabel statShadow;
            public FSprite statsArrow;
            public bool requestRemove;
            public bool initialized;
            public int scavLevel;
            public Vector2 lastScavPos;
            public Vector2 scavPos;
            public float deathFade;
            public Color color;

            public ScavStatInfo(Scavenger followScav, int origRoom)
            {
                this.followScav = followScav;
                this.origRoom = origRoom;

                initialized = false;

                stats = new FLabel(Custom.GetFont(), "");
                statShadow = new FLabel(Custom.GetFont(), "") { color = new Color(0.01f, 0.01f, 0.01f) };
                statsArrow = new FSprite("keyArrowA") { shader = Custom.rainWorld.Shaders["Hologram"], alpha = 0.9f, rotation = 180f, scaleY = 0.8f };

                scavPos = followScav.mainBodyChunk.pos + new Vector2(0f, 50f);
                lastScavPos = scavPos;
            }

            public void Update()
            {
                if (requestRemove)
                {
                    deathFade = Mathf.Max(0f, deathFade - 0.09f);
                }
                if (followScav == null || followScav.room == null || followScav.dead || origRoom == -1 || followScav.room.abstractRoom.index != origRoom)
                {
                    requestRemove = true;
                    return;
                }

                if (!initialized)
                {
                    Initialize();
                }

                lastScavPos = scavPos;
                scavPos = followScav.mainBodyChunk.pos + new Vector2(0f, 60f);
            }

            public void Draw(float timeStacker, Vector2 camPos)
            {
                stats.SetPosition(Vector2.Lerp(lastScavPos, scavPos, timeStacker) - camPos);
                statShadow.SetPosition(stats.GetPosition() + new Vector2(2f, -2f));
                statsArrow.SetPosition(stats.GetPosition() - new Vector2(0f, 15f));

                stats.alpha = deathFade;
                statShadow.alpha = deathFade;
                statsArrow.alpha = deathFade;
            }

            public void Initialize()
            {
                scavLevel = GetScavLevel(followScav);
                color = GetScavLevelColor(scavLevel);

                if (followScav.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) scavLevel += 20;
                stats.text = "lvl. " + scavLevel.ToString();
                statShadow.text = stats.text;

                deathFade = 1f;

                stats.color = color;
                statsArrow.color = color;

                initialized = true;
            }

            public void RemoveSprites()
            {
                stats?.RemoveFromContainer();
                statShadow?.RemoveFromContainer();
                statsArrow?.RemoveFromContainer();
            }
        }
    }
}
