using UnityEngine;
using System.Collections;
using CreativeSpore.SuperTilemapEditor;
using System;
using System.Collections.Generic;

public class BoundsCheck : MonoBehaviour
{
    const float _raySpacing = .25f;

    public LayerMask collisionMask = 1;
    public bool autoTestEdges = true;
    public bool checkAll = false;

    public float horizontalCheckDistance = 1f;
    public float verticalCheckDistance = 1f;
    public float horizontalTouchingDistance = 0.1f;
    public float verticalTouchingDistance = 0.1f;
    public string[] belowTileParameters;

    [Range(0.05f, 0.2f)]
    public float skinWidth = 0.05f;

    public EdgeCheck leftEdge;
    public EdgeCheck rightEdge;
    public EdgeCheck bottomEdge;
    public EdgeCheck topEdge;

    [HideInInspector]
    public int verticalRayCount;
    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public float horizontalRaySpacing;
    [HideInInspector]
    public float verticalRaySpacing;

    new public Collider2D collider2D;
    private BoxCollider2D _boxCollider2D;
    private CircleCollider2D _circleCollider2D;

    protected Vector2 _up, _down, _left, _right;
    public Vector2 topLeft, topRight, bottomLeft, bottomMiddle, bottomRight;

    private RaycastHit2D _hit;

    public Vector3 extents
    {
        get { return collider2D.bounds.extents; }
    }

    protected virtual void Awake()
    {
        if (!collider2D)
        {
            collider2D = GetComponent<Collider2D>();
        }
        _boxCollider2D = collider2D.GetComponent<BoxCollider2D>();
        _circleCollider2D = collider2D.GetComponent<CircleCollider2D>();
    }

    protected virtual void Start()
    {
        UpdateRaycastOrigins();
    }

    public virtual void FixedUpdate()
    {
        if (autoTestEdges) TestEdges();
    }

    public void UpdateRaycastOrigins()
    {
        var origin = (Vector2)(transform.position + transform.TransformDirection(collider2D.offset));
        float halfHeight, halfWidth;

        var sizeX = _boxCollider2D ? _boxCollider2D.size.x : _circleCollider2D ? _circleCollider2D.radius * 2 : collider2D.bounds.size.x;
        var sizeY = _boxCollider2D ? _boxCollider2D.size.y : _circleCollider2D ? _circleCollider2D.radius * 2 : collider2D.bounds.size.y;

        halfHeight = (sizeY - skinWidth) * 0.5f;
        halfWidth = (sizeX - skinWidth) * 0.5f;

        _up = transform.up;
        _down = -_up;
        _right = transform.right;
        _left = -_right;

        var top = origin + _up * halfHeight;
        topLeft =  top + _left * halfWidth;
        topRight = top + _right * halfWidth;
        bottomMiddle = origin + _down * halfHeight;
        bottomLeft = bottomMiddle + _left * halfWidth;
        bottomRight = bottomMiddle + _right * halfWidth;

        float boundsWidth = halfWidth * 2;
        float boundsHeight = halfHeight * 2;

        verticalRayCount = Mathf.RoundToInt(Mathf.Clamp(boundsWidth / _raySpacing, 2, int.MaxValue));
        horizontalRayCount = Mathf.RoundToInt(Mathf.Clamp(boundsHeight / _raySpacing, 2, int.MaxValue));

        horizontalRaySpacing = boundsHeight / (horizontalRayCount - 1);
        verticalRaySpacing = boundsWidth / (verticalRayCount - 1);
    }

    public virtual void TestEdges()
    {
        UpdateRaycastOrigins();

        TestTop();
        TestBottom();
        TestRight();
        TestLeft();
    }

    public void TestTop()
    {
        topEdge.PrepareForTest();
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topLeft, topRight, (float)i / (verticalRayCount - 1));
            _hit = Physics2D.Raycast(rayOrigin, _up, verticalCheckDistance + skinWidth, collisionMask);

