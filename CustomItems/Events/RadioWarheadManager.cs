using Exiled.Events.EventArgs.Scp0492;
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
        public static string WarheadMessage { get; set; } = "$seconds seconds till <color=red>Warhead Detonation Sequence Initiation!</color>!";

        public static void TriggerEvent(Player player, bool wd)
        {
            if (wd)
            {
                Warhead.DetonationTimer = 60f;
                Warhead.Start();
                Warhead.IsLocked = true;

                foreach (Player p in Player.List)
                {
                    Timing.RunCoroutine(CountdownTimer(60, p));
                }
            }
        }

        private static IEnumerator<float> CountdownTimer(int duration, Player player)
        {
            int timeLeft = duration;
            while (true)
            {
                Timing.WaitForSeconds(0.1f);
                player.Broadcast(5, WarheadMessage.Replace("$seconds", timeLeft.ToString()), global::Broadcast.BroadcastFlags.Normal, true);
                yield return Timing.WaitForSeconds(1f);

                timeLeft -= 1;

                if (timeLeft != 0)
                    continue;
                player.Broadcast(5, "Alpha Warhead Denotation", global::Broadcast.BroadcastFlags.Normal, true);
                yield break;
            }
        }
    }
}
