﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Rognir.Projectiles.Rognir;
using static Terraria.ModLoader.ModContent;

namespace Rognir.NPCs.Rognir
{
	/*
	 * Class Rognir is the base class of the Rognir boss. 
	 * It defines the defaults and AI for the boss.
	 */
	[AutoloadBossHead]
    class RognirBoss : ModNPC
    {
		private const float rogMaxSpeedOne = 5.0f;			// Rognir's max speed in stage one.
		private const float rogMaxSpeedTwo = 7.5f;			// Rognir's max speed in stage two.
		private const float rogAcceleration = 2000f;		// Rognir's acceleration divider.  A smaller number means a faster acceleration.
		private const float rogDashSpeedOne = 10f;			// Rognir's max dash speed in stage one.
		private const float rogDashSpeedTwo = 20f;          // Rognir's max dash speed in stage two.
		private const float rogSecondDashChance = 0.5f;     // Rognir's chance that he will do another dash in stage two.
		private const float rogShardVelocity = 30f;			// Rognir's ice shard velocity.

		private const int rogMinMoveTimer = 60;				// Rognir's minimum move timer
		private const int rogMaxMoveTimer = 90;				// Rognir's maximum move timer.
		private const int rogAttackCoolOne = 120;			// Rognir's attack cooldown for stage one.
		private const int rogAttackCoolTwo = 90;			// Rognir's attack cooldown for stage two.
		private const int rogChilledLenghtOne = 120;		// Rognir's chilled buff length for stage one.
		private const int rogChilledLenghtTwo = 300;		// Rognir's chilled buff length for stage two.
		private const int rogVikingSpawnCool = 300;			// Rognir's time until next viking spawn.

		private float moveTimer				// Stores the time until a new movement offset is chosen.
		{
			get => npc.ai[0];
			set => npc.ai[0] = value;
		}
		private float target				// 0 is targeting above the player.  1 is targeting left of the player. 2 is targeting right of the player.
		{
			get => npc.ai[1];
			set => npc.ai[1] = value;
		}
		private float anchorID				// Stores the y movement offset.
		{
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}
		private float stage                 // Stores the current boss fight stage.
		{
			get => npc.ai[3];
			set => npc.ai[3] = value;
		}

		private int attackCool = 240;		// Stores the cooldown until the next attack.
		private int attack = 0;				// Selects the attack to use.
		private int dashTimer = 0;          // Stores the countdown untl the dash is complete.
		private int vikingCool = 0;
		private Vector2 dashDirection;      // Direction of the current dash attack.
		private Vector2 targetOffset;       // Target position for movement.

		/*
		 * Method SetStaticDefaults overrides the default SetStaticDefaults from the ModNPC class.
		 * The method sets the DisplayName to Rognir.
		 */
		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rognir");
            Main.npcFrameCount[npc.type] = 14;
        }

		// Method SetDefaults declares the default settings for the boss.
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 50;
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

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(attackCool);
			writer.Write(dashTimer);
			writer.Write(vikingCool);
			writer.Write(attack);
			writer.Write(dashDirection.X);
			writer.Write(dashDirection.Y);
			writer.Write(targetOffset.X);
			writer.Write(targetOffset.Y);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			attackCool = reader.ReadInt32();
			dashTimer = reader.ReadInt32();
			vikingCool = reader.ReadInt32();
			attack = reader.ReadInt32();
			float dashX = reader.ReadSingle();
			float dashY = reader.ReadSingle();

			dashDirection = new Vector2(dashX, dashY);

			float targetX = reader.ReadSingle();
			float targetY = reader.ReadSingle();

