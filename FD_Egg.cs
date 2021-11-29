using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace FlappyDragon
{
    public class FD_Egg : ScriptableObject
    {

        [BoxGroup("Basic Information")] public int id;
        [BoxGroup("Basic Information")] public int sortId;
        [BoxGroup("Basic Information")] public new string name;

        [BoxGroup("Basic Information"), SuffixLabel("h.", Overlay = true), Range(0, 48)]
        public int incubationTime;

        [BoxGroup("Shop Information")]
        [PreviewField(75, ObjectFieldAlignment.Left)]
        public Sprite shopIcon;

        [BoxGroup("Dragon Pool")] public FD_Database database;
        [BoxGroup("Dragon Pool")]
        [EnumToggleButtons]
        public rarityDragonPool rarityPool;

        [BoxGroup("Dragon Pool")]
        [EnumToggleButtons]
        public typeDragonPool typePool;

        public enum rarityDragonPool
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythic,
            All
        }

        public enum typeDragonPool
        {
            Western,
            Leviathan,
            Feathered,
            Eastern,
            Mountain,
            Fairy,
            Cosmic,
            All
        }

        [BoxGroup("Egg Model"), InlineEditor(InlineEditorModes.LargePreview)]
        public Material material;

        [BoxGroup("Transform Ofset")]
        public Vector3 eggScale;

        public FD_Dragon GenerateDragon()
        {
            List<FD_Dragon> dragonPool = new List<FD_Dragon>();

            if (rarityPool == rarityDragonPool.All)
            {
                dragonPool = database.dragons;
            } 
            else 
            {
                foreach (FD_Dragon dragon in database.dragons)
                {
                    if ((int) dragon.rarity == (int) rarityPool)
                    {
                        dragonPool.Add(dragon);
                    }
                }
            }
            
            return dragonPool[Random.Range(0, dragonPool.Count)];
        }
    }
}
