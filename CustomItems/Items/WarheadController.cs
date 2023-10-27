// -----------------------------------------------------------------------
// <copyright file="Scp1499.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CustomItems.Events;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Mono.CompilerServices.SymbolWriter;
using UnityEngine;
using VoiceChat;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.Radio)]
public class WarheadController : CustomItem
{
    private readonly Dictionary<Player, Vector3> warheadcontroller = new();

    /// <inheritdoc/>
    public override uint Id { get; set; } = 19;

    /// <inheritdoc/>
    public override string Name { get; set; } = "REDACTED";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "REDACTED";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1.5f;

    /// <inheritdoc/>
    [Description("REDACTED")]

    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 2,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 5,
                Location = SpawnLocationType.Inside049Armory,
            },
            new()
            {
                Chance = 5,
                Location = SpawnLocationType.InsideGr18,
            },
            new()
            {
                Chance = 5,
                Location = SpawnLocationType.Inside330Chamber,
            },
            new()
            {
                Chance = 5,
                Location = SpawnLocationType.InsideEscapeSecondary,
            },
            new()
            {
                Chance = 25,
                Location = SpawnLocationType.InsideLocker,
            },
        },
    };

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting += OnVoiceChatting;
        Exiled.Events.Handlers.Player.Destroying += OnDestroying;
        Exiled.Events.Handlers.Player.Dying += OnDying;
        Exiled.Events.Handlers.Player.UsingRadioBattery += OnUsingRadio;

        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.VoiceChatting -= OnVoiceChatting;
        Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
        Exiled.Events.Handlers.Player.Dying -= OnDying;
        Exiled.Events.Handlers.Player.UsingRadioBattery -= OnUsingRadio;

        base.UnsubscribeEvents();
    }

    private void OnDying(DyingEventArgs ev)
    {
        if (warheadcontroller.ContainsKey(ev.Player))
            warheadcontroller.Remove(ev.Player);
    }

    private void OnDestroying(DestroyingEventArgs ev)
    {
        if (warheadcontroller.ContainsKey(ev.Player))
            warheadcontroller.Remove(ev.Player);
    }

    private void OnVoiceChatting(VoiceChattingEventArgs ev)
    {
        if (ev.VoiceMessage.Channel == VoiceChatChannel.Radio && (Check(ev.Player.CurrentItem) && ev.Player.CurrentItem.Base.name == "REDACTED"))
        {
            RadioWarheadManager.TriggerEvent(ev.Player, Warhead.IsInProgress, Warhead.IsDetonated);
        }
    }

    private void OnUsingRadio(UsingRadioBatteryEventArgs ev)
    {
        if (Check(ev.Item) && ev.Item.Base.name == "REDACTED")
        {
            ev.Radio.BatteryLevel = 100;
        }
    }
}