			targetOffset = new Vector2(targetX, targetY);
		}

		// Updates frames for Rognir. To change frames, set the %= to how many ticks you want (numberOfTicks). Then change the frameCounter / n so that n = numberOfTicks / numberOfFrames
		public override void FindFrame(int frameHeight)
		{
			if (dashTimer <= 0 && stage == 1)
			{
				npc.frameCounter += 1.0; //This makes the animation run. Don't change this
				npc.frameCounter %= 60.0; //This makes it so that after NUMBER ticks, the animation resets to the beginning.
										  //To help you with timing, there are 60 ticks in one second.
				int frame = (int)(npc.frameCounter / 5) + 2; //Chooses an animation frame based on frameCounter.
				npc.frame.Y = frame * frameHeight; //Actually sets the frame
			}
			else if (stage == 2)
			{
				npc.frameCounter += 1.0; //This makes the animation run. Don't change this
				npc.frameCounter %= 60.0; //This makes it so that after NUMBER ticks, the animation resets to the beginning.
										  //To help you with timing, there are 60 ticks in one second.
				int frame = (int)(npc.frameCounter / 60) + 1; //Chooses an animation frame based on frameCounter.
				npc.frame.Y = frame * frameHeight; //Actually sets the frame
			}
			else if (dashTimer > 0)
			{
				npc.frame.Y = 0;
			}
			npc.spriteDirection = npc.direction; //Makes Rognir turn in the direction of his target.
		}

		//TODO Make boss AI less dumb.
		// Method AI defines the AI for the boss.
		public override void AI()
		{
			// Set the current stage based on current health.
			if ((stage != 1) && (npc.life > npc.lifeMax / 2))
			{
				stage = 1;
			}
			else if (stage != 2 && (npc.life < npc.lifeMax / 2))
			{
				SwitchStage();
				if (Main.netMode != 1)
				{
					stage = 2;
					npc.netUpdate = true;
				}
			}

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
				if (moveTimer <= 0)
				{
					if (Main.rand.NextFloat() > 0.8f)
					{
						NewPosition(player);
					}

					// Store a random amount of ticks until next update of the movement offset.
					moveTimer = (int)Main.rand.NextFloat(rogMinMoveTimer, rogMaxMoveTimer);

					// Update network.
					npc.netUpdate = true;
				}
			}

			if (dashTimer <= 0)
			{
				Vector2 targetPosition = player.Center + targetOffset;

				// Apply a velocity based on the distance between moveTo and the bosses current position and scale down the velocity.
				npc.velocity += (targetPosition - npc.Center) / rogAcceleration;

				/*
				 * Check if the velocity is above the maximum. 
				 * If so set the velocity to max.
				 */
				float speed = npc.velocity.Length();
				npc.velocity.Normalize();
				if (speed > rogMaxSpeedOne)
				{
					speed = rogMaxSpeedOne;
				}
				npc.velocity *= speed;

				/*
				 * Rotate Rognir based on his velocity.
				 */
				npc.rotation = npc.velocity.X / 50;
				if (npc.rotation > 0.1f)
					npc.rotation = 0.1f;
				else if (npc.rotation < -0.1f)
					npc.rotation = -0.1f;
			}
			else
				Dash();

			DoAttack();	

			npc.ai[0]--;
		}

		private void NewPosition(Player player)
		{
			Vector2 above = new Vector2(0, -300);
			Vector2 left = new Vector2(-300, -100);
			Vector2 right = new Vector2(300, -100);
			if (target == 0)
			{
				if (Main.rand.NextFloat() > 0.5f)
				{
					targetOffset = left;
					target = 1;
				}
				else
				{
					targetOffset = right;
					target = 2;
				}
			}
			else
			{
				targetOffset = above;
				target = 0;
			}
		}

		/*
		 * DoAttack selects which attack to do randomly and then calls the apropriate function.
		 */
		private void DoAttack()
		{
			// Get next attack ten tick before attack happens to avoid desync.
			if (attackCool == 10)
			{
				if (Main.netMode != 1)
				{
					attack = Main.rand.Next(3);     // Choose what attack to do.
					npc.netUpdate = true;
				}
			}

			if (stage == 2)
				SpawnViking();

			// If attack cooldown is still active then subtract one from it and exit.
			if (attackCool > 0)
			{
				attackCool -= 1;
				return;
			}

			// Check if in stage 1 or 2.
			if (stage == 1)
				attackCool = rogAttackCoolOne;                   // Reset attack cooldown to 60.
			else
				attackCool = rogAttackCoolTwo;

			switch (attack)
			{
				case 0:
					Dash();						// Perform a dash attack.
					break;
				case 1:							// Shoot out a shard.
				case 2:
					Shards();					// Shoot out a shard.  Same as case 1.
					break;
				default:
					return;
			}
		}

		/*
		 * Causes Rognir to perform a quick dash attack.
		 * Normal movement needs to be stopped durring the dash in AI().
		 */
		private void Dash()
		{
			if (dashTimer <= 0)
			{
				//npc.rotation = 0f;
				npc.velocity = Vector2.Zero;
				if (Main.netMode != 1)
				{
					// dashTimer is the number of ticks the dash will last.  Increase dashTimer to increase the lenght of the dash.
					dashTimer = 60;
					// Direction to dash in.
					dashDirection = Main.player[npc.target].Center - npc.Center;
					npc.netUpdate = true;
				}

				Main.PlaySound(SoundID.ForceRoar);
			}

			else
			{
				dashTimer--;

				// Get the speed of the dash and limit it.
				float speed = dashTimer;
				if (speed > rogDashSpeedOne && stage == 1)
				{
					speed = rogDashSpeedOne;
				}
				else if (speed > rogDashSpeedTwo && stage == 2)
				{
					speed = rogDashSpeedTwo;
				}

				// Normalize the direction, add the speed, and then update position.  
				dashDirection.Normalize();
				dashDirection *= speed;
				npc.position += dashDirection;

				// Face in the direction of the dash.
				if (dashDirection.X < 0)
				{
					npc.direction = 0;
				}
				else
				{
					npc.direction = 1;
				}

				if (dashTimer <= 0 && stage == 2 && Main.netMode != 1)
				{
					if (Main.rand.NextFloat() > rogSecondDashChance)
					{
						Dash();
					}
				}
			}
		}

		/*
		 * Shoots out an ice shard that attacks the player.
		 */
		private void Shards()
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

			Vector2 projVelocity = player.Center - npc.Center;
			projVelocity.Normalize();

			projVelocity *= rogShardVelocity;

			Projectile.NewProjectile(npc.Center, projVelocity, ProjectileType<RognirBossIceShard>(), 50, 0f, Main.myPlayer);

			if (stage == 2)
			{
				// Shoot out an ice shard 30 degrees offset
				Projectile.NewProjectile(npc.Center, projVelocity.RotatedBy((Math.PI / 180) * 30), ProjectileType<RognirBossIceShard>(), 50, 0f, Main.myPlayer);
				// Shoot out an ice shard 330 degrees offset
				Projectile.NewProjectile(npc.Center, projVelocity.RotatedBy((Math.PI / 180) * 330), ProjectileType<RognirBossIceShard>(), 50, 0f, Main.myPlayer);
			}
		}

		private void SpawnViking()
		{
			if (vikingCool > 0)
			{
				vikingCool--;
				return;
			}

			/*
			 * Checks a 3 by 3 area arround the center of rognir to see if 
			 * an undead viking can be spawned in.
			 */
			bool canSpawn = true;
			for (int i = -1; i < 2; i++)
			{
				for (int j = -1; j < 2; j++)
				{
					Tile tile = Main.tile[((int)npc.Center.X / 16) + i, ((int)npc.Center.Y / 16) + j];
					// Check if block is type 0 (air or dirt) or is not active and is solid.
					if ((tile.type != 0 || tile.active()) && Main.tileSolid[tile.type])
						canSpawn = false;
				}
			}
			if (canSpawn)
				NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, 167, 0, 0f, 0f, 0f, 0f, npc.target);		//Spawn undead viking

			vikingCool = rogVikingSpawnCool;	
		}
		
		/*
		 * Gets called when Rognir switches to stage two.
		 * Put code that needs to run at the start of stage two here.
		 */
		private void SwitchStage()
		{
			if (anchorID == 0)
			{
				anchorID = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<RognirBossAnchor>(), 0, npc.whoAmI);
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.Chilled, stage == 1 ? rogChilledLenghtOne : rogChilledLenghtTwo);        // Chilled buff.
		}
	}
}
