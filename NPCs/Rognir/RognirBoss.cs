using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Rognir.NPCs.Rognir
{
	/// <summary>
	/// Class <c>Rognir</c> is the base class of the Rognir boss. 
	/// It defines the defaults and AI for the boss.
	/// </summary>
	[AutoloadBossHead]
    class RognirBoss : ModNPC
    {
		/// <summary>
		/// Method <c>SetStaticDefaults</c> overrides the default <c>SetStaticDefaults</c> from the <c>ModNPC</c> class.
		/// The method sets the DisplayName to Rognir.
		/// </summary>
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rognir");
            Main.npcFrameCount[npc.type] = 2;
        }

		/// <summary>
		/// Method <c>SetDefaults</c> declares the default settings for the boss.
		/// </summary>
		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 40000;
			npc.damage = 100;
			npc.defense = 55;
			npc.knockBackResist = 0f;
			npc.width = 100;
			npc.height = 100;
			npc.value = Item.buyPrice(0, 20, 0, 0);
			npc.npcSlots = 15f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			npc.buffImmune[24] = true;

			//TODO Replace Boos_Fight_2 with final music.
			// Sets the music that plays when the boss spawns in and the priority of the music.  
			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/Boos_Fight_2");
			musicPriority = MusicPriority.BossMedium;
		}

		//TODO Make boss AI less dumb.
		/// <summary>
		/// Method <c>AI</c> defines the AI for the boss.
		/// </summary>
		public override void AI()
		{
			base.AI();
		}
	}
}
