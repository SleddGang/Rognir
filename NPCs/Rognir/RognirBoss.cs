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
            Main.npcFrameCount[npc.type] = 1;
        }

		// Method SetDefaults declares the default settings for the boss.
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 1;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.width = 197;
			npc.height = 311;
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
			// player is the current player that Rognir is targeting.
			Player player = Main.player[npc.target];

			/*
			 * Checks if the current player target is alive and active.  
			 * If not then the boss will run away and despawn.
			 */
			if (!player.active || player.dead)
			{
				npc.TargetClosest(true);
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

			// Target the closest player and turn towards it.
			npc.TargetClosest(true);
			// player is the currently targeted player.
			player = Main.player[npc.target];


			/*
			 * Checks if running on singleplayer, client, or server.
			 * True if not on client.
			 */
			if (Main.netMode != 1)
			{
				// Check if it is time to reupdate the movement offset.
				if (npc.ai[0] <= 0)
				{
					// Store the X and Y offset in ai[1] and ai[2].
					npc.ai[1] = Main.rand.NextFloat(-300, 300);
					npc.ai[2] = Main.rand.NextFloat(-100, 100);
					// Store a random amount of ticks until next update of the movement offset.
					npc.ai[0] = (int)Main.rand.NextFloat(30, 60);

					// Update network.
					npc.netUpdate = true;
				}
			}

			// moveTo is the location that the boss is going to arrive at.  Add the position above the players head plus a random offset.
			Vector2 moveTo = player.Center + new Vector2(0 + npc.ai[1], -300 + npc.ai[2]);
			// Gets the distance to moveTo.  May be used later.
			float distance = (float)Math.Sqrt(Math.Pow(moveTo.X - npc.Center.X, 2) + Math.Pow(moveTo.Y - npc.Center.Y, 2));

			// Apply a velocity based on the distance between moveTo and the bosses current position and scale down the velocity.
			npc.velocity += (moveTo - npc.Center) / (750);

			/*
			 * Check if velocity magnitude is greater than the max.
			 * If so then slow down the velocity.  
			 */
			if (npc.velocity.Length() > 5.0f)
			{
				npc.velocity *= 0.8f;
			}

			npc.ai[0]--;

		}
	}
}
