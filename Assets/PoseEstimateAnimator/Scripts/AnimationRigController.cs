using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseEstimateAnimator
{
    public class AnimationRigController : MonoBehaviour
    {
        public Rigidbody rb;
        public float speed = 10;
        public float rotation = 5;
        public AnimationRig animRig;

        public string idleAnim = "walk", walkAnim = "test";

        // Start is called before the first frame update
        void Start()
        {
            if (animRig) animRig.StartAnimation(idleAnim);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");

            Vector3 velocity = vertical * rb.transform.forward * speed;

            rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

            float rotate = rotation * horizontal * Time.fixedDeltaTime;
            rb.transform.Rotate(Vector3.up * rotate);

            if (rb.velocity.magnitude > 0.1f) animRig.ChangeAnimation(walkAnim);
            else animRig.ChangeAnimation(idleAnim);

            if (animRig.smoothRotate) animRig.AnimUpdate();
        }
    }
}