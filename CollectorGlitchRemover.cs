using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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

            On.HutongGames.PlayMaker.Actions.CheckCollisionSide.OnExit += OnCheckCollisionSideExit;

            Log("Initialized");
        }

        /*
        This mod fixes two glitches.

        Glitch 1: "Insta-grab" - Collector doesn't jump before doing the grab attack, instead instantly performing the grab.
        Always happens after the Collector has performed a normal grab attack that lands perfectly in the corner of the arena.

        Glitch 2: "Sticky feet" - Collector doesn't jump when about to hop, instead cancelling before leaving the ground and
        then performing the next action, sometimes causing loops where Collector keeps trying to hop but doesn't. Always happens
        after the Collector has landed perfectly in the corner after a normal hop.

        This glitch happens because the CheckCollisionSide action from the midair states do not properly reset in these scenarios,
        so the next time the midair state is entered, it acts as though it is still in the previous state it was at when Collector
        was in the corner of the arena and triggers the landing transition immediately instead of waiting for GameObject collision.

        Credit to shownyoung for pointing out the real root cause of the issue so I could create this "correct" solution.
        */
        public void OnCheckCollisionSideExit(On.HutongGames.PlayMaker.Actions.CheckCollisionSide.orig_OnExit orig, CheckCollisionSide self) {
            orig(self);

            if (self.Fsm.GameObjectName == "Jar Collector") {
                self.topHit = false;
                self.rightHit = false;
                self.bottomHit = false;
                self.leftHit = false;
            }
        }
    }
}
