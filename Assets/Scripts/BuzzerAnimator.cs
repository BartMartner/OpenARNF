using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzerAnimator : MonoBehaviour
{
    public Controller2D controller2D;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public NewPongerBehavior newPonger;    

    public void Start()
    {
        if (!controller2D) { controller2D = GetComponent<Controller2D>(); }
        if (!animator) { animator = GetComponent<Animator>(); }
        if (!spriteRenderer) { spriteRenderer = GetComponent<SpriteRenderer>(); }
    }

    public void Update()
    {
        var yTouching = controller2D.topEdge.touching || controller2D.bottomEdge.touching;
        var xTouching = controller2D.rightEdge.touching || controller2D.leftEdge.touching;
        animator.SetBool("yTouching", yTouching);
        animator.SetBool("xTouching", xTouching);

        bool flipX = controller2D.rightEdge.touching;
        if (newPonger && !xTouching)
        {
            flipX = newPonger.direction.x < 0;
            if (!yTouching)
            {
                animator.speed = newPonger.currentSpeed / 3;
            }
            else
            {
                animator.speed = 1;
            }

        }
        spriteRenderer.flipX = flipX;        
        spriteRenderer.flipY = controller2D.topEdge.touching;
    }
}
