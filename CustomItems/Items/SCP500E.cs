// -----------------------------------------------------------------------
// <copyright file="AntiMemeticPills.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

/*
// #nullable enable
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
public class SCP500E : CustomItem
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 21;

    /// <inheritdoc/>
    public override string Name { get; set; } = "SCP500E";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "Drugs that make you explode. If you use these, you will explode yourself, and thus no damaging around everyone thats not in your team.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1f;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new ()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new () { Chance = 100, Location = SpawnLocationType.InsideLocker },
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

        Timing.CallDelayed(1f, () =>
        {
            ev.Player.Explode(ProjectileType.FragGrenade, ev.Player);
        });
    }
}
*/