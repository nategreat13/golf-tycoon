using System;
using System.Collections.Generic;

namespace GolfGame.Core
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> listeners = new();

        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (!listeners.ContainsKey(type))
                listeners[type] = new List<Delegate>();
            listeners[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (listeners.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (!listeners.TryGetValue(type, out var list)) return;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ((Action<T>)list[i])?.Invoke(evt);
            }
        }

        public static void Clear()
        {
            listeners.Clear();
        }
    }

    // Game Events
    public struct ShotStartedEvent { public int strokeNumber; }
    public struct ShotCompletedEvent { public float distance; public int strokeNumber; }
    public struct BallHoledEvent { }
    public struct HoleCompletedEvent { public int holeIndex; public int strokes; public int par; }
    public struct RoundCompletedEvent { public int totalStrokes; public int totalPar; public int holeCount; }
    public struct CurrencyChangedEvent { public long oldAmount; public long newAmount; }
    public struct ConstructionStartedEvent { public int slotIndex; public float duration; }
    public struct ConstructionCompletedEvent { public int slotIndex; }
    public struct PropertyExpandedEvent { public int newTier; }
    public struct ReputationChangedEvent { public int oldReputation; public int newReputation; }
    public struct ClubUnlockedEvent { public string clubName; }
    public struct CourseVisitedEvent { public string courseOwnerId; public int score; }
}
