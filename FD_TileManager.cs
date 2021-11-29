using System.Linq;
using Microsoft.Win32;
using UnityEngine;
using Sirenix.OdinInspector;

namespace FlappyDragon
{
    public class FD_TileManager: MonoBehaviour
    {
        //public FD_Tower tower;
        [BoxGroup("Tile Manager References")] public FD_DragonController dragonController;
        [BoxGroup("Tile Manager Settings")] public int quantity;
        [BoxGroup("Tile Manager Settings")] public int initialDistance;
        [BoxGroup("Tile Manager Settings")] public float spacing;
        [BoxGroup("Tile Manager Settings")] public GameObject tileSwapper;

        [BoxGroup("Tiles"), ReadOnly] public FD_Tile[] tiles;

        private bool initialized;
        private bool tileSwapperInitialized;

        private float progressionLimit = 35f;
        private int tileCounter;
        
        private void Update()
        {
            if (FD_GameManager.instance.gameState == FD_GameManager.GameState.Gameplay && !initialized)
            {
                initialized = true;
                SpawnTiles();
                RepositionTileSwapper();
                RepositionTiles();
            }

            if (FD_GameManager.instance.gameState != FD_GameManager.GameState.Gameplay || !initialized ||
                !(dragonController.transform.position.x >= tileSwapper.transform.localPosition.x)) return;
            
            RepositionTileSwapper();
            
            spacing = FD_GameMode.instance.difficulty != 0 
                ? FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(1f) 
                : FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(FD_Tile.difficultyScore / progressionLimit);
                
            tiles[0].transform.position = new Vector3(tiles[quantity-1].transform.position.x + spacing, 0, 0);
            tiles[0].gameObject.SetActive(false);
            tiles[0].gameObject.SetActive(true);
            
            //Rearrange array
            FD_Tile aux = tiles[0];
            for (int i=0; i<quantity-1; i++)
            {
                tiles[i] = tiles[i+1];
            }
            tiles[quantity-1] = aux;
            tileCounter++;
            tiles[quantity-1].tileCounter = tileCounter;
            tiles[quantity-1].GetComponent<FD_Tile>().UpdateTile();

        }

        private void SpawnTiles()
        {
            Vector3 spawnPosition;
            tiles = new FD_Tile[quantity];
            tileCounter = FD_GameMode.instance.currentScore;

            for (int i = 0; i < quantity; i++)
            {
                spacing = FD_GameMode.instance.difficulty != 0 
                    ? FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(1f) 
                    : FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(FD_Tile.difficultyScore / progressionLimit);

                spawnPosition = new Vector3(dragonController.transform.position.x + initialDistance + spacing * i, 0, 0);
                tiles[i] = Instantiate(FD_GameMode.instance.currentWorld.tile, spawnPosition, Quaternion.identity).GetComponent<FD_Tile>();
                tiles[i].transform.parent = transform;
                tiles[i].name = "Tile "+ (i+1);
                tiles[i].dragonController = dragonController;
                tiles[i].Start();
                tileCounter++;
                tiles[i].tileCounter = tileCounter;
            }
        }

        private void RepositionTileSwapper()
        {
            spacing = FD_GameMode.instance.difficulty != 0 
                ? FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(1f) 
                : FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(FD_Tile.difficultyScore / progressionLimit);

            if (!tileSwapperInitialized)
            {
                tileSwapperInitialized = true;
                tileSwapper.transform.localPosition = new Vector3(tiles[0].transform.position.x + ((quantity)*spacing/2), 0, 0);
            } else {
                tileSwapper.transform.localPosition = new Vector3(tiles[1].transform.position.x + ((quantity)*spacing/2), 0, 0);                 
            }
        }

        public void RepositionTiles()
        {
            if (tiles.Length == 0) return;
            for (int i = 0; i < quantity; i++)
            {
                tiles[i].GetComponent<FD_Tile>().ResetDifficulty();
                
                spacing = FD_GameMode.instance.difficulty != 0 
                    ? FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(1f) 
                    : FD_GameMode.instance.currentWorld.spacingCurve.Evaluate(FD_Tile.difficultyScore / progressionLimit);
                    
                Vector3 spawnPosition = new Vector3(dragonController.transform.position.x + initialDistance + spacing * i, 0, 0);
                tiles[i].transform.position = spawnPosition;
            }
            RepositionTileSwapper();
        }
    }
}
