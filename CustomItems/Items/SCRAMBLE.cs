// -----------------------------------------------------------------------
// <copyright file="Scp1499.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using CustomItems.Events;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.Features;
using Exiled.Events.Handlers;
using MEC;
using PlayerRoles;
using PluginAPI.Roles;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.SCP268)]
public class SCRAMBLE : CustomItem
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 20;

    /// <inheritdoc/>
    private readonly List<Player> SCRAMBLEPlayers = new();

    /// <inheritdoc/>
    public override string Name { get; set; } = "SCRAMBLE";

    /// <inheritdoc/>
    public override string Description { get; set; } = "Wearing this googles will not trigger 096";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1.5f;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new () { Chance = 100, Location = SpawnLocationType.Inside096 },
        },
    };

    /// <summary>
    /// Gets or sets how long the SCP-1499 can be wore, before automaticly player takes it off. (set to 0 for no limit).
    /// </summary>
    [Description("How long the Hat can be wore, before automaticly player takes it off. (set to 0 for no limit)")]
    public int Duration { get; set; } = 10;

    /// <inheritdoc/>
    public override bool ShouldMessageOnGban { get; } = true;

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
        Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        Exiled.Events.Handlers.Player.Dying += OnDying;
        Scp096.AddingTarget += OnAddingTarget;
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy += OnUsingMicroHIDEnergy;
        Exiled.Events.Handlers.Player.ThrownProjectile += OnThrowingProjectile;
        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
        Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        Scp096.AddingTarget -= OnAddingTarget;
        Exiled.Events.Handlers.Player.UsingMicroHIDEnergy -= OnUsingMicroHIDEnergy;
        Exiled.Events.Handlers.Player.ThrownProjectile -= OnThrowingProjectile;
        base.UnsubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Remove(ev.Player);
    }

    protected override void OnWaitingForPlayers()
    {
        SCRAMBLEPlayers.Clear();

        base.OnWaitingForPlayers();
    }

    protected void OnThrowingProjectile(ThrownProjectileEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Remove(ev.Player);
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Remove(ev.Player);
    }

    private void OnUsingMicroHIDEnergy(UsingMicroHIDEnergyEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Remove(ev.Player);
    }

    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Remove(ev.Player);
    }

    private void OnUsedItem(UsedItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;

        if (!SCRAMBLEPlayers.Contains(ev.Player))
            SCRAMBLEPlayers.Add(ev.Player);

        ev.Player.DisableEffect(EffectType.Invisible);

        Timing.RunCoroutine(DurationTimer(Duration, ev.Player));

        Timing.CallDelayed(Duration, () =>
        {
            SCRAMBLEPlayers.Remove(ev.Player);
        });
    }

    private void OnAddingTarget(AddingTargetEventArgs ev)
    {
        if (SCRAMBLEPlayers.Contains(ev.Player))
        {
            ev.IsAllowed = false;
        }
    }

    private IEnumerator<float> DurationTimer(int duration, Player player)
    {
        int timeLeft = duration;
        while (true)
        {
            player.ShowHint($"ALERT: SCRAMBLE Battery at {timeLeft * 5}%", 1f);
            yield return Timing.WaitForSeconds(1f);

            if (!SCRAMBLEPlayers.Contains(player))
            {
                timeLeft = 0;
            }

            if (timeLeft > 0)
                timeLeft -= 1;

            if (timeLeft != 0)
                continue;
            player.ShowHint("ALERT: SCRAMBLE Battery depleted! Goggles ability will be disabled!", 5f);
            yield break;
        }
    }
}
