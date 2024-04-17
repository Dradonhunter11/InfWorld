using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        private static void OnBordersMovement(Terraria.On_Player.orig_BordersMovement orig, Terraria.Player self)
        {
            if (self.position.X < Main.leftWorld + 640f + 16f)
            {
                Main.cameraX = 0f;
                self.position.X = Main.leftWorld + 640f + 16f;
                self.velocity.X = 0f;
            }

            if (self.position.Y < Main.topWorld + 640f + 16f)
            {
                if (Main.remixWorld || self.forcedGravity > 0)
                {
                    if (self.position.Y < Main.topWorld + 640f + 16f - (float)self.height && !self.dead)
                        self.KillMe(PlayerDeathReason.ByOther(19), 10.0, 0);

                    if (self.position.Y < Main.topWorld + 320f + 16f)
                    {
                        self.position.Y = Main.topWorld + 320f + 16f;
                        if (self.velocity.Y < 0f)
                            self.velocity.Y = 0f;

                        self.gravDir = 1f;
                    }
                }
                else
                {
                    self.position.Y = Main.topWorld + 640f + 16f;
                    if ((double)self.velocity.Y < 0.11)
                        self.velocity.Y = 0.11f;

                    self.gravDir = 1f;
                }

                AchievementsHelper.HandleSpecialEvent(self, 11);
            }

            if (self.position.Y > Main.bottomWorld + 200 * 16 - 640f - 32f - (float)self.height)
            {
                self.position.Y = Main.bottomWorld + 200 * 16 - 640f - 32f - (float)self.height;
                self.velocity.Y = 0f;
            }

            if (self.position.Y > Main.bottomWorld + 200 * 16 - 640f - 150f - (float)self.height)
                AchievementsHelper.HandleSpecialEvent(self, 10);
            return;
        }
    }
}
