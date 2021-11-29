using UnityEngine;
using Sirenix.OdinInspector;
using EasyMobile;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ES3Types;
using Random = UnityEngine.Random;

namespace FlappyDragon
{   
    public class FD_PersistenceManager : MonoBehaviour
    {
        public static FD_PersistenceManager instance;

        // To store the opened Save Game.
        private SavedGame mySavedGame;

        [BoxGroup("SaveData")] public FD_SaveData saveData;

        private ES3Settings settings;
        private bool isSaving;
        private bool isDeleting;

        [BoxGroup("Daily Egg Pool")] public FD_Egg[] dailyEggs;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                SetupSettings();
            } else if (instance != null)
            {
                DestroyImmediate(gameObject);
            }
        }

        private void SetupSettings()
        {
            settings = new ES3Settings(ES3.EncryptionType.XXX, "XXXXXXXXXXXXXX")
            {
                directory = ES3.Directory.PersistentDataPath,
                location = ES3.Location.File,
                compressionType = ES3.CompressionType.Gzip
            };
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                LoadTimers();
            }
        }

        private void OpenSavedGame()
        {
            // Open a saved game named "My_Saved_Game" and resolve conflicts automatically if any.
            GameServices.SavedGames.OpenWithAutomaticConflictResolution("My_Saved_Game", OpenSavedGameCallback);
        }
        
        
        // Open saved game callback
        private void OpenSavedGameCallback(SavedGame savedGame, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                Debug.Log("<b><color=teal>PERSISTENCE ► </color></b> " + "Saved game opened successfully!");
                mySavedGame = savedGame; // Keep a reference for later operations      

                if(!isSaving) 
                {
                    ReadSavedGame(mySavedGame);
                } else {
                    // If we can save we will in both LOCAL and CLOUD
                    SaveLocalSaveData();                       // Save in LOCAL
                    byte[] bytes = ES3.LoadRawBytes(settings); // Transform in bytes
                    WriteSavedGame(mySavedGame, bytes);        // Write in CLOUD
                }

            } else {

                Debug.Log("<b><color=teal>PERSISTENCE ► </color></b> " + "Open saved game failed with error: " + error);
                if(!isSaving)
                {
                    LoadLocalSaveData();
                } else {
                    SaveLocalSaveData();
                }
            }
        }
 
        private void WriteSavedGame(SavedGame savedGame, byte[] data)
        {
            if (savedGame.IsOpen)
            {
                // The saved game is open and ready for writing
                GameServices.SavedGames.WriteSavedGameData(
                    savedGame,
                    data,
                    (updatedSavedGame, error) =>
                    {
                        if (string.IsNullOrEmpty(error))
                        {
                            Debug.Log("PERSITENCE: Cloud Saved Game Data has been written successfully!");
                        } else {
                            Debug.Log("PERSITENCE: Writing saved game data failed with error: " + error);
                        }
                    }
                );
            } else {
                Debug.Log("PERSITENCE: The Saved Game is not open, trying to opening it again and save.");
            }
        }

        private void ReadSavedGame(SavedGame savedGame)
        {
            if (savedGame.IsOpen)
            {
                // The saved game is open and ready for reading
                GameServices.SavedGames.ReadSavedGameData(
                    savedGame,
                    (game, data, error) =>
                    {
                        if (string.IsNullOrEmpty(error))
                        {
                            if (data.Length > 0)
                            {
                                if (ES3.KeyExists("SaveData", settings))
                                {
                                    // If both LOCAL and CLOUD exists we check which one is better!
                                    FD_SaveData localSave = ES3.Load<FD_SaveData>("SaveData", settings);
                                    ES3.SaveRaw(data, settings); // Override local data
                                    FD_SaveData cloudSave = ES3.Load<FD_SaveData>("SaveData", settings);

                                    if (cloudSave.totalPlayTime <= localSave.totalPlayTime)
                                    {
                                        ES3.Save("SaveData", localSave, settings);
                                        Debug.Log("READING: LOCAL was better than CLOUD. LOADING LOCAL SAVE");
                                    }
                                    else
                                    {
                                        // Else... the LOCAL data is overwritten by the CLOUD one, so the CLOUD one will be the selected
                                        // In the LoadLocalSaveData
                                        Debug.Log("READING: CLOUD was better than LOCAL. LOADING CLOUD SAVE");
                                    }
                                }
                                else
                                {
                                    // 1 - If LOCAL save doesnt exist BUT there is a CLOUD one, we will use the CLOUD one
                                    // 2 - After that, we will generate a new LOCAL save with the CLOUD data
                                    ES3.SaveRaw(data, settings);
                                    Debug.Log(
                                        "READING: LOCAL didnt exist, but there was a CLOUD save. LOADING CLOUD SAVE");
                                }
                            }
                            else
                            {
                                // If data length = 0, we try again until 5 seconds in iOS (in that case, a time out occurs!) due to possible false negatives in their API calls 
                                #if UNITY_IOS
                                timeOutTimer += 0.2f;
                                if (timeOutTimer < 5)
                                {
                                    Invoke(nameof(LoadSave), 0.2f);
                                }
                                else
                                {
                                    // There was no data in the cloud :[
                                    LoadLocalSaveData();
                                }
                                return;
                                #endif
                            }
                        }
                        else
                        {
                            // If there is an error reading the CLOUD data, we use LOCAL data
                            Debug.Log("READING: COULD NOT READ CLOUD DATA, LOADING LOCAL");
                        }

                        LoadLocalSaveData();
                    }
                );
            }
            else
            {
                // If we could NOT open the CLOUD save, we simply load the LOCAL one
                Debug.Log("READING: COULD NOT OPEN CLOUD DATA, LOADING LOCAL");
                LoadLocalSaveData();
            }
        }

        public void SaveGame()
        {
            // We save the game ALWAYS in LOCAL, and if logged, we will try to save it in the CLOUD aswell!
            SaveLocalSaveData();
            if (!FD_GameServicesManager.instance.isLogged()) return;
            isSaving = true;
            OpenSavedGame();
        }
        
        private void SaveLocalSaveData()
        {
            Debug.Log("PERSITENCE: Save LOCAL data");
            saveData.UpdateLastDate();
            ES3.Save("SaveData", saveData, settings);
            LoadCurrentDragon();
            LoadTimers();
        }

        // We will only load the content of the game once, at the beginning!
        public void LoadSave()
        {
            // If the player is logged, we try to open his save and check if we should use CLOUD or LOCAL
            if (FD_GameServicesManager.instance.isLogged())
            {
                isSaving = false;
                OpenSavedGame();
            }
            else
            {
                // If he is not logged, we simply load the LOCAL SAVE
                LoadLocalSaveData();
            }
        }
        
        public void LoadLocalSaveData()
        {
            // If no LOCAL SAVE exists, then we will use the default savedata
            if (!ES3.KeyExists("SaveData", settings))
            {
                Debug.Log("PERSISTENCE: NO KEY FOUNDED! Creating new SAVE!");
                GoToTitle();
                return;
            }
            
            Debug.Log("PERSISTENCE: Loading LOCAL data");
            ES3.LoadInto("SaveData", saveData, settings);
            LoadCurrentDragon();
            LoadTimers();
            GoToTitle();
        }

        public void GoToTitle()
        {
            if (FD_GameManager.instance.gameState == FD_GameManager.GameState.Boot)
            {
                FD_GameManager.instance.Title();
            }
        }
        
        public void LoadCurrentDragon()
        {
            if(saveData.currentDragonID > FD_DragonManager.instance.dragons.Count)
            {
                Debug.Log("DRAGON ID > DRAGON COUNT -> Dragon ID will be zero.");
                FD_GameMode.instance.currentDragon = FD_DragonManager.instance.dragons[0];
            } else
            {
                FD_GameMode.instance.currentDragon = FD_DragonManager.instance.GetDragon(saveData.currentDragonID);
            }
        }
        
        private void LoadTimers()
        {
            if (!saveData.hasPlayed)
            {
                saveData.hasPlayed = true;
                saveData.UpdateLastDate();
                saveData.dailyRefreshRemainingTime = 86400;
                saveData.totalPlayTime = 0;
            }
            
            DateTime nowTime  = DateTime.Now;
            DateTime lastTime = DateTime.Parse(saveData.lastDate, CultureInfo.InvariantCulture);

            TimeSpan timeSpan = nowTime - lastTime;
            int timeSpanSeconds = (int) timeSpan.TotalSeconds;

            if(timeSpanSeconds > 0)
            {
                // Egg Timers
                foreach(FD_EggItem eggItem in saveData.eggItems)
                {
                    if(eggItem.nestedSlot != -1 && eggItem.remainingTime > 0 && eggItem.remainingTime - timeSpanSeconds > 0)
                    {
                        eggItem.remainingTime -= timeSpanSeconds;

                    } else if(eggItem.nestedSlot != -1 && eggItem.remainingTime > 0 && eggItem.remainingTime - timeSpanSeconds <= 0)
                    {
                        eggItem.remainingTime = 0;
                    }
                }

                // Daily Refresh Timer
                if(saveData.dailyRefreshRemainingTime - timeSpanSeconds > 0)
                {
                    saveData.dailyRefreshRemainingTime -= timeSpanSeconds;
                } else {
                    saveData.dailyRefreshRemainingTime = 0;
                }
            }

            CancelInvoke();
            InvokeRepeating(nameof(UpdateEggItemsRemainingTime), 1, 1);
            InvokeRepeating(nameof(UpdateDailyRefreshRemainingTime), 1, 1);
        }

        public void UpdateEggItemsRemainingTime()
        {
            foreach (FD_EggItem eggItem in saveData.eggItems.Where(eggItem => eggItem.nestedSlot != -1))
            {
                if (eggItem.remainingTime > 0)
                {
                    eggItem.remainingTime -= 1;
                }
                else if (!eggItem.notified 
                         && FD_GameManager.instance.gameState != FD_GameManager.GameState.Boot
                         && FD_GameManager.instance.gameState != FD_GameManager.GameState.Nest
                         && FD_GameManager.instance.gameState != FD_GameManager.GameState.EggOpening)
                {
                    eggItem.notified = true;
                    FD_NotificationsManager.instance.SendEggNotification(eggItem.eggID);
                }
            }
        }

        public void UpdateDailyRefreshRemainingTime()
        {
            saveData.totalPlayTime += 1;
            
            if(saveData.dailyRefreshRemainingTime - 1 > 0)
            {
                saveData.dailyRefreshRemainingTime -= 1;
            } else {
                saveData.dailyRefreshRemainingTime = 86400; // 24h = 86400 sec.
                RefreshFreeEggOffers();
                RefreshDailyEggOffers();
                RefreshDailyBonus();
            }            
        }

        public void RefreshFreeEggOffers()
        {
            saveData.freeOfferEgg1Claimed = false;
            saveData.freeOfferEgg2Claimed = false;
            saveData.freeOfferEgg3Claimed = false;
            
            saveData.freeOfferEgg1ID = GetWeightedEggID();
            saveData.freeOfferEgg2ID = GetWeightedEggID();
            saveData.freeOfferEgg3ID = GetWeightedEggID();
        }

        private int GetWeightedEggID()
        {
            int randomType = Random.Range(0, 101);
            int randomID = Random.Range(0, 101);
            
            if (randomType <= 70)
            {
                return 
                    randomID <= 2  ? 6 :
                    randomID <= 7  ? 5 :
                    randomID <= 16 ? 4 :
                    randomID <= 34 ? 3 :
                    randomID <= 62 ? 2 :
                    1 ;    
            }
            return dailyEggs[Random.Range(0, dailyEggs.Length)].id;
        }
        
        public void RefreshDailyEggOffers()
        {
            saveData.dailyOfferEgg1ID = dailyEggs[Random.Range(0, dailyEggs.Length)].id;
            saveData.dailyOfferEgg2ID = dailyEggs[Random.Range(0, dailyEggs.Length)].id;
            while(saveData.dailyOfferEgg2ID == saveData.dailyOfferEgg1ID)
            {
                saveData.dailyOfferEgg2ID = dailyEggs[Random.Range(0, dailyEggs.Length)].id;
            }
            saveData.dailyOfferEgg3ID = dailyEggs[Random.Range(0, dailyEggs.Length)].id;
            while(saveData.dailyOfferEgg3ID == saveData.dailyOfferEgg1ID || saveData.dailyOfferEgg3ID == saveData.dailyOfferEgg2ID)
            {
                saveData.dailyOfferEgg3ID = dailyEggs[Random.Range(0, dailyEggs.Length)].id;
            }
        }

        public void RefreshDailyBonus()
        {
            if (saveData.dailyBonusDay == 6)
            {
                saveData.dailyBonusDay = 0;
            }
            else
            {
                saveData.dailyBonusDay++;
            }
            saveData.dailyBonusCollected = false;
        }
    }
}

