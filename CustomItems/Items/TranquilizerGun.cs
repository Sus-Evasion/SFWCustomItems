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
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pools;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Handlers;
using Interactables.Interobjects;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Item = Exiled.API.Features.Items.Item;
using Player = Exiled.API.Features.Player;
using Random = UnityEngine.Random;
using Warhead = Exiled.API.Features.Warhead;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.GunCOM18)]
public class TranquilizerGun : CustomWeapon
{
    private readonly Dictionary<Player, float> tranquilizedPlayers = new();
    private readonly List<Player> activeTranqs = new();

    /// <inheritdoc/>
    public override uint Id { get; set; } = 9;

    /// <inheritdoc/>
    public override string Name { get; set; } = "TG-119";

    /// <inheritdoc/>
    public override string Description { get; set; } = "This modifier USP fires non-lethal tranquilizing darts. Those affected will be rendered unconscious for a short duration. Unreliable against SCPs. Repeated tranquilizing of the same person will render them resistant to it's effect.";

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
                Chance = 50,
                Location = SpawnLocationType.InsideGr18,
            },
            new()
            {
                Chance = 80,
                Location = SpawnLocationType.Inside173Armory,
            },
        },
    };

    /// <inheritdoc/>
    public override byte ClipSize { get; set; } = 1;

    /// <inheritdoc/>
    public override float Damage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not SCPs should be resistant to tranquilizers. (Being resistant gives them a chance to not be tranquilized when shot).
    /// </summary>
    [Description("Whether or not SCPs should be resistant to tranquilizers. (Being resistant gives them a chance to not be tranquilized when shot).")]
    public bool ResistantScps { get; set; } = true;

    /// <summary>
    /// Gets or sets the amount of time a successful tranquilization lasts for.
    /// </summary>
    [Description("The amount of time a successful tranquilization lasts for.")]
    public float Duration { get; set; } = 5f;

    /// <summary>
    /// Gets or sets the exponential modifier used to determine how much time is removed from the effect, everytime a player is tranquilized, they gain a resistance to further tranquilizations, reducing the duration of future effects.
    /// </summary>
    [Description("Everytime a player is tranquilized, they gain a resistance to further tranquilizations, reducing the duration of future effects. This number signifies the exponential modifier used to determine how much time is removed from the effect.")]
    public float ResistanceModifier { get; set; } = 1.2f;

    /// <summary>
    /// Gets or sets a value indicating how often player resistances are reduced.
    /// </summary>
    [Description("How often the plugin should reduce the resistance amount for players, in seconds.")]
    public float ResistanceFalloffDelay { get; set; } = 120f;

    /// <summary>
    /// Gets or sets a value indicating whether or not tranquilized targets should drop all of their items.
    /// </summary>
    [Description("Whether or not tranquilized targets should drop all of their items.")]
    public bool DropItems { get; set; } = true;

    /// <summary>
    /// Gets or sets the percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.
    /// </summary>
    [Description("The percent chance an SCP will resist being tranquilized. This has no effect if ResistantScps is false.")]
    public int ScpResistChance { get; set; } = 40;

    public float Enrage { get; set; }

    /// <inheritdoc/>
    protected override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.PickingUpItem -= OnDeniableEvent;
        Exiled.Events.Handlers.Player.ChangingItem -= OnDeniableEvent;
        Scp049.StartingRecall -= OnDeniableEvent;
        Scp106.Teleporting -= OnDeniableEvent;
        Scp096.Charging -= OnDeniableEvent;
        Scp096.Enraging -= OnDeniableEvent;
        Scp096.AddingTarget -= OnDeniableEvent;
        Scp939.PlacingAmnesticCloud -= OnDeniableEvent;
        Exiled.Events.Handlers.Player.VoiceChatting -= OnDeniableEvent;
        activeTranqs.Clear();
        tranquilizedPlayers.Clear();
        Timing.KillCoroutines($"{nameof(TranquilizerGun)}-{Id}-reducer");
        base.UnsubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void SubscribeEvents()
    {
        Timing.RunCoroutine(ReduceResistances(), $"{nameof(TranquilizerGun)}-{Id}-reducer");
        Exiled.Events.Handlers.Player.PickingUpItem += OnDeniableEvent;
        Exiled.Events.Handlers.Player.ChangingItem += OnDeniableEvent;
        Scp049.StartingRecall += OnDeniableEvent;
        Scp106.Teleporting += OnDeniableEvent;
        Scp096.Charging += OnDeniableEvent;
        Scp096.Enraging += OnDeniableEvent;
        Scp096.AddingTarget += OnDeniableEvent;
        Scp939.PlacingAmnesticCloud += OnDeniableEvent;
        Exiled.Events.Handlers.Player.VoiceChatting += OnDeniableEvent;
        Exiled.Events.Handlers.Player.PickingUpItem += OnDeniableEvent;
        base.SubscribeEvents();
    }

    /// <inheritdoc/>
    protected override void OnHurting(HurtingEventArgs ev)
    {
        base.OnHurting(ev);

        if (ev.Attacker == ev.Player)
            return;

        if (ev.Attacker.Role.Team == ev.Player.Role.Team)
            return;

        if (ev.Player.Role.Team == Team.SCPs)
        {
            int r = Random.Range(1, 101);
            Log.Debug($"{Name}: SCP roll: {r} (must be greater than {ScpResistChance})");
            if (r <= ScpResistChance)
            {
                Log.Debug($"{Name}: {r} is too low, no tranq.");
                return;
            }
        }

        float duration = Duration;

        if (!tranquilizedPlayers.TryGetValue(ev.Player, out _))
            tranquilizedPlayers.Add(ev.Player, 1);

        tranquilizedPlayers[ev.Player] *= ResistanceModifier;
        Log.Debug($"{Name}: Resistance Duration Mod: {tranquilizedPlayers[ev.Player]}");

        duration -= tranquilizedPlayers[ev.Player];
        Log.Debug($"{Name}: Duration: {duration}");

        if (duration > 0f)
            Timing.RunCoroutine(DoTranquilize(ev.Player, duration));
    }

    private IEnumerator<float> DoTranquilize(Player player, float duration)
    {
        activeTranqs.Add(player);
        Item previousItem = player.CurrentItem;
        float newHealth = player.Health - Damage;
        List<StatusEffectBase> activeEffects = ListPool<StatusEffectBase>.Pool.Get();
        player.CurrentItem = null;

        if (newHealth <= 0)
            yield break;

        activeEffects.AddRange(player.ActiveEffects.Where(effect => effect.IsEnabled));

        try
        {
            if (DropItems)
            {
                if (player.Items.Count < 0)
                {
                    foreach (Item item in player.Items.ToList())
                    {
                        if (TryGet(item, out CustomItem? customItem))
                        {
                            customItem?.Spawn(player.Position, item, player);
                            player.RemoveItem(item);
                        }
                    }

                    player.DropItems();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(DoTranquilize)}: {e}");
        }

        if (player.Role is Scp096Role scp)
        {
            if (scp.RageManager.EnragedTimeLeft >= 20f)
            {
                scp.RageManager.EnragedTimeLeft -= 20f;
            }
            else
            {
                scp.RageManager.ServerEndEnrage();
            }
        }

        try
        {
            player.Health = newHealth;

            player.EnableEffect<AmnesiaVision>(duration);
            player.EnableEffect<AmnesiaItems>(duration);
            player.EnableEffect<Ensnared>(duration);
            if (player.Role.Type is RoleTypeId.Scp173)
            {
                player.EnableEffect<Flashed>(duration);
            }
        }
        catch (Exception e)
        {
            Log.Error(e);
        }

        yield return Timing.WaitForSeconds(duration);

        try
        {
            if (player.GameObject == null)
                yield break;

            newHealth = player.Health;
            player.Health = newHealth;

            if (!DropItems)
                player.CurrentItem = previousItem;

            foreach (StatusEffectBase effect in activeEffects.Where(effect => (effect.Duration - duration) > 0))
                player.EnableEffect(effect, effect.Duration);

            activeTranqs.Remove(player);
            ListPool<StatusEffectBase>.Pool.Return(activeEffects);
        }
        catch (Exception e)
        {
            Log.Error($"{nameof(DoTranquilize)}: {e}");
        }

        if (Warhead.IsDetonated && player.Position.y < 900)
        {
            player.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Warhead));
        }
    }

    private IEnumerator<float> ReduceResistances()
    {
        for (; ;)
        {
            foreach (Player player in tranquilizedPlayers.Keys)
                tranquilizedPlayers[player] = Mathf.Max(0, tranquilizedPlayers[player] / 2);

            yield return Timing.WaitForSeconds(ResistanceFalloffDelay);
        }
    }

    private void OnDeniableEvent(IDeniableEvent ev)
    {
        if (ev is IPlayerEvent eP)
        {
            if (activeTranqs.Contains(eP.Player))
                ev.IsAllowed = false;
        }
    }
}