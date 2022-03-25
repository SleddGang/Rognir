using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
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
            Item.maxStack = 999;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.rare = 9;
            Item.expert = true;
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
            var source = player.GetItemSource_OpenItem(Type);
            player.TryGettingDevArmor(source); // Gives player chance at getting dev gear
            if (Main.rand.NextFloat() > 0.5f)
            {
                player.QuickSpawnItem(source, ItemType<FrozenHookItem>()); // 50% chance to give Rognir's Frozen Hook
            } else
            {
                player.QuickSpawnItem(source, ItemType<RognirsAnchor>()); // 50% chance to give Rognir's Anchor
            }
            player.QuickSpawnItem(source, ItemType<VikingCrown>()); // Gives the player the Viking Crown item.  This is the expert mode item
            player.QuickSpawnItem(source, ItemID.HealingPotion, Main.rand.Next(5, 15)); // Gives the player 5-15 Healing Potions
        }

        /// <summary>
        /// Sets what boss will drop this loot bag. Determined with the npc.DropBossBags() method within the NPCLoot() hook
        /// </summary>
        public override int BossBagNPC => NPCType<NPCs.Rognir.RognirBoss>();

    }
}
