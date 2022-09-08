using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Controller2D))]
public class FloatingPacer : BaseMovementBehaviour
{
    public float speed = 5f;
    private Controller2D _controller2D;
    public UnityEvent onChangeDirection;
    public float changeDirectionLimit = 0.05f;
    private bool _justChangedDirection;

    protected void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
    }

    public void Update()
    {
        if (_controller2D.leftEdge.touching && _controller2D.rightEdge.touching)
        {
            return;
        }

        _controller2D.Move(transform.right * speed * _slowMod * Time.deltaTime);        

        if (!_justChangedDirection && _controller2D.rightEdge.touching && Time.timeScale > 0)
        {
            var eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y == 0 ? 180 : 0, eulerRotation.z);
            onChangeDirection.Invoke();
            if (changeDirectionLimit > 0) StartCoroutine(JustChangedDirection());
        }
    }

    private IEnumerator JustChangedDirection()
    {
        _justChangedDirection = true;
        yield return new WaitForSeconds(changeDirectionLimit);
        _justChangedDirection = false;
    }
}
