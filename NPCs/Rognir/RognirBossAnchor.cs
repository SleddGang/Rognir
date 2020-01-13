using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
			Main.npcFrameCount[npc.type] = 1;
			NPCID.Sets.MustAlwaysDraw[npc.type] = true;
		}

		public override void SetDefaults()
		{
			npc.aiStyle = -1;
			npc.lifeMax = 400000;
			npc.damage = 50;
			npc.defense = 70;
			npc.knockBackResist = 0f;
			npc.width = 179;
			npc.height = 311;
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

		public override void AI()
		{
			// Check if owner is Rognir and if it is still alive.
			NPC owner = Main.npc[(int)npc.ai[0]];
			if (!owner.active || owner.type != NPCType<RognirBoss>())
			{
				npc.active = false;
				return;
			}

			// player is the current player that Rognir is targeting.
			Player player = Main.player[npc.target];

			/*
			 * Checks if the current player target is alive and active.  
			 * If not then the boss will run away and despawn.
			 */
			if (!player.active || player.dead)
			{
				npc.TargetClosest(true);
				player = Main.player[npc.target];
				npc.netUpdate = true;
				if (!player.active || player.dead)
				{
					npc.velocity = new Vector2(0f, 10f);
					if (npc.timeLeft > 10)
					{
						npc.timeLeft = 10;
					}
					return;
				}
			}

			npc.velocity += (player.Center - npc.Center) / 1000;
			float speed = npc.velocity.Length();

			if (speed > 15f)
				speed = 15f;

			npc.velocity.Normalize();

			npc.velocity *= speed;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			if (target.HasBuff(BuffID.Chilled))
			{
				damage += 10;
			}
			base.OnHitPlayer(target, damage, crit);
		}
	}
}
