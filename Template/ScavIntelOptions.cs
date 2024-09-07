using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace ScavIntel
{
    public class ScavIntelOptions : OptionInterface
    {
        public readonly Configurable<KeyCode> StatsKeybind;
        public readonly Configurable<bool> ShowPointer;
        public readonly Configurable<bool> ShowKills;
        public readonly Configurable<bool> ShowKillsMax;
        public readonly Configurable<bool> ShowTotal;
        public readonly Configurable<bool> ShowSquadCooldown;
        public readonly Configurable<bool> ShowSquadCount;
        private UIelement[] optionse;

        public ScavIntelOptions() : base()
        {
            StatsKeybind = config.Bind<KeyCode>("StatsKeybind", KeyCode.I);
            ShowPointer = config.Bind<bool>("ShowPointer", true);
            ShowKills = config.Bind<bool>("ShowKills", true);
            ShowKillsMax = config.Bind<bool>("ShowKillsMax", true);
            ShowTotal = config.Bind<bool>("ShowTotal", true);
            ShowSquadCooldown = config.Bind<bool>("ShowSquadCooldown", true);
            ShowSquadCount = config.Bind<bool>("ShowSquadCount", true);
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab tab = new OpTab(this, "Config");
            Tabs = new[] { tab };

            optionse = new UIelement[]
            {
                new OpLabel(10f, 560f, "Scav Intel Config", true),
                new OpLabel(10f, 510f, "Toggle scavenger level keybind:") {alignment = FLabelAlignment.Left},
                new OpLabel(10f, 470f, "Show scavenger pointer:") {alignment = FLabelAlignment.Left, description = "Whether to show an arrow pointing to the nearest scavenger or elite scavenger"},
                new OpLabel(10f, 430f, "Show scavenger kills:") {alignment = FLabelAlignment.Left, description = "Whether to show the number of current scavenger and elite scavenger kills"},
                new OpLabel(10f, 390f, "Show max scavenger kills:") {alignment = FLabelAlignment.Left, description = "Whether to show the maximum number of possible scavenger and elite scavenger kills at any moment"},
                new OpLabel(10f, 350f, "Show total scavengers:") {alignment = FLabelAlignment.Left, description = "Whether to show the total amount of scavengers available in the current cycle"},
                new OpLabel(10f, 310f, "Show scavenger squad cooldown:") {alignment = FLabelAlignment.Left, description = "Whether to show the countdown towards a scavenger kill squad forming"},
                new OpLabel(10f, 270f, "Show scavengers in squad amount:") {alignment = FLabelAlignment.Left, description = "Whether to show the amount of scavengers and elite scavengers in all kill squads currently after the player"},

                new OpKeyBinder(StatsKeybind, new Vector2(222.5f, 505f), new Vector2(80f, 20f)),
                new OpCheckBox(ShowPointer, 225f, 467.5f) {description = "Which key to press in order to show combat levels of all scavengers"},
                new OpCheckBox(ShowKills, 225f, 427.5f) {description = "Whether to show the number of current scavenger and elite scavenger kills"},
                new OpCheckBox(ShowKillsMax, 225f, 387.5f) {description = "Whether to show the maximum number of possible scavenger and elite scavenger kills at any moment"},
                new OpCheckBox(ShowTotal, 225f, 347.5f) {description = "Whether to show the total amount of scavengers available in the current cycle"},
                new OpCheckBox(ShowSquadCooldown, 225f, 307.5f) {description = "Whether to show the countdown towards a scavenger kill squad forming"},
                new OpCheckBox(ShowSquadCount, 225f, 267.5f) {description = "Whether to show the amount of scavengers and elite scavengers in all kill squads currently after the player"}
            };
            tab.AddItems(optionse);
        }
    }
}
