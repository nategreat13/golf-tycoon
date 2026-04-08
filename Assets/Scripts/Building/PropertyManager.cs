using System;
using System.Collections.Generic;
using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Building
{
    public class PropertyManager : MonoBehaviour
    {
        [SerializeField] private int maxHolesPerTier = 18;

        public string CourseName { get; private set; } = "My Course";
        public int CurrentTier { get; private set; } = 1;
        public int MaxHoles => CurrentTier;
        public bool IsExpanding { get; private set; }

        private List<HoleSaveData> holes = new();
        private DrivingRangeSaveData drivingRange;
        private long expansionStartTime;

        public IReadOnlyList<HoleSaveData> Holes => holes;
        public DrivingRangeSaveData DrivingRange => drivingRange;

        public event Action OnPropertyChanged;

        public void LoadFromSave(CourseSaveData data)
        {
            if (data == null)
            {
                data = CourseSaveData.CreateDefault();
            }

            CourseName = data.courseName;
            CurrentTier = data.currentTier;
            holes = data.holes ?? new List<HoleSaveData>();
            drivingRange = data.drivingRange ?? new DrivingRangeSaveData { level = 1 };
            IsExpanding = data.isExpanding;
            expansionStartTime = data.expansionStartTimeUtc;

            // Check if expansion completed while offline
            if (IsExpanding)
            {
                CheckExpansionCompletion();
            }

            // Check any construction timers
            CheckConstructionTimers();
        }

        public CourseSaveData GetSaveData()
        {
            return new CourseSaveData
            {
                courseName = CourseName,
                currentTier = CurrentTier,
                holes = new List<HoleSaveData>(holes),
                drivingRange = drivingRange,
                isExpanding = IsExpanding,
                expansionStartTimeUtc = expansionStartTime
            };
        }

        public int GetBuiltHoleCount()
        {
            int count = 0;
            foreach (var h in holes)
            {
                if (h.state == HoleSlotState.Built)
                    count++;
            }
            return count;
        }

        public int GetTotalHoleCount() => holes.Count;

        public bool CanBuildNewHole()
        {
            return GetTotalHoleCount() < MaxHoles && !IsExpanding;
        }

        public bool TryStartHoleConstruction(HoleSaveData newHole)
        {
            if (!CanBuildNewHole()) return false;

            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency == null || !currency.TrySpend(GameConstants.HoleConstructionCost))
                return false;

            newHole.slotIndex = holes.Count;
            newHole.state = HoleSlotState.UnderConstruction;
            newHole.constructionStartTimeUtc = DateTime.UtcNow.ToBinary();
            holes.Add(newHole);

            EventBus.Publish(new ConstructionStartedEvent
            {
                slotIndex = newHole.slotIndex,
                duration = GameConstants.HoleConstructionTime
            });

            OnPropertyChanged?.Invoke();
            return true;
        }

        public bool TryStartHoleUpgrade(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= holes.Count) return false;
            var hole = holes[slotIndex];
            if (hole.state != HoleSlotState.Built) return false;

            long cost = (long)(GameConstants.HoleUpgradeCostBase *
                Mathf.Pow(GameConstants.HoleUpgradeCostMultiplier, hole.qualityLevel));

            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (currency == null || !currency.TrySpend(cost)) return false;

            hole.state = HoleSlotState.Upgrading;
            hole.constructionStartTimeUtc = DateTime.UtcNow.ToBinary();

            OnPropertyChanged?.Invoke();
            return true;
        }

        public bool CanExpand()
        {
            if (IsExpanding) return false;
            int nextTier = GetNextTier();
            if (nextTier < 0) return false;

            // Check reputation requirement
            var rep = ServiceLocator.Get<Progression.ReputationSystem>();
            int requiredRep = nextTier switch
            {
                3 => GameConstants.Rep3HolesUnlock,
                9 => GameConstants.Rep9HolesUnlock,
                18 => GameConstants.Rep18HolesUnlock,
                _ => int.MaxValue
            };
            if (rep != null && rep.Reputation < requiredRep) return false;

            // Check currency
            long cost = GetExpansionCost();
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            return currency != null && currency.Amount >= cost;
        }

        public bool TryStartExpansion()
        {
            if (!CanExpand()) return false;

            long cost = GetExpansionCost();
            var currency = ServiceLocator.Get<Economy.CurrencyManager>();
            if (!currency.TrySpend(cost)) return false;

            IsExpanding = true;
            expansionStartTime = DateTime.UtcNow.ToBinary();

            OnPropertyChanged?.Invoke();
            return true;
        }

        public int GetNextTier()
        {
            for (int i = 0; i < GameConstants.CourseTiers.Length - 1; i++)
            {
                if (CurrentTier == GameConstants.CourseTiers[i])
                    return GameConstants.CourseTiers[i + 1];
            }
            return -1; // Already max
        }

        public long GetExpansionCost()
        {
            return CurrentTier switch
            {
                1 => GameConstants.Expansion1To3Cost,
                3 => GameConstants.Expansion3To9Cost,
                9 => GameConstants.Expansion9To18Cost,
                _ => long.MaxValue
            };
        }

        public float GetExpansionTime()
        {
            return CurrentTier switch
            {
                1 => GameConstants.HoleConstructionTime * 3f,
                3 => GameConstants.HoleConstructionTime * 6f,
                9 => GameConstants.HoleConstructionTime * 12f,
                _ => float.MaxValue
            };
        }

        private void Update()
        {
            CheckConstructionTimers();
            if (IsExpanding) CheckExpansionCompletion();
        }

        private void CheckConstructionTimers()
        {
            var time = ServiceLocator.Get<TimeManager>();
            if (time == null) return;

            bool changed = false;
            foreach (var hole in holes)
            {
                if (hole.state == HoleSlotState.UnderConstruction)
                {
                    if (time.HasTimePassed(hole.constructionStartTimeUtc, GameConstants.HoleConstructionTime))
                    {
                        hole.state = HoleSlotState.Built;
                        hole.constructionStartTimeUtc = 0;
                        changed = true;

                        EventBus.Publish(new ConstructionCompletedEvent { slotIndex = hole.slotIndex });

                        var rep = ServiceLocator.Get<Progression.ReputationSystem>();
                        rep?.AddReputation(GameConstants.ReputationPerHoleBuilt);
                    }
                }
                else if (hole.state == HoleSlotState.Upgrading)
                {
                    if (time.HasTimePassed(hole.constructionStartTimeUtc, GameConstants.HoleUpgradeTime))
                    {
                        hole.state = HoleSlotState.Built;
                        hole.qualityLevel++;
                        hole.constructionStartTimeUtc = 0;
                        changed = true;

                        EventBus.Publish(new ConstructionCompletedEvent { slotIndex = hole.slotIndex });
                    }
                }
            }

            if (changed) OnPropertyChanged?.Invoke();
        }

        private void CheckExpansionCompletion()
        {
            var time = ServiceLocator.Get<TimeManager>();
            if (time == null || !IsExpanding) return;

            if (time.HasTimePassed(expansionStartTime, GetExpansionTime()))
            {
                int nextTier = GetNextTier();
                if (nextTier > 0)
                {
                    CurrentTier = nextTier;
                    IsExpanding = false;
                    expansionStartTime = 0;

                    EventBus.Publish(new PropertyExpandedEvent { newTier = nextTier });

                    var rep = ServiceLocator.Get<Progression.ReputationSystem>();
                    rep?.AddReputation(GameConstants.ReputationForExpansion);

                    OnPropertyChanged?.Invoke();
                }
            }
        }

        public void SetCourseName(string name)
        {
            CourseName = name;
            OnPropertyChanged?.Invoke();
        }
    }
}
