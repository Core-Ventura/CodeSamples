# CodeSamples

Here you will find some code samples of my projects:

**FD_CameraShake**
A simple script that I use all the time to shake the camera using tweens.

**FD_ShareScore**
A script that generates a texture with the player score and then share it via mobile utilities.

**FD_TileManager**
In Flappy Dragon there are set of towers (tiles) coming constantly to the player. I pooled these tiles and reposition them with this script, and evaluate some properties like the spacing between the tiles based on the current game difficulty.

**FD_Egg**
In the game there are plenty of eggs, each with contains different types of dragons and rarities. The eggs are ScriptableObjects that contains all that information, and are decoupled from their view representation.

**FD_PersistenceManager**
This is a singleton that I use to Save/Load the game data using EasySave3 and EasyMobile as auxiliary plugins to handle the mobile native APIs. 
