using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.Items
{
	/* The internal class that defines the stats of the Frozen Hook grappling hook item and
	   establishes the hook projectile that this grappling hook shoots*/
	internal class FrozenHookItem : ModItem
    {

		private const int maxHooks = 3;
		private const float maxRange = 420f;
		private const float pullSpeed = 12f;
		private float retreatSpeed = 16f;

		/// <summary>
		/// Sets the static default values for the hook projectile shot by the grappling hook item.
		/// </summary>
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frozen Hook");
		}

		/// <summary>
		/// Sets the default values for the hook projectile shot by the grappling hook item.
		/// </summary>
		public override void SetDefaults()
        {
            item.shootSpeed = 20f;
            item.shoot = ProjectileType<FrozenHookProjectile>();
            item.damage = 9;
            item.knockBack = 13;
			item.rare = 6;
			item.melee = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			TooltipLine reach = new TooltipLine(mod, "Reach", "Reach: " + (maxRange/16).ToString());
			TooltipLine launchVelocity = new TooltipLine(mod, "LaunchVelocity", "Launch Velocity: " + item.shootSpeed);
			TooltipLine pullVelocity = new TooltipLine(mod, "PullVelocity", "Pull Velocity: " + pullSpeed.ToString());
			tooltips.Add(reach);
			tooltips.Add(launchVelocity);
			tooltips.Add(pullVelocity);
		}
	}

	/* The internal class that defines the stats and behavior of the hook that is shot by the
	   Frozen Hook grappling hook item*/
    internal class FrozenHookProjectile : ModProjectile
    {
		// The hook's defined stats. Initilaized as variables here to make later stat updates easier
		private const int maxHooks		= 3;
		private const float maxRange	= 420f;
		private const float pullSpeed	= 12f;
		private float retreatSpeed		= 16f;

		/// <summary>
		/// Sets the static default values for the hook projectile shot by the grappling hook item.
		/// </summary>
        public override void SetStaticDefaults()
        {
            Main.projHook[projectile.type] = true;
            DisplayName.SetDefault("${ProjectileName.FrozenAnchorHook}");
        }
		/// <summary>
		/// Sets the default values for the hook projectile shot by the grappling hook item.
		/// </summary>
        public override void SetDefaults()
        {
			projectile.netImportant = true;			// Updates server when a new player joins so that the new player can see the active projectile
            projectile.width = 36;					// Width of hook hitbox
            projectile.height = 45;					// Height of hook hitbox
            projectile.timeLeft *= 10;				// Time left before the projectile dies
            projectile.friendly = true;				// Does not hurt other players/friendly npcs
            projectile.ignoreWater = true;			// Ignores water
            projectile.tileCollide = false;			// Collides with tiles
            projectile.penetrate = -1;				// Penetrates through infinite enemies
            projectile.usesLocalNPCImmunity = true;	// Makes NPCs immune to only the active projectile that hit them rather than to the whole item
            projectile.localNPCHitCooldown = -1;	// Immunity per NPC per projectile lasts until the projectile dies
        }

		/// <summary>
		/// A tModLoader hook method that is called whenever the player presses the grapple button.
		/// This method prevents the player from sending out more hooks than the grappling hook's
		/// maximum number of hooks. The player can always shoot out one additional hook beyond the
		/// grappling hook's maximum, but if that hook grapples a block, the ShouldKillOldestHook
		/// method will kill the oldest hook.
		/// </summary>
		/// <param name="player"> The player who owns the grappling hook</param>
		/// <returns></returns>
		public override bool? CanUseGrapple(Player player)
		{
			int hooksOut = 0;
			for (int i = 0; i < 1000; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].owner == Main.myPlayer && Main.projectile[i].type == projectile.type)
				{
					hooksOut++;
				}
			}
			return hooksOut <= maxHooks ? true : false;
		}

		/// <summary>
		/// A tModLoader hook method that is used to set the speed of the grappling hook. This will
		/// override any pre-set values for the grappling hook speed (hence why the "speed" input
		/// variable is a ref)
		/// </summary>
		/// <param name="player"> The player who owns the grappling hook</param>
		/// <param name="speed"> The grappling hook's speed variable</param>
		public override void GrapplePullSpeed(Player player, ref float speed)
		{
			speed = pullSpeed;
		}

		/// <summary>
		/// This method simply draws the chain between the player and the hook while the hook is active
		/// </summary>
		/// <param name="spriteBatch"> The sprite batch that defines what the chain will look like</param>
		/// <param name="lightColor"> The color of the light if the projectile emits light</param>
		/// <returns></returns>
		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 playerCenter = Main.player[projectile.owner].MountedCenter;
            Vector2 center = projectile.Center;
            Vector2 distToProj = playerCenter - projectile.Center;
            float projRotation = distToProj.ToRotation() - 1.57f;
            float distance = distToProj.Length();
            while (distance > 30f && !float.IsNaN(distance))
            {
                distToProj.Normalize();                 // get unit vector
                distToProj *= retreatSpeed;             // speed = 16
                center += distToProj;                   // update draw position
                distToProj = playerCenter - center;     // update distance
                distance = distToProj.Length();
                Color drawColor = lightColor;

                // Draws chain
                spriteBatch.Draw(mod.GetTexture("Items/FrozenHookChain"), new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0,0, Main.chain30Texture.Width, Main.chain30Texture.Height), drawColor, projRotation,
                    new Vector2(Main.chain30Texture.Width * 0.5f, Main.chain30Texture.Height * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return true;
        }

		/// <summary>
		/// This method controls the AI for the grappling hook. Note that there is a separate AI run
		/// for each active hook. There are three states that a hook can be in:
		/// 1(0f) -- Extending
		/// 2(1f) -- Retreating
		/// 3(2f) -- Grappled
		/// The AI starts at Extending when shot and updates the state of the hook based on certain
		/// conditions. If the hook has been shot, but has not reached the max grapple distance, the
		/// ai state stays at Extending. If the hook has reached the max grapple distance, the ai state
		/// is updated to Retreating. If the hook ever reaches a solid tile that it can grapple to, the
		/// ai state is updated to Grappled. The AI then continues to check that the block that the
		/// hook grappled to is still active. If the block is ever removed (e.g. broken), the ai state
		/// of the hook is updated to Retreating.
		/// </summary>
		public override void AI()
		{
			Player hookPlayer = Main.player[projectile.owner];
			if (hookPlayer.dead || hookPlayer.stoned || hookPlayer.webbed || hookPlayer.frozen)
			{
				projectile.Kill();
			}
			else
			{
				// Determines distance from player to grapple
				int newPosition;
				Vector2 playerLocation = hookPlayer.MountedCenter;
				Vector2 projectileLocation = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float xDistance = playerLocation.X - projectileLocation.X;
				float yDistance = playerLocation.Y - projectileLocation.Y;
				float grappleDistance = (float)Math.Sqrt((double)(xDistance * xDistance + yDistance * yDistance));
				projectile.rotation = (float)Math.Atan2((double)yDistance, (double)xDistance) - 1.57f;
				
				// ai state for when grapple is extending
				if (projectile.ai[0] == 0f)
				{
					// Sets grappling hook ai to retreat (1) if the hook is outside of the max grappling range
					if (grappleDistance > maxRange)
					{
						projectile.ai[0] = 1f;
					}

					/* If the hook is still within the max grappling range, it checks to see if the grapple has collided
					   with a tile and, if it has, sets the grappling hook's ai to grappled (2)*/
					
					// Sets position of hook's hitbox
					Vector2 bottomLeftVector = projectile.Center - new Vector2(5f);
					Vector2 topRightVector = projectile.Center + new Vector2(5f);
					Point bottomLeftPoint = (bottomLeftVector - new Vector2(16f)).ToTileCoordinates();
					Point topRightPoint = (topRightVector + new Vector2(32f)).ToTileCoordinates();
					int grappleLeft2 = bottomLeftPoint.X;
					int grappleRight2 = topRightPoint.X;
					int grappleBottom2 = bottomLeftPoint.Y;
					int grappleTop2 = topRightPoint.Y;

					// Verifies hook's hitbox position is within map bounds. If not, sets  hitbox to map bounds.
					if (grappleLeft2 < 0)
					{
						grappleLeft2 = 0;
					}
					if (grappleRight2 > Main.maxTilesX)
					{
						grappleRight2 = Main.maxTilesX;
					}
					if (grappleBottom2 < 0)
					{
						grappleBottom2 = 0;
					}
					if (grappleTop2 > Main.maxTilesY)
					{
						grappleTop2 = Main.maxTilesY;
					}

					/* Iterates through all tiles within hook's hitbox position to check for a solid tile the hook
					   can grapple to. If there is, the grapple's ai is set to grappled (2)*/
					for (int xPos = grappleLeft2; xPos < grappleRight2; xPos = newPosition + 1)
					{
						for (int yPos = grappleBottom2; yPos < grappleTop2; yPos = newPosition + 1)
						{
							Vector2 tilePosition = default;
							tilePosition.X = (float)(xPos * 16);
							tilePosition.Y = (float)(yPos * 16);
							if (bottomLeftVector.X + 10f > tilePosition.X &&
								bottomLeftVector.X < tilePosition.X + 16f &&
								bottomLeftVector.Y + 10f > tilePosition.Y &&
								bottomLeftVector.Y < tilePosition.Y + 16f &&
								Main.tile[xPos, yPos].nactive() &&
								(Main.tileSolid[Main.tile[xPos, yPos].type] || Main.tile[xPos, yPos].type == 314))
							{
								// Updates player's grapple count
								if (hookPlayer.grapCount < 10)
								{
									hookPlayer.grappling[hookPlayer.grapCount] = projectile.whoAmI;
									hookPlayer.grapCount += 1;
								}
								ShouldKillOldestHook(); // Kills the oldest hook if player currently has max number of hooks grappled
								WorldGen.KillTile(xPos, yPos, true, true, false);
								Main.PlaySound(0, xPos * 16, yPos * 16, 1, 1f, 0f);
								projectile.velocity.X = 0f;
								projectile.velocity.Y = 0f;
								projectile.ai[0] = 2f;
								projectile.position.X = (float)(xPos * 16 + 8 - projectile.width / 2);
								projectile.position.Y = (float)(yPos * 16 + 8 - projectile.height / 2);
								projectile.damage = 0;
								projectile.netUpdate = true;
								if (Main.myPlayer == projectile.owner)
								{
									NetMessage.SendData(13, -1, -1, null, projectile.owner, 0f, 0f, 0f, 0, 0, 0);
								}
								break;
							}
							newPosition = yPos;
						}
						if (projectile.ai[0] == 2f)
						{
							break;
						}
						newPosition = xPos;
					}
					return;
				}

				// ai state for when grapple is retreating
				if (projectile.ai[0] == 1f)
				{
					ProjectileLoader.GrappleRetreatSpeed(projectile, hookPlayer, ref retreatSpeed);
					// Kills the hook once close enough to the player on retreat
					if (grappleDistance < 24f)
					{
						projectile.Kill();
					}
					grappleDistance = retreatSpeed / grappleDistance;
					xDistance *= grappleDistance;
					yDistance *= grappleDistance;
					projectile.velocity.X = xDistance;
					projectile.velocity.Y = yDistance;
				}

				// ai state for when grapple is hooked
				else if (projectile.ai[0] == 2f)
				{
					// Sets position of hook's hitbox
					int grappleLeft = (int)(projectile.position.X / 16f) - 1;
					int grappleRight = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
					int grappleBottom = (int)(projectile.position.Y / 16f) - 1;
					int grappleTop = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;

					// Verifies hook's hitbox position is within map bounds. If not, sets  hitbox to map bounds.
					if (grappleLeft < 0)
					{
						grappleLeft = 0;
					}
					if (grappleRight > Main.maxTilesX)
					{
						grappleRight = Main.maxTilesX;
					}
					if (grappleBottom < 0)
					{
						grappleBottom = 0;
					}
					if (grappleTop > Main.maxTilesY)
					{
						grappleTop = Main.maxTilesY;
					}

					/* Iterates through all tiles within hook's hitbox position to check for a solid tile the hook
					   is grappled to. If the hook is no longer grappled to a tile (e.g. if the tile is broken with
					   a pickaxe), the grapple ai is set to retreat (1)*/
					bool notGrappled = true;
					for (int xPos = grappleLeft; xPos < grappleRight; xPos = newPosition + 1)
					{
						for (int yPos = grappleBottom; yPos < grappleTop; yPos = newPosition + 1)
						{
							Vector2 tilePosition = default;
							tilePosition.X = (float)(xPos * 16);
							tilePosition.Y = (float)(yPos * 16);
							if (projectile.position.X + (float)(projectile.width / 2) > tilePosition.X &&
								projectile.position.X + (float)(projectile.width / 2) < tilePosition.X + 16f &&
								projectile.position.Y + (float)(projectile.height / 2) > tilePosition.Y &&
								projectile.position.Y + (float)(projectile.height / 2) < tilePosition.Y + 16f &&
								Main.tile[xPos, yPos].nactive() &&
								(Main.tileSolid[Main.tile[xPos, yPos].type] || Main.tile[xPos, yPos].type == 314 || Main.tile[xPos, yPos].type == 5))
							{
								notGrappled = false;
							}
							newPosition = yPos;
						}
						newPosition = xPos;
					}

					// Sets the grapple's ai to retreat (1) if the tile that he hook was grappled to is no longer there
					if (notGrappled)
					{
						projectile.ai[0] = 1f;
					}
					else if (hookPlayer.grapCount < 10)
					{
						hookPlayer.grappling[hookPlayer.grapCount] = projectile.whoAmI;
						hookPlayer.grapCount += 1;
					}
					KillHookOnJump(hookPlayer); // checks if the player has jumped. On a jump, all hooks are killed (including actively extending hooks)
				}
				return;
			}
		}

		/// <summary>
		/// This method fires when a hook grapples a new tile and checks to see if the oldest hook
		/// should be killed. Since grappling hooks have a maximum number of hooks that can be used,
		/// this checks to see if the player already has the maximum number of hooks out and grappled.
		/// If so, the oldest grappled hook is killed.
		/// </summary>
		private void ShouldKillOldestHook()
		{
			int oldestHookTimeLeft = 100000;
			int oldestHookIndex = -1;
			int hooksOut = 0;

			// For all projectiles
			for (int i = 0; i < 1000; i++)
			{
				// If the projectile is active AND the current projectile matches this class projectile AND is owned by the current player
				if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type)
				{
					// If the current projectile has the least amount of time left, mark it as the oldest active hook
					if (Main.projectile[i].timeLeft < oldestHookTimeLeft)
					{
						oldestHookIndex = i;
						oldestHookTimeLeft = Main.projectile[i].timeLeft;
					}

					// If the current projectile's ai stage is set to Grappled, increase the number of active hooks out
					if (Main.projectile[i].ai[0] == 2)
					{
						hooksOut++;
					}
				}
			}

			// If there are as many or more active hooks out as the maximum number of hooks for this grappling hook, kill the oldest hook
			if (hooksOut >= maxHooks)
			{
				Main.projectile[oldestHookIndex].Kill();
			}
		}

		/// <summary>
		/// This method will kill all active hooks if a player jumps. It only kills hooks if at least
		/// one hook is grappled. This way it will not kill hooks every time the player presses the
		/// jump button.
		/// </summary>
		/// <param name="player"> The player who owns the grappling hook</param>
		private void KillHookOnJump(Player player)
		{

			// If the player has active hooks grappled AND the player presses the jump button
			if (player.grapCount > 0 && player.controlJump)
			{
				// For all projectiles
				for (int i = 0; i < 1000; i++)
				{
					// If the projectile is active AND the current projectile matches this class projectile AND is owned by the current player AND the current projectile is NOT Grappled, kill it
					if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type && (Main.projectile[i].ai[0] == 0 || Main.projectile[i].ai[0] == 1))
					{
						Main.projectile[i].Kill();
					}
				}
				projectile.Kill(); // Kills all Grappled hooks
			}
		}

	}

}