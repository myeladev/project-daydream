using System;
using ProjectDaydream.Data;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField]
        private TimeSettings settings;
        public DateTime CurrentTime { get; private set; }

        public TimeSpan SunriseTime { get; private set; }
        public TimeSpan SunsetTime { get; private set; }

        private void Awake()
        {
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
    }
}
