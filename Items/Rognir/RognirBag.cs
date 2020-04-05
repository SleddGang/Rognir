using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.Items.Rognir
{
    public class RognirBag : ModItem
    {
        /// <summary>
        /// Simply sets the static defaults (for reference elsewhere) of the item
        /// </summary>
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Treasure Bag");
            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
        }

        /// <summary>
        /// Simply sets the local defaults (for use here) of the item
        /// </summary>
        public override void SetDefaults()
        {
            item.maxStack = 1;
            item.consumable = true;
            item.width = 24;
            item.height = 24;
            item.rare = 11;
            item.expert = true;
        }

        /// <summary>
        /// Determines whether the player can right click to use item
        /// </summary>
        /// <returns>boolean value of whether the item can be used with right click</returns>
        public override bool CanRightClick()
        {
            return true;
        }

        /// <summary>
        /// Defines what happens when the boss bag is opened
        /// </summary>
        /// <param name="player"> The player who owns the boss bag</param>
        public override void OpenBossBag(Player player)
        {
            player.TryGettingDevArmor(); // Gives player chance at getting dev gear
            if (Main.rand.NextFloat() > 0.5f)
            {
                player.QuickSpawnItem(ItemType<FrozenHookItem>()); // 50% chance to give Rognir's Frozen Hook
            } else
            {
                player.QuickSpawnItem(ItemType<RognirsAnchor>()); // 50% chance to give Rognir's Anchor
            }
            player.QuickSpawnItem(ItemType<VikingCrown>()); // Gives the player the Viking Crown item.  This is the expert mode item.
        }

        /// <summary>
        /// Sets what boss will drop this loot bag. Determined with the npc.DropBossBags() method within the NPCLoot() hook
        /// </summary>
        public override int BossBagNPC => NPCType<NPCs.Rognir.RognirBoss>();

    }
}
