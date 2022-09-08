using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MolemanShaman : BaseMovementBehaviour
{
    public ProjectileStats projectileStats;

    public GameObject eyeballPrefab;
    public GameObject eyeballShieldPrefab;

    private Controller2D _controller2D;
    private Animator _animator;
    private bool _busy;
    private List<GameObject> _instantiatedObjects = new List<GameObject>();
    private Player _player;

    public void Awake()
    {
        _controller2D = GetComponent<Controller2D>();
        _animator = GetComponent<Animator>();
        StartCoroutine(SetBusy(2));
    }

    protected override void Start()
    {
        _player = PlayerManager.instance.player1;
    }

    // Update is called once per frame
    public void Update()
    {
        if (!_busy)
        {
            FacePlayer();

            var coolDown = Random.Range(0.5f, 1);
            var pickAction = Random.Range(0, _player.targetable ? 5 : 3);
            switch(pickAction)
            {
                case 0:
                    StartCoroutine(LongJump(coolDown));
                    break;
                case 1:
                    StartCoroutine(EyeballStorm(coolDown));
                    break;
                case 2:
                    StartCoroutine(EyeballShield(coolDown));
                    break;
                case 3:
                    StartCoroutine(JumpShootSequence(3, 1, coolDown));
                    break;
                case 4:
                    StartCoroutine(JumpShootSequence(6, 3, coolDown));
                    break;
            }
        }
    }

    //Major Actions
    public IEnumerator JumpShootSequence(float jumpHeight, int shotCount, float coolDown)
    {
        _busy = true;

        Debug.Log("Start Jump Shoot");

        FacePlayer();

        _animator.SetTrigger("Jump");

        yield return new WaitForSeconds(2 / 12f);

        var jumpTime = jumpHeight / 6f;
        var timeToApex = jumpTime / 2f;
        var gravity = -(2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        var maxJumpVelocity = Mathf.Abs(gravity) * timeToApex;
        var velocity = new Vector3(0, maxJumpVelocity, 0);

        var timer = 0f;
        while(timer < timeToApex)
        {
            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }

        StartCoroutine(CheckForLanding());

        var shotsFired = 0;
        var apex = transform.position;
        var shotDistance = jumpHeight / shotCount;

        while (timer < jumpTime)
        {
            var distanceFromApex = Vector3.Distance(transform.position, apex);
            if (distanceFromApex > shotDistance * shotsFired && shotsFired < shotCount)
            {
                Debug.Log("Shot Time: " + timer);
                shotsFired++;
                StartCoroutine(JumpShot());
                yield return new WaitForSeconds(0.5f);
            }

            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }

        while (!_controller2D.bottomEdge.touching)
        {
            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }

        if (coolDown > 0)
        {
            yield return new WaitForSeconds(coolDown);
        }

        _busy = false;
    }

    public IEnumerator LongJump(float coolDown)
    {
        _busy = true;

        Debug.Log("Start Long Jump");

        FacePlayer();

        var maxDistance = 20;
        var distanceCheck = Physics2D.Raycast(transform.position, transform.right, maxDistance, LayerMask.GetMask("Default"));
        var trueDistance = Mathf.Clamp(distanceCheck.collider ? distanceCheck.distance : maxDistance, 3, maxDistance);
        var ratio = trueDistance / maxDistance;
        var jumpHeight = 7 * ratio;
        var jumpTime = 1.75f * ratio;

        yield return StartCoroutine(Jump(jumpHeight, jumpTime, trueDistance));

        FacePlayer();

        if (coolDown > 0)
        {
            yield return new WaitForSeconds(coolDown);
        }

        _busy = false;
    }

    public IEnumerator EyeballStorm(float coolDown)
    {
        _busy = true;

        Debug.Log("Start Eyeball Storm");

        _animator.SetTrigger("SpellStart");

        yield return new WaitForSeconds(8 / 12f);

        var eyeballGrid = new List<Vector2>();
        Vector2 gridBottomRight = transform.position + transform.right * 3 + transform.up;
        gridBottomRight.y += 0.25f;
        var xSign = Mathf.Sign(transform.right.x);
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                eyeballGrid.Add(gridBottomRight + new Vector2(x * 2.5f * xSign, y * 2.25f));
            }
        }

        var eyeballs = new List<GameObject>();
        for (int i = 0; i < 6; i++)
        {
            var eyeball = Instantiate(eyeballPrefab, transform.parent);
            eyeball.transform.position = transform.position;
            eyeballs.Add(eyeball);
            _instantiatedObjects.Add(eyeball);
            var randIndex = Random.Range(0, eyeballGrid.Count);
            Vector2 randomPosition = eyeballGrid[randIndex] + Random.insideUnitCircle * 0.25f;
            eyeballGrid.RemoveAt(randIndex);
            StartCoroutine(ScaleFromZeroToOne(eyeball.transform, 0.5f));
            StartCoroutine(MoveToPosition(eyeball.transform, randomPosition, 9));
        }

        yield return new WaitForSeconds(1.5f);
        _animator.SetTrigger("SpellCast");

        foreach (var eyeball in eyeballs)
        {
            StartCoroutine(MoveObjectInDirectionAndDestroy(eyeball, transform.right, 12, 6));
        }

        if (coolDown > 0)
        {
            yield return new WaitForSeconds(coolDown);
        }

        _busy = false;
    }

    public IEnumerator EyeballShield(float coolDown)
    {
        Debug.Log("Start Eyeball Sheild");

        _busy = true;
        _animator.SetTrigger("SpellStart");
        yield return new WaitForSeconds(8 / 12f);

        var shield = Instantiate(eyeballShieldPrefab, transform.parent);
        _instantiatedObjects.Add(shield);
        shield.transform.position = transform.position;

        foreach (Transform t in shield.transform)
        {
            if(t.gameObject.name == "Eyeball")
            {
                StartCoroutine(ScaleFromZeroToOne(t, 1));
            }
        }

        yield return new WaitForSeconds(1f);

        Vector3 lineStart = transform.position + transform.up * 8;

        if(MainCamera.instance)
        {
            lineStart.x = MainCamera.instance.transform.position.x - 7.5f;
        }
        else
        {
            lineStart.x += transform.rotation == Quaternion.identity ? 3 : - 18;
        }

        var eyeballs = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            var eyeball = Instantiate(eyeballPrefab, transform.parent);
            eyeball.transform.position = lineStart + Vector3.right * i * 5f;
            eyeballs.Add(eyeball);
            _instantiatedObjects.Add(eyeball);
            StartCoroutine(ScaleFromZeroToOne(eyeball.transform, 0.5f));
        }

        yield return new WaitForSeconds(0.5f);

        _animator.SetTrigger("SpellCast");

        foreach (var eyeball in eyeballs)
        {
            StartCoroutine(MoveObjectInDirectionAndDestroy(eyeball, Vector3.down, 6, 12));
        }

        StartCoroutine(MoveObjectInDirectionAndDestroy(shield, transform.right, 14, 8));

        yield return new WaitForSeconds(8/12f);
        yield return StartCoroutine(Jump(4, 0.65f, 3));

        if (coolDown > 0)
        {
            yield return new WaitForSeconds(coolDown);
        }

        _busy = false;
    }

    //HELPERS
    public void FacePlayer()
    {
        var direction = (_player.position - transform.position).normalized;
        transform.rotation = direction.x < 0 ? Constants.flippedFacing : Quaternion.identity;
    }

    public IEnumerator Jump(float jumpHeight, float time, float horizontalDistance)
    {        
        _animator.SetTrigger("Jump");

        yield return new WaitForSeconds(2 / 12f);

        var timeToApex = time / 2f;
        var gravity = -(2 * jumpHeight) / Mathf.Pow(timeToApex, 2);
        var maxJumpVelocity = Mathf.Abs(gravity) * timeToApex;
        var velocity = new Vector3(0, maxJumpVelocity, 0);
        velocity.x = Mathf.Sign(transform.right.x) * horizontalDistance / time;

        var timer = 0f;
        while (timer < timeToApex)
        {
            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }

        StartCoroutine(CheckForLanding());

        while (timer < time)
        {
            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }

        while (!_controller2D.bottomEdge.touching)
        {
            timer += Time.deltaTime;
            _controller2D.Move(velocity * Time.deltaTime);
            velocity.y += gravity * Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator SetBusy(float time)
    {
        _busy = true;
        yield return new WaitForSeconds(time);
        _busy = false;
    }

    public IEnumerator ScaleFromZeroToOne(Transform t, float time)
    {
        var timer = 0f;
        while(timer < time)
        {
            timer += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / time);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    public IEnumerator MoveToPosition(Transform t, Vector3 targetPosition, float speed)
    {
        if (speed < 0)
        {
            Debug.LogError("MoveToPosition should not be passed a speed of 0 or less");
        }

        while(t && t.position != targetPosition)
        {
            t.position = Vector3.MoveTowards(t.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator MoveObjectInDirectionAndDestroy(GameObject obj, Vector3 direction, float speed, float lifespan)
    {
        var timer = 0f;
        while(timer < lifespan)
        {
            timer += Time.deltaTime;
            obj.transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }

        _instantiatedObjects.Remove(obj);
        Destroy(obj);
    }

    public IEnumerator CheckForLanding()
    {
        while (!_controller2D.bottomEdge.near)
        {
            yield return null;
        }
        _animator.SetTrigger("Land");
    }

    public IEnumerator JumpShot()
    {
        _animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(11 / 24f);

        var direction = (_player.position - transform.position).normalized;
        ProjectileManager.instance.Shoot(projectileStats, transform.position, direction);
    }

    private void OnDestroy()
    {
        foreach (var obj in _instantiatedObjects)
        {
            if (obj) { Destroy(obj); }
        }
    }

    public override void Pause()
    {
        if (!_paused)
        {
            _busy = false;
            StopAllCoroutines();
            foreach (var obj in _instantiatedObjects)
            {
                if (obj) { Destroy(obj); }
            }
            _instantiatedObjects.Clear();
        }
        base.Pause();
    }

    public override void Unpause()
    {
        if(_paused)
        {
            StartCoroutine(SetBusy(2));
        }
        base.Unpause();
    }
}
