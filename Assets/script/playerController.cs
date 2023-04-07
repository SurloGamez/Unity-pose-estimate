using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] LayerMask ground;
    [SerializeField] float skinWidth;
    [SerializeField] Vector2 RayAmount; // x for up and down, y for left and right
    [SerializeField] float Speed;
    [SerializeField] float Friction;
    [SerializeField] float Gravity;
    [SerializeField] float jumpForce;
    [SerializeField] GameObject trail;
    [SerializeField] float sideChargeWait;
    [SerializeField] float uppercutWait;
    [SerializeField] float groundPoundWait;
    [SerializeField] GameObject robot;
    [SerializeField] AnimationRig anim;

    KeyCode attackButton;
    KeyCode jumpButton;
    int attackButtonCounter = 0;
    float sideChargecd = 0;
    float uppercutcd = 0;
    float groundPoundcd = 0;

    SpriteRenderer sr;

    [HideInInspector] public Vector2 velocity;
    Vector2 extraMoveAmount;

    [HideInInspector] public Vector2 input = new Vector2();
    [HideInInspector] public Vector2 smoothInput = new Vector2();
    int dir = 1;
    int jumpCount = 2;
    bool jumpPressed = false;
    
    int chargeCounter = 0; // if greater than 0 that means currently in attack
    Vector2 chargeDir; // for attacking sideways


    Vector2 RaySpacing;
    BoxCollider2D col;
    Bounds bounds;
    bool grounded = false;
    bool downslope;
    float slopeVector;
    float chargeVelocity;
    float groundPoundCooldown = 0; //this is the time it takes to exit the iron man position
    void Start()
    {
        
        attackButton = KeyCode.X;
        jumpButton = KeyCode.C;
        col = GetComponent<BoxCollider2D>();
        GetOrigins();
        CalculateRaySpacing();
        sr = GetComponent<SpriteRenderer>();
    }


    void FixedUpdate()
    {
        GetInput();

        grounded = false;
        extraMoveAmount = Vector2.zero;
        downslope = false;

        rayOrigins origins = GetOrigins();
        if (chargeCounter <= 0 || chargeDir.y == 1) UpdateYVel();
        if (chargeCounter <= 0) CheckDescendSlope(ref velocity, origins);
        VerticalMovement(ref velocity, origins);
        transform.Translate(new Vector2(extraMoveAmount.x, velocity.y), Space.World);


        origins = GetOrigins();
        if (chargeCounter <= 0) UpdateXVel();
        HorizontalMovement(ref velocity, origins);
        transform.Translate(new Vector2(velocity.x, extraMoveAmount.y), Space.World);

        if (velocity.x > 0f) dir = 1;
        if (velocity.x < 0f) dir = -1;
        robot.transform.forward = Vector3.right * dir;

        if (grounded)
        {
            jumpCount = 2;
        }
        if (Input.GetKey(jumpButton) && groundPoundCooldown <= 0 && jumpCount > 0 && !jumpPressed && chargeCounter <= 0)
        {
            jumpPressed = true;
            velocity.y = jumpForce;
            grounded = false;
            jumpCount--;
        }

        if (velocity.magnitude > 0.1f) anim.ChangeAnimation("run");
        else anim.ChangeAnimation("walk");

    }

    void UpdateYVel()
    {
        if (velocity.y > 0)
        {
            velocity.y -= Gravity;
        }
        else
        {
            velocity.y -= Gravity * 2;
        }
        if (velocity.y <= -1.5) velocity.y = -1.5f;
    }

    void UpdateXVel()
    {
        velocity.x = smoothInput.x * Speed;
        if (downslope) velocity.x *= slopeVector;
    }

    void GetInput()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (groundPoundCooldown > 0) input = Vector2.zero;

        if (chargeCounter > 0) input.x = 0;

        if (!Input.GetKey(jumpButton)) jumpPressed = false;

        if (Input.GetKey(attackButton) && groundPoundCooldown <= 0) attackButtonCounter += 1; else attackButtonCounter = 0;


        smoothInput.x += input.x;
        smoothInput.x *= Friction;
        if (Mathf.Abs(smoothInput.x) <= 0.001f) smoothInput.x = 0;
    }

    void CheckAttack()
    {
        if (groundPoundCooldown > 0) groundPoundCooldown -= 0.02f;

        if (sideChargecd > 0) sideChargecd -= 0.02f;
        if (uppercutcd > 0) uppercutcd -= 0.02f;
        if (groundPoundcd > 0) groundPoundcd -= 0.02f;

        if (attackButtonCounter == 1 && chargeCounter <= 0) // this means this is the first frame attack button is pressed (get the attack key down)
        {
            bool endAttack = true;

            if (input.y > 0 && uppercutcd <= 0) //uppercut
            {
                chargeDir = Vector2.up;
                velocity.y = jumpForce;
                uppercutcd = uppercutWait;
                endAttack = false;
            }
            else if (input.y != 1 && (input.x == 0 || Mathf.Abs(input.x) == 1) && sideChargecd <= 0) //side charge
            {
                chargeDir = Vector2.right * dir;
                sideChargecd = sideChargeWait;
                endAttack = false;
            }

            if (input.y < 0 && groundPoundcd <= 0 && jumpCount <= 0) //ground strike
            {
                chargeDir = Vector2.down;
                groundPoundcd = groundPoundWait;
                endAttack = false;
            }

            if (endAttack) { return; }

            chargeVelocity = 1;
            chargeCounter = 20;

        }

        if (chargeCounter == 0) // when attack ends
        {
            chargeCounter -= 1;

            if (chargeDir == Vector2.down) groundPoundCooldown = 0.5f;
        }

        if (chargeCounter > 0) // during attack
        {
            if (chargeDir.x != 0) // this means going side ways
            {

                velocity = chargeDir * chargeVelocity;
                chargeCounter -= 1;
                if (chargeVelocity >= 0.3f) chargeVelocity *= 0.9f;
            }

            if (chargeDir == Vector2.down) // this means going down
            {
                velocity = chargeDir;

                if (grounded) chargeCounter = 0;
            }

            if (chargeDir == Vector2.up) // this means going up
            {
                if (velocity.y <= 0 && !grounded) chargeCounter = 0;
                velocity.x *= 0.95f;
            }

        }
    }

    rayOrigins GetOrigins()
    {
        bounds = col.bounds;
        bounds.Expand(-skinWidth * 2);
        rayOrigins origins = new rayOrigins();
        origins.tl = new Vector2(bounds.min.x, bounds.max.y);
        origins.tr = new Vector2(bounds.max.x, bounds.max.y);
        origins.bl = new Vector2(bounds.min.x, bounds.min.y);
        origins.br = new Vector2(bounds.max.x, bounds.min.y);
        return origins;
    }

    void CalculateRaySpacing()
    {
        RaySpacing.x = bounds.size.x / (RayAmount.x - 1); //for vertical movement
        RaySpacing.y = bounds.size.y / (RayAmount.y - 1); //for horizontal movement
    }

    void VerticalMovement(ref Vector2 velocity, rayOrigins origins)
    {
        float dir = Mathf.Sign(velocity.y);
        if (velocity.y == 0) dir = -1;
        Vector2 origin = dir == 1 ? origins.tl : origins.bl;
        float dist = Mathf.Abs(velocity.y) + skinWidth;
        for (int i = 0; i < RayAmount.x; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2.right * i * RaySpacing.x) + origin, Vector2.up * dir, dist, ground);
            if (hit)
            {
                if (dir == -1) grounded = true;
                dist = hit.distance;
            }
        }
        velocity.y = (dist - skinWidth) * dir;

        if (!grounded && velocity.y <= 0 && velocity.y >= -0.1f)
        {
            RaycastHit2D checkgroundleft = Physics2D.Raycast(origins.bl, Vector2.down, 0.4f, ground);
            RaycastHit2D checkgroundright = Physics2D.Raycast(origins.br, Vector2.down, 0.4f, ground);

            if (checkgroundleft || checkgroundright) grounded = true;
        }


    }

    void CheckDescendSlope(ref Vector2 velocity, rayOrigins origins)
    {
        if (velocity.y > 0 || velocity.x == 0) return;

        float dirx = Mathf.Sign(velocity.x);
        Vector2 origin = dirx == 1 ? origins.bl : origins.br;

        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.5f + skinWidth, ground);

        if (hit)
        {
            Vector2 slopeVec = -1 * Vector2.Perpendicular(hit.normal);
            if (hit.normal == Vector2.up || Mathf.Sign(hit.normal.x) != dirx) return;
            float dist2Slope = hit.distance - skinWidth;
            velocity.y += dist2Slope;
            DescendSlope(ref velocity, slopeVec);
            velocity.y -= dist2Slope;
            downslope = true;
            slopeVector = Mathf.Abs(slopeVec.x);
        }


    }

    void DescendSlope(ref Vector2 velocity, Vector2 vec)
    {
        float moveAmount = Mathf.Abs(velocity.y) * velocity.x;
        extraMoveAmount.x = moveAmount * vec.x;
        velocity.y = moveAmount * vec.y;
    }


    void HorizontalMovement(ref Vector2 velocity, rayOrigins origins)
    {
        if (velocity.x == 0)
        {
            return;
        }
        float dir = Mathf.Sign(velocity.x);
        Vector2 origin = dir == 1 ? origins.br : origins.bl;
        float dist = Mathf.Abs(velocity.x) + skinWidth;
        for (int i = 0; i < RayAmount.y; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast((Vector2.up * i * RaySpacing.y) + origin, Vector2.right * dir, dist, ground);
            if (hit)
            {
                dist = hit.distance;
                if (i == 0)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);


                    if (slopeAngle <= 80)
                    {
                        float dist2Slope = hit.distance - skinWidth;
                        velocity.x -= dist2Slope * dir;
                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += dist2Slope * dir;
                        return;
                    }
                }

            }

        }
        velocity.x = (dist - skinWidth) * dir;
    }

    void ClimbSlope(ref Vector2 velocity, float angle)
    {
        float dir = Mathf.Sign(velocity.x);
        float moveAmount = Mathf.Abs(velocity.x);
        extraMoveAmount.y = Mathf.Sin(angle * Mathf.Deg2Rad) * moveAmount;
        velocity.x = Mathf.Cos(angle * Mathf.Deg2Rad) * moveAmount * dir;
    }

    public struct rayOrigins
    {
        public Vector2 tl, tr, bl, br;
    }

}
