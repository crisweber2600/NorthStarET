using System;
using System.Collections.Generic;
using System.Linq;
using NorthStarET.NextGen.Lms.Contracts.DistrictAdmins;
using NorthStarET.NextGen.Lms.Contracts.Districts;

namespace NorthStarET.NextGen.Lms.Web.Testing;

public interface IPlaywrightStubStore
{
    IReadOnlyList<DistrictResponse> GetDistricts();
    DistrictDetailResponse? GetDistrictDetail(Guid districtId);
    DistrictResponse? GetDistrict(Guid districtId);
    DistrictResponse CreateDistrict(string name, string suffix);
    bool UpdateDistrict(Guid districtId, string name);
    bool DeleteDistrict(Guid districtId);
    IReadOnlyList<DistrictAdminResponse> GetAdmins(Guid districtId);
    InviteDistrictAdminResponse InviteAdmin(Guid districtId, string firstName, string lastName, string email);
    bool ResendAdminInvitation(Guid districtId, Guid adminId);
    bool RemoveAdmin(Guid districtId, Guid adminId);
}

internal sealed class PlaywrightStubStore : IPlaywrightStubStore
{
    private const string StatusVerified = "Verified";
    private const string StatusUnverified = "Unverified";
    private const string StatusRevoked = "Revoked";

    private readonly object sync = new();
    private readonly Dictionary<Guid, DistrictStubEntry> districts = new();

    public PlaywrightStubStore()
    {
        // Seed demo district to mirror existing Playwright expectations
        var demoDistrict = new DistrictStubEntry(
            TestEnvironment.DefaultDistrictId,
            "Demo District",
            "demo.edu",
            DateTime.UtcNow);

        var unverifiedAdmin = demoDistrict.AddAdmin("Dana", "Unverified", "dana.unverified@demo.edu");

        var verifiedAdmin = demoDistrict.AddAdmin("Victor", "Verified", "victor.verified@demo.edu");
        verifiedAdmin.Status = StatusVerified;
        verifiedAdmin.VerifiedAtUtc = DateTime.UtcNow;

        lock (sync)
        {
            if (!districts.ContainsKey(demoDistrict.Id))
            {
                districts.Add(demoDistrict.Id, demoDistrict);
            }
        }
    }

    public IReadOnlyList<DistrictResponse> GetDistricts()
    {
        lock (sync)
        {
            return districts.Values
                .OrderBy(d => d.Name)
                .Select(d => d.ToDistrictResponse())
                .ToList();
        }
    }

    public DistrictDetailResponse? GetDistrictDetail(Guid districtId)
    {
        lock (sync)
        {
            return districts.TryGetValue(districtId, out var entry)
                ? entry.ToDistrictDetailResponse()
                : null;
        }
    }

    public DistrictResponse? GetDistrict(Guid districtId)
    {
        lock (sync)
        {
            return districts.TryGetValue(districtId, out var entry)
                ? entry.ToDistrictResponse()
                : null;
        }
    }

