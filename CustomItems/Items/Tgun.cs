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
using System.Net;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = System.Random;

namespace CustomItems.Items;

/// <summary>
/// A gun that kills you.
/// </summary>
[CustomItem(ItemType.GunCOM15)]
public class Tgun : CustomWeapon
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 17;

    /// <inheritdoc/>
    public override string Name { get; set; } = "Tgun";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "When this weapon is fired, it teleports yourself randomly across the facility. Do beware that it is fragile and will be destroyed after single use.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 3.95f;

    /// <inheritdoc/>
    [YamlIgnore]
    public override byte ClipSize { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether the gun should despawn instead of drop when it is fired.
    /// </summary>
    [Description("Whether or not the weapon should despawn itself after it's been used.")]
    public bool DespawnAfterUse { get; set; } = true;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new();

    public override float Damage { get; set; } = 0;

    /// <inheritdoc/>
    [Description("The amount of damage the weapon deals to user when weapon is used.")]
    public float DamagePerTp { get; set; } = 50;

    /// <inheritdoc/>
    [Description("The amount of ammo the weapon takes to teleport user when weapon is used.")]
    public ushort AmmoCost { get; set; } = 50;

    [Description(
        "The zone to which the player will be teleported to. If this is anything but Unspecified it will teleport the player to a random room within that zone")]
    public ZoneType Zone { get; set; } = ZoneType.Unspecified;

    [Description(
        "Ignored if zone is anything other than Unspecified. Room that the player will teleport too. Set this to Unknown along with Zone Unspecified to teleport to a random place across the entire facility")]
    public RoomType Room { get; set; } = RoomType.Unknown;

    public Vector3? GetTeleportLocation()
    {
        if (Zone == ZoneType.Unspecified && Room == RoomType.Unknown)
        {
            List<Door> doors = Door.List.Where(door => door.Rooms.Count > 1).ToList();
            Door door = doors[new Random().Next(doors.Count)];

            if (Map.IsLczDecontaminated)
            {
                do
                {
                    door = doors[new Random().Next(doors.Count)];
                }
                while (door.Zone is ZoneType.LightContainment);
            }

            return door.Position + Vector3.up + door.Transform.forward;
        }

        if (Zone == ZoneType.Unspecified)
        {
            List<Door> doors = Door.List.Where(door => door.Room.Type == Room).ToList();
            Door door = doors[new Random().Next(doors.Count)];

            if (Map.IsLczDecontaminated)
            {
                do
                {
                    door = doors[new Random().Next(doors.Count)];
                }
                while (door.Zone is ZoneType.LightContainment);
            }

            return door.Position + Vector3.up + door.Transform.forward;
        }

        if (Zone != ZoneType.Unspecified)
        {
            List<Door> doors = Door.List.Where(door => door.Zone == Zone).ToList();
            Door door = doors[new Random().Next(doors.Count)];
            if (Map.IsLczDecontaminated)
            {
                do
                {
                    door = doors[new Random().Next(doors.Count)];
                }
                while (door.Zone is ZoneType.LightContainment);
            }

            return door.Position + Vector3.up + door.Transform.forward;
        }

        return null;
    }

    public void TryTeleport(Player player)
    {
        try
        {
            player.Teleport(GetTeleportLocation());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void OnShooting(ShootingEventArgs ev)
    {
        if (ev.Player.Health > DamagePerTp)
        {
            ev.Player.Health -= DamagePerTp;
            TryTeleport(ev.Player);
            if (DespawnAfterUse)
                ev.Player.RemoveItem(ev.Item);
        }
        else
        {
            ev.Player.ShowHint("You do not have enough health required for teleportation");
            ev.Firearm.Ammo += 1;
        }
    }

    /// <inheritdoc/>
    protected override void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Attacker != ev.Player)
        {
            ev.Amount = 0;
        }
    }

    /// <inheritdoc/>
    protected override void OnReloading(ReloadingWeaponEventArgs ev)
    {
        ev.IsAllowed = false;
        if (ev.Player.Ammo[ItemType.Ammo9x19] >= AmmoCost)
        {
            ev.Player.Ammo[ItemType.Ammo9x19] -= AmmoCost;
            ev.Player.Connection.Send(new RequestMessage(ev.Firearm.Serial, RequestType.Reload));
            ev.Firearm.Ammo += 1;
        }
        else
        {
            ev.Player.ShowHint("You do not have enough ammo to reload teleportation gun [Required: 50 9MM]");
        }
    }
}

