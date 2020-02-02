using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir.Projectiles.Rognir
{
    class RognirBossIceShard : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ice Shard");
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			//projectile.CloneDefaults(ProjectileID.MagicMissile);
			/*
			projectile.arrow = true;
			projectile.width = 10;
			projectile.height = 10;
			projectile.aiStyle = 1;
			projectile.friendly = false;
			projectile.ranged = true;
			*/
			projectile.height = 32;
			projectile.width = 32;
			projectile.penetrate = -1;
			projectile.magic = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = false;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.timeLeft = 240;
			projectile.light = 0.5f;            //How much light emit around the projectile
			projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			projectile.ai[0] += 1;

			if (Main.rand.Next(5) == 0) // only spawn 20% of the time
			{
				int choice = Main.rand.Next(3); // choose a random number: 0, 1, or 2
				if (choice == 0) // use that number to select dustID: 15, 57, or 58
				{
					choice = 15;
				}
				else if (choice == 1)
				{
					choice = 57;
				}
				else
				{
					choice = 58;
				}
				// Spawn the dust
				Dust.NewDust(projectile.position, projectile.width, projectile.height, choice, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 150, Color.LightBlue, 0.7f);

				// Constantly play the Item9 sound.
				if (projectile.soundDelay <= 0)
				{
					projectile.soundDelay = 10;
					Main.PlaySound(SoundID.Item9, projectile.position);
				}

				// Check if projectile should be a banana.
				if (projectile.ai[1] == 1f)
				{
					projectile.frame = 1;
					projectile.rotation += (float)Math.PI / 15f;
				}
				else
				{
					projectile.frame = 0;
					projectile.rotation = projectile.velocity.ToRotation();
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			Main.PlaySound(SoundID.Item101, projectile.position);
		}
	}
}
