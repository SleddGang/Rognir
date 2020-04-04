using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir.Items
{
    class VikingCrown : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Makes undead in the frozen biome friendly.");
        }

        public override void SetDefaults()
        {
            item.width = 20;
            item.height = 20;
            item.accessory = true;
            item.value = Item.sellPrice(silver: 30);
            item.rare = ItemRarityID.Expert;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RognirPlayer>().vikingCrown = true;
        }
    }
}
