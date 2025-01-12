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

            On.PlayMakerFSM.OnEnable += OnEnable;

            Log("Initialized");
        }

        /*
        This mod fixes two glitches.

        Glitch 1: "Insta-grab" - Collector doesn't jump before doing the grab attack, instead instantly performing the grab.
        Always happens after the Collector has performed a normal grab attack that lands perfectly in the corner of the arena.

        Glitch 2: "Sticky feet" - Collector doesn't jump when about to hop, instead cancelling before leaving the ground and
        then performing the next action, sometimes causing loops where Collector keeps trying to hop but doesn't. Always happens
        after the Collector has landed perfectly in the corner after a normal hop.

        Collector landing perfectly in a corner while in a "midair" state on high frame rate is definitely the cause of both
        these glitches, but I can find no indication in the code and in-game events of this after a good amount of investigation.
        What is common between these two glitches is that the Collector stays in the "midair" state for only one frame before
        triggering landing events from the floor that the Collector has presumably not launched off of yet.

        This mod simply changes the launch events for hop and grab attack jump to be 0.05s waits instead of 1-frame waits, giving
        Collector enough time to launch off the ground before entering into the "midair" state.
        */
        public void OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            
            if (self.Fsm.GameObjectName == "Jar Collector" && self.FsmName == "Control") {
                self.RemoveAction("Lunge Launch", 3);
                self.AddAction("Lunge Launch", new Wait {
                    time = 0.05f,
                    finishEvent = new FsmEvent("FINISHED"),
                    realTime = false
                });
                self.RemoveAction("Hop Start", 12);
                self.AddAction("Hop Start", new Wait {
                    time = 0.05f,
                    finishEvent = new FsmEvent("FINISHED"),
                    realTime = false
                });
            }
        }
    }
}
