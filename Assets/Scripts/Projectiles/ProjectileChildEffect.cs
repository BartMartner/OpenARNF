using UnityEngine;
using System.Collections;

public class ProjectileChildEffect : MonoBehaviour
{
    public bool scaleBySize;
    private Projectile _projectile;
    private Vector3 _originalScale;

	// Use this for initialization
	public void Awake ()
    {
        _originalScale = transform.localScale;
        _projectile = GetComponentInParent<Projectile>();

        _projectile.onDeath += Remove;
        _projectile.onRecycle += Remove;
        _projectile.onSize += Size;

        gameObject.layer = _projectile.gameObject.layer;

        var renderer = GetComponent<Renderer>();
        if (renderer && _projectile.renderer)
        {
            renderer.sortingLayerID = _projectile.renderer.sortingLayerID;
            renderer.sortingOrder = _projectile.renderer.sortingOrder - 1;
        }

        var teamComponents = gameObject.GetInterfacesInChildren<IHasTeam>(true);
        foreach (var teamComponent in teamComponents)
        {
            teamComponent.team = _projectile.stats.team;
        }

        var damageTriggers = GetComponentsInChildren<DamageCreatureTrigger>();
        foreach (var trigger in damageTriggers)
        {
            Constants.SetCollisionForTeam(trigger.collider2D, _projectile.team);
        }
	}

    public void Size()
    {
        if (scaleBySize) { transform.localScale = _originalScale * _projectile.currentSize; }
    }

	public void Remove()
    {
        _projectile.onDeath -= Remove;
        _projectile.onRecycle -= Remove;
        _projectile.onSize -= Size;
        Destroy(gameObject);
    }
}
