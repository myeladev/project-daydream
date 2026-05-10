using System;
using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.Objects.Furniture;
using ProjectDaydream.Objects.Items;

namespace ProjectDaydream.DataPersistence
{
    [System.Serializable]
    public class GameData
    {
        public PlayerSaveData player;
        public TimeManagerSaveData timeManager;
        public List<PropSaveData> props = new ();
        public List<FurnitureSaveData> furniture = new ();
    }
    
    [Serializable]
    public class PlayerSaveData
    {
        public float hunger;
        public float sleep;
        public float[] position;
        public float[] rotation;
    }
}