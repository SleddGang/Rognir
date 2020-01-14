using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.Items
{
    internal class FrozenHookItem : ModItem
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Frozen Hook");

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.AmethystHook);
            item.shootSpeed = 20f;
            item.shoot = ProjectileType<FrozenHookProjectile>();
            item.damage = 10;
            item.knockBack = 100;
        }
    }

    internal class FrozenHookProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projHook[projectile.type] = true;
            DisplayName.SetDefault("${ProjectileName.GemHookAmethyst}");
        }

        public override void SetDefaults()
        {
			projectile.netImportant = true;
            projectile.width = 18;
            projectile.height = 18;
            projectile.timeLeft *= 10;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
        }

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
			return hooksOut <= 2 ? true : false;
		}

		public override float GrappleRange()
        {
            return 500f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 2;
        }

        public override void GrappleRetreatSpeed(Player player, ref float speed)
        {
            speed = 16f;
        }

        public override void GrapplePullSpeed(Player player, ref float speed)
        {
            speed = 12f;
        }

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
                distToProj *= 16f;                      // speed = 16
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

		public override void AI()
		{
			Player hookPlayer = Main.player[projectile.owner];
			if (hookPlayer.dead || hookPlayer.stoned || hookPlayer.webbed || hookPlayer.frozen)
			{
				projectile.Kill();
			}
			else
			{
				int newPosition;
				Vector2 playerLocation = hookPlayer.MountedCenter;
				Vector2 projectileLocation = new Vector2(projectile.position.X + (float)projectile.width * 0.5f, projectile.position.Y + (float)projectile.height * 0.5f);
				float xDistance = playerLocation.X - projectileLocation.X;
				float yDistance = playerLocation.Y - projectileLocation.Y;
				float grappleDistance = (float)Math.Sqrt((double)(xDistance * xDistance + yDistance * yDistance));
				projectile.rotation = (float)Math.Atan2((double)yDistance, (double)xDistance) - 1.57f;
				if (projectile.ai[0] == 0f)
				{
					if (grappleDistance > 500f)
					{
						projectile.ai[0] = 1f;
					}
					else if (ProjectileLoader.GrappleOutOfRange(grappleDistance, projectile))
					{
						projectile.ai[0] = 1f;
					}
					goto Next;
				}
				if (projectile.ai[0] == 1f)
				{
					float retreatSpeed = 16f;

					ProjectileLoader.GrappleRetreatSpeed(projectile, hookPlayer, ref retreatSpeed);
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
				else if (projectile.ai[0] == 2f)
				{
					int grappleLeft = (int)(projectile.position.X / 16f) - 1;
					int grappleRight = (int)((projectile.position.X + (float)projectile.width) / 16f) + 2;
					int grappleBottom = (int)(projectile.position.Y / 16f) - 1;
					int grappleTop = (int)((projectile.position.Y + (float)projectile.height) / 16f) + 2;
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
					bool hasGrappled = true;
					for (int xPos = grappleLeft; xPos < grappleRight; xPos = newPosition + 1)
					{
						for (int yPos = grappleBottom; yPos < grappleTop; yPos = newPosition + 1)
						{
							if (Main.tile[xPos, yPos] == null)
							{
								Tile[,] tile5 = Main.tile;
								Tile tile6 = new Tile();
								tile5[xPos, yPos] = tile6;
							}
							Vector2 vector155 = default(Vector2);
							vector155.X = (float)(xPos * 16);
							vector155.Y = (float)(yPos * 16);
							if (projectile.position.X + (float)(projectile.width / 2) > vector155.X && projectile.position.X + (float)(projectile.width / 2) < vector155.X + 16f && projectile.position.Y + (float)(projectile.height / 2) > vector155.Y && projectile.position.Y + (float)(projectile.height / 2) < vector155.Y + 16f && Main.tile[xPos, yPos].nactive() && (Main.tileSolid[Main.tile[xPos, yPos].type] || Main.tile[xPos, yPos].type == 314 || Main.tile[xPos, yPos].type == 5))
							{
								hasGrappled = false;
							}
							newPosition = yPos;
						}
						newPosition = xPos;
					}
					if (hasGrappled)
					{
						projectile.ai[0] = 1f;
					}
					else if (hookPlayer.grapCount < 10)
					{
						hookPlayer.grappling[hookPlayer.grapCount] = projectile.whoAmI;
						hookPlayer.grapCount += 1;
					}
					if (hookPlayer.grapCount > 0 && hookPlayer.controlJump)
					{
						for (int i = 0; i < 1000; i++)
						{
							if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type && (Main.projectile[i].ai[0] == 0 || Main.projectile[i].ai[0] == 1))
							{
								Main.projectile[i].Kill();
							}
						}
						projectile.Kill();
					}
				}
				return;

			Next:
				Vector2 value169 = projectile.Center - new Vector2(5f);
				Vector2 value168 = projectile.Center + new Vector2(5f);
				Point point17 = (value169 - new Vector2(16f)).ToTileCoordinates();
				Point point16 = (value168 + new Vector2(32f)).ToTileCoordinates();
				int grappleLeft2 = point17.X;
				int grappleRight2 = point16.X;
				int grappleBottom2 = point17.Y;
				int grappleTop2 = point16.Y;
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
				for (int xPos = grappleLeft2; xPos < grappleRight2; xPos = newPosition + 1)
				{
					for (int yPos = grappleBottom2; yPos < grappleTop2; yPos = newPosition + 1)
					{
						if (Main.tile[xPos, yPos] == null)
						{
							Tile[,] tile7 = Main.tile;
							Tile tile8 = new Tile();
							tile7[xPos, yPos] = tile8;
						}
						Vector2 vector153 = default(Vector2);
						vector153.X = (float)(xPos * 16);
						vector153.Y = (float)(yPos * 16);
						if (value169.X + 10f > vector153.X && value169.X < vector153.X + 16f && value169.Y + 10f > vector153.Y && value169.Y < vector153.Y + 16f && Main.tile[xPos, yPos].nactive() && (Main.tileSolid[Main.tile[xPos, yPos].type] || Main.tile[xPos, yPos].type == 314) && (projectile.type != 403 || Main.tile[xPos, yPos].type == 314))
						{
							if (hookPlayer.grapCount < 10)
							{
								hookPlayer.grappling[hookPlayer.grapCount] = projectile.whoAmI;
								hookPlayer.grapCount += 1;
							}
							if (Main.myPlayer == projectile.owner)
							{
								int hooksOut = 0;
								int oldestHookIndex = -1;
								int oldestHookTimeLeft = 100000;
								int numberOfHooks = 2;
								ProjectileLoader.NumGrappleHooks(projectile, hookPlayer, ref numberOfHooks);
								for (int i = 0; i < 1000; i++)
								{
									if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type)
									{
										if (Main.projectile[i].timeLeft < oldestHookTimeLeft)
										{
											oldestHookIndex = i;
											oldestHookTimeLeft = Main.projectile[i].timeLeft;
										}
										if (Main.projectile[i].ai[0] == 2)
										{
											hooksOut++;
										}
									}
								}
								if (hooksOut >= numberOfHooks)
								{
									Main.projectile[oldestHookIndex].Kill();
								}
							}
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
		}

	}
}