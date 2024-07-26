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

    public class AppearanceManager
    {
        [Description("If hat is being used")]
        public bool ChangedPlayerAppearance { get; set; } = false;

        [Description("List of roles the player can turn to. As you can imagine scp-079 is not an option.")]
        public List<RoleTypeId> PossibleRoles { get; set; } = new ()
        {
            RoleTypeId.Scp173,
            RoleTypeId.Scp049,
            RoleTypeId.Scp096,
            RoleTypeId.Scp106,
            RoleTypeId.Scp939,
        };

        [Description("Amount of time the player's appearance will be changed")]
        public int Duration { get; set; } = 15;

        [Description("Hint displayed once the player changes appearance and counts the time left. Make sure to add '$new_role_name' , '$old_role_name' '$time_left', these will be replaced by the actual values")]
        public string DisguiseMessage { get; set; } = "You are disguised as $new_role_name. You have <color=#ff0000>$time_left</color> seconds left.";

        [Description("Hint displayed once the player's disguise is over")]
        public string NoLongerInDisguise { get; set; } = "You are no longer in disguise";

        public void ChangeAppearance(Player player)
        {
            RoleTypeId DisguiseRole = PossibleRoles.Where(role => role != RoleTypeId.Scp079).ToList()[new Random().Next(PossibleRoles.Count)];
            player.ChangeAppearance(DisguiseRole);
            ChangedPlayerAppearance = true;
            Log.Debug($"Generated Disguise was: {DisguiseRole.GetFullName()}");

            RoleTypeId OldRole = player.Role.Type;
            Timing.RunCoroutine(DurationTimer(Duration, player, DisguiseRole, OldRole));
        }

        private IEnumerator<float> DurationTimer(int duration, Player player, RoleTypeId new_role, RoleTypeId old_role)
        {
            int timeLeft = duration;
            while (true)
            {
                player.ShowHint(DisguiseMessage.Replace("$new_role_name", new_role.GetFullName()).Replace("$time_left", timeLeft.ToString().Replace("$old_role_name", old_role.GetFullName())));
                yield return Timing.WaitForSeconds(1f);

                timeLeft -= 1;

                if (timeLeft != 0)
                    continue;
                player.ChangeAppearance(player.Role.Type);
                player.ShowHint(NoLongerInDisguise);
                yield break;
            }
        }
    }
}