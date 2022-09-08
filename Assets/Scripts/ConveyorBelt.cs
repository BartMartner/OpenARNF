using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public Direction direction = Direction.Left;
    public float size;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private ConveyorBounds _conveyorBounds;

    public void SetSize(float newSize)
    {
        size = newSize;
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider2D.size = new Vector2(size, 1);
        _spriteRenderer.size = new Vector2(size, 1);
    }

    public void SetDirection(Direction newDirection)
    {
        direction = newDirection;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.flipX = newDirection != Direction.Right;
        _conveyorBounds = GetComponent<ConveyorBounds>();
        _conveyorBounds.direction = direction == Direction.Right ? Vector3.right : Vector3.left;
    }
}
