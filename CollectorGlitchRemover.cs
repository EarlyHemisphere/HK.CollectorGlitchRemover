using Modding;
using SFCore.Utils;
using System.Linq;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Diagnostics;

namespace CollectorGlitchRemover {
    public class CollectorGlitchRemover : Mod {
        public static CollectorGlitchRemover instance;
        private string prevPrevState = "";
        private string prevState = "";
        private int delay = 0;
        private int stateCounter = 0;
        private int prevHops = 0;
        private bool prevEventWasWall = false;
        private bool curStateIsLunge = false;

        public CollectorGlitchRemover() : base("Collector Glitch Remover") => instance = this;

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public bool ToggleButtonInsideMenu => false;

        public override void Initialize() {
            Log("Initializing");

            On.PlayMakerFSM.OnEnable += OnEnable;
            On.PlayMakerFSM.Update += OnUpdate;
            On.HutongGames.PlayMaker.Fsm.ProcessEvent += OnProcessEvent;
            On.HutongGames.PlayMaker.Fsm.EnterState += OnEnterState;
            On.HutongGames.PlayMaker.FsmState.OnEnter += OnEnterFsmState;
            // On.PlayMakerFSM.SendEvent += OnSendEvent;
            // On.PlayMakerCollisionEnter2D.OnCollisionEnter2D += OnPlayMakerCollisionEnter2D;
            // On.PlayMakerCollisionStay2D.OnCollisionStay2D += OnPlayMakerCollisionStay2D;
            // On.PlayMakerFSM.ChangeState_FsmEvent += OnChangeStateFsmEvent;
            // On.PlayMakerFSM.ChangeState_string += OnChangeStateString;
            // On.PlayMakerFSM.SendEvent += OnSendEvent;
            // On.HutongGames.PlayMaker.Fsm.Event_FsmEvent += OnFsmEvent;

            Log("Initialized");
        }

        public void OnEnterFsmState(On.HutongGames.PlayMaker.FsmState.orig_OnEnter orig, FsmState self) {
            orig(self);

            if (self.Name == "Lunge Air") {
                Modding.Logger.Log("ActiveActions:");
                self.ActiveActions.ForEach(action => Modding.Logger.Log(action.GetType()));
            }
        }

        public void OnEnterState(On.HutongGames.PlayMaker.Fsm.orig_EnterState orig, Fsm self, FsmState toState) {
            if (self.GameObjectName == "Jar Collector" && self.Name == "Control") {
                Log(toState.Name);
                if (toState.Name == "Lunge Launch") {
                    curStateIsLunge = true;
                } else {
                    curStateIsLunge = false;
                }
            }

            orig(self, toState);
        }

        public void OnProcessEvent(On.HutongGames.PlayMaker.Fsm.orig_ProcessEvent orig, Fsm self, FsmEvent fsmEvent, FsmEventData eventData) {
            if (self.GameObjectName == "Jar Collector") {
                if (fsmEvent.Name == "WALL") {
                    prevEventWasWall = true;
                } else if (fsmEvent.Name == "LAND") {
                    // Log("DETECTED COMBO");
                    // fsmEvent = null;
                    prevEventWasWall = false;
                    // Log(new StackTrace());
                    if (prevState == "Lunge Antic" && stateCounter == 0) {
                        
                    }
                } else if (fsmEvent.Name == "FINISHED" && curStateIsLunge) {
                    // Log(new StackTrace());
                } else {
                    prevEventWasWall = false;
                }
            }

            orig(self, fsmEvent, eventData);
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

        public void OnEnable(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);

            if (self.gameObject.name == "Jar Collector" && self.FsmName == "Control") {
                // self.gameObject.AddComponent<CollectorCollisionDetector>();
                // self.GetState("Hop").RemoveAction(1);
                // self.GetState("Hop").RemoveAction(0);
            }
        }

