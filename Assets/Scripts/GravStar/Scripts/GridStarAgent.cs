using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridStarAgent : BaseAgent
{
    public GridStarPathFinder gridStarPathFinder;
    public override BasePathFinder pathFinder { get { return gridStarPathFinder; } }
    protected float _speedMod = 1;
    
    protected override IEnumerator NavigateTo(Vector3 position)
    {
        _navigatingTo = position;
        position.y += nodeOffset;

        while (transform.position != position)
        {
            var speed = _currentMoveSpeed * _speedMod;
            transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        currentNodeIndex = pathFinder.PositionToIndex(_navigatingTo.Value);
        _navigatingTo = null;
        _navigationCoroutine = null;
    }
}
