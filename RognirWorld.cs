using Rognir.Items.Rognir;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace Rognir
{
    class RognirWorld : ModSystem
    {
		public static bool downedRognir;

		public override void OnWorldLoad()
		{
			downedRognir = false;
		}

		public override void OnWorldUnload()
		{
			downedRognir = false;
		}

		public override void SaveWorldData(TagCompound tag)
		{
			if (downedRognir)
				tag["downedRognir"] = true;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			downedRognir = tag.ContainsKey("downedRognir");
		}

		public override void NetSend(BinaryWriter writer)
		{
			var flags = new BitsByte();
			flags[0] = downedRognir;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader)
		{
			BitsByte flags = reader.ReadByte();
			downedRognir = flags[0];
		}

		public override void PostWorldGen()
		{
			int itemToPlaceInDungeonChests = ItemType<FrozenCrown>();
			for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
			{
				Chest chest = Main.chest[chestIndex];
				// If you look at the sprite for Chests by extracting Tiles_21.xnb, you'll see that the 3rd chest is the Dungeon Chest. Since we are counting from 0, this is where 2 comes from. 36 comes from the width of each tile including padding. 
				if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == 2 * 36)
				{
					for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++)
					{
						if (chest.item[inventoryIndex].type == 0)
						{
							if (Main.rand.NextFloat() < 0.2f)
							{
								chest.item[inventoryIndex].SetDefaults(itemToPlaceInDungeonChests);
							}
							break;
						}
					}
				}
			}
		}
    }
}
