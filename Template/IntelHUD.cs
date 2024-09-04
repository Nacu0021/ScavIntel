using UnityEngine;
using RWCustom;
using HUD;
using MoreSlugcats;

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

        public const int animationStart = 36;
        public const int animationLength = 8;
        public const float fadeSpeed = 0.2f;

        //public FSprite[] scavPointer;
        public FSprite[] scavPointerLines;
        public Vector2 shouldPointAt;
        public Vector2 pointAt;
        public Vector2 lastPointAt;
        public float pointerFade;
        public bool showPointer;
        public float pointerRotation;
        public float lastPointerRotation;
        public float pointerRotationAdd;
        public float pointer3DWidth = 2f;
        public float pointerWidth = 6f;

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
                scavPointerLines[i] = new FSprite("pixel") { shader = Custom.rainWorld.Shaders["Hologram"], color = RainWorld.SaturatedGold };
                hud.fContainers[1].AddChild(scavPointerLines[i]);
            }

            pointerRotationAdd = 0.025f;
        }

        public void UpdateInfo()
        {
            data[0].text = string.Concat(
                "Scavenger Counts (kills/kills max/total): ",
                Hooks.GlobalInfo.killedScavs[0],
                "/",
                Hooks.GlobalInfo.killedScavs[0] + Hooks.GlobalInfo.availableScavs[0],
                "/",
                Hooks.GlobalInfo.allScavs[0]
                );

            data[1].text = string.Concat(
                "Elite Scavenger Counts (kills/kills max/total): ",
                Hooks.GlobalInfo.killedScavs[1],
                "/",
                Hooks.GlobalInfo.killedScavs[1] + Hooks.GlobalInfo.availableScavs[1],
                "/",
                Hooks.GlobalInfo.allScavs[1]
                );
            data[3].text = "Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString();
        }

        public override void Update()
        {
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
                        data[i].color = Color.Lerp(Color.white, RainWorld.SaturatedGold, colorOverrideFac * 0.75f);

                        if (dataAnimations[i] > animationStart)
                        {
                            switch (i)
                            {
                                case 0:
                                    data[i].text = string.Concat(
                                        "Scavenger Counts (kills/kills max/total): ",
                                        Hooks.GlobalInfo.killedScavs[0],
                                        "/",
                                        Hooks.GlobalInfo.killedScavs[0] + Hooks.GlobalInfo.availableScavs[0],
                                        "/",
                                        Hooks.GlobalInfo.allScavs[0]
                                        );
                                    break;
                                case 1:
                                    data[i].text = string.Concat(
                                        "Elite Scavenger Counts (kills/kills max/total): ",
                                        Hooks.GlobalInfo.killedScavs[1],
                                        "/",
                                        Hooks.GlobalInfo.killedScavs[1] + Hooks.GlobalInfo.availableScavs[1],
                                        "/",
                                        Hooks.GlobalInfo.allScavs[1]
                                        );
                                    break;
                                case 3:
                                    data[i].text = "Squad count: " + Hooks.GlobalInfo.squadScavsCount.ToString();
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

            #region ScavengerPointer
            showPointer = false;
            if (hud.owner is Player player && player.room != null && !player.dead)
            {
                AbstractCreature pointCreature = null;
                foreach (var crit in player.room.abstractRoom.creatures)
                {
                    if (!crit.state.dead && 
                        (crit.creatureTemplate.type == CreatureTemplate.Type.Scavenger || crit.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite) &&
                        crit.realizedCreature != null)
                    {
                        if (pointCreature == null || Custom.ManhattanDistance(player.abstractCreature.pos, crit.pos) < Custom.ManhattanDistance(player.abstractCreature.pos, pointCreature.pos))
                        {
                            pointCreature = crit;
                        }
                    }
                }
                if (pointCreature != null)
                {
                    shouldPointAt = Custom.DirVec(player.mainBodyChunk.pos, pointCreature.realizedCreature.mainBodyChunk.pos);
                }
                showPointer = pointCreature != null && !player.inShortcut;
            }
            lastPointAt = pointAt;
            pointAt = Vector3.Slerp(pointAt, shouldPointAt, 0.3f);

            if (showPointer)
            {
                pointerFade = Mathf.Min(1f, pointerFade + fadeSpeed * 0.1f);
            }
            else pointerFade = Mathf.Max(0f, pointerFade - fadeSpeed * 0.1f);

            lastPointerRotation = pointerRotation;
            pointerRotation += pointerRotationAdd + Mathf.Lerp(-0.005f, 0.005f, UnityEngine.Random.value);
            if (pointerRotation > 1f)
            {
                pointerRotation -= 1f;
                lastPointerRotation -= 1f;
            }
            #endregion
        }

        public override void Draw(float timeStacker)
        {
            Vector2 drawPos = DrawPos(timeStacker);

            for (int i = 0; i < 4; i++)
            {
                data[i].SetPosition(drawPos + new Vector2(0f, 0f - 30f * i));
                data[i].alpha = Mathf.Max(mapFade, dataFades[i]);
                dataShadows[i].SetPosition(drawPos + new Vector2(2f, -2f - 30f * i));
                dataShadows[i].alpha = Mathf.Max(mapFade, dataFades[i]);
            }

            // This is literally the worst thing ive ever written but it looks cool and works so idc
            if (hud.owner is Player player && player.room != null)
            {
                float rotationFac = Mathf.Sin(Mathf.Lerp(lastPointerRotation, pointerRotation, timeStacker) * Mathf.PI);
                Vector2 pointerDir = Vector3.Slerp(lastPointAt, pointAt, timeStacker);
                Vector2 playerPos = Vector2.Lerp(player.mainBodyChunk.lastPos, player.mainBodyChunk.pos, timeStacker);
                Vector2 perp = Custom.PerpendicularVector(pointerDir);
                Vector2 backPoint = playerPos + pointerDir * 29f;
                Vector2 frontPoint = playerPos + pointerDir * 43f;
                Vector2 rightPoint = backPoint + perp * pointerWidth * (1f - rotationFac);
                Vector2 leftPoint = backPoint - perp * pointerWidth * (1f - rotationFac);
                Vector2 rightFac = perp * pointer3DWidth * rotationFac;
                float gruh = 43f - 29f;
                float grug = Mathf.Sqrt(gruh * gruh + pointerWidth * pointerWidth);

                scavPointerLines[3].SetPosition(backPoint + rightFac - player.room.game.cameras[0].pos);
                scavPointerLines[3].scaleX = pointerWidth * 2f * (1f - rotationFac);
                scavPointerLines[3].rotation = Custom.VecToDeg(pointerDir);
                
                scavPointerLines[4].SetPosition(Vector2.Lerp(rightPoint + rightFac, frontPoint + rightFac, 0.5f) - player.room.game.cameras[0].pos);
                scavPointerLines[4].scaleY = grug;
                scavPointerLines[4].rotation = Custom.VecToDeg(Custom.DirVec(rightPoint + rightFac, frontPoint + rightFac));

                scavPointerLines[5].SetPosition(Vector2.Lerp(leftPoint + rightFac, frontPoint + rightFac, 0.5f) - player.room.game.cameras[0].pos);
                scavPointerLines[5].scaleY = grug;
                scavPointerLines[5].rotation = Custom.VecToDeg(Custom.DirVec(leftPoint + rightFac, frontPoint + rightFac));

                scavPointerLines[6].SetPosition(backPoint - rightFac - player.room.game.cameras[0].pos);
                scavPointerLines[6].scaleX = pointerWidth * 2f * (1f - rotationFac);
                scavPointerLines[6].rotation = Custom.VecToDeg(pointerDir);

                scavPointerLines[7].SetPosition(Vector2.Lerp(rightPoint - rightFac, frontPoint - rightFac, 0.5f) - player.room.game.cameras[0].pos);
                scavPointerLines[7].scaleY = grug;
                scavPointerLines[7].rotation = Custom.VecToDeg(Custom.DirVec(rightPoint - rightFac, frontPoint - rightFac));

                scavPointerLines[8].SetPosition(Vector2.Lerp(leftPoint - rightFac, frontPoint - rightFac, 0.5f) - player.room.game.cameras[0].pos);
                scavPointerLines[8].scaleY = grug;
                scavPointerLines[8].rotation = Custom.VecToDeg(Custom.DirVec(leftPoint - rightFac, frontPoint - rightFac));

                for (int i = 0; i < 2; i++)
                {
                    scavPointerLines[i + 1].SetPosition(backPoint + perp * pointer3DWidth * (i == 0 ? 1 : -1) * (1f - rotationFac) - player.room.game.cameras[0].pos);
                    scavPointerLines[i + 1].scaleX = pointer3DWidth * 2f * rotationFac;
                    scavPointerLines[i + 1].rotation = Custom.VecToDeg(pointerDir);
                }
                scavPointerLines[0].SetPosition(frontPoint - player.room.game.cameras[0].pos);
                scavPointerLines[0].scaleX = pointer3DWidth * 2f * rotationFac;
                scavPointerLines[0].rotation = Custom.VecToDeg(pointerDir);
            }
            for (int i = 0; i < 9; i++)
            {
                scavPointerLines[i].alpha = pointerFade;
                scavPointerLines[i].isVisible = scavPointerLines[i].alpha > 0f;
            }
        }

        public Vector2 DrawPos(float timeStacker)
        {
            return Vector2.Lerp(lastPos, pos, timeStacker);
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

            Hooks.GlobalInfo.StatsUpdated -= UpdateInfo;
        }
    }
}
