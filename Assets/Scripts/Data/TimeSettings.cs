using System;
using UnityEngine;

namespace ProjectDaydream.Data
{
    [CreateAssetMenu(fileName = "New Time Settings", menuName = "Time/New Time Settings")]
    public class TimeSettings : ScriptableObject
    {
        public float timeMultiplier = 2000f;
        public int startingDay = 24;
        public int startingMonth = 8;
        public int startingYear = 2054;
        public int startingHour = 3;
        public int startingMinute = 33;
        public float sunriseHour = 7f;
        public float sunsetHour = 17f;
    }
}