    public DistrictResponse CreateDistrict(string name, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(suffix);

        var trimmedName = name.Trim();
        var trimmedSuffix = suffix.Trim().ToLowerInvariant();

        lock (sync)
        {
            if (districts.Values.Any(d => string.Equals(d.Suffix, trimmedSuffix, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Suffix '{trimmedSuffix}' is already in use.");
            }

            var entry = new DistrictStubEntry(Guid.NewGuid(), trimmedName, trimmedSuffix, DateTime.UtcNow);
            districts.Add(entry.Id, entry);
            return entry.ToDistrictResponse();
        }
    }

    public bool UpdateDistrict(Guid districtId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmedName = name.Trim();

        lock (sync)
        {
            if (!districts.TryGetValue(districtId, out var entry))
            {
                return false;
            }

            entry.Name = trimmedName;
            entry.UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }
    }

    public bool DeleteDistrict(Guid districtId)
    {
        lock (sync)
        {
            return districts.Remove(districtId);
        }
    }

    public IReadOnlyList<DistrictAdminResponse> GetAdmins(Guid districtId)
    {
        lock (sync)
        {
            if (!districts.TryGetValue(districtId, out var entry))
            {
                return Array.Empty<DistrictAdminResponse>();
            }

            return entry.Admins
                .OrderBy(admin => admin.InvitedAtUtc)
                .Select(admin => admin.ToResponse(entry.Id))
                .ToList();
        }
    }

    public InviteDistrictAdminResponse InviteAdmin(Guid districtId, string firstName, string lastName, string email)
    {
        lock (sync)
        {
            if (!districts.TryGetValue(districtId, out var entry))
            {
                throw new InvalidOperationException("District not found.");
            }

            if (!email.EndsWith($"@{entry.Suffix}", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Email domain must match district suffix");
            }

            var admin = entry.AddAdmin(firstName, lastName, email);
            return admin.ToInviteResponse(entry.Id);
        }
    }

    public bool ResendAdminInvitation(Guid districtId, Guid adminId)
    {
        lock (sync)
        {
            if (!districts.TryGetValue(districtId, out var entry))
            {
                return false;
            }

            return entry.ResendInvitation(adminId);
        }
    }

    public bool RemoveAdmin(Guid districtId, Guid adminId)
    {
        lock (sync)
        {
            if (!districts.TryGetValue(districtId, out var entry))
            {
                return false;
            }

            return entry.RemoveAdmin(adminId);
        }
    }

    private sealed class DistrictStubEntry
    {
        private readonly List<DistrictAdminStubEntry> admins = new();

        internal DistrictStubEntry(Guid id, string name, string suffix, DateTime createdAtUtc)
        {
            Id = id;
            Name = name;
            Suffix = suffix;
            CreatedAtUtc = createdAtUtc;
        }

        internal Guid Id { get; }
        internal string Name { get; set; }
        internal string Suffix { get; }
        internal DateTime CreatedAtUtc { get; }
        internal DateTime? UpdatedAtUtc { get; set; }

        internal IReadOnlyList<DistrictAdminStubEntry> Admins => admins;

        internal DistrictAdminStubEntry AddAdmin(string firstName, string lastName, string email)
        {
            var now = DateTime.UtcNow;
            var stub = new DistrictAdminStubEntry
            {
                Id = Guid.NewGuid(),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                Status = StatusUnverified,
                InvitedAtUtc = now,
                InvitationExpiresAtUtc = now.AddDays(14)
            };

            admins.Add(stub);
            return stub;
        }

        internal bool ResendInvitation(Guid adminId)
        {
            var admin = admins.FirstOrDefault(a => a.Id == adminId);
            if (admin is null)
            {
                return false;
            }

            admin.InvitedAtUtc = DateTime.UtcNow;
            admin.InvitationExpiresAtUtc = admin.InvitedAtUtc.AddDays(14);
            return true;
        }

        internal bool RemoveAdmin(Guid adminId)
        {
            var index = admins.FindIndex(a => a.Id == adminId);
            if (index < 0)
            {
                return false;
            }

            admins.RemoveAt(index);
            return true;
        }

        internal DistrictResponse ToDistrictResponse()
        {
            return new DistrictResponse
            {
                Id = Id,
                Name = Name,
                Suffix = Suffix,
                CreatedAtUtc = CreatedAtUtc,
                UpdatedAtUtc = UpdatedAtUtc,
                ActiveAdminCount = admins.Count(a => string.Equals(a.Status, StatusVerified, StringComparison.OrdinalIgnoreCase)),
                PendingAdminCount = admins.Count(a => string.Equals(a.Status, StatusUnverified, StringComparison.OrdinalIgnoreCase)),
                RevokedAdminCount = admins.Count(a => string.Equals(a.Status, StatusRevoked, StringComparison.OrdinalIgnoreCase))
            };
        }

        internal DistrictDetailResponse ToDistrictDetailResponse()
        {
            var response = ToDistrictResponse();
            return new DistrictDetailResponse
            {
                Id = response.Id,
                Name = response.Name,
                Suffix = response.Suffix,
                CreatedAtUtc = response.CreatedAtUtc,
                UpdatedAtUtc = response.UpdatedAtUtc,
                ActiveAdminCount = response.ActiveAdminCount,
                PendingAdminCount = response.PendingAdminCount,
                RevokedAdminCount = response.RevokedAdminCount
            };
        }
    }

    private sealed class DistrictAdminStubEntry
    {
        internal Guid Id { get; set; }
        internal string FirstName { get; set; } = string.Empty;
        internal string LastName { get; set; } = string.Empty;
        internal string Email { get; set; } = string.Empty;
        internal string Status { get; set; } = string.Empty;
        internal DateTime InvitedAtUtc { get; set; }
        internal DateTime? VerifiedAtUtc { get; set; }
        internal DateTime? RevokedAtUtc { get; set; }
        internal DateTime InvitationExpiresAtUtc { get; set; }

        internal InviteDistrictAdminResponse ToInviteResponse(Guid districtId)
        {
            return new InviteDistrictAdminResponse
            {
                Id = Id,
                DistrictId = districtId,
                Email = Email,
                Status = Status,
                InvitedAtUtc = InvitedAtUtc,
                InvitationExpiresAtUtc = InvitationExpiresAtUtc
            };
        }

        internal DistrictAdminResponse ToResponse(Guid districtId)
        {
            var now = DateTime.UtcNow;
            return new DistrictAdminResponse
            {
                Id = Id,
                DistrictId = districtId,
                Email = Email,
                Status = Status,
                InvitedAtUtc = InvitedAtUtc,
                VerifiedAtUtc = VerifiedAtUtc,
                RevokedAtUtc = RevokedAtUtc,
                InvitationExpiresAtUtc = InvitationExpiresAtUtc,
                IsInvitationExpired = InvitationExpiresAtUtc < now
            };
        }
    }
}
