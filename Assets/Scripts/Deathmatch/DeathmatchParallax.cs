using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeathmatchParallax : MonoBehaviour
{
    new public MainCamera camera;
    [Range(0, 1)]
    public float xAmount;
    [Range(0, 1)]
    public float yAmount;
    public Vector3 originalLocalPostion;    
    public bool original = true;
    public int id = 0;

    private IEnumerator Start()
    {
        while(!DeathmatchManager.instance)
        {
            yield return null;
        }

        originalLocalPostion = transform.localPosition;
        if (original)
        {
            AssignId(id);
            DeathmatchManager.instance.onJoin.AddListener(OnPlayerJoin);

            foreach (var p in DeathmatchManager.instance.players)
            {
                if (p.playerId != id)
                {
                    OnPlayerJoin(p.playerId);
                }
            }
        }
        else
        {
            DeathmatchManager.instance.onDrop.AddListener(OnPlayerDrop);
        }
    }

    private void SetPosition()
    {
        var original = (transform.parent ? transform.parent.TransformPoint(originalLocalPostion) : originalLocalPostion);
        transform.position = new Vector3(Mathf.Lerp(original.x, camera.transform.position.x, xAmount), Mathf.Lerp(original.y, camera.transform.position.y, yAmount), originalLocalPostion.z);
    }

    public void AssignId(int id)
    {
        this.id = id;
        camera = DeathmatchManager.instance.cameras[id];
        camera.OnFrameFinished += SetPosition;
        var backUpLayer = LayerMask.NameToLayer("BackupLayer" + id);
        gameObject.layer = backUpLayer;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = backUpLayer;
        }
    }

    private void OnPlayerJoin(int id)
    {
        if (id == this.id) return;

        var copy = Instantiate(gameObject).GetComponent<DeathmatchParallax>();
        copy.original = false;
        copy.transform.parent = transform.parent;
        copy.originalLocalPostion = originalLocalPostion;
        copy.AssignId(id);
    }

    public void OnPlayerDrop(int id)
    {
        if (id == this.id) { Destroy(gameObject); }
    }

    private void OnDestroy()
    {
        if(original && DeathmatchManager.instance)
        {
            DeathmatchManager.instance.onJoin.AddListener(OnPlayerJoin);
        }

        if(camera)
        {
            camera.OnFrameFinished -= SetPosition;
        }
    }
}
