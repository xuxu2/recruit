using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    [Serializable]
    class ApiResponse
    {
        public bool success;
        public int code;
        public List<BuildingDataProxy> data;
    }

    [Serializable]
    class BuildingSubDataProxy
    {
        public int bd_id;
        public string 동;
        public int 지면높이;
    }

    [Serializable]
    class BuildingDataProxy
    {
        public List<RoomType> roomtypes;
        public BuildingSubDataProxy meta;
    }

    [Serializable]
    class RoomTypeSubDataProxy
    {
        public int 룸타입id;
    }

    [Serializable]
    class RoomType
    {
        public List<string> coordinatesBase64s;
        public RoomTypeSubDataProxy meta;
    }

    class BuildingDataBase
    {
        public int bd_id;
        public string label;
        public int elevation;
        public Mesh mesh;
    }
}

