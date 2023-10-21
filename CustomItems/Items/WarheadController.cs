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
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
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
    public override string Name { get; set; } = "Warhead Controller";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "This Radio bypasses warhead clearance and activates countdown of alpha warhead";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 1.5f;

    [Description("Battery for activating alpha warhead")]

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 100,
                Location = SpawnLocationType.Inside079First,
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
        if (ev.VoiceMessage.Channel == VoiceChatChannel.Radio)
        {
            RadioWarheadManager.TriggerEvent(ev.Player, Warhead.IsInProgress, Warhead.IsDetonated);
        }
    }

    private void OnUsingRadio(UsingRadioBatteryEventArgs ev)
    {
        ev.Radio.BatteryLevel = 100;
    }
}