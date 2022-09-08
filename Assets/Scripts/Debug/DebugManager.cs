using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using System.Reflection;
using System.Net;
using System.Net.Security;
using Random = UnityEngine.Random;
using Rewired;
using UnityEngine.Networking;

[DisallowMultipleComponent]
public class DebugManager : MonoBehaviour
{
    public bool _active;

#if DEBUG || ALPHA
    // Update is called once per frame
    void Update ()
    {
        var keyboard = ReInput.controllers.Keyboard;

        if (keyboard != null && keyboard.GetKey(KeyCode.LeftShift))
        {
            if(keyboard.GetKeyDown(KeyCode.D))
            {
                _active = !_active;
                UISounds.instance.Confirm();
            }

            if (_active)
            {
                var player = PlayerManager.instance.player1;
                RoomAbstract room = null;
                RoomLayout layout = null;
                if (LayoutManager.instance)
                {
                    layout = LayoutManager.instance.layout;
                }

                if (keyboard.GetKeyDown(KeyCode.O))
                {
                    EnemyManager.instance.DestroyAllEnemies();
                }

                if (keyboard.GetKeyDown(KeyCode.K))
                {
                    player.Hurt(10000);
                }

                if (keyboard.GetKey(KeyCode.Q))
                {
                    int item = 0;
                    if (keyboard.GetKeyDown(KeyCode.Alpha1)) { item = 1; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha2)) { item = 2; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha3)) { item = 3; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha4)) { item = 4; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha5)) { item = 5; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha6)) { item = 6; }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha7)) { item = 7; }

                    if (item > 0)
                    {
                        if (layout != null)
                        {
                            var minorItems = Mathf.Lerp(0, layout.minorItemsAdded.Count, item / 7f);
                            for (int i = 0; i < minorItems; i++)
                            {
                                var minorItem = layout.minorItemsAdded[i];
                                SaveGameManager.activeGame.minorItemIdsCollected.Add(i);
                                player.CollectMinorItem(minorItem);
                            }

                            for (int i = 0; i < item; i++)
                            {
                                if (i < layout.itemOrder.Count)
                                    player.CollectMajorItem(layout.itemOrder[i]);
                            }

                            var bonusItems = Mathf.Lerp(0, layout.bonusItemsAdded.Count, item / 7f);
                            for (int i = 0; i < bonusItems; i++)
                            {
                                player.CollectMajorItem(layout.bonusItemsAdded[i]);
                            }
                        }
                        else
                        {
                            var minorItems = (int)Mathf.Lerp(0, RoomLayout.standardMinorItemCount, item / 7f);
                            var bonusItems = (int)Mathf.Lerp(0, 4, item / 7f);

                            var evenSplit = Constants.EvenSplit(minorItems, 9);
                            //standardMinorItemCount = 35
                            player.redScrap = evenSplit[0];
                            player.blueScrap = evenSplit[1];
                            player.greenScrap = evenSplit[2];
                            player.healthUps = evenSplit[3];
                            player.speedUps = evenSplit[4];
                            player.energyUps = evenSplit[5];
                            player.damageUps = evenSplit[6];
                            player.attackUps = evenSplit[7];
                            player.shotSpeedUps = evenSplit[8];

                            if (item > 0) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.Slide : MajorItem.Arachnomorph);
                            if (item > 1) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.DoubleJump : MajorItem.PowerJump);
                            if (item > 2) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.ElectroCharge : MajorItem.LightningGun);
                            if (item > 3) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.Flamethrower : MajorItem.FireBolt);
                            if (item > 4) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.JetPack : MajorItem.ViridianShell);
                            if (item > 5) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.RocketLauncher : MajorItem.ExplosiveBolt);
                            if (item > 6) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.Dash : MajorItem.PhaseShot);

                            if (bonusItems > 0) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.CelestialCharge : MajorItem.ETHChip);
                            if (bonusItems > 1) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.RegenerationHelm : MajorItem.HunterKiller);
                            if (bonusItems > 2) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.TripleShot : MajorItem.MegaDamage);
                            if (bonusItems > 3) player.CollectMajorItem(Random.value > 0.5f ? MajorItem.AuraBolt : MajorItem.BigBolt);
                        }
                        return;
                    }
                }

                if(keyboard.GetKey(KeyCode.C))
                {
                    if (keyboard.GetKeyDown(KeyCode.Backspace))
                    {
                        player.CollectMajorItem(MajorItem.TheRedKey);
                        player.CollectMajorItem(MajorItem.TheGreenKey);
                        player.CollectMajorItem(MajorItem.TheBlueKey);
                        player.CollectMajorItem(MajorItem.TheBlackKey);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha1))
                    {
                        player.CollectMajorItem(MajorItem.BuzzsawShell);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                    {
                        player.CollectMajorItem(MajorItem.Phaserang);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                    {
                        player.CollectMajorItem(MajorItem.PowerShield);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha4))
                    {
                        player.CollectMajorItem(MajorItem.HiveHelm);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha5))
                    {
                        player.CollectMajorItem(MajorItem.RepairKit);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha6))
                    {
                        player.CollectMajorItem(MajorItem.NecroluminantSpray);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha7))
                    {
                        player.CollectMajorItem(MajorItem.PrismaticOrb);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha8))
                    {
                        player.CollectMajorItem(MajorItem.LilTyr);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha9))
                    {
                        player.CollectMajorItem(MajorItem.LilOrphy);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Alpha0))
                    {
                        player.CollectMajorItem(MajorItem.HoverBoots);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Minus))
                    {
                        player.CollectMajorItem(MajorItem.ArtificeHelm);
                    }
                    else if (keyboard.GetKeyDown(KeyCode.Plus))
                    {
                        player.CollectMajorItem(MajorItem.Arachnomorph);
                    }
                    return;
                }

                if(keyboard.GetKeyDown(KeyCode.A))
                {
                    if (layout != null)
                    {
                        if (SaveGameManager.activeGame.collectRate != 1)
                        {
                            for (int i = 0; i < layout.minorItemsAdded.Count; i++)
                            {
                                var item = layout.minorItemsAdded[i];
                                SaveGameManager.activeGame.minorItemIdsCollected.Add(i);
                                player.CollectMinorItem(item);
                            }

                            foreach (var item in layout.itemOrder)
                            {
                                player.CollectMajorItem(item);
                            }

                            foreach (var item in layout.bonusItemsAdded)
                            {
                                player.CollectMajorItem(item);
                            }
                        }
                        else
                        {
                            Debug.Log("Collection Rate is already 100%");
                        }
                    }
                    else
                    {
                        //standardMinorItemCount = 35
                        player.redScrap = 4;
                        player.blueScrap = 4;
                        player.greenScrap = 4;
                        player.healthUps = 3;
                        player.speedUps = 3;
                        player.energyUps = 3;
                        player.damageUps = 4;
                        player.attackUps = 3;
                        player.shotSpeedUps = 3;

                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.Slide : MajorItem.Arachnomorph);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.DoubleJump : MajorItem.PowerJump);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.ElectroCharge : MajorItem.LightningGun);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.Flamethrower : MajorItem.FireBolt);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.JetPack : MajorItem.ViridianShell);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.RocketLauncher : MajorItem.ExplosiveBolt);

                        var roll = Random.Range(0, 3);
                        switch(roll)
                        {
                            case 0:
                                player.CollectMajorItem(MajorItem.Dash);
                                break;
                            case 1:
                                player.CollectMajorItem(MajorItem.PhaseShot);
                                break;
                            case 2:
                                player.CollectMajorItem(MajorItem.PhaseShell);
                                break;
                        }
                        

                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.CelestialCharge : MajorItem.ETHChip);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.RegenerationHelm : MajorItem.HunterKiller);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.TripleShot : MajorItem.MegaDamage);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.AuraBolt : MajorItem.BigBolt);
                        player.CollectMajorItem(Random.value > 0.5f ? MajorItem.BuzzOrb : MajorItem.ShieldOrb);
                    }
                }

                if(keyboard.GetKeyDown(KeyCode.S))
                {
                    player.redScrap = 99;
                    player.blueScrap = 99;
                    player.greenScrap = 99;
                    player.grayScrap = 99;
                }

                if (keyboard.GetKeyDown(KeyCode.H))
                {
                    if (keyboard.GetKey(KeyCode.LeftControl))
                    {
                        player.Hurt(1f);
                    }
                    else
                    {
                        player.health = player.maxHealth;
                    }
                }

                if (keyboard.GetKeyDown(KeyCode.E))
                {
                    player.energy = player.maxEnergy;
                }

                if(keyboard.GetKeyDown(KeyCode.M))
                {
                    player.RevealEnvironments(new List<EnvironmentType>
                    {
                        EnvironmentType.BeastGuts,
                        EnvironmentType.BuriedCity,
                        EnvironmentType.Cave,
                        EnvironmentType.Factory,
                        EnvironmentType.Surface,
                        EnvironmentType.Glitch,
                        EnvironmentType.ForestSlums,
                        EnvironmentType.CoolantSewers,
                        EnvironmentType.CrystalMines,
                    });
                }

                if (layout != null)
                {
                    if(keyboard.GetKey(KeyCode.V))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetSaveRoom(layout.environmentOrder[1]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetSaveRoom(layout.environmentOrder[2]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetSaveRoom(layout.environmentOrder[3]);
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.T))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetTeleporterRoom(layout.environmentOrder[1]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetTeleporterRoom(layout.environmentOrder[2]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetTeleporterRoom(layout.environmentOrder[3]);
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.I))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetItemRoom(0);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetItemRoom(1);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetItemRoom(2);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha4))
                        {
                            room = layout.GetItemRoom(3);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha5))
                        {
                            room = layout.GetItemRoom(4);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha6))
                        {
                            room = layout.GetItemRoom(5);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha7))
                        {
                            room = layout.GetItemRoom(6);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha8))
                        {
                            room = layout.GetItemRoom(7);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha9))
                        {
                            room = layout.GetItemRoom(8);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha0))
                        {
                            room = layout.GetItemRoom(9);
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.B))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetBossRoom(0);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetBossRoom(1);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetBossRoom(2);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha4))
                        {
                            room = layout.GetBossRoom(3);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha5))
                        {
                            room = layout.roomAbstracts.Find((r) => r.assignedRoomInfo.roomType == RoomType.MegaBeast);
                            if(room == null)
                            {
                                room = layout.roomAbstracts.Find((r) => r.assignedRoomInfo.boss == BossName.BeastRemnants);
                            }
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha6))
                        {
                            room = layout.roomAbstracts.Find((r) => r.assignedRoomInfo.boss == BossName.MegaBeastCore);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha7))
                        {
                            room = layout.roomAbstracts.Find((r) => r.assignedRoomInfo.boss == BossName.GlitchBoss);
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.W))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetShopRoom(ShopType.Artificer);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetShopRoom(ShopType.GunSmith);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetShopRoom(ShopType.OrbSmith);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha4))
                        {
                            room = layout.GetShopRoom(ShopType.TheTraitor);
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.U))
                    {
                        if(keyboard.GetKeyDown(KeyCode.Alpha6))
                        {
                            MusicController.instance.PlaySecretBossMusic();
                        }
                    }
                    else if (keyboard.GetKey(KeyCode.N))
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetShrineRoom(ShrineType.Tyr);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha5))
                        {
                            room = layout.GetShrineRoom(ShrineType.Zurvan);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha6))
                        {
                            room = layout.GetShrineRoom(ShrineType.Hephaestus);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha7))
                        {
                            room = layout.GetShrineRoom(ShrineType.WadjetMikail);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha8))
                        {
                            room = layout.GetShrineRoom(ShrineType.BulucChabtan);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha9))
                        {
                            room = layout.GetShrineRoom(ShrineType.Orphiel);
                        }
                    }
                    else
                    {
                        if (keyboard.GetKeyDown(KeyCode.Alpha1))
                        {
                            room = layout.GetRoomAtPositon(layout.startingPosition);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha2))
                        {
                            room = layout.GetEnvironmentStart(layout.environmentOrder[1]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha3))
                        {
                            room = layout.GetEnvironmentStart(layout.environmentOrder[2]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha4))
                        {
                            room = layout.GetEnvironmentStart(layout.environmentOrder[3]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha5))
                        {
                            room = layout.GetEnvironmentStart(layout.environmentOrder[4]);
                        }
                        else if (keyboard.GetKeyDown(KeyCode.Alpha6))
                        {
                            room = layout.GetEnvironmentStart(layout.environmentOrder[5]);
                        }
                    }

                    if (room != null)
                    {
                        LayoutManager.instance.TeleportToPosition(room.gridPosition);
                    }
                }
            }
        }
    }

    public string EscapeCharacters(string text)
    {
        return UnityWebRequest.EscapeURL(text).Replace("+", "%20");
    }
#endif
}
