// -----------------------------------------------------------------------
// <copyright file="TranquilizerGun.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pools;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using InventorySystem.Items;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Events;
using UnityEngine;
using RadioManager = CustomItems.Events.RadioWarheadManager;
using Item = Exiled.API.Features.Items.Item;
using Player = Exiled.Events.Handlers.Player;
using Random = UnityEngine.Random;
using Warhead = Exiled.API.Features.Warhead;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.Radio)]
public class WarheadDetonator : CustomItem
{

    /// <inheritdoc/>
    public override uint Id { get; set; } = 666;

    /// <inheritdoc/>
    public override string Name { get; set; } = "WD-EOW";

    /// <inheritdoc/>
    public override string Description { get; set; } = "This modified radio was synced to the Alpha Warhead and activates it in 60 seconds with locked mechanism, should there be any containment breach occured. Restricted until 20 minutes of round";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1.55f;

    /// <inheritdoc />
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 100,
                Location = SpawnLocationType.InsideSurfaceNuke,
            },
        },
    };

    /// <summary>
    /// Gets or sets the amount of time warhead detonation will take.
    /// </summary>
    [Description("Warhead Detonation Duration")]
    public float Duration { get; set; } = 60f;

    /// <summary>
    /// Gets or sets the percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.
    /// </summary>
    [Description("The percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.")]
    public int ScpResistChance { get; set; } = 40;
    
    public static string Dropped { get; set; } = "<color=red>Warhead Detonator was dropped at ROOM !</color>";

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Player.DroppedItem -= OnDropping;
        Player.PickingUpItem -= OnPickingUp;
        Player.UsingRadioBattery -= OnUsingRadio;
        base.UnsubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Player.DroppedItem += OnDropping;
        Player.PickingUpItem += OnPickingUp;
        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected void OnDropping(DroppedItemEventArgs ev)
    {
        foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
        {
            player.Broadcast(5, Dropped.Replace("ROOM", ev.Player.CurrentRoom.Name), global::Broadcast.BroadcastFlags.Normal, true);
        }
    }

    protected void OnPickingUp(PickingUpItemEventArgs ev)
    {
        foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
        {
            player.Broadcast(5, Dropped.Replace("ROOM", ev.Player.CurrentRoom.Name), global::Broadcast.BroadcastFlags.Normal, true);
        }
    }

    protected void OnUsingRadio(UsingRadioBatteryEventArgs ev)
    {
        if (Check(ev.Player) && (Round.ElapsedTime.TotalSeconds > 1200))
        {
            RadioManager.TriggerEvent(ev.Player, true);
        }
    }
}