        public void OnUpdate(On.PlayMakerFSM.orig_Update orig, PlayMakerFSM self) {
            orig(self);

            if (self.gameObject.name == "Jar Collector" && self.FsmName == "Control") {
                string curState = self.ActiveStateName;

                if (prevPrevState == "Lunge Antic" && prevState == "Lunge Air" && curState == "Lunge Swipe") {
                    Log("Preventing Collector insta-grab glitch");

                    // Temporarily remove landing trigger so we know instant landing can't happen
                    // self.RemoveFsmTransition("Lunge Air", "LAND");
                    
                    // // Remove premature landing effects
                    // GameObject.Find("Lunge Hit")?.SetActive(false);
                    // EmissionModule dustEmission = GameObject.Find("Dust Land").GetComponent<ParticleSystem>().emission;
                    // dustEmission.enabled = false;
                    // GameObject.FindObjectsOfType<AudioSource>().ToList().ForEach(audioSource => {
                    //     if (audioSource.gameObject.name.Contains("Audio Player Actor")) {
                    //         audioSource.Stop();
                    //     }
                    // }); // Stop swipe sound

                    // // Stop launch sound to avoid double after re-launch
                    // self.gameObject.GetComponent<AudioSource>().Stop();

                    // // Reset back to initial launch state
                    // self.SetState("Lunge Launch");

                    // // Start timer to wait for launch
                    // delay = 0;
                } else if (curState != "Hop" && prevState == "Hop" && prevPrevState != "Hop") {
                    Log("Preventing Collector sticky feet glitch");

                    // Temporarily remove landing trigger so we know instant landing can't happen
                    // self.RemoveFsmTransition("Hop", "LAND");

                    // // Remove premature landing effects
                    // EmissionModule dustEmission = GameObject.Find("Dust Land").GetComponent<ParticleSystem>().emission;
                    // dustEmission.enabled = false;
                    // self.FsmVariables.GetFsmInt("Hops").Value = self.FsmVariables.GetFsmInt("Hops").Value + 1;

                    // // Remove launch sounds to avoid double after re-launch
                    // GameObject.FindObjectsOfType<AudioSource>().ToList().ForEach(audioSource => {
                    //     if (audioSource.gameObject.name.Contains("Audio Player Actor")) {
                    //         audioSource.Stop();
                    //     }
                    // });
                    // self.gameObject.GetComponent<AudioSource>().Stop();

                    // // Reset back to initial launch state
                    // self.SetState("Hop Start");

                    // // Start timer to wait for launch
                    // delay = 0;
                } else if (delay < 20) {
                    // Wait for launch to occur
                    delay++;
                } else if (delay == 20) {
                    // After launch, add back landing trigger
                    self.AddFsmTransition("Lunge Air", "LAND", "Lunge Swipe");
                    self.AddFsmTransition("Hop", "LAND", "Hop Land");
                    delay++;
                    // Log("newHops: " + self.FsmVariables.GetFsmInt("Hops").Value);
                }

                if (prevState != curState && curState != "") {
                    // Log(prevState + " " + stateCounter);
                    stateCounter = 0;
                } else {
                    stateCounter++;
                    if (self.Fsm.DelayedEvents.Count > 0) {
                        // Log("Delayed Events:" + self.Fsm.DelayedEvents.Count);
                    }
                }

                prevPrevState = prevState;
                prevState = curState;
                prevHops = self.FsmVariables.GetFsmInt("Hops").Value;
            }
        }

        public void OnPlayMakerCollisionEnter2D(On.PlayMakerCollisionEnter2D.orig_OnCollisionEnter2D orig, PlayMakerCollisionEnter2D self, Collision2D collisionInfo) {
            orig(self, collisionInfo);

            if (self.gameObject.name == "Jar Collector") {
                Log("CollisionEnter2D");
                Log(collisionInfo.gameObject.name);
            }
        }

        public void OnPlayMakerCollisionStay2D(On.PlayMakerCollisionStay2D.orig_OnCollisionStay2D orig, PlayMakerCollisionStay2D self, Collision2D collisionInfo) {
            orig(self, collisionInfo);

            if (self.gameObject.name == "Jar Collector") {
                Log("CollisionStay2D");
                Log(collisionInfo.gameObject.name);
            }
        }

        public void OnChangeStateFsmEvent(On.PlayMakerFSM.orig_ChangeState_FsmEvent orig, PlayMakerFSM self, FsmEvent fsmEvent) {
            orig(self, fsmEvent);

            if (self.gameObject.name == "Jar Collector") {
                Log("ChangeStateFsmEvent");
                Log(fsmEvent.Name);
            }
        }

        public void OnChangeStateString(On.PlayMakerFSM.orig_ChangeState_string orig, PlayMakerFSM self, string eventName) {
            orig(self, eventName);

            if (self.gameObject.name == "Jar Collector") {
                Log("ChangeStateString");
                Log(eventName);
            }
        }

        public void OnSendEvent(On.PlayMakerFSM.orig_SendEvent orig, PlayMakerFSM self, string eventName) {
            orig(self, eventName);

            if (self.gameObject.name == "Jar Collector") {
                Log("SendEvent");
                Log(eventName);
            }
        }
    }
}
