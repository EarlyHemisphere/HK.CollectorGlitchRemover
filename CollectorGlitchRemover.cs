using Modding;
using SFCore.Utils;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace CollectorGlitchRemover {
    public class CollectorGlitchRemover : Mod {
        public static CollectorGlitchRemover instance;
        private string prevPrevState = "";
        private string prevState = "";
        private int delay = 0;

        public CollectorGlitchRemover() : base("Collector Glitch Remover") => instance = this;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public bool ToggleButtonInsideMenu => false;

        public override void Initialize() {
            Log("Initializing");

            On.PlayMakerFSM.Update += OnUpdate;

            Log("Initialized");
        }

        /*
        This glitch occurs after The Collector lands perfectly in a corner of the arena.
        
        Landing perfectly in the corner during a grab attack triggers the insta-grab glitch,
        and landing perfectly in the corner during a normal jump triggers the sticky feet glitch.

        My hypothesis is that in this scenario, extra FINISHED FSM events are sent due to activating
        both the LAND and WALL Hop state transitions at the same time. Presumably, if The Collector
        landed perfectly in a corner after the grab attack then this double state transition occurs
        after starting a new hop, whereas if The Collector landed perfectly in a corner after hopping
        then it would occur at the end of a hop, which may explain why two glitches stem from this
        scenario.

        This mod works by detecting when a LAND transition happens too fast (either the Hop LAND
        transition or the Lunge LAND transition) and intervenes to revert The Collector back to its
        pre-hop or pre-lunge state.
        */
        public void OnUpdate(On.PlayMakerFSM.orig_Update orig, PlayMakerFSM self) {
            orig(self);

            if (self.FsmName == "Control" && self.gameObject.name == "Jar Collector") {
                string curState = self.ActiveStateName;

                if (prevPrevState == "Lunge Antic" && prevState == "Lunge Air" && curState == "Lunge Swipe") {
                    Log("Preventing Collector insta-grab glitch");

                    prevPrevState = "";
                    prevState = "";

                    // Temporarily remove landing trigger so we know instant landing can't happen
                    self.RemoveFsmTransition("Lunge Air", "LAND");
                    
                    // Remove premature landing effects
                    GameObject.Find("Lunge Hit")?.SetActive(false);
                    EmissionModule dustEmission = GameObject.Find("Dust Land").GetComponent<ParticleSystem>().emission;
                    dustEmission.enabled = false;
                    GameObject.FindObjectsOfType<AudioSource>().ToList().ForEach(audioSource => {
                        if (audioSource.gameObject.name.Contains("Audio Player Actor")) {
                            audioSource.Stop();
                        }
                    }); // Stop swipe sound

                    // Stop launch sound to avoid double after re-launch
                    self.gameObject.GetComponent<AudioSource>().Stop();

                    // Reset back to initial launch state
                    self.SetState("Lunge Launch");

                    // Start timer to wait for launch
                    delay = 0;
                } else if (curState != "Hop" && prevState == "Hop" && prevPrevState != "Hop") {
                    Log("Preventing Collector sticky feet glitch");

                    prevPrevState = "";
                    prevState = "";

                    // Temporarily remove landing trigger so we know instant landing can't happen
                    self.RemoveFsmTransition("Hop", "LAND");

                    // Remove premature landing effects
                    EmissionModule dustEmission = GameObject.Find("Dust Land").GetComponent<ParticleSystem>().emission;
                    dustEmission.enabled = false;
                    self.FsmVariables.GetFsmInt("Hops").Value = self.FsmVariables.GetFsmInt("Hops").Value + 1;

                    // Remove launch sounds to avoid double after re-launch
                    GameObject.FindObjectsOfType<AudioSource>().ToList().ForEach(audioSource => {
                        if (audioSource.gameObject.name.Contains("Audio Player Actor")) {
                            audioSource.Stop();
                        }
                    });
                    self.gameObject.GetComponent<AudioSource>().Stop();

                    // Reset back to initial launch state
                    self.SetState("Hop Start");

                    // Start timer to wait for launch
                    delay = 0;
                } else if (delay < 20) {
                    // Wait for launch to occur
                    delay++;
                } else if (delay == 20) {
                    // After launch, add back landing trigger
                    self.AddFsmTransition("Lunge Air", "LAND", "Lunge Swipe");
                    self.AddFsmTransition("Hop", "LAND", "Hop Land");
                    delay++;
                } else {
                    prevPrevState = prevState;
                    prevState = curState;
                }
            }
        }
    }
}
