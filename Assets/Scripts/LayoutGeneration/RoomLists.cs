using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomLists
{
    public List<RoomInfo> generalRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> majorItemRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> minorItemRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> startingRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> bossRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> transitionRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> saveRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> teleporterRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> megaBeastRoomInfos = new List<RoomInfo>();
    public List<RoomInfo> shopRoomsInfos = new List<RoomInfo>();
    public List<RoomInfo> shrineRoomsInfos = new List<RoomInfo>();
    public List<RoomInfo> otherSpecialRoomsInfos = new List<RoomInfo>();

    public List<RoomInfo> GetList(RoomType type)
    {
        switch (type)
        {
            case RoomType.StartingRoom:
                return startingRoomInfos;
            case RoomType.Teleporter:
                return teleporterRoomInfos;
            case RoomType.TransitionRoom:
                return transitionRoomInfos;
            case RoomType.MegaBeast:
                return megaBeastRoomInfos;
            case RoomType.OtherSpecial:
                return otherSpecialRoomsInfos;
            case RoomType.SaveRoom:
                return saveRoomInfos;
            case RoomType.Shop:
                return shopRoomsInfos;
            case RoomType.Shrine:
                return shrineRoomsInfos;
            case RoomType.ItemRoom:
                return majorItemRoomInfos;
            case RoomType.BossRoom:
                return bossRoomInfos;
            case RoomType.None:
            default:
                return generalRoomInfos;
        }
    }
}
