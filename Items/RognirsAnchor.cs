using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.Items
{
    /* Boss drop from Rognir. Pickaxe that creates a chilling aura, slowing enemies within a certain radius of the player.
     * The effect only occurs while the player is actively using the pickaxe.*/
    class RognirsAnchor : ModItem
    {
        private const float auraRadius = 420f;
        private const int debuffTimer = 180;

        /// <summary>
		/// Sets the static default values for Rognir's Anchor.
		/// </summary>
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Able to mine Adamantite and Titanium ore");
        }

        /// <summary>
		/// Sets the default values for Rognir's Anchor.
		/// </summary>
        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Melee;    // 1.4
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 10;
            Item.useTurn = true;
            Item.useAnimation = 10;
            Item.pick = 150;
            Item.useStyle = 1;
            Item.knockBack = 6;
            Item.value = 6000;
            Item.rare = 2;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.tileBoost += 1;
        }

        /// <summary>
        /// A tModLoader hook method that is called when the player uses this item. This method is called
        /// for two effects. First, it adds the "Anchored" debuff to enemies within a certain distance.
        /// Second, it creates dust effects around the player within the same distance as the debuff.
        /// </summary>
        /// <param name="player"> The player who owns the Rognir's Anchor</param>
        /// <returns></returns>
        public override bool? UseItem(Player player)
        {
            /* For loop that iterates through enemies to find all enemies within the specified distance
             * And applies the debuff to them */
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active && !Main.npc[i].friendly)
                {
                    float distance = (Main.npc[i].Center - player.Center).Length();
                    if (distance < auraRadius)
                    {
                        Main.npc[i].AddBuff(BuffType<Buffs.Anchored>(), debuffTimer);
                    }
                }
            }

            // If block that determines if dust will appear within the debuff aura
            if (Main.rand.NextFloat() < 0.85f)
            {
                float xOffset = Main.rand.Next(-(int)auraRadius, (int)auraRadius);
                float yOffset = Main.rand.Next(-(int)auraRadius, (int)auraRadius);

                Vector2 position = new Vector2(player.Center.X + xOffset, player.Center.Y + yOffset);
                float distance = (player.Center - position).Length();
                if (distance <= auraRadius)
                {
                    Dust.NewDust(position, 10, 10, DustType<Dusts.RognirDust>());
                }
            }
            return true;
        }

    }
}