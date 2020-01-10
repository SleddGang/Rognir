using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir.Projectiles.Rognir
{
    class RognirBossIceShard : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ice Shard");
		}

		public override void SetDefaults()
		{
			projectile.CloneDefaults(ProjectileID.EnchantedBoomerang);
			/*
			projectile.arrow = true;
			projectile.width = 10;
			projectile.height = 10;
			projectile.aiStyle = 1;
			projectile.friendly = false;
			projectile.ranged = true;
			*/
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.timeLeft = 240;
			aiType = ProjectileID.EnchantedBoomerang;
		}
	}
}
