using System.Collections.Generic;
using UnityEngine;
using Modding;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel;

namespace CollectorGlitchRemover {
    public class CollectorCollisionDetector : MonoBehaviour {
        private const float RAYCAST_LENGTH = 0.08f;

        private Collider2D col2d;
        private PlayMakerFSM controlFSM;

        private List<Vector2> rightRays;
        private List<Vector2> bottomRays;
        private List<Vector2> leftRays;
        
        private bool sideStay = false;
        private bool sideEnter = false;
        private bool bottomStay = false;
        private bool bottomEnter = false;

        private bool eventEmitted = false;

        private void Awake() {
            rightRays = new List<Vector2>(3);
            bottomRays = new List<Vector2>(3);
            leftRays = new List<Vector2>(3);
        }

        private void Start() {
            col2d = gameObject.GetComponent<Collider2D>();
            controlFSM = gameObject.LocateMyFSM("Control");
            controlFSM.AddAction("Hop", new CallMethod {
                behaviour = this,
                methodName = "ResetHop",
                parameters = new FsmVar[0],
                everyFrame = false,
            });
        }

        public void CheckTouching(bool enter = true) {
            if (eventEmitted || controlFSM.ActiveStateName != "Hop") return;

            rightRays.Clear();
            rightRays.Add(col2d.bounds.max);
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.center.y));
            rightRays.Add(new Vector2(col2d.bounds.max.x, col2d.bounds.min.y));
            for (int i = 0; i < 3; i++) {
                RaycastHit2D raycastHit2D2 = Physics2D.Raycast(rightRays[i], Vector2.right, RAYCAST_LENGTH, 1 << 8);
                if (raycastHit2D2.collider != null) {
                    if (enter) {
                        sideEnter = true;
                    } else {
                        sideStay = true;
                    }
                }
            }

            leftRays.Clear();
            leftRays.Add(col2d.bounds.min);
            leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.center.y));
		    leftRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.max.y));
            for (int i = 0; i < 3; i++) {
                RaycastHit2D raycastHit2D2 = Physics2D.Raycast(leftRays[i], Vector2.left, RAYCAST_LENGTH, 1 << 8);
                if (raycastHit2D2.collider != null) {
                    if (enter) {
                        sideEnter = true;
                    } else {
                        sideStay = true;
                    }
                }
            }

            bottomRays.Clear();
            bottomRays.Add(new Vector2(col2d.bounds.min.x, col2d.bounds.min.y));
            bottomRays.Add(new Vector2(col2d.bounds.center.x, col2d.bounds.min.y));
            bottomRays.Add(col2d.bounds.min);
            for (int i = 0; i < 3; i++) {
                RaycastHit2D raycastHit2D2 = Physics2D.Raycast(bottomRays[i], Vector2.down, RAYCAST_LENGTH, 1 << 8);
                if (raycastHit2D2.collider != null) {
                    if (enter) {
                        bottomEnter = true;
                    } else {
                        bottomStay = true;
                    }
                }
            }
        }

        public void FixedUpdate() {
            if (eventEmitted || controlFSM.ActiveStateName != "Hop") {
                bottomEnter = false;
                bottomStay = false;
                sideEnter = false;
                sideStay = false;
                return;
            }

            if (bottomEnter || bottomStay || sideEnter || sideStay) {
                Modding.Logger.Log("bottomEnter: " + bottomEnter);
                Modding.Logger.Log("bottomStay: " + bottomStay);
                Modding.Logger.Log("sideEnter: " + sideEnter);
                Modding.Logger.Log("sideStay: " + sideStay);
            }

            if (bottomEnter && sideEnter) {
                controlFSM.Fsm.Event("LAND");
                eventEmitted = true;
            } else if (bottomStay && sideStay) {
                controlFSM.Fsm.Event("WALL");
                eventEmitted = true;
            } else if (bottomEnter || bottomStay) {
                controlFSM.Fsm.Event("LAND");
                eventEmitted = true;
            } else if (sideEnter || sideStay) {
                controlFSM.Fsm.Event("WALL");
                eventEmitted = true;
            }
        }

        public void OnCollisionStay2D(Collision2D _) => CheckTouching(false);

        public void OnCollisionEnter2D(Collision2D _) => CheckTouching();

        public void ResetHop() => eventEmitted = false;
    }
}