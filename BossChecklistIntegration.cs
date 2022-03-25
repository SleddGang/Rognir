// 1.4 https://github.com/JavidPack/BossChecklist/blob/1.4/BossChecklistIntegrationExample.cs

using Rognir.Items.Rognir;
using System;
using System.Linq;
using System.Collections.Generic;
using Rognir.Items;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Rognir
{
    public class BossChecklistIntegration : ModSystem
    {
        // Boss Checklist might add new features, so a version is passed into GetBossInfo. 
        // If a new version of the GetBossInfo Call is implemented, find this class in the Boss Checklist Github once again and replace this version with the new version: https://github.com/JavidPack/BossChecklist/blob/master/BossChecklistIntegrationExample.cs
        private static readonly Version BossChecklistAPIVersion = new Version(1, 1); // Do not change this yourself.

        public class RognirInfo
        {
            internal string key = "Rognir Rognir"; // equal to "modSource internalName"
            internal string modSource = "Rognir";
            internal string internalName = "Rognir";
            internal string displayName = "Rognir";

            internal float
                progression =
                    5.5f; // See https://github.com/JavidPack/BossChecklist/blob/master/BossTracker.cs#L13 for vanilla boss values

            internal Func<bool> downed = () => RognirWorld.downedRognir;
            internal bool isBoss = true;
            internal bool isMiniboss = false;
            internal bool isEvent = false;

            internal List<int>
                npcIDs = new List<int>(); // Does not include minions, only npcids that count towards the NPC still being alive.

            internal List<int> spawnItem = new List<int>(ItemType<FrozenCrown>());
            internal List<int> loot = new List<int>() {ItemType<FrozenHookItem>(), ItemType<RognirsAnchor>()};
            internal List<int> collection = new List<int>();
        }

        public static Dictionary<string, RognirInfo> rognirInfos = new Dictionary<string, RognirInfo>();

        public static bool IntegrationSuccessful { get; private set; }

        public override void PostAddRecipes()
        {
            // For best results, this code is in PostAddRecipes
            rognirInfos.Clear();
            if (ModLoader.TryGetMod("BossChecklist", out Mod bossChecklist) &&
                bossChecklist.Version >= BossChecklistAPIVersion)
            {
                object currentBossInfoResponse =
                    bossChecklist.Call("GetBossInfoDictionary", Mod, BossChecklistAPIVersion.ToString());
                if (currentBossInfoResponse is Dictionary<string, Dictionary<string, object>> bossInfoList)
                {
                    rognirInfos = bossInfoList.ToDictionary(boss => boss.Key, boss => new RognirInfo()
                    {
                        key = boss.Value.ContainsKey("key") ? boss.Value["key"] as string : "",
                        modSource = boss.Value.ContainsKey("modSource") ? boss.Value["modSource"] as string : "",
                        internalName = boss.Value.ContainsKey("internalName")
                            ? boss.Value["internalName"] as string
                            : "",
                        displayName = boss.Value.ContainsKey("displayName") ? boss.Value["displayName"] as string : "",
                        progression = boss.Value.ContainsKey("progression")
                            ? Convert.ToSingle(boss.Value["progression"])
                            : 0f,
                        downed = boss.Value.ContainsKey("downed") ? boss.Value["downed"] as Func<bool> : () => false,
                        isBoss = boss.Value.ContainsKey("isBoss") ? Convert.ToBoolean(boss.Value["isBoss"]) : false,
                        isMiniboss = boss.Value.ContainsKey("isMiniboss")
                            ? Convert.ToBoolean(boss.Value["isMiniboss"])
                            : false,
                        isEvent = boss.Value.ContainsKey("isEvent") ? Convert.ToBoolean(boss.Value["isEvent"]) : false,
                        npcIDs = boss.Value.ContainsKey("npcIDs") ? boss.Value["npcIDs"] as List<int> : new List<int>(),
                        spawnItem = boss.Value.ContainsKey("spawnItem")
                            ? boss.Value["spawnItem"] as List<int>
                            : new List<int>(),
                        loot = boss.Value.ContainsKey("loot") ? boss.Value["loot"] as List<int> : new List<int>(),
                        collection = boss.Value.ContainsKey("collection")
                            ? boss.Value["collection"] as List<int>
                            : new List<int>(),
                    });
                    IntegrationSuccessful = true;
                }
            }
        }
        
        public override void Unload() {
            rognirInfos.Clear();
        }
    }
}