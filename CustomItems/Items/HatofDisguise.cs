// -----------------------------------------------------------------------
// <copyright file="Scp1499.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using CustomItems.Events;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.SCP268)]
public class HatofDisguise : CustomItem
{
    private readonly Dictionary<Player, Vector3> hatofDisguise = new();

    /// <inheritdoc/>
    public override uint Id { get; set; } = 16;

    /// <inheritdoc/>
    public override string Name { get; set; } = "Hat of Disguise";

    /// <inheritdoc/>
    public override string Description { get; set; } = "The Hat that changes your appearance, when you put it on.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1.5f;

    public AppearanceManager AppearanceOptions { get; set; } = new ();

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new();

    /// <summary>
    /// Gets or sets how long the SCP-1499 can be wore, before automaticly player takes it off. (set to 0 for no limit).
    /// </summary>
    [Description("How long the Hat can be wore, before automaticly player takes it off. (set to 0 for no limit)")]
    public float Duration { get; set; } = 15f;

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem += OnUsedItem;
        Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        Exiled.Events.Handlers.Player.Dying += OnDying;

        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.UsedItem -= OnUsedItem;
        Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        Exiled.Events.Handlers.Player.Dying -= OnDying;

        base.UnsubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        if (AppearanceOptions.ChangedPlayerAppearance)
            ev.Player.ChangeAppearance(ev.Player.Role.Type);
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (AppearanceOptions.ChangedPlayerAppearance)
            ev.Player.ChangeAppearance(ev.Player.Role.Type);
    }

    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (AppearanceOptions.ChangedPlayerAppearance)
            ev.Player.ChangeAppearance(ev.Player.Role.Type);
    }

    private void OnUsedItem(UsedItemEventArgs ev)
    {
        if (!Check(ev.Player.CurrentItem))
            return;
        AppearanceOptions.ChangeAppearance(ev.Player);

        ev.Player.DisableEffect(EffectType.Invisible);

        if (Duration > 0)
        {
            Timing.CallDelayed(Duration, () =>
            {
                hatofDisguise.Remove(ev.Player);
            });
        }
    }
}
