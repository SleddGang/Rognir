using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Rognir.Dusts
{
	public class RognirDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity.Y = Main.rand.NextFloat(-0.05f, 0.05f);
			dust.velocity.X = Main.rand.NextFloat(-0.05f, 0.05f);
			dust.scale *= 1.5f;
		}

		public override bool MidUpdate(Dust dust) {
			if (dust.noLight) {
				return false;
			}

			float strength = dust.scale * 1.4f;
			if (strength > 1f) {
				strength = 1f;
			}
			Lighting.AddLight(dust.position, 0.1f * strength, 0.2f * strength, 0.7f * strength);
			return false;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor) 
			=> new Color(lightColor.R, lightColor.G, lightColor.B, 25);
	}
}