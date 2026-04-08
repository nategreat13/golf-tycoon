using System;
using System.IO;
using UnityEngine;
using GolfGame.Data;

namespace GolfGame.Core
{
    public class SaveSystem : MonoBehaviour
    {
        private const string SAVE_FILE = "player_save.json";
        public PlayerSaveData Data { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);

        private void Awake()
        {
            Data = new PlayerSaveData();
        }

        public void Save()
        {
            try
            {
                Data.lastSaveTimeUtc = DateTime.UtcNow.ToBinary();
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public void Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    Data = JsonUtility.FromJson<PlayerSaveData>(json);
                }
                else
                {
                    Data = PlayerSaveData.CreateDefault();
                    Save();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                Data = PlayerSaveData.CreateDefault();
            }

            ApplyLoadedData();
        }

        private void ApplyLoadedData()
        {
            if (ServiceLocator.TryGet<Economy.CurrencyManager>(out var currency))
                currency.SetAmount(Data.currency);

            if (ServiceLocator.TryGet<Building.PropertyManager>(out var property))
                property.LoadFromSave(Data.courseData);

            if (ServiceLocator.TryGet<Progression.ReputationSystem>(out var rep))
                rep.SetReputation(Data.reputation);
        }

        public void GatherSaveData()
        {
            if (ServiceLocator.TryGet<Economy.CurrencyManager>(out var currency))
                Data.currency = currency.Amount;

            if (ServiceLocator.TryGet<Building.PropertyManager>(out var property))
                Data.courseData = property.GetSaveData();

            if (ServiceLocator.TryGet<Progression.ReputationSystem>(out var rep))
                Data.reputation = rep.Reputation;
        }
    }
}
