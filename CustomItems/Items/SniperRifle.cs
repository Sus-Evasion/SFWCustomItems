// -----------------------------------------------------------------------
// <copyright file="SniperRifle.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Exiled.API.Enums;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;
using PlayerRoles;
using PlayerStatsSystem;
using YamlDotNet.Serialization;

namespace CustomItems.Items;

/// <inheritdoc />
[CustomItem(ItemType.GunE11SR)]
public class SniperRifle : CustomWeapon
{
    /// <inheritdoc/>
    public override uint Id { get; set; } = 8;

    /// <inheritdoc/>
    public override string Name { get; set; } = "SR-119";

    /// <inheritdoc/>
    public override string Description { get; set; } =
        "This modified E-11 Rifle fires high-velocity anti-personnel sniper rounds.";

    /// <inheritdoc/>
    public override float Weight { get; set; } = 3.25f;

    /// <inheritdoc/>
    public override byte ClipSize { get; set; } = 3;

    /// <inheritdoc/>
    public override bool ShouldMessageOnGban { get; } = true;

    public byte ReloadCost { get; set; } = 10;

    /// <inheritdoc/>
    public override SpawnProperties? SpawnProperties { get; set; } = new()
    {
        Limit = 2,
        DynamicSpawnPoints = new List<DynamicSpawnPoint>
        {
            new()
            {
                Chance = 25,
                Location = SpawnLocationType.InsideHid,
            },
            new()
            {
                Chance = 25,
                Location = SpawnLocationType.InsideHczArmory,
            },
        },
    };

    /// <inheritdoc />
    [YamlIgnore]
    public override AttachmentName[] Attachments { get; set; } = {
        AttachmentName.ExtendedBarrel,
        AttachmentName.ScopeSight,
        AttachmentName.Flashlight,
        AttachmentName.LowcapMagAP,
    };

    /// <summary>
    /// Gets or sets the amount of damage this weapon does.
    /// </summary>
    [Description("The amount of damage this weapon does, damage will still be reduced by armor")]
    public override float Damage { get; set; } = 90f;

    [Description("The amount of extra damage this weapon does to SCPs, as a multiplier.")]
    public float DamageMultiplierScp { get; set; } = 3f;

    /// <inheritdoc/>
    protected override void OnHurting(HurtingEventArgs ev)
    {
        if (ev.Attacker != ev.Player && ev.DamageHandler.Base is FirearmDamageHandler firearmDamageHandler &&
            firearmDamageHandler.WeaponType == ev.Attacker.CurrentItem.Type)
            if (ev.Player.IsScp)
            {
                float damageTotal = Damage * DamageMultiplierScp;
                ev.Amount = damageTotal;
            }
            else if (!ev.Player.IsScp && ev.Player.Role.Type == RoleTypeId.Tutorial)
            {
                ev.Amount = -1f;
            }
            else
            {
                ev.Amount = Damage;
            }
    }

    /// <inheritdoc/>
    protected override void OnReloading(ReloadingWeaponEventArgs ev)
    {
        ev.IsAllowed = false;
        int ammoToReload = Math.Min(ev.Player.Ammo[ItemType.Ammo556x45] / ReloadCost, 3 - ev.Firearm.Ammo);
        if (ammoToReload > 0)
        {
            ev.Player.Ammo[ItemType.Ammo556x45] -= (ushort)(ammoToReload * ReloadCost);
            ev.Firearm.Ammo += (byte)ammoToReload;
            ev.Player.Connection.Send(new RequestMessage(ev.Firearm.Serial, RequestType.Reload));
            ev.Player.SetAmmo(AmmoType.Nato556, ev.Player.Ammo[ItemType.Ammo556x45]);
        }
    }
}
