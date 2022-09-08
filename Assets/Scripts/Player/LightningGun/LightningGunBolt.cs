using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningGunBolt : MonoBehaviour
{
    public Team team;
    private float _damagePerSecond;
    public float damagePerSecond
    {
        get
        {
            return _damagePerSecond;
        }

        set
        {
            _damagePerSecond = value;
            if(_damageCreatureTrigger)
            {
                _damageCreatureTrigger.damage = _damagePerSecond;
            }
        }
    }

    public float granularity = 0.125f;
    public float minMagnitude = 0;
    public float maxMagnitude = 0.5f;
    public float fps = 12;
    new public SpriteRenderer light;

    private DamageCreatureTrigger _damageCreatureTrigger;
    private LineRenderer _lineRenderer;
    private float _lastTime;
    private float _fpsTimer;
    private float[] _perpindiculars;
    private Team _assignedTeam;

    private float _damageTimer;

	public void Awake ()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.sortingLayerName = "AboveFadeAways";
        _lineRenderer.sortingOrder = 5;
        _damageCreatureTrigger = GetComponentInChildren<DamageCreatureTrigger>();
        _damageCreatureTrigger.damageType = DamageType.Electric;
        _damageCreatureTrigger.enabled = true;
        AssignTeam();
	}

    public void DrawLightning(Vector3 origin, Vector3 target, bool slerp)
    {
        if(_assignedTeam != team)
        {
            AssignTeam();
        }

        if (granularity <= 0)
        {
            Debug.LogError("LightningGunBolt granularity can't be <= 0!");
            return;
        }

        var direction = (target - origin).normalized;
        var distance = Vector3.Distance(origin, target);
        var subDivisions = (int)(distance / granularity);
        var positions = new List<Vector3>();

        _fpsTimer += Time.time - _lastTime;
        _lastTime = Time.time;
        if(_fpsTimer > 1/fps)
        {
            _fpsTimer = 0;
            _perpindiculars = CalculatePerpindiculars(subDivisions);

            _lineRenderer.numCornerVertices = _lineRenderer.numCornerVertices == 0 ? 1 : 0;            
        }

        positions.Add(origin);
        if (subDivisions > 0 && _perpindiculars.Length > 0)
        {
            for (float i = 1; i <= subDivisions; i++)
            {
                var pos = slerp ? Vector3.Slerp(origin, target, i / subDivisions) : Vector3.Lerp(origin, target, i / subDivisions);
                pos += Vector3.Cross(direction, Vector3.forward) * _perpindiculars[((int)i - 1) % _perpindiculars.Length] * Mathf.Lerp(0.25f, 1f, i / subDivisions);
                positions.Add(pos);
            }
        }

        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());

        var center = Vector3.Lerp(origin, target, 0.5f);
        _damageCreatureTrigger.transform.position = center;
        _damageCreatureTrigger.transform.localScale = new Vector3(distance, maxMagnitude*2,1);
        _damageCreatureTrigger.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);

        if(light)
        {
            light.transform.position = center;
        }
    }

    public void AssignTeam()
    {
        _assignedTeam = team;
        _damageCreatureTrigger.team = team;

        switch(team)
        {
            case Team.Enemy:
                _damageCreatureTrigger.gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
                break;
            case Team.DeathMatch0:
            case Team.DeathMatch1:
            case Team.DeathMatch2:
            case Team.DeathMatch3:
            case Team.Player:
                _damageCreatureTrigger.gameObject.layer = LayerMask.NameToLayer("Projectile");
                break;
            case Team.None:
                _damageCreatureTrigger.gameObject.layer = LayerMask.NameToLayer("CreatureOnly");
                break;
        }
    }

    public float[] CalculatePerpindiculars(float subDivisions)
    {
        var perps = new List<float>();
        for (float i = 1; i <= subDivisions; i++)
        {
            perps.Add(Random.Range(minMagnitude, maxMagnitude) * (Random.value > 0.5f ? -1 : 1));            
        }
        return perps.ToArray();
    }
}
