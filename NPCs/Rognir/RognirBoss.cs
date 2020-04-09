using Microsoft.Xna.Framework;
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
	/// <summary>
	/// Class Rognir is the base class of the Rognir boss. 
	/// It defines the defaults and AI for the boss.
	/// </summary>
	[AutoloadBossHead]
    class RognirBoss : ModNPC
    {
		private const float rogMaxSpeedOne = 5.0f;			// Rognir's max speed in stage one.
		private const float rogMaxSpeedTwo = 7.5f;			// Rognir's max speed in stage two.
		private const float rogAcceleration = 2000f;		// Rognir's acceleration divider.  A smaller number means a faster acceleration.
		private const float rogDashSpeedOne = 15f;			// Rognir's max dash speed in stage one.
		private const float rogDashSpeedTwo = 25f;          // Rognir's max dash speed in stage two.
		private const float rogSecondDashChance = 0.75f;	// Rognir's chance that he will do another dash in stage two.
		private const float rogSecondDashReduction = 0.25f;	// Rognir's change in dash chance each dash.  Limits the number of dashes Rognir can do.
		private const float rogShardVelocity = 7.5f;		// Rognir's ice shard velocity.


		private const int rogMinMoveTimer = 60;				// Rognir's minimum move timer
		private const int rogMaxMoveTimer = 90;				// Rognir's maximum move timer.
		private const int rogAttackCoolOne = 105;			// Rognir's attack cooldown for stage one.
		private const int rogAttackCoolTwo = 75;            // Rognir's attack cooldown for stage two.
		private const int rogDashLenght = 60;				// Rognir's dash timer to set the lenght of the dash.
		private const int rogChilledLenghtOne = 120;		// Rognir's chilled buff length for stage one.
		private const int rogChilledLenghtTwo = 120;        // Rognir's chilled buff length for stage two.
		private const int rogShardDamage = 10;				// Rognir's ice shard damage.
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
		
		private float dashCounter
		{
			get => npc.localAI[0];
			set => npc.localAI[0] = value;
		}
		private float spinTimer				// Stores the timer for when the boss is spinning while he changes stage.
		{
			get => npc.localAI[1];
			set => npc.localAI[1] = value;
		}

		private int attackCool = 240;		// Stores the cooldown until the next attack.
		private int attack = 0;				// Selects the attack to use.
		private int dashTimer = 0;          // Stores the countdown untl the dash is complete.
		private int vikingCool = 0;
		private Vector2 dashDirection;      // Direction of the current dash attack.
		private Vector2 targetOffset;       // Target position for movement.

		/// <summary>
		///  Method SetStaticDefaults overrides the default SetStaticDefaults from the ModNPC class.
		/// The method sets the DisplayName to Rognir.
		/// </summary>
	   public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rognir");
            Main.npcFrameCount[npc.type] = 3;
        }

		/// <summary>
		/// Method SetDefaults declares the default settings for the boss.
		/// </summary>
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 3000;
			npc.damage = 32;
			npc.defense = 10;
			npc.knockBackResist = 0f;
			npc.width = 198;
			npc.height = 310;
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
			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/RognirStage1");
			musicPriority = MusicPriority.BossMedium;
			bossBag = ItemType<Items.Rognir.RognirBag>();
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			if (numPlayers > 1)
			{
				for (int i = 0; i < numPlayers; i++)
				{
					npc.lifeMax = (int)(npc.lifeMax * 1.35);
				}
			}
		}

		/// <summary>
		/// Sends extra ai variables over the network.
		/// </summary>
		/// <param name="writer">Writer to send the variables through.</param>
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

		/// <summary>
		/// Receives data sent through <c>SendExtraAI</c>.
		/// </summary>
		/// <param name="reader">Reader to read the variables from.</param>
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

		/// <summary>
		/// Updates frames for Rognir. To change frames, set the %= to how many ticks you want (numberOfTicks).
		/// Then change the frameCounter / n so that n = numberOfTicks / numberOfFrames
		/// </summary>
		/// <param name="frameHeight">Height of each frame in the sprite sheet.</param>
		public override void FindFrame(int frameHeight)
		{
			if (dashTimer <= 0 && stage == 1)
			{
				/*npc.frameCounter += 1.0; //This makes the animation run. Don't change this
				npc.frameCounter %= 60.0; //This makes it so that after NUMBER ticks, the animation resets to the beginning.
										  //To help you with timing, there are 60 ticks in one second.
				int frame = (int)(npc.frameCounter / 5) + 2; //Chooses an animation frame based on frameCounter.
				npc.frame.Y = frame * frameHeight; //Actually sets the frame*/
				npc.frame.Y = frameHeight * 2;
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
		/// <summary>
		/// Method AI defines the AI for the boss.
		/// <c>AI</c> Starts out by checking if the the stage should be set to stage two.
		/// Then it gets the npc's target player and checks if the player is still alive.
		/// If the player is dead the npc targets the player closest to the npc and does the same check.
		/// If there are no players left the npc despawns after ten seconds.
		/// A random move timer is set and the npc moves to one of three locations unless the npc is dashing.
		/// If not dashing the 
		/// </summary>
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
						NewPosition();
					}

					// Store a random amount of ticks until next update of the movement offset.
					moveTimer = (int)Main.rand.NextFloat(rogMinMoveTimer, rogMaxMoveTimer);

					// Update network.
					npc.netUpdate = true;
				}
			}
			
			// Check if Rognir is spinning while he swiches stages.
			if (spinTimer <= 0)
			{
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
					if (speed > (stage == 1 ? rogMaxSpeedOne : rogMaxSpeedTwo))
					{
						speed = (stage == 1 ? rogMaxSpeedOne : rogMaxSpeedTwo);
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

					DoAttack();
				}
				else
					Dash();
			}
			// Spin.
			else
			{
				npc.velocity = Vector2.Zero;
				npc.rotation += 2 * (float)Math.PI / 30f;
				spinTimer--;
				if ((2f * (float)Math.PI) - npc.rotation <= 0)
				{
					npc.rotation = 0;
				}
				if (spinTimer == 0)
				{
					npc.dontTakeDamage = false;
					Main.PlaySound(SoundID.ZombieMoan);
				}
			}

			npc.ai[0]--;
		}

		/// <summary>
		/// Selects a target position for Rognir.  
		/// The position can be above, to the left of, or to the right of the player.
		/// </summary>
		private void NewPosition()
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

		/// <summary>
		/// <c>DoAttack</c> selects which attack to do randomly and then calls the apropriate function.
		/// </summary>
		private void DoAttack()
		{
			// Get next attack ten tick before attack happens to avoid desync.
			if (attackCool == 10)
			{
				if (Main.netMode != 1)
				{
					attack = Main.rand.Next(3);     // Choose what attack to do.  Shards happen twice as often as a dash.
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

		/// <summary>
		/// Causes Rognir to perform a quick dash attack.
		/// Normal movement needs to be stopped durring the dash in AI().
		/// </summary>
		private void Dash()
		{
			if (dashTimer <= 0)
			{
				//npc.rotation = 0f;
				npc.velocity = Vector2.Zero;
				if (Main.netMode != 1)
				{
					// dashTimer is the number of ticks the dash will last.  Increase dashTimer to increase the lenght of the dash.
					dashTimer = rogDashLenght;
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
					if (Main.rand.NextFloat() < rogSecondDashChance - (rogSecondDashReduction * dashCounter))
					{
						dashCounter++;
						Dash();
					}
					else
					{
						dashCounter = 0;
					}
				}
			}
		}

		/// <summary>
		/// Shoots out an ice shard that attacks the player.
		/// </summary>
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

			if (Main.netMode != 1)
			{
				Projectile.NewProjectile(npc.Center, projVelocity, ProjectileType<RognirBossIceShard>(), rogShardDamage, 0f, Main.myPlayer, 0f, Main.rand.Next(0, 1000));

				if (stage == 2)
				{
					// Shoot out an ice shard 30 degrees offset
					Projectile.NewProjectile(npc.Center, projVelocity.RotatedBy((Math.PI / 180) * 30), ProjectileType<RognirBossIceShard>(), rogShardDamage, 0f, Main.myPlayer, 0f, Main.rand.Next(0, 1000));
					// Shoot out an ice shard 330 degrees offset
					Projectile.NewProjectile(npc.Center, projVelocity.RotatedBy((Math.PI / 180) * 330), ProjectileType<RognirBossIceShard>(), rogShardDamage, 0f, Main.myPlayer, 0f, Main.rand.Next(0, 1000));
				}
			}
		}

		/// <summary>
		/// Spawns an Undead Viking on Rognir unless Rognir is inside of tiles.
		/// </summary>
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

		/// <summary>
		/// Gets called when Rognir switches to stage two.
		/// Put code that needs to run at the start of stage two here.
		/// </summary>
		private void SwitchStage()
		{
			if (anchorID == 0)
			{
				anchorID = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, NPCType<RognirBossAnchor>(), 0, npc.whoAmI);
				music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/RognirStage2");
				npc.height = 191;
				npc.width = 168;
			}

			spinTimer = 60;				// Start spinning
			npc.dontTakeDamage = true;  //Don't take damage while spinning.
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.Chilled, stage == 1 ? rogChilledLenghtOne : rogChilledLenghtTwo);        // Chilled buff.
		}

		/// <summary>
		/// <c>NPCLoot</c> selects what loot Rognir will drop.
		/// </summary>
		public override void NPCLoot()
		{
			// Drops boss bags if world is in Expert mode
			if (Main.expertMode)
			{
				npc.DropBossBags();

			// If world is in Normal mode, Rognir will drop his Frozen Hook
			} else
			{
				Item.NewItem(npc.getRect(), ItemType<Items.FrozenHookItem>());
			}
		}

		/// <summary>
		/// Allows customization of boss name in defeat message as well as what potions he drops.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="potionType"></param>
		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.HealingPotion;
		}
	}
}
