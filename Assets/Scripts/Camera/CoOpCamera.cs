using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoOpCamera : BaseCamera
{
    public static CoOpCamera instance;
    private Vector3 _position;
    private AudioListener _audioListener;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            camera = GetComponent<Camera>();
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        if (!PlayerManager.instance.trueCoOp) { Destroy(gameObject); }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        var players = PlayerManager.instance.players;

        if (players.Count > 1)
        {
            camera.enabled = true;
            AudioListener audioListener = null;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minPlayerX = float.MaxValue;
            var maxPlayerX = float.MinValue;
            var minPlayerY = float.MaxValue;
            var maxPlayerY = float.MinValue;

            foreach (var p in players)
            {
                if (p.mainCamera.camera) { p.mainCamera.camera.enabled = false; }
                if (p.mainCamera.audioListener) { audioListener = p.mainCamera.audioListener; }
                var cHalfWidth = p.mainCamera.halfWidth;
                var cHalfHeight = p.mainCamera.orthographicSize;
                var pos = p.mainCamera.transform.position;
                var cMin = new Vector2(pos.x - cHalfWidth, pos.y - cHalfHeight);
                var cMax = new Vector2(pos.x + cHalfWidth, pos.y + cHalfHeight);
                if (cMin.y < minY) { minY = cMin.y; }
                if (cMax.y > maxY) { maxY = cMax.y; }
                if (cMin.x < minX) { minX = cMin.x; }
                if (cMax.x > maxX) { maxX = cMax.x; }
                if (p.position.x < minPlayerX) { minPlayerX = p.position.x; }
                if (p.position.x > maxPlayerX) { maxPlayerX = p.position.x; }
                if (p.position.y < minPlayerY) { minPlayerY = p.position.y; }
                if (p.position.y > maxPlayerY) { maxPlayerY = p.position.y; }
            }
            _position = new Vector3(Mathf.Lerp(minPlayerX, maxPlayerX, 0.5f), Mathf.Lerp(minPlayerY, maxPlayerY, 0.5f), -10);
            var deltaX = ((maxPlayerX - minPlayerX) + 3) / 24;
            var deltaY = ((maxPlayerY - minPlayerY) + 4) / 14;
            var maxDelta = Mathf.Max(deltaX, deltaY);
            camera.orthographicSize = Mathf.Clamp(7 * maxDelta, 7, 14);
            var halfWidth = this.halfWidth;
            var halfHeight = camera.orthographicSize;
            var thisMaxX = _position.x + halfWidth;
            var thisMinX = _position.x - halfWidth;
            var roomBounds = LayoutManager.CurrentRoom ? LayoutManager.CurrentRoom.worldBounds.size : new Vector3(maxX - minX, maxY - minY);
            if (roomBounds.x <= 24) { _position.x = Mathf.Lerp(minX, maxX, 0.5f); }
            else if (thisMaxX > maxX) { _position.x -= thisMaxX - maxX; }
            else if (thisMinX < minX) { _position.x += minX - thisMinX; }

            var thisMaxY = _position.y + halfHeight;
            var thisMinY = _position.y - halfHeight;
            if (roomBounds.y <= 14) { _position.y = Mathf.Lerp(minY, maxY, 0.5f); }
            else if (thisMaxY > maxY) { _position.y -= thisMaxY - maxY; }
            else if (thisMinY < minY) { _position.y += minY - thisMinY; }
        }
        else
        {
            camera.enabled = false;
        }

        transform.position = _position;
    }
}
