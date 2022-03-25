using Rognir.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir
{
    class RognirPlayer : ModPlayer
    {
        // Type ids of the monsters that will become friendly.  
        private static int[] ids = new int[] { -55, -54, -45, -44, -37, -36, -35, -34, -33, -32, -31, -30, -29, -28, -27, -26, 3, 132, 161, 186, 187, 188, 189, 200, 223, 254, 255, 319, 320, 321, 331, 332, 338, 339, 340, 430, 431, 432, 433, 434, 435, 436, 489, NPCID.ArmoredViking,
                        NPCID.UndeadViking, NPCID.UndeadMiner, -52, -51, -50, -49, -48, -47, -46, -15, 21, 77, 110, 201, 202, 203, 291, 292, 293, 322, 323, 324, 449, 450, 451, 452, 453, 481, 566, 567 };

        public bool vikingCrown = false;

        public override void ResetEffects()
        {
            vikingCrown = false;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
        {
            if (vikingCrown)
            {
                if (Player.ZoneSnow)
                {
                    if (Array.Exists(ids, element => element == npc.type))
                    {
                        return false;
                    }
                }
            }
            return base.CanBeHitByNPC(npc, ref cooldownSlot);
        }
    }
}
