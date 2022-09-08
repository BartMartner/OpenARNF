using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleTransmogrifier", menuName = "Player Activated Items/Module Transmogrifier", order = 1)]
public class ModuleTransmogrifier : PlayerActivatedItem
{
    public override void ButtonDown()
    {
        base.ButtonDown();

        if (Usable())
        {
            _player.energy -= energyCost;

            Room currentRoom = LayoutManager.CurrentRoom ?? FindObjectOfType<Room>();

            if (currentRoom != null)
            {
                var items = currentRoom.GetComponentsInChildren<MinorItemPickUp>();
                MainCamera.instance.Shake(0.1f);

                foreach (var item in items)
                {
                    var types = Enum.GetValues(typeof(MinorItemType)).Cast<MinorItemType>().ToList();
                    types.Remove(MinorItemType.None);
                    types.Remove(item.data.type);

                    var type = types[UnityEngine.Random.Range(0, types.Count)];

                    if (FXManager.instance)
                    {
                        FXManager.instance.SpawnFX(FXType.AnimeSplode, item.transform.position);
                    }

                    item.data.type = type;
                    var explodeChance = Mathf.Clamp(item.data.rerolls * 0.2f, 0, 0.8f);

                    if (UnityEngine.Random.value < explodeChance)
                    {
                        if (FXManager.instance)
                        {
                            FXManager.instance.SpawnFX(FXType.ExplosionMedium, item.transform.position);
                        }

                        if (SaveGameManager.activeGame != null)
                        {
                            if (item.data.globalID != -99)
                            {
                                SaveGameManager.activeGame.minorItemIdsCollected.Add(item.data.globalID);
                            }
                            if (Automap.instance) { Automap.instance.RefreshItems(); }
                        }

                        Destroy(item.gameObject);
                    }
                    else
                    {
                        if (FXManager.instance)
                        {
                            FXManager.instance.SpawnFX(FXType.AnimeSplode, item.transform.position);
                        }

                        MinorItemPickUp prefab = ResourcePrefabManager.instance.LoadGameObject("PickUps/" + item.data.type.ToString()).GetComponent<MinorItemPickUp>(); ;
                        MinorItemPickUp newItem = Instantiate(prefab, item.transform.position, Quaternion.identity, item.transform.parent) as MinorItemPickUp;
                        newItem.data = item.data;
                        newItem.data.rerolls++;
                        Destroy(item.gameObject);
                    }

                    SaveGameManager.instance.Save();
                }
            }
        }
    }
}
