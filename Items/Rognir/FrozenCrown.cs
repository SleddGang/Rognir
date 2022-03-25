/*
 * YourMom is a placeholder for the real summon item to summon Rognir.  
 * TODO Replace YourMom.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Rognir.NPCs.Rognir;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;


namespace Rognir.Items.Rognir
{
    class FrozenCrown : ModItem
	{
		public override void AddRecipes()
		{
			// 1.4
			CreateRecipe()
				.AddIngredient(ItemID.GoldCrown, 1)
				.AddIngredient(ItemID.IceBlock, 20)
				.AddIngredient(ItemID.Bone, 50)
				.AddIngredient(ItemID.Bone, 50)
				.AddTile(TileID.WorkBenches)
				.Register();
			CreateRecipe()
				.AddIngredient(ItemID.PlatinumCrown, 1)
				.AddIngredient(ItemID.IceBlock, 20)
				.AddIngredient(ItemID.Bone, 50)
				.AddIngredient(ItemID.Bone, 50)
				.AddTile(TileID.WorkBenches)
				.Register();
			// ModRecipe recipe = new ModRecipe(mod);
			// recipe.AddIngredient(ItemID.GoldCrown, 1);
			// recipe.AddIngredient(ItemID.IceBlock, 20);
			// recipe.AddIngredient(ItemID.Bone, 50);
			// recipe.AddTile(TileID.WorkBenches);
			// recipe.SetResult(ModContent.ItemType<FrozenCrown>());
			// recipe.AddRecipe();
			//
			// recipe = new ModRecipe(mod);
			// recipe.AddIngredient(ItemID.PlatinumCrown, 1);
			// recipe.AddIngredient(ItemID.IceBlock, 20);
			// recipe.AddIngredient(ItemID.Bone, 50);
			// recipe.AddTile(TileID.WorkBenches);
			// recipe.SetResult(ModContent.ItemType<FrozenCrown>());
			// recipe.AddRecipe();
		}
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("The crown yearns for the cold.");
			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 13; // This helps sort inventory know this is a boss summoning item.
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 20;
			Item.rare = 9;
			Item.useTime = 45;
			Item.useAnimation = 45;
			Item.useStyle = 4;
			Item.UseSound = SoundID.Item44;
			Item.consumable = true;
		}

		// We use the CanUseItem hook to prevent a player from using this item while the boss is present in the world.
		public override bool CanUseItem(Player player)
		{
			if (NPC.AnyNPCs(ModContent.NPCType<NPCs.Rognir.RognirBoss>()) | !player.ZoneSnow | Main.dayTime)
			{
				return false;
			}
			return true;
		}

		// Defines what happens when the item is used.
		public override bool? UseItem(Player player)
		{
			NPC.SpawnOnPlayer(player.whoAmI, NPCType<NPCs.Rognir.RognirBoss>());
			SoundEngine.PlaySound(SoundID.Roar, player.position, 0);
			return true;
		}
	}
}
