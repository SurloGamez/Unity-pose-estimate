using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRigController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 10;
    public float rotation = 5;
    public AnimationRig animRig;

    // Start is called before the first frame update
    void Start()
    {
        //if (animRig) animRig.autoUpdate = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float vertical = Input.GetAxisRaw("Vertical");
        float horizontal = Input.GetAxisRaw("Horizontal");
        //Vector2 direction = new Vector2(horizontal, 0);
        ////if (direction.magnitude > 1) direction = direction.normalized;
        //rb.velocity = direction * speed + rb.velocity.y * Vector2.up;

        ////bool directionChanged = direction == rb.transform.forward;

        //if (direction != Vector2.zero) rb.transform.right = direction;

        

        //if (directionChanged) animRig.UpdatePose();
        //animRig.AnimUpdate();

        Vector3 velocity = vertical * rb.transform.forward * speed; //new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //rb.velocity += velocity;
        //rb.velocity = new Vector3(rb.velocity.x * 0.9f, rb.velocity.y, rb.velocity.z * 0.9f);
        //if (velocity != Vector3.zero) transform.forward = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        float rotate = rotation * horizontal * Time.fixedDeltaTime;
        rb.transform.Rotate(Vector3.up * rotate);

        if (rb.velocity.magnitude > 0.1f) animRig.ChangeAnimation("run");
        else animRig.ChangeAnimation("walk");

        animRig.UpdatePose();
    }
}
