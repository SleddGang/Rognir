using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
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
			Main.projFrames[Projectile.type] = 2;
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
			Projectile.height = 32;
			Projectile.width = 32;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Magic;	//Replaces projectile.magic = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.timeLeft = 240;
			Projectile.light = 0.5f;            //How much light emit around the projectile
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.ai[0] += 1;

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
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, choice, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f, 150, Color.LightBlue, 0.7f);

				// Constantly play the Item9 sound.
				if (Projectile.soundDelay <= 0)
				{
					Projectile.soundDelay = 10;
					SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
				}

				// Check if projectile should be a banana.
				if (Projectile.ai[1] == 1f)
				{
					Projectile.frame = 1;
					Projectile.rotation += (float)Math.PI / 15f;
				}
				else
				{
					Projectile.frame = 0;
					Projectile.rotation = Projectile.velocity.ToRotation();
				}
			}
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			SoundEngine.PlaySound(SoundID.Item101, Projectile.position);
		}
	}
}
