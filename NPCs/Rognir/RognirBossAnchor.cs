using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir.NPCs.Rognir
{
    class RognirBossAnchor : ModNPC
    {
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Anchor of Rognir");
			Main.npcFrameCount[npc.type] = 4;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 400000;
			npc.damage = 0;
			npc.defense = 70;
			npc.knockBackResist = 0f;
			npc.width = 10;
			npc.height = 10;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath6;
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax / Main.expertLife * bossLifeScale);
			npc.defense = 72;
		}

		public override void AI()
		{
			NPC owner = Main.npc[(int)npc.ai[0]];
			if (!owner.active || owner.type != NPCType<RognirBoss>())
			{
				npc.active = false;
				return;
			}
			RognirBoss modOwner = (RognirBoss)owner.modNPC;
			if (npc.localAI[0] == 0f)
			{
				/*if (modOwner.targets.Contains(Main.myPlayer))
				{
					Main.PlaySound(SoundID.Item2);
				}
				else
				{
					Main.PlaySound(SoundID.Item2, npc.position);
				}*/
				npc.localAI[0] = 1f;
			}
			/*Vector2 targetPos = owner.Center;
			Vector2 direction = targetPos - npc.Center;
			if (direction != Vector2.Zero)
			{
				float speed = direction.Length();
				if (speed > 2f)
				{
					speed = 2f;
				}
				direction.Normalize();
				direction *= speed;
				npc.position += direction;
			}
			else
			{
				npc.localAI[1] = 1f;
			}*/

			Vector2 moveTo = owner.Center + new Vector2(0, -200);
			npc.velocity = (moveTo - npc.Center) / 50;
		}
	}
}
