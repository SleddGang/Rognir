using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.Items
{
    internal class SpikedHookItem : ModItem
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("Spiked Hook");

        public override void SetDefaults()
        {
            item.CloneDefaults(ItemID.AmethystHook);
            item.shootSpeed = 20f;
            item.shoot = ProjectileType<SpikedHookProjectile>();
            item.damage = 10;
            item.knockBack = 100;
        }
    }

    internal class SpikedHookProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projHook[projectile.type] = true;
            DisplayName.SetDefault("${ProjectileName.GemHookAmethyst}");
        }

        public override void SetDefaults()
        {
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

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (target.FullName.Equals("Rognir"))
            {
                damage *= 1000;
            }
            
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
                spriteBatch.Draw(mod.GetTexture("Items/SpikedHookChain"), new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0,0, Main.chain30Texture.Width, Main.chain30Texture.Height), drawColor, projRotation,
                    new Vector2(Main.chain30Texture.Width * 0.5f, Main.chain30Texture.Height * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return true;
        }

		public override void AI()
		{
			if (Main.player[projectile.owner].dead || Main.player[projectile.owner].stoned || Main.player[projectile.owner].webbed || Main.player[projectile.owner].frozen || Main.player[projectile.owner].controlJump)
			{
				projectile.Kill();
			}
			else
			{
				int num2475;
				Vector2 playerLocation = Main.player[projectile.owner].MountedCenter;
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

					ProjectileLoader.GrappleRetreatSpeed(projectile, Main.player[projectile.owner], ref retreatSpeed);
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
					bool flag148 = true;
					for (int num2380 = grappleLeft; num2380 < grappleRight; num2380 = num2475 + 1)
					{
						for (int num2379 = grappleBottom; num2379 < grappleTop; num2379 = num2475 + 1)
						{
							if (Main.tile[num2380, num2379] == null)
							{
								Tile[,] tile5 = Main.tile;
								int num2479 = num2380;
								int num2480 = num2379;
								Tile tile6 = new Tile();
								tile5[num2479, num2480] = tile6;
							}
							Vector2 vector155 = default(Vector2);
							vector155.X = (float)(num2380 * 16);
							vector155.Y = (float)(num2379 * 16);
							if (projectile.position.X + (float)(projectile.width / 2) > vector155.X && projectile.position.X + (float)(projectile.width / 2) < vector155.X + 16f && projectile.position.Y + (float)(projectile.height / 2) > vector155.Y && projectile.position.Y + (float)(projectile.height / 2) < vector155.Y + 16f && Main.tile[num2380, num2379].nactive() && (Main.tileSolid[Main.tile[num2380, num2379].type] || Main.tile[num2380, num2379].type == 314 || Main.tile[num2380, num2379].type == 5))
							{
								flag148 = false;
							}
							num2475 = num2379;
						}
						num2475 = num2380;
					}
					if (flag148)
					{
						projectile.ai[0] = 1f;
					}
					else if (Main.player[projectile.owner].grapCount < 10)
					{
						Main.player[projectile.owner].grappling[Main.player[projectile.owner].grapCount] = projectile.whoAmI;
						Player player22 = Main.player[projectile.owner];
						Player player23 = player22;
						num2475 = player22.grapCount;
						player23.grapCount = num2475 + 1;
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
				for (int num2394 = grappleLeft2; num2394 < grappleRight2; num2394 = num2475 + 1)
				{
					for (int num2393 = grappleBottom2; num2393 < grappleTop2; num2393 = num2475 + 1)
					{
						if (Main.tile[num2394, num2393] == null)
						{
							Tile[,] tile7 = Main.tile;
							int num2511 = num2394;
							int num2512 = num2393;
							Tile tile8 = new Tile();
							tile7[num2511, num2512] = tile8;
						}
						Vector2 vector153 = default(Vector2);
						vector153.X = (float)(num2394 * 16);
						vector153.Y = (float)(num2393 * 16);
						if (value169.X + 10f > vector153.X && value169.X < vector153.X + 16f && value169.Y + 10f > vector153.Y && value169.Y < vector153.Y + 16f && Main.tile[num2394, num2393].nactive() && (Main.tileSolid[Main.tile[num2394, num2393].type] || Main.tile[num2394, num2393].type == 314) && (projectile.type != 403 || Main.tile[num2394, num2393].type == 314))
						{
							if (Main.player[projectile.owner].grapCount < 10)
							{
								Main.player[projectile.owner].grappling[Main.player[projectile.owner].grapCount] = projectile.whoAmI;
								Player player22 = Main.player[projectile.owner];
								Player player24 = player22;
								num2475 = player22.grapCount;
								player24.grapCount = num2475 + 1;
							}
							if (Main.myPlayer == projectile.owner)
							{
								int hooksOut = 0;
								int oldestHookIndex = -1;
								int oldestHookTimeLeft = 100000;
								int numberOfHooks = 2;
								ProjectileLoader.NumGrappleHooks(projectile, Main.player[projectile.owner], ref numberOfHooks);
								for (int i = 0; i < 1000; i++)
								{
									if (Main.projectile[i].active && Main.projectile[i].owner == projectile.owner && Main.projectile[i].type == projectile.type)
									{
										if (Main.projectile[i].timeLeft < oldestHookTimeLeft)
										{
											oldestHookIndex = i;
											oldestHookTimeLeft = Main.projectile[i].timeLeft;
										}
										hooksOut++;
									}
								}
								if (hooksOut > numberOfHooks)
								{
									Main.projectile[oldestHookIndex].Kill();
								}
							}
							WorldGen.KillTile(num2394, num2393, true, true, false);
							Main.PlaySound(0, num2394 * 16, num2393 * 16, 1, 1f, 0f);
							projectile.velocity.X = 0f;
							projectile.velocity.Y = 0f;
							projectile.ai[0] = 2f;
							projectile.position.X = (float)(num2394 * 16 + 8 - projectile.width / 2);
							projectile.position.Y = (float)(num2393 * 16 + 8 - projectile.height / 2);
							projectile.damage = 0;
							projectile.netUpdate = true;
							if (Main.myPlayer == projectile.owner)
							{
								NetMessage.SendData(13, -1, -1, null, projectile.owner, 0f, 0f, 0f, 0, 0, 0);
							}
							break;
						}
						num2475 = num2393;
					}
					if (projectile.ai[0] == 2f)
					{
						break;
					}
					num2475 = num2394;
				}
				return;
			}
		}

    }
}