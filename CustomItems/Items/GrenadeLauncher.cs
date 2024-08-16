// -----------------------------------------------------------------------
// <copyright file="GrenadeLauncher.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Components;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms.BasicMessages;
using MEC;
using PlayerStatsSystem;
using UnityEngine;
using Player = PluginAPI.Core.Player;
using Loader = Exiled.Loader.Loader;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.GunLogicer)]
public class GrenadeLauncher : CustomWeapon
{
    private CustomGrenade? loadedCustomGrenade;

    private ProjectileType loadedGrenade = ProjectileType.FragGrenade;

    /// <inheritdoc/>
    public override uint Id { get; set; } = 1;

    /// <inheritdoc/>
    public override string Name { get; set; } = "GL-119";

    /// <inheritdoc/>
    public override string Description { get; set; } = "This weapon will launch grenades in the direction you are firing, instead of bullets. Requires Frag Grenades in your inventory to reload.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 2.95f;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 1,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 50,
                Location = SpawnLocationType.Inside049Armory,
            },
            new()
            {
                Chance = 50,
                Location = SpawnLocationType.InsideHczArmory,
            },
        },
    };

    /// <inheritdoc/>
    public override float Damage { get; set; }

    /// <inheritdoc/>
    public override byte ClipSize { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether or not players will need actual frag grenades in their inventory to use as ammo. If false, the weapon's base ammo type is used instead.
    /// </summary>
    [Description("Whether or not players will need actual frag grenades in their inventory to use as ammo. If false, the weapon's base ammo type is used instead.")]
    public bool UseGrenades { get; set; } = true;

    /// <summary>
    /// Gets or sets the speed of grenades when they shoot out of the weapon.
    /// </summary>
    [Description("The speed of grenades when they shoot out of the weapon.")]
    public float GrenadeSpeed { get; set; } = 1.5f;

    /// <summary>
    /// Gets or sets the max duration of the fuse of grenades shot from the weapon. Note, these grenades will always explode immediatly when they collide with something, but this can be used with slow-moving grenades to cause mid-air explosions.
    /// </summary>
    [Description("The max duration of the fuse of grenades shot from the weapon. Note, these grenades will always explode immediatly when they collide with something, but this can be used with slow-moving grenades to cause mid-air explosions.")]
    public float FuseTime { get; set; } = 1f;

    /// <summary>
    /// Gets or sets a value indicating whether the GL should ignore modded grenades.
    /// </summary>
    [Description("Whether or not the Grenade Launcher will consider modded frag grenades as viable grenades for reloading.")]
    public bool IgnoreModdedGrenades { get; set; } = false;

    /// <summary>
    /// sets a value indicating the chance of Grenade Launcher exploding in the face of player.
    /// </summary>
    [Description("The Chance of Grenade Launcher exploding, Range from 0 to 100. (Set 0 for no explosion)")]

    public int Chance { get; set; } = 35;

    /// <summary>
    /// Gets or sets a value indicating projectile's serial number for tracking purpose.
    /// </summary>
    public ushort ProjectileSerial { get; set; }

    /// <inheritdoc/>
    protected override void OnReloading(ReloadingWeaponEventArgs ev)
    {
        if (UseGrenades)
        {
            ev.IsAllowed = false;

            if (!(ev.Player.CurrentItem is Firearm firearm) || firearm.Ammo >= ClipSize)
                return;

            Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} is reloading!");

            foreach (Item item in ev.Player.Items.ToList())
            {
                Log.Debug($"{Name}.{nameof(OnReloading)}: Found item: {item.Type} - {item.Serial}");
                if (item.Type != ItemType.GrenadeHE && item.Type != ItemType.GrenadeFlash && item.Type != ItemType.SCP018)
                    continue;
                if (TryGet(item, out CustomItem? cItem))
                {
                    if (IgnoreModdedGrenades)
                        continue;

                    if (cItem is CustomGrenade customGrenade)
                        loadedCustomGrenade = customGrenade;
                }

                ev.Player.DisableEffect(EffectType.Invisible);
                ev.Player.Connection.Send(new RequestMessage(ev.Firearm.Serial, RequestType.Reload));

                Timing.CallDelayed(3f, () => firearm.Ammo = ClipSize);

                loadedGrenade = item.Type == ItemType.GrenadeFlash ? ProjectileType.Flashbang :
                    item.Type == ItemType.GrenadeHE ? ProjectileType.FragGrenade : ProjectileType.Scp018;
                Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} successfully reloaded. Grenade type: {loadedGrenade} IsCustom: {loadedCustomGrenade != null}");
                ev.Player.RemoveItem(item);

                return;
            }

            Log.Debug($"{Name}.{nameof(OnReloading)}: {ev.Player.Nickname} was unable to reload - No grenades in inventory.");
        }
    }

    /// <inheritdoc/>
    protected override void OnShooting(ShootingEventArgs ev)
    {
        ev.IsAllowed = false;
        if (ev.Player.CurrentItem is Firearm firearm)
                firearm.Ammo -= 1;

        Vector3 pos = ev.Player.CameraTransform.TransformPoint(new Vector3(0.0715f, 0.0225f, 0.45f));
        Projectile projectile;

        switch (loadedGrenade)
            {
                case ProjectileType.Scp018:
                    projectile = ev.Player.ThrowGrenade(ProjectileType.Scp018).Projectile;
                    break;
                case ProjectileType.Flashbang:
                    projectile = ev.Player.ThrowGrenade(ProjectileType.Flashbang).Projectile;
                    break;
                default:
                    projectile = ev.Player.ThrowGrenade(ProjectileType.FragGrenade).Projectile;
                    break;
            }

        projectile.GameObject.AddComponent<CollisionHandler>().Init(ev.Player.GameObject, projectile.Base);

        if (Loader.Random.Next(100) <= Chance)
        {
            try
            {
                foreach (Item item in ev.Player.Items)
                {
                    if (Check(item))
                    {
                        Log.Debug("found GL in inventory, doing funni");
                        Timing.CallDelayed(1f, () =>
                            {
                                ev.Player.ExplodeEffect(ProjectileType.FragGrenade);
                                ev.Player.AddItem(ItemType.GunE11SR);
                                ev.Player.AddItem(ItemType.Ammo556x45);
                                ev.Player.RemoveItem(item);
                            });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
