using Modding;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;

namespace CollectorGlitchRemover {
    public class CollectorGlitchRemover : Mod {
        public static CollectorGlitchRemover instance;
        private string prevPrevState = "";
        private string prevState = "";
        private int delay = 11;

        public CollectorGlitchRemover() : base("Collector Glitch Remover") => instance = this;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public bool ToggleButtonInsideMenu => false;

        public override void Initialize() {
            Log("Initializing");

            On.PlayMakerFSM.Update += OnUpdate;

            Log("Initialized");
        }

        // This glitch occurs after the collector lands perfectly in a corner of the arena after a jump or grab attack.
        // It appears that extra FINISHED FSM events are sent, presumably because of entering both the Hop Wall and Hop Land states.
        // These extra FSM events cause the grab attack Lunge Launch state to be instantly progressed to the Lunge Air state,
        // where the landing event is triggered before the Collector can jump off the ground.
        // If this progression is detected, the FSM is reverted back to the launch state, and landing event is removed until collector is
        // in the air, after which the landing event is added back.
        public void OnUpdate(On.PlayMakerFSM.orig_Update orig, PlayMakerFSM self) {
            orig(self);

            if (self.FsmName == "Control" && self.gameObject.name == "Jar Collector") {
                string curState = self.ActiveStateName;

                if (prevPrevState == "Lunge Antic" && prevState == "Lunge Air" && curState == "Lunge Swipe") {
                    // Remove landing trigger
                    Modding.Logger.Log("Preventing collector insta-grab glitch");
                    self.RemoveFsmTransition("Lunge Air", "LAND");
                    delay = 0;
                    prevPrevState = "";
                    prevState = "";
                    self.SetState("Lunge Launch");
                } else if (delay < 10) {
                    // Wait for launch to occur
                    delay++;
                } else if (delay == 10) {
                    // After launch, add back landing trigger
                    self.AddFsmTransition("Lunge Air", "LAND", "Lunge Swipe");
                    delay++;
                } else {
                    prevPrevState = prevState;
                    prevState = curState;
                }
            }
        }
    }
}
