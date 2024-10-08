// -----------------------------------------------------------------------
// <copyright file="AntiMemeticPills.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#nullable enable

namespace CustomItems.Items;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using MEC;
using PlayerRoles;

/// <inheritdoc />
[CustomItem(ItemType.SCP500)]
public class Scp500B : CustomItem
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 19;

    /// <inheritdoc/>
    public override string Name { get; set; } = "SCP500B";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "Drug that allows you to betray your current faction.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1f;

    public override bool ShouldMessageOnGban { get; } = true;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new ()
    {
        Limit = 3,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new () { Chance = 50, Location = SpawnLocationType.Inside173Gate },
            new () { Chance = 20, Location = SpawnLocationType.Inside096 },
            new () { Chance = 10, Location = SpawnLocationType.InsideLocker },
            new () { Chance = 20, Location = SpawnLocationType.InsideHczArmory },
        },
    };

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Player.UsingItem += OnUsingItem;
        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Player.UsingItem -= OnUsingItem;
        base.UnsubscribeEvents();
    }

    private void OnUsingItem(UsingItemEventArgs ev)
    {
        // Check if the player can switch roles using the item
        if (!Check(ev.Player.CurrentItem))
            return;

        // Delayed role switch logic
        Timing.CallDelayed(1f, () =>
        {
            if (ev.Player.Role.Side == Side.ChaosInsurgency && ev.Player.Role.Type == RoleTypeId.ClassD)
            {
                // Switch to Scientist
                SwitchRole(ev.Player, RoleTypeId.Scientist, "MTF", "Scientist", "blue", "yellow");
            }
            else if (ev.Player.Role.Side == Side.Mtf && ev.Player.Role.Type == RoleTypeId.Scientist)
            {
                // Switch to D Boi
                SwitchRole(ev.Player, RoleTypeId.ClassD, "Chaos Insurgency", "D Boi", "green", "orange");
            }
            else if (IsChaosRole(ev.Player.Role.Type))
            {
                // Switch to Ntf Private
                SwitchRole(ev.Player, RoleTypeId.NtfPrivate, "MTF", "Ntf Private", "blue", "blue");
            }
            else if (IsNtfRole(ev.Player.Role.Type))
            {
                // Switch to Chaos Conscript
                SwitchRole(ev.Player, RoleTypeId.ChaosConscript, "Chaos Insurgency", "Chaos Conscript", "green", "green");
            }
        });
    }

    // Helper function to check if a role is one of the Chaos roles
    private bool IsChaosRole(RoleTypeId roleType)
    {
    return roleType is RoleTypeId.ChaosConscript or RoleTypeId.ChaosRifleman or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor;
    }

    private bool IsNtfRole(RoleTypeId roleType)
    {
    return roleType is RoleTypeId.NtfPrivate or RoleTypeId.NtfCaptain or RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist;
    }

    // Helper function to switch the player's role and broadcast a message
    private void SwitchRole(Exiled.API.Features.Player player, RoleTypeId newRole, string faction, string roleName, string factionColor, string roleColor)
    {
        player.ClearInventory();
        player.Role.Set(newRole, SpawnReason.ForceClass, RoleSpawnFlags.AssignInventory);
        string message = $"{player.Nickname} has committed treason & switched sides to <color={factionColor}>{faction}</color> Faction & became a <color={roleColor}>{roleName}</color>";
        Exiled.API.Features.Map.Broadcast(5, message);
    }
}
