using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Golf.Course
{
    /// <summary>
    /// Runtime representation of a playable golf hole.
    /// Instantiates geometry from HoleData and manages tee, fairway, green, pin positions.
    /// </summary>
    public class HoleInstance : MonoBehaviour
    {
        [Header("Hole Components")]
        [SerializeField] private Transform teeMarker;
        [SerializeField] private Transform pinPosition;
        [SerializeField] private Transform greenCenter;
        [SerializeField] private Collider holeCollider; // Trigger for "ball holed"
        [SerializeField] private GreenPhysics greenPhysics;

        [Header("Hazard Containers")]
        [SerializeField] private Transform hazardContainer;

        [Header("Prefabs")]
        [SerializeField] private GameObject bunkerPrefab;
        [SerializeField] private GameObject waterPrefab;
        [SerializeField] private GameObject treePrefab;

        public HoleData Data { get; private set; }
        public Vector3 TeePosition => teeMarker != null ? teeMarker.position : transform.position;
        public Vector3 PinPosition => pinPosition != null ? pinPosition.position : transform.position + Vector3.forward * 100;
        public Transform PinTransform => pinPosition;
        public float DistanceTeeToPin => Vector3.Distance(TeePosition, PinPosition);

        public void Initialize(HoleData holeData)
        {
            Data = holeData;

            // Position tee and pin based on yardage
            if (teeMarker != null)
                teeMarker.localPosition = Vector3.zero;

            if (pinPosition != null)
            {
                // Calculate pin position with optional dogleg
                Vector3 pinPos;
                if (Mathf.Abs(holeData.doglegAngle) > 0.1f)
                {
                    // Two-segment path: tee to dogleg point, dogleg point to pin
                    float doglegDist = holeData.yardage * holeData.doglegPosition;
                    float remainingDist = holeData.yardage - doglegDist;

                    Vector3 doglegPoint = Vector3.forward * doglegDist;
                    Vector3 doglegDir = Quaternion.AngleAxis(holeData.doglegAngle, Vector3.up) * Vector3.forward;
                    pinPos = doglegPoint + doglegDir * remainingDist;
                }
                else
                {
                    pinPos = Vector3.forward * holeData.yardage;
                }

                pinPos.y = holeData.elevationChange;
                pinPosition.localPosition = pinPos;
            }

            // Setup green
            if (greenPhysics != null)
            {
                greenPhysics.SetSlope(holeData.greenSlopeDirection, holeData.greenSlope);
            }

            if (greenCenter != null)
            {
                greenCenter.localPosition = pinPosition.localPosition;
                greenCenter.localScale = Vector3.one * holeData.greenSize;
            }

            // Spawn hazards
            SpawnHazards(holeData);
        }

        private void SpawnHazards(HoleData holeData)
        {
            if (holeData.hazards == null || hazardContainer == null) return;

            // Clear existing hazards
            foreach (Transform child in hazardContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var hazard in holeData.hazards)
            {
                GameObject prefab = hazard.type switch
                {
                    HazardType.Bunker => bunkerPrefab,
                    HazardType.Water => waterPrefab,
                    HazardType.Trees => treePrefab,
                    _ => null
                };

                if (prefab == null) continue;

                // Calculate world position from normalized coordinates
                Vector3 holeDir = (PinPosition - TeePosition).normalized;
                Vector3 perpendicular = Vector3.Cross(holeDir, Vector3.up);
                float distAlongHole = hazard.longitudinalPosition * DistanceTeeToPin;
                float lateralDist = hazard.lateralOffset * 20f; // 20 yards max lateral offset

                Vector3 pos = TeePosition + holeDir * distAlongHole + perpendicular * lateralDist;

                GameObject hazardObj = Instantiate(prefab, pos, Quaternion.identity, hazardContainer);
                hazardObj.transform.localScale *= hazard.size;
            }
        }
    }
}
