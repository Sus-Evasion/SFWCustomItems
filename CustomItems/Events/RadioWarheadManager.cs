using Waits;

namespace CustomItems.Events
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using MEC;
    using PlayerRoles;

    public class RadioWarheadManager
    {

        [Description("cooldown message displayed when used")]
        public static string CooldownMessage { get; set; } = "$seconds seconds till <color=red>Warhead Battery</color> charged";

        [Description("Hint displayed once the player uses the controller and warhead was detonated.")]
        public static string DetonatedMessage { get; set; } = "Unable to detect Alpha Warhead Signal!";

        [Description("Hint displayed once the player's disguise is over")]
        public static string RadioAvaiability { get; set; } = "Warhead Controller Recharged";

        public static bool CanDetonate { get; set; } = true;

        public static void TriggerEvent(Player player, bool warheadtriggered, bool warheaddetonated)
        {
            if (CanDetonate)
            {
                if (warheaddetonated)
                {
                    player.Broadcast(5, DetonatedMessage, global::Broadcast.BroadcastFlags.Normal);
                }
                else
                {
                    if (warheadtriggered)
                    {
                        Warhead.Stop();
                        CanDetonate = false;
                        Timing.RunCoroutine(CountdownTimer(120, player));
                    }
                    else
                    {
                        if (Round.ElapsedTime.TotalSeconds > 1320)
                        {
                            Warhead.IsLocked = true;
                        }

                        Timing.WaitForSeconds(1);
                        Warhead.Start();
                        CanDetonate = false;
                        Timing.RunCoroutine(CountdownTimer(120, player));
                    }
                }
            }
        }

        private static IEnumerator<float> CountdownTimer(int duration, Player player)
        {
            int timeLeft = duration;
            while (true)
            {
                Timing.WaitForSeconds(0.1f);
                player.ShowHint(CooldownMessage.Replace("$seconds", timeLeft.ToString()));
                yield return Timing.WaitForSeconds(1f);

                timeLeft -= 1;

                if (timeLeft != 0)
                    continue;
                CanDetonate = true;
                player.ShowHint(RadioAvaiability, 5f);
                yield break;
            }
        }
    }
}