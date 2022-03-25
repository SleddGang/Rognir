using Rognir.NPCs.Rognir;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir
{
	public class Rognir : Mod
	{

		public Rognir()
		{
		}

		// public override void PostSetupContent()
		// {
		// 	Mod bossChecklist = ModLoader.GetMod("BossChecklist");
		// 	if (bossChecklist != null)
		// 	{
		// 		bossChecklist.Call(
		// 			"AddBoss",											// Call AddBoss in BossChecklist.
		// 			5.5f,												// Sets the progression level of the boss.  5.0 is Skeletron and 6.0 is Wall of Flesh.
		// 			ModContent.NPCType<RognirBoss>(),					// Npc id of the boss.
		// 			this,												// Mod.
		// 			"Rognir",											// Display name.  Could also be a localization.
		// 			(Func<bool>)(() => RognirWorld.downedRognir),		// downedRognir stores whether Rognir has been defeated or not.  Syncing and saving of this variable is handled in RognirWorld.cs.
		// 			ModContent.ItemType<Items.Rognir.FrozenCrown>(),	// Sets the spawn item.
		// 			new List<int> { },									// Collectables.  We don't really have any.
		// 			new List<int> { ModContent.ItemType<Items.Rognir.RognirBag>(), ModContent.ItemType<Items.FrozenHookItem>(), ModContent.ItemType<Items.RognirsAnchor>()},    // List of Rognir's drops,  Includes the bag.  
		// 			"Use a [i:" + ModContent.ItemType<Items.Rognir.FrozenCrown>() + "] in a snow biome.");	// Sets the message that tells the player how to spawn Rognir.  
		// 	}
		// }
	}
}