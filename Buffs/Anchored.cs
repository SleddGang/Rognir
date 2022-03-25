using Terraria;
using Terraria.ModLoader;

namespace Rognir.Buffs
{
    class Anchored : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Anchored");
            Description.SetDefault("I think you took 'Break a leg' a little too literally");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity.X *= 0.925f;
            npc.velocity.Y *= 0.925f;
        }
    }
}
