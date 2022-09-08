using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DevilerParent : MonoBehaviour, IPausable
{
    public DevilerFace face;
    public DevilerChild[] children;
    public SimpleAnimation[] gapAnims;
    public List<SimpleMaskAnimator> segmentTransformMasks;
    public List<Animator> lumps;
    public bool busy { get; private set; }
    private DevilerChild _activeChild;
    private Enemy _enemy;
    private int _phase;
    private int _lastPhase = -1;
    private int _phaseStep;
    private bool _paused;

    //0 = straight
    //1 = arc
    //2 = sine
    private int _moveMode = 0;

    //0 = random to match
    //1 = descend to ascend
    //2 = ascend to random
    private int _moveOrder = 0;

    private float _lumpSpeed = 12f;
    private float _lumpArc = 6;

    private List<Action[]> phases;

    public void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _enemy.onStartDeath.AddListener(Die);

        children = GetComponentsInChildren<DevilerChild>();
       
        phases = new List<Action[]>()
        {
            new Action[]
            {
                () => Wait(2),
                () => Shoot(3,1),
            },

            new Action[]
            {
                () => Wait(2),
                SlowDodge,
                () => Wait(1),
                () => Shoot (3, 0.88f),
            },

            new Action[]
            {
                () => Wait(1f),
                SlowDodge,
                () => Wait(1f),
                () => Shoot(3,0.75f),
                () => Wait(1),
                Jump,
                ()=>Wait(0.5f),
                () => Shoot(3,1),
            },

            new Action[]
            {
                ()=>Wait(1f),
                Jump,
                () => Wait(1f),
                () => Shoot(3,0.75f),
                () => Wait(2),
                Swarm,
            },

            new Action[]
            {
                ()=>Wait(2f),
                () => Shoot(5,0.25f),
                ()=>Wait(2f),
                () => Shoot(5,0.25f),
                ()=>Wait(2f),
                () => Shoot(5,0.25f),
                ()=>Wait(2f),
                Swarm,
                () => Wait(0.5f),
                Jump,
                () => Wait(1),
                Swarm,
                () => Wait(2f),
                () => Shoot(10,0.5f),
            },

             new Action[]
            {
                ()=>Wait(2f),
                () => Shoot(5,0.25f),
                ()=>Wait(0.5f),
                SlowDodge,
                Jump,
                Swarm,
            },
        };
    }

    public IEnumerator Start()
    {
        var parentRoom = GetComponentInParent<Room>();

        while (!PlayerManager.instance || (LayoutManager.instance &&
        (LayoutManager.CurrentRoom != parentRoom ||
        (LayoutManager.CurrentRoom && LayoutManager.CurrentRoom.roomAbstract == null))))
        {
            yield return null;
        }

        var maxDistance = 0f;
        var bestChoice = children[0];
        if (PlayerManager.instance)
        {
            var closestPlayer = PlayerManager.instance.GetClosestPlayer(transform.position);
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                child.parent = this;
                var d = Vector3.Distance(child.transform.position, closestPlayer.position);
                if (d > maxDistance)
                {
                    maxDistance = d;
                    bestChoice = child;
                }
            }
        }

        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (child == bestChoice)
            {
                child.active = true;
                child.EnableAllSegments();
                _activeChild = child;
                face.Show(_activeChild);
            }
            else
            {
                child.active = false;
                child.DisableAllSegments();
            }
        }
    }

    private void Update()
    {
        if (!busy && !face.busy && !_paused)
        {
            Debug.Log("Phase Started");
            _phase = Mathf.CeilToInt(Mathf.Lerp(0, phases.Count - 1, 1 - _enemy.health / _enemy.maxHealth));
            _phaseStep = _phase == _lastPhase ? (_phaseStep + 1) % phases[_phase].Length : 0;
            phases[_phase][_phaseStep]();
            _lastPhase = _phase;
        }
    }

    public void Wait(float delay)
    {
        StartCoroutine(WaitRoutine(delay));
    }

    public void Shoot(int shots, float delay)
    {
        var closestPlayer = PlayerManager.instance.GetClosestPlayer(transform.position);
        face.Shoot(closestPlayer.transform, shots, delay);
    }

    public void SlowDodge()
    {
        var others = children.Where(c => !c.active).ToArray();
        var pick = others[Random.Range(0, others.Length)];
        _moveMode = 0;
        _moveOrder = 0;
        _lumpSpeed = _phase < phases.Count / 2 ? 12 : 16;
        var delay = _phase < phases.Count / 2 ? 0.75f : 1f;
        StartCoroutine(Move(pick, delay));
    }

    public void Jump()
    {
        var others = children.Where(c => !c.active).ToArray();
        var pick = others[Random.Range(0, others.Length)];
        _moveMode = 1;
        _moveOrder = 1;
        _lumpArc = 8;
        _lumpSpeed = 16;
        StartCoroutine(Move(pick, 0.05f));
    }

    public void Swarm()
    {
        var others = children.Where(c => !c.active).ToArray();
        var pick = others[Random.Range(0, others.Length)];
        _moveMode = 2;
        _moveOrder = 2;
        _lumpArc = 1.25f;
        _lumpSpeed = 12;
        StartCoroutine(Move(pick, 0.025f));
    }

    public IEnumerator Move(DevilerChild target, float delay)
    {
        busy = true;
        _enemy.notTargetable = true;
        face.Hide();
        while (face.busy) { yield return null; }

        var originIndices = _activeChild.usedIndices.ToArray();
        var targetIndices = target.usedIndices.ToArray();
        switch (_moveOrder)
        {
            case 0:
                originIndices.Shuffle();
                targetIndices = originIndices;
                break;
            case 1:
                targetIndices = targetIndices.Reverse().ToArray();
                break;
            case 2:
                originIndices = originIndices.Reverse().ToArray();
                targetIndices.Shuffle();
                break;
        }

        for (int i = 0; i < originIndices.Length; i++)
        {
            StartCoroutine(TransferLump(originIndices[i], _activeChild, targetIndices[i], target));
            yield return new WaitForSeconds(delay);
        }

        _activeChild.active = false;
        while (!target.IsComplete()) { yield return null; }

        target.active = true;
        _activeChild = target;
        face.Show(_activeChild);
        while (face.busy) { yield return null; }

        _enemy.notTargetable = false;
        busy = false;
    }

    public IEnumerator TransferLump(int originIndex, DevilerChild origin, int targetIndex, DevilerChild target)
    {
        origin.segments[originIndex] = 0;
        var position = origin.GetSegmentPos(originIndex);
        var tMask = SpawnTMask(position, true);
        var lump = SpawnLump(position);
        lump.gameObject.SetActive(true);
        lump.Play("Form");
        yield return null;
        origin.RefreshSegments();
        yield return new WaitForSeconds(1);
        tMask.gameObject.SetActive(false);
        StartCoroutine(MoveLump(lump, targetIndex, target));
    }

    public IEnumerator MoveLump(Animator lump, int index, DevilerChild target)
    {
        var position = target.GetSegmentPos(index);
        switch (_moveMode)
        {
            case 0:
                while (lump.transform.position != position)
                {
                    lump.transform.position = Vector3.MoveTowards(lump.transform.position, position, _lumpSpeed * Time.deltaTime);
                    yield return null;
                }
                break;
            case 1:
                {
                    var start = lump.transform.position;
                    var end = position;
                    var delta = end - start;
                    var control = start + delta * 0.5f;
                    var tangentDirection = Vector3.Cross(delta.normalized, Vector3.forward);
                    control += tangentDirection * (tangentDirection.y > 0 ? _lumpArc : -_lumpArc);
                    //approximateTime
                    var time = (Vector3.Distance(start, control) + Vector3.Distance(control, end)) / _lumpSpeed;
                    var timer = 0f;
                    while (timer < time)
                    {
                        timer += Time.deltaTime;
                        var t = timer / time;
                        lump.transform.position = (((1 - t) * (1 - t)) * start) + (2 * t * (1 - t) * control) + ((t * t) * end);
                        yield return null;
                    }
                    break;
                }
            case 2:
                {
                    var start = lump.transform.position;
                    var end = position;
                    var delta = end - start;
                    var direction = delta.normalized;
                    var tangentDirection = Vector3.Cross(direction, Vector3.forward);
                    var time = delta.magnitude / _lumpSpeed;
                    var timer = 0f;
                    while (timer < time)
                    {
                        timer += Time.deltaTime;
                        var t = timer / time;
                        var p = start + direction * delta.magnitude * t;
                        p += tangentDirection * Mathf.Sin(t * 2 * Mathf.PI) * _lumpArc;
                        lump.transform.position = p;
                        yield return null;
                    }
                }
                break;
        }

        lump.Play("Merge");
        var tMask = SpawnTMask(position, false);

        yield return new WaitForSeconds(1);
        target.segments[index] = 1;
        target.RefreshSegments();
        yield return null;
        lump.gameObject.SetActive(false);
        tMask.gameObject.SetActive(false);
    }

    public SimpleMaskAnimator GetTransformMask()
    {
        var mask = segmentTransformMasks.FirstOrDefault(m => !m.gameObject.activeInHierarchy);
        if (!mask)
        {
            mask = Instantiate(segmentTransformMasks[0]);
            mask.gameObject.SetActive(false);
            mask.transform.parent = transform;
            segmentTransformMasks.Add(mask);
        }

        return mask;
    }

    public SimpleMaskAnimator SpawnTMask(Vector2 position, bool lumpSeparate)
    {
        var tMask = GetTransformMask();
        tMask.gameObject.SetActive(true);
        tMask.transform.position = position;
        tMask.reverse = !lumpSeparate;
        tMask.clearFrameOnEnd = lumpSeparate;
        tMask.Reset();
        if(lumpSeparate) { tMask.SetFrame(); }
        tMask.fps = tMask.simpleAnim.sprites.Length;
        return tMask;
    }

    public Animator SpawnLump(Vector3 position)
    {
        var lump = lumps.FirstOrDefault(m => !m.gameObject.activeInHierarchy);
        if (!lump)
        {
            lump = Instantiate(lumps[0], transform);
            lump.name = "Lump " + lumps.Count;
            lump.gameObject.SetActive(false);
            lumps.Add(lump);
        }

        lump.transform.position = position;
        return lump;
    }

    public IEnumerator WaitRoutine(float time)
    {
        busy = true;
        yield return new WaitForSeconds(time);
        busy = false;
    }

    public void Die()
    {
        StopAllCoroutines();
        StartCoroutine(DieRoutine());
    }

    public IEnumerator DieRoutine()
    {
        busy = true;

        var damageTriggers = GetComponentsInChildren<DamageCreatureTrigger>(true);
        foreach (var d in damageTriggers) {  d.enabled = false; }

        var rectDamage = GetComponentsInChildren<SimpleDamageTriggerRect>(true);
        foreach (var d in rectDamage) { Destroy(d); }

        if (face.gameObject.activeInHierarchy)
        {
            face.Hide(true);
        }
        while (face.busy) { yield return null; }

        foreach (var child in children)
        {
            if(!child.gameObject.activeInHierarchy) { continue; }
            var indices = child.usedIndices.ToArray();
            indices.Shuffle();
            foreach (var i in indices)
            {
                if (child.segments[i] == 1)
                {
                    StartCoroutine(KillLump(i, child));
                    var time = Mathf.Lerp(0.3f, 0.1f, (float)i / indices.Length);
                    yield return new WaitForSeconds(time);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        while(lumps.Any(l => l.gameObject.activeInHierarchy))
        {
            yield return null;
        }
        busy = false;
        Destroy(gameObject);
    }

    public IEnumerator KillLump(int originIndex, DevilerChild origin)
    {
        origin.segments[originIndex] = 0;
        var position = origin.GetSegmentPos(originIndex);
        var tMask = SpawnTMask(position, true);
        var lump = SpawnLump(position);
        lump.gameObject.SetActive(true);
        lump.Play("Form");
        yield return null;
        origin.RefreshSegments();
        yield return new WaitForSeconds(1f);
        tMask.gameObject.SetActive(false);
        StartCoroutine(KillLump(lump));
    }

    public IEnumerator KillLump(Animator lump)
    {
        var speed = 0f;
        while (lump.gameObject.activeInHierarchy)
        {
            speed += Time.deltaTime * 18f;
            lump.transform.position += Vector3.down * Time.deltaTime * speed;
            if(Physics2D.OverlapCircle(lump.transform.position, 0.4f,Constants.defaultMask))
            {
                GibManager.instance.SpawnGibs(GibType.Meat, lump.transform.position, 1);
                GibManager.instance.SpawnBloodSplatter(lump.transform.position, 2);
                lump.gameObject.SetActive(false);
            }
            yield return null;
        }
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Unpause()
    {
        _paused = false;
    }
}
