using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.NPCs.Rognir
{
	/// <summary>
	/// <c>RognirBossAnchor</c> is the anchor that spawns in Rognir's phase two.
	/// It is unable to tak damage and only spins and dashes at the player.
	/// </summary>
	class RognirBossAnchor : ModNPC
	{
		private const float anchDashMaxSpeed = 15.0f;       // Maximum speed of the dash.
		private const int anchTargetMin = 2;                // Minimum number of dashes before a new target is selected.
		private const int anchTargetMax = 4;                // Maximum number of dashes before a new target is selected.

		public float dashTimer          // Countdown until stop spinning and start dash.
		{
			get => npc.ai[1];
			set => npc.ai[1] = value;
		}
		public float dashX          // Dashes until next target is selected.  
		{
			get => npc.ai[2];
			set => npc.ai[2] = value;
		}
		public float dashY          // Dashes until next target is selected.  
		{
			get => npc.ai[3];
			set => npc.ai[3] = value;
		}

		private int targetTimer = 4;

		/// <summary>
		/// Sets the display name and other static defaults.
		/// </summary>
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Anchor of Rognir");
			Main.npcFrameCount[npc.type] = 1;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		/// <summary>
		/// Sets the default options for the npc like health and size
		/// dontTakeDamage means the the npc can't be hurt.
		/// </summary>
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 600;
			npc.damage = 20;
			npc.defense = 14;
			npc.knockBackResist = 0f;
			npc.width = 163;
			npc.height = 236;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.dontTakeDamage = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath6;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
		}

		/// <summary>
		/// At the start of <c>AI</c> the npc cheks to see if it's owner is Rognir and if the owner is alive.
		/// After <c>targetTimer</c> reaches zero the npc will select the closest player to target.
		/// The npc will check to see if it's target is alive.  If not then It will select the next closest target.
		/// The npc will spin until it faces it's target and then dash at a constant speed towards the target's location
		/// </summary>
		public override void AI()
		{
			// Check if owner is Rognir and if it is still alive.
			NPC owner = Main.npc[(int)npc.ai[0]];
			if (!owner.active || owner.type != NPCType<RognirBoss>())
			{
				npc.active = false;
				return;
			}

			// Check if it is time to select a new target.
			if (targetTimer <= 0)
			{
				npc.TargetClosest(false);
				if (Main.netMode != 1)
				{
					targetTimer = Main.rand.Next(anchTargetMin, anchTargetMax);
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
				npc.TargetClosest(false);
				player = Main.player[npc.target];
				npc.netUpdate = true;
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

			Vector2 dashDirection = new Vector2(dashX, dashY);
			if (dashTimer <= 0)
			{
				dashDirection = npc.Center - Main.player[npc.target].Center;
				double angle = Math.Atan2(dashDirection.Y, dashDirection.X);

				// Difference between angle to player and npc rotation.
				double difference = angle - npc.rotation + 3.5 * Math.PI;
				if (difference > Math.PI / 30)
				{
					npc.rotation += 2 * (float)Math.PI / 30f;
				}
				else
				{
					targetTimer--;

					if (Main.netMode != 1)
					{
						// dashTimer is the number of ticks the dash will last.  Increase dashTimer to increase the lenght of the dash.
						dashTimer = 60;
						// Direction to dash in.
						dashDirection = Main.player[npc.target].Center - npc.Center;
						dashX = dashDirection.X;
						dashY = dashDirection.Y;

						npc.netUpdate = true;

						Main.PlaySound(SoundID.ForceRoar);
					}
				}
			}

			else
			{
				dashTimer--;

				// Get the speed of the dash and limit it.
				float speed = dashDirection.Length();
				if (speed > anchDashMaxSpeed)
				{
					speed = anchDashMaxSpeed;
				}

				// Normalize the direction, add the speed, and then update position.  
				dashDirection.Normalize();
				dashDirection *= speed;
				npc.position += dashDirection;
				npc.rotation = (float)Math.Atan2(dashY, dashX) + 0.5f * (float)Math.PI;
			}
		}

		/// <summary>
		/// Syncs extra variables over the network.
		/// </summary>
		/// <param name="writer"></param>
		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(targetTimer);
		}

		/// <summary>
		/// Receives the data from <c>SendExtraAI</c> and stores it it the proper variable.
		/// </summary>
		/// <param name="reader"></param>
		public override void ReceiveExtraAI(BinaryReader reader)
		{
			targetTimer = reader.ReadInt32();
		}

		/// <summary>
		/// If the player has the chilled debuf the anchor will deal extra damage.
		/// </summary>
		/// <param name="target">Player hit by the anchor</param>
		/// <param name="damage">Damage dealt by the anchor</param>
		/// <param name="crit">Wether the attack is a crit</param>
		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			if (target.HasBuff(BuffID.Chilled))
			{
				damage += 10;
			}
		}
	}
}
