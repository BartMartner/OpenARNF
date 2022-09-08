using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;
using System.Collections.Generic;

public class Controller2D : BoundsCheck
{
    float maxClimbAngle = 80;
    float maxDescendAngle = 80;

    public CollisionInfo collisions;
    public bool resistConveyorsAndIce;
    private bool _applyGravity;

    protected override void Start()
    {
        base.Start();
        collisions.faceDir = 1;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_applyGravity) { Move(transform.up * Physics2D.gravity.y * Time.deltaTime); }
    }

    public void SetGravity(bool toggle)
    {
        _applyGravity = toggle;
    }

    public void Move(Vector2 moveAmount, bool resetCollisions = true)
    {
        moveAmount = transform.InverseTransformDirection(moveAmount);

        UpdateRaycastOrigins();

        if (resetCollisions) { collisions.Reset(); }
        collisions.moveAmountOld = moveAmount;

        var previous = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = false;

        if (moveAmount.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0)
        {
            VerticalCollisions(ref moveAmount);
        }

        Physics2D.queriesHitTriggers = previous;

        moveAmount = transform.TransformDirection(moveAmount);
        transform.position = transform.position + (Vector3)moveAmount;
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        Vector2 rayOrigin;
        RaycastHit2D hit;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            rayOrigin = (directionX == -1) ? bottomLeft : bottomRight;
            rayOrigin += _up * (horizontalRaySpacing * i);
            hit = Physics2D.Raycast(rayOrigin, _right * directionX, rayLength, collisionMask);

            //Debug.DrawRay(rayOrigin, _right * directionX, Color.red);

            if (hit)
            {
                if (hit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(hit.normal, _up);

                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        moveAmount = collisions.moveAmountOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveAmount, slopeAngle);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        Vector2 rayOrigin;
        RaycastHit2D hit;

        for (int i = 0; i < verticalRayCount; i++)
        {
            rayOrigin = (directionY == -1) ? bottomLeft : topLeft;
            rayOrigin += _right * (verticalRaySpacing * i + moveAmount.x);
            hit = Physics2D.Raycast(rayOrigin, _up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, _up * directionY, Color.red);

            if (hit)
            {
                var down = moveAmount.y < 0;

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            rayOrigin = ((directionX == -1) ? bottomLeft : bottomRight) + _up * moveAmount.y;
            hit = Physics2D.Raycast(rayOrigin, _right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, _up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector2 moveAmount)
    {
        float directionX = Mathf.Sign(moveAmount.x);
        Vector2 rayOrigin = (directionX == -1) ? bottomRight : bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -_up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            var transformedNormal = transform.InverseTransformDirection(hit.normal);
            float slopeAngle = Vector2.Angle(hit.normal, _up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(transformedNormal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                    {
                        float moveDistance = Mathf.Abs(moveAmount.x);
                        float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                        moveAmount.y -= descendmoveAmountY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 moveAmountOld;
        public int faceDir;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

}
