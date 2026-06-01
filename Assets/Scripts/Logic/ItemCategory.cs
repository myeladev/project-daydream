using System;

namespace ProjectDaydream.Logic
{
    [Flags]
    public enum ItemCategory
    {
        Backpack   = 1 << 0,
        Flashlight = 1 << 1,
    }
}
