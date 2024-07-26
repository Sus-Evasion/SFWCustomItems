// -----------------------------------------------------------------------
// <copyright file="Scp2818.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = System.Random;

namespace CustomItems.Items;

/// <summary>
/// A gun that kills you.
/// </summary>
[CustomItem(ItemType.GunE11SR)]
public class Scp2818 : CustomWeapon
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 12;

    /// <inheritdoc/>
    public override string Name { get; set; } = "SCP-2818";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "When this weapon is fired, it uses the biomass of the shooter as the bullet.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 3.95f;

    /// <inheritdoc/>
    [YamlIgnore]
    public override byte ClipSize { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the gun should despawn instead of drop when it is fired.
    /// </summary>
    [Description("Whether or not the weapon should despawn itself after it's been used.")]
    public bool DespawnAfterUse { get; set; } = false;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new();

    /// <inheritdoc/>
    [Description("The amount of Minimum damage the weapon deals when the projectile hits another player.")]
    public int MinimumDamage { get; set; } = 300;

    /// <inheritdoc/>
    [Description("The amount of Maximum damage the weapon deals when the projectile hits another player.")]
    public int MaximumDamage { get; set; } = 1000;

    /// <inheritdoc/>
    [Description(
        "[Note]: do not edit here | The amount of Final damage the weapon deals when the projectile hits another player.")]
    public override float Damage { get; set; }

    /// <inheritdoc/>
    protected override void OnShooting(ShootingEventArgs ev)
    {
        foreach (Item item in ev.Player.Items.ToList())
            if (Check(item))
            {
                Log.Debug("SCP-2818: Found a 2818 in inventory of shooter, removing.");
                ev.Player.RemoveItem(item);
            }

        Player target = Player.Get(ev.TargetNetId);
        if (DespawnAfterUse)
        {
            Log.Debug($"inv count: {ev.Player.Items.Count}");
            foreach (Item item in ev.Player.Items)
            {
                if (Check(item))
                {
                    Log.Debug("found 2818 in inventory, doing funni");
                    ev.Player.RemoveItem(item);
                }
            }
        }

        // Adds randomized damage and fixed issue of teamkilling due to lack of checks to team sides
        Random randomdamage = new Random();
        Damage = randomdamage.Next(MinimumDamage, MaximumDamage);

        if (target?.Role != RoleTypeId.Spectator && target?.Role.Side != ev.Player.Role.Side)
        {
            target?.Hurt(new UniversalDamageHandler(Damage, DeathTranslations.BulletWounds));
        }
    }
}