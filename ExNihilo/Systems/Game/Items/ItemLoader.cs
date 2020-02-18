
using System;
using System.Collections.Generic;
using System.IO;

namespace ExNihilo.Systems.Game.Items
{
    public static class ItemLoader
    {
        private static readonly List<Equipment> _equipSet = new List<Equipment>();
        private static readonly List<InstantItem> _instantSet = new List<InstantItem>();
        private static readonly List<UseItem> _useSet = new List<UseItem>();

        public static void LoadItems(string materialFile)
        {
            Equipment.SetUpMaterials(materialFile);
            var fileSet = Directory.GetFiles(Environment.CurrentDirectory + "/Content/Items/");
            foreach (var file in fileSet)
            {
                
            }
        }

    }
}
