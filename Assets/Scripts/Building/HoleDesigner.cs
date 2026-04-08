using UnityEngine;
using GolfGame.Core;
using GolfGame.Data;

namespace GolfGame.Building
{
    /// <summary>
    /// Handles the hole design UI/logic. Players choose par, length, hazards, and theme
    /// within a structured system to design a new hole.
    /// </summary>
    public class HoleDesigner : MonoBehaviour
    {
        // Current design choices
        private int selectedPar = 3;
        private HoleLength selectedLength = HoleLength.Short;
        private HoleHazardSet selectedHazards = HoleHazardSet.None;
        private int selectedTheme;

        // Validation
        private static readonly int[] ValidPars = { 3, 4, 5 };
        private static readonly HoleLength[] ValidLengthsForPar3 = { HoleLength.Short };
        private static readonly HoleLength[] ValidLengthsForPar4 = { HoleLength.Short, HoleLength.Medium };
        private static readonly HoleLength[] ValidLengthsForPar5 = { HoleLength.Medium, HoleLength.Long };

        public int SelectedPar => selectedPar;
        public HoleLength SelectedLength => selectedLength;
        public HoleHazardSet SelectedHazards => selectedHazards;
        public int SelectedTheme => selectedTheme;
        public long EstimatedCost => GameConstants.HoleConstructionCost;

        public event System.Action OnDesignChanged;

        public void SetPar(int par)
        {
            if (par < 3 || par > 5) return;
            selectedPar = par;

            // Auto-adjust length to be valid
            if (!IsLengthValidForPar(selectedLength, selectedPar))
            {
                selectedLength = GetDefaultLengthForPar(selectedPar);
            }

            OnDesignChanged?.Invoke();
        }

        public void SetLength(HoleLength length)
        {
            if (!IsLengthValidForPar(length, selectedPar)) return;
            selectedLength = length;
            OnDesignChanged?.Invoke();
        }

        public void SetHazards(HoleHazardSet hazards)
        {
            selectedHazards = hazards;
            OnDesignChanged?.Invoke();
        }

        public void SetTheme(int theme)
        {
            selectedTheme = theme;
            OnDesignChanged?.Invoke();
        }

        public HoleSaveData CreateHoleSaveData()
        {
            return new HoleSaveData
            {
                par = selectedPar,
                length = selectedLength,
                hazardSet = selectedHazards,
                themeIndex = selectedTheme,
                qualityLevel = 1,
                state = HoleSlotState.Empty
            };
        }

        public bool TryBuildHole()
        {
            var property = ServiceLocator.Get<PropertyManager>();
            if (property == null) return false;

            var holeData = CreateHoleSaveData();
            return property.TryStartHoleConstruction(holeData);
        }

        public bool IsLengthValidForPar(HoleLength length, int par)
        {
            HoleLength[] valid = par switch
            {
                3 => ValidLengthsForPar3,
                4 => ValidLengthsForPar4,
                5 => ValidLengthsForPar5,
                _ => ValidLengthsForPar3
            };

            foreach (var v in valid)
            {
                if (v == length) return true;
            }
            return false;
        }

        public HoleLength[] GetValidLengths()
        {
            return selectedPar switch
            {
                3 => ValidLengthsForPar3,
                4 => ValidLengthsForPar4,
                5 => ValidLengthsForPar5,
                _ => ValidLengthsForPar3
            };
        }

        private HoleLength GetDefaultLengthForPar(int par)
        {
            return par switch
            {
                3 => HoleLength.Short,
                4 => HoleLength.Medium,
                5 => HoleLength.Long,
                _ => HoleLength.Short
            };
        }

        public float GetEstimatedYardage()
        {
            return (selectedPar, selectedLength) switch
            {
                (3, HoleLength.Short) => 150f,
                (4, HoleLength.Short) => 280f,
                (4, HoleLength.Medium) => 350f,
                (5, HoleLength.Medium) => 450f,
                (5, HoleLength.Long) => 540f,
                _ => 150f
            };
        }

        public void ResetDesign()
        {
            selectedPar = 3;
            selectedLength = HoleLength.Short;
            selectedHazards = HoleHazardSet.None;
            selectedTheme = 0;
            OnDesignChanged?.Invoke();
        }
    }
}
