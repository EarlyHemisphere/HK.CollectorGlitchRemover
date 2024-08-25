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