            if (_hit.collider)
            {
                topEdge.near = true;
                topEdge.nearAngle = Vector2.Angle(_hit.normal, Vector2.up);
                if (topEdge.distance == -1 || _hit.distance < topEdge.distance)
                {
                    topEdge.distance = _hit.distance;
                }

                if (_hit.distance <= verticalTouchingDistance + skinWidth)
                {
                    OnTouch(_hit, Direction.Up);
                    topEdge.touching = true;
                    topEdge.angle = topEdge.nearAngle;
                    if (!checkAll) { break; }
                }
            }
        }
    }

    public void TestBottom()
    {
        bottomEdge.PrepareForTest();
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(bottomLeft, bottomRight, (float)i / (verticalRayCount - 1));
            _hit = Physics2D.Raycast(rayOrigin, _down, verticalCheckDistance + skinWidth, collisionMask);

            if (_hit.collider)
            {
                bottomEdge.near = true;
                bottomEdge.nearAngle = Vector2.Angle(_hit.normal, Vector2.up);
                if (bottomEdge.distance == -1 || _hit.distance < bottomEdge.distance)
                {
                    bottomEdge.distance = _hit.distance;
                }

                if (_hit.distance <= verticalTouchingDistance + skinWidth)
                {
                    OnTouch(_hit, Direction.Down);
                    bottomEdge.touching = true;
                    bottomEdge.angle = bottomEdge.nearAngle;
                    if (!checkAll) { break; }

                    if (belowTileParameters.Length > 0)
                    {
                        var tmap = _hit.transform.GetComponentInParent<STETilemap>();
                        if (tmap)
                        {
                            var tile = tmap.GetTile(tmap.transform.InverseTransformPoint(_hit.point + _down * skinWidth));
                            if (tile != null)
                            {
                                foreach (var param in belowTileParameters)
                                {
                                    if (tile.paramContainer.GetBoolParam(param)) { bottomEdge.tileParams.Add(param); }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void TestRight()
    {
        rightEdge.PrepareForTest();
        for (int i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topRight, bottomRight, (float)i / (horizontalRayCount - 1));
            _hit = Physics2D.Raycast(rayOrigin, _right, horizontalCheckDistance + skinWidth, collisionMask);

            if (_hit.collider)
            {
                rightEdge.near = true;
                rightEdge.nearAngle = Vector2.Angle(_hit.normal, Vector2.up);
                if (rightEdge.distance == -1 || _hit.distance < rightEdge.distance)
                {
                    rightEdge.distance = _hit.distance;
                }

                if (_hit.distance <= horizontalTouchingDistance + skinWidth)
                {
                    OnTouch(_hit, Direction.Right);
                    rightEdge.touching = true;
                    rightEdge.angle = rightEdge.nearAngle;
                    if (!checkAll) { break; }
                }
            }
        }
    }

    public void TestLeft()
    {
        leftEdge.PrepareForTest();
        for (int i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topLeft, bottomLeft, (float)i / (horizontalRayCount - 1));
            _hit = Physics2D.Raycast(rayOrigin, _left, horizontalCheckDistance + skinWidth, collisionMask);

            if (_hit.collider)
            {
                leftEdge.near = true;
                leftEdge.nearAngle = Vector2.Angle(_hit.normal, Vector2.up);
                if (leftEdge.distance == -1 || _hit.distance < leftEdge.distance)
                {
                    leftEdge.distance = _hit.distance;
                }

                if (_hit.distance <= horizontalTouchingDistance + skinWidth)
                {
                    OnTouch(_hit, Direction.Left);
                    leftEdge.touching = true;
                    leftEdge.angle = leftEdge.nearAngle;
                    if (!checkAll) { break; }
                }
            }
        }
    }

    public float topDistance()
    {
        var shortestDistance = float.MaxValue;
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topLeft, topRight, (float)i / (verticalRayCount - 1));
            var hit = Physics2D.Raycast(rayOrigin, _up, float.MaxValue, collisionMask);

            if (hit.collider && hit.distance < shortestDistance)
            {
                shortestDistance = hit.distance;
            }
        }

        return shortestDistance;
    }

    public float bottomDistance()
    {
        var shortestDistance = float.MaxValue;
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(bottomLeft, bottomRight, (float)i / (verticalRayCount - 1));
            var hit = Physics2D.Raycast(rayOrigin, _down, float.MaxValue, collisionMask);

            if (hit.collider && hit.distance < shortestDistance)
            {
                shortestDistance = hit.distance;
            }
        }

        return shortestDistance;
    }

    public virtual void OnTouch(RaycastHit2D hit, Direction direction) { }

    public bool CheckForGroundBehind(float offsetX = 0, float offsetY = 0)
    {
        if (offsetY == 0)
        {
            offsetY = skinWidth;
        }

        var origin = bottomLeft;
        origin.x -= offsetX * Mathf.Sign(transform.right.x);
        origin.y += offsetY * Mathf.Sign(transform.up.y);

        var distance = 1 + offsetY + verticalTouchingDistance;

        //Debug.DrawLine(origin, origin + _down * distance, Color.green);
        var raycast = Physics2D.Raycast(origin, _down, distance, collisionMask);
        var result = raycast.collider && (raycast.distance <= verticalTouchingDistance + offsetY);

        if (result)
        {
            return true;
        }
        else
        {
            var start = bottomRight;
            var end = start + (Vector2)transform.right * -2;
            for (int i = 0; i < 8; i++)
            {
                var rayOrigin = Vector2.Lerp(start, end, i / 7f);
                raycast = Physics2D.Raycast(rayOrigin, _down, 1 + skinWidth, collisionMask);

                if (raycast.collider)
                {
                    var downAngle = Vector2.Angle(raycast.normal, Vector2.up);
                    if (downAngle % 90 != 0)
                    {
                        //Debug.DrawLine(rayOrigin, rayOrigin + _down * (1 + skinWidth), Color.red);
                        return true;
                    }
                }
                else
                {
                    //Debug.DrawLine(rayOrigin, rayOrigin + _down * (1 + skinWidth), Color.yellow);
                }
            }

            return false;
        }
    }

    public bool CheckBottomCorners(bool ignoreSpecialPlatforms)
    {
        var hit = Physics2D.Raycast(bottomLeft, _down, verticalCheckDistance + skinWidth, collisionMask);

        if (!hit.collider || hit.distance > verticalCheckDistance + skinWidth || (ignoreSpecialPlatforms && hit.collider.gameObject.GetComponent<ISpecialPlatform>() != null))
        {
            return false;
        }

        hit = Physics2D.Raycast(bottomRight, _down, verticalCheckDistance + skinWidth, collisionMask);

        if (!hit.collider || hit.distance > verticalCheckDistance + skinWidth || (ignoreSpecialPlatforms && hit.collider.gameObject.GetComponent<ISpecialPlatform>() != null))
        {
            return false;
        }

        return true;
    }

    public bool CheckForGroundAhead(float offsetX = 0, float offsetY = 0)
    {
        if (offsetY == 0) { offsetY = skinWidth; }

        var origin = bottomRight;
        origin.x += offsetX * Mathf.Sign(transform.right.x);
        origin.y += offsetY * Mathf.Sign(transform.up.y); ;

        var distance = 1 + offsetY + verticalTouchingDistance; 

        //Debug.DrawLine(origin, origin + _down * distance, Color.green);
        var raycast = Physics2D.Raycast(origin, _down, distance, collisionMask);
        var result = raycast.collider && (raycast.distance <= offsetY + verticalTouchingDistance * 2f);

        if(result)
        {
            return true;
        }
        else
        {
            var start = bottomLeft;
            var end = start + (Vector2)transform.right * 2;
            Vector2 rayOrigin;
            for (int i = 0; i < 8; i++)
            {                
                rayOrigin = Vector2.Lerp(start, end, i /7f);
                raycast = Physics2D.Raycast(rayOrigin, _down, 1 + skinWidth, collisionMask);

                if (raycast.collider)
                {
                    var downAngle = Vector2.Angle(raycast.normal, Vector2.up);
                    if (downAngle % 90 != 0)
                    {
                        //Debug.DrawLine(rayOrigin, rayOrigin + _down * (1 + skinWidth), Color.red);
                        return true;
                    }
                }
                else
                {
                    //Debug.DrawLine(rayOrigin, rayOrigin + _down * (1 + skinWidth), Color.yellow);
                }
            }

            return false;
        }
    }

    public bool AnyTouching()
    {
        return bottomEdge.touching || topEdge.touching || leftEdge.touching || rightEdge.touching;
    }

    public bool AnyJustTouched()
    {
        return bottomEdge.justTouched || topEdge.justTouched || leftEdge.justTouched || rightEdge.justTouched;
    }

    public virtual void OnDrawGizmosSelected()
    {
        if (!collider2D)
        {
            collider2D = GetComponent<Collider2D>();
        }
        _boxCollider2D = collider2D.GetComponent<BoxCollider2D>();
        _circleCollider2D = collider2D.GetComponent<CircleCollider2D>();

        UpdateRaycastOrigins();
        
        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topLeft, topRight, (float)i / (verticalRayCount - 1));
            Debug.DrawLine(rayOrigin, rayOrigin + _up * (verticalCheckDistance + skinWidth), Color.yellow);
            Debug.DrawLine(rayOrigin, rayOrigin + _up * (verticalTouchingDistance + skinWidth), Color.white);
        }

        for (int i = 0; i < verticalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(bottomLeft, bottomRight, (float)i / (verticalRayCount - 1));
            Debug.DrawLine(rayOrigin, rayOrigin + _down * (verticalCheckDistance + skinWidth), Color.green);
            Debug.DrawLine(rayOrigin, rayOrigin + _down * (verticalTouchingDistance + skinWidth), Color.white);
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topLeft, bottomLeft, (float)i / (horizontalRayCount - 1));
            Debug.DrawLine(rayOrigin, rayOrigin + _left * (horizontalCheckDistance + skinWidth), Color.blue);
            Debug.DrawLine(rayOrigin, rayOrigin + _left * (horizontalTouchingDistance + skinWidth), Color.white);
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            var rayOrigin = Vector2.Lerp(topRight, bottomRight, (float)i / (horizontalRayCount - 1));
            Debug.DrawLine(rayOrigin, rayOrigin + _right * (horizontalCheckDistance + skinWidth), Color.red);
            Debug.DrawLine(rayOrigin, rayOrigin + _right * (horizontalTouchingDistance + skinWidth), Color.white);
        }
    }
}

[Serializable]
public class EdgeCheck
{
    public bool near;
    public bool touching;
    public bool lastTouching;
    public float angle;
    public float distance;
    public float nearAngle;
    public bool justTouched
    {
        get { return touching && !lastTouching; }
    }

    public HashSet<string> tileParams;

    public void PrepareForTest()
    {
        tileParams = new HashSet<string>();
        lastTouching = touching;
        near = touching = false;
        angle = 0;
        distance = -1;
    }
}
