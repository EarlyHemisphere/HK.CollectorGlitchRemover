using Modding;
using SFCore.Utils;

namespace CollectorGlitchRemover {
    public class CollectorGlitchRemover : Mod {
        public static CollectorGlitchRemover instance;

        public CollectorGlitchRemover() : base("Collector Glitch Remover") => instance = this;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public bool ToggleButtonInsideMenu => false;

        public override void Initialize() {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnEnable;

            Log("Initialized");
        }

        public void OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);

            if (self.gameObject.name == "Jar Collector" && self.FsmName == "Control") {
                Modding.Logger.Log("Removing collector glitch");
                self.RemoveAction("Hop Wall", 6);
            }
        }
    }
}
