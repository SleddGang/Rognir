using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir.NPCs.Rognir
{
	/*
	 * Class Rognir is the base class of the Rognir boss. 
	 * It defines the defaults and AI for the boss.
	 */
	[AutoloadBossHead]
    class RognirBoss : ModNPC
    {
		
		/*
		 * Method SetStaticDefaults> overrides the default SetStaticDefaults from the ModNPC class.
		 * The method sets the DisplayName to Rognir.
		 */
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rognir");
            Main.npcFrameCount[npc.type] = 2;
        }

		// Method SetDefaults declares the default settings for the boss.
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 15f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.buffImmune[24] = true;

			//TODO Replace Boos_Fight_2 with final music.
			// Sets the music that plays when the boss spawns in and the priority of the music.  
			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Boos_Fight_2");
			musicPriority = MusicPriority.BossMedium;
		}

		//TODO Make boss AI less dumb.
		// Method AI defines the AI for the boss.
		public override void AI()
		{			
			Player player = Main.player[npc.target];
			if (!player.active || player.dead)
			{
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				if (!player.active || player.dead)
				{
					npc.velocity = new Vector2(0f, 10f);
					if (npc.timeLeft > 10)
					{
						npc.timeLeft = 10;
					}
					return;
				}
			}
			
			if (Main.netMode != 1)
			{
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				Vector2 moveTo = player.Center;
				npc.velocity = (moveTo - npc.Center) / 10;
				//npc.velocity = new Vector2(-0.5f, -0.5f);
				npc.netUpdate = true;
			}
		}
	}
}
