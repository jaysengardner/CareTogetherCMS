﻿using JsonPolymorph;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace CareTogether.Resources
{
    public record ReferralEntry(Guid Id, string PolicyVersion, DateTime TimestampUtc,
        ReferralCloseReason? CloseReason,
        Guid PartneringFamilyId,
        ImmutableList<FormUploadInfo> ReferralFormUploads,
        ImmutableList<ActivityInfo> ReferralActivitiesPerformed,
        ImmutableDictionary<Guid, ArrangementEntry> Arrangements);

    public record ArrangementEntry(Guid Id, string PolicyVersion, string ArrangementType,
        ArrangementState State,
        ImmutableList<FormUploadInfo> ArrangementFormUploads,
        ImmutableList<ActivityInfo> ArrangementActivitiesPerformed,
        ImmutableList<VolunteerAssignment> VolunteerAssignments,
        ImmutableList<PartneringFamilyChildAssignment> PartneringFamilyChildAssignments,
        ImmutableList<ChildrenLocationHistoryEntry> ChildrenLocationHistory,
        ImmutableDictionary<Guid, NoteEntry> Notes);

    public record NoteEntry(Guid Id, Guid AuthorId, DateTime LastEditTimestampUtc, NoteStatus Status,
        string FinalizedNoteContents, Guid? ApproverId, DateTime? ApprovedTimestampUtc);

    public enum ReferralCloseReason { NotAppropriate, Resourced, NoCapacity, NoLongerNeeded, NeedMet };

    public enum ArrangementState { Setup, Open, Closed };

    public sealed record FormUploadInfo(Guid UserId, DateTime TimestampUtc,
        string FormName, string FormVersion, string UploadedFileName);
    public sealed record ActivityInfo(Guid UserId, DateTime TimestampUtc,
        string ActivityName);

    [JsonHierarchyBase]
    public abstract partial record VolunteerAssignment(string ArrangementFunction);
    public sealed record IndividualVolunteerAssignment(Guid PersonId, string ArrangementFunction)
        : VolunteerAssignment(ArrangementFunction);
    public sealed record FamilyVolunteerAssignment(Guid FamilyId, string ArrangementFunction)
        : VolunteerAssignment(ArrangementFunction);

    public sealed record PartneringFamilyChildAssignment(Guid PersonId);
    public sealed record ChildrenLocationHistoryEntry(Guid UserId, DateTime TimestampUtc,
        ImmutableList<Guid> ChildrenIds, Guid FamilyId, ChildrenLocationPlan Plan, string AdditionalExplanation);

    public enum ChildrenLocationPlan { OvernightHousing, DaytimeChildCare, ReturnToFamily }

    public enum NoteStatus { Draft, Approved };

    [JsonHierarchyBase]
    public abstract partial record ReferralCommand(Guid ReferralId);
    public sealed record CreateReferral(Guid ReferralId, Guid FamilyId, string PolicyVersion)
        : ReferralCommand(ReferralId);
    public sealed record PerformReferralActivity(Guid ReferralId, string ActivityName)
        : ReferralCommand(ReferralId);
    public sealed record UploadReferralForm(Guid ReferralId,
        string FormName, string FormVersion, string UploadedFileName)
        : ReferralCommand(ReferralId);
    public sealed record CloseReferral(Guid ReferralId, ReferralCloseReason CloseReason)
        : ReferralCommand(ReferralId);

    [JsonHierarchyBase]
    public abstract partial record ArrangementCommand(Guid ReferralId, Guid ArrangementId);
    public sealed record CreateArrangement(Guid ReferralId, Guid ArrangementId,
        string PolicyVersion, string ArrangementType)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record AssignIndividualVolunteer(Guid ReferralId, Guid ArrangementId,
        Guid PersonId, string ArrangementFunction)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record AssignVolunteerFamily(Guid ReferralId, Guid ArrangementId,
        Guid FamilyId, string ArrangementFunction)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record AssignPartneringFamilyChildren(Guid ReferralId, Guid ArrangementId,
        ImmutableList<Guid> ChildrenIds)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record InitiateArrangement(Guid ReferralId, Guid ArrangementId)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record UploadArrangementForm(Guid ReferralId, Guid ArrangementId,
        string FormName, string FormVersion, string UploadedFileName)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record PerformArrangementActivity(Guid ReferralId, Guid ArrangementId,
        Guid CompletedByPersonId, string ActivityName)
        : ArrangementCommand(ReferralId, ArrangementId);
    public sealed record TrackChildrenLocationChange(Guid ReferralId, Guid ArrangementId, DateTime ChangedAtUtc,
        ImmutableList<Guid> ChildrenIds, Guid FamilyId, ChildrenLocationPlan Plan, string AdditionalExplanation)
        : ArrangementCommand(ReferralId, ArrangementId);
    //TODO: EndArrangement?

    [JsonHierarchyBase]
    public abstract partial record ArrangementNoteCommand(Guid ReferralId, Guid ArrangementId, Guid NoteId);
    public sealed record CreateDraftArrangementNote(Guid ReferralId, Guid ArrangementId, Guid NoteId)
        : ArrangementNoteCommand(ReferralId, ArrangementId, NoteId);
    public sealed record EditDraftArrangementNote(Guid ReferralId, Guid ArrangementId, Guid NoteId)
        : ArrangementNoteCommand(ReferralId, ArrangementId, NoteId);
    public sealed record DiscardDraftArrangementNote(Guid ReferralId, Guid ArrangementId, Guid NoteId)
        : ArrangementNoteCommand(ReferralId, ArrangementId, NoteId);
    public sealed record ApproveArrangementNote(Guid ReferralId, Guid ArrangementId, Guid NoteId,
        string FinalizedNoteContents)
        : ArrangementNoteCommand(ReferralId, ArrangementId, NoteId);

    /// <summary>
    /// The <see cref="IReferralsResource"/> models the lifecycle of people's referrals to CareTogether organizations,
    /// including various forms, arrangements, and policy changes, as well as authorizing related queries.
    /// </summary>
    public interface IReferralsResource
    {
        Task<ImmutableList<ReferralEntry>> ListReferralsAsync(Guid organizationId, Guid locationId);

        Task<ResourceResult<ReferralEntry>> GetReferralAsync(Guid organizationId, Guid locationId, Guid referralId);

        Task<ResourceResult<ReferralEntry>> ExecuteReferralCommandAsync(Guid organizationId, Guid locationId,
            ReferralCommand command, Guid userId);

        Task<ResourceResult<ReferralEntry>> ExecuteArrangementCommandAsync(Guid organizationId, Guid locationId,
            ArrangementCommand command, Guid userId);

        Task<ResourceResult<ReferralEntry>> ExecuteArrangementNoteCommandAsync(Guid organizationId, Guid locationId,
            ArrangementNoteCommand command, Guid userId);
    }
}
