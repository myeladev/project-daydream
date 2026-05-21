using System;
using System.Linq;
using ProjectDaydream.Data;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.Objects.Furniture;
using ProjectDaydream.SaveData;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class TimeManager : MonoBehaviour, IDataPersistence
    {
        [SerializeField]
        private TimeSettings settings;
        public DateTime CurrentTime { get; private set; }

        public TimeSpan SunriseTime { get; private set; }
        public TimeSpan SunsetTime { get; private set; }
        public static TimeManager Instance { get; private set; }

        private DateTime StartDate => new DateTime(settings.startingYear, settings.startingMonth, settings.startingDay,
            settings.startingHour, settings.startingMinute, 0);

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            if (settings == null)
            {
                Debug.LogError("No time settings were found.");
            }
            
            SunriseTime = TimeSpan.FromHours(settings.sunriseHour);
            SunsetTime = TimeSpan.FromHours(settings.sunsetHour);
            CurrentTime = new DateTime(settings.startingYear, settings.startingMonth, settings.startingDay, settings.startingHour, settings.startingMinute, 0);
        }

        void Update()
        {
            CurrentTime = CurrentTime.AddSeconds(Time.deltaTime * settings.timeMultiplier);
        }
        
        public bool IsDayTime() => CurrentTime.TimeOfDay >= SunriseTime && CurrentTime.TimeOfDay <= SunsetTime;
        
        public string GetFriendlyTimeString()
        {
            return CurrentTime.ToString("HH:mm");
        }

        public void LoadData(GameData data)
        {
            TimeManagerSaveData saveData = data.timeManager;
            if (saveData == null || saveData.ticks == 0) return;

            CurrentTime = new DateTime(saveData.ticks);
            Debug.Log($"Loaded time: {CurrentTime}");
        }

        public void SaveData(ref GameData data)
        {
            data.timeManager = new TimeManagerSaveData() {
                ticks = CurrentTime.Ticks
            };
        }

        public int GetDay()
        {
            return (CurrentTime - StartDate).Days + 1;
        }
    }
    
    [Serializable]
    public class TimeManagerSaveData
    {
        public long ticks;
    }
}
