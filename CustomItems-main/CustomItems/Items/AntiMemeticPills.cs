// -----------------------------------------------------------------------
// <copyright file="AntiMemeticPills.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#nullable enable
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using MEC;
using PlayerRoles;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.SCP500)]
public class AntiMemeticPills : CustomItem
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 13;

    /// <inheritdoc/>
    public override string Name { get; set; } = "AM-119";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "Drugs that make you forget things. If you use these while you are targeted by SCP-096, you will forget what his face looks like, and thus no longer be a target.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1f;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new ()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new () { Chance = 100, Location = SpawnLocationType.Inside096 },
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
        if (!Check(ev.Player.CurrentItem))
            return;

        IEnumerable<Exiled.API.Features.Player> scp096S = Exiled.API.Features.Player.Get(RoleTypeId.Scp096);

        Timing.CallDelayed(1f, () =>
        {
            foreach (Exiled.API.Features.Player scp in scp096S)
            {
                if (scp.Role is Scp096Role scp096)
                {
                    if (scp096.HasTarget(ev.Player))
                        scp096.RemoveTarget(ev.Player);
                }
            }

            ev.Player.EnableEffect<AmnesiaVision>(10f, true);
        });
    }
}