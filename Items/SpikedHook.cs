using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            // base.SetDefaults();
            item.CloneDefaults(ItemID.AmethystHook);
            item.shootSpeed = 20f;
            item.shoot = ProjectileType<SpikedHookProjectile>();
        }
    }

    internal class SpikedHookProjectile : ModProjectile
    {
        public override void SetStaticDefaults() => DisplayName.SetDefault("${ProjectileName.GemHookAmethyst}");

        public override void SetDefaults() => projectile.CloneDefaults(ProjectileID.GemHookAmethyst);

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

        public override void UseGrapple(Player player, ref int type)
        {
            int hooksout = 0;
            int oldesthookindex = -1;
            int oldesthooktimeleft = 100000;
            for (int i = 0; i < 1000; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == projectile.whoAmI && Main.projectile[i].type == projectile.type)
                {
                    hooksout++;
                    if (Main.projectile[i].timeLeft < oldesthooktimeleft)
                    {
                        oldesthookindex = i;
                        oldesthooktimeleft = Main.projectile[i].timeLeft;
                    }
                }
            }
            if (hooksout > 2)
            {
                Main.projectile[oldesthookindex].Kill();
            }
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
                spriteBatch.Draw(mod.GetTexture("Items/SpikedHookChain"), new Vector2(center.X - Main.screenPosition.X, center.Y - Main.screenPosition.Y),
                    new Rectangle(0,0, Main.chain30Texture.Width, Main.chain30Texture.Height), drawColor, projRotation,
                    new Vector2(Main.chain30Texture.Width * 0.5f, Main.chain30Texture.Height * 0.5f), 1f, SpriteEffects.None, 0f);
            }
            return true;
        }
    }
}