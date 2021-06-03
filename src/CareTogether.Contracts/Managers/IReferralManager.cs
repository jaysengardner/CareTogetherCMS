﻿using System;

namespace CareTogether.Managers
{
    public record Referral(Guid Id);
    public enum ReferralCloseReason { NotAppropriate }; //TODO: This could be policy-driven eventually?

    public record Arrangement(Guid Id);
    public enum ArrangementType { Hosting, Friending }; //TODO: This could be policy-driven eventually?

    public record ReferralInfo();


    //TODO: Workflow states can be reviewed to return **potential/allowed next steps/events**, to help drive UI behavior.
    public interface IReferralManager
    {
        //#region Referrals and Arrangements Workflows

        //internal IEnumerable<Referral> QueryReferrals()
        //{
        //    throw new NotImplementedException();
        //}

        //internal Guid CreateReferral(Guid partneringFamilyId, ReferralInfo referralInfo)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void UpdateReferral(Guid referralId, ReferralInfo referralInfo)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void CloseReferral(Guid referralId, ReferralCloseReason referralCloseReason)
        //{
        //    throw new NotImplementedException();
        //}

        //internal IEnumerable<Arrangement> QueryArrangements()
        //{
        //    throw new NotImplementedException();
        //}

        //internal Guid CreateArrangement(Guid referralId, ArrangementType arrangementType)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void RecordArrangementSetupStep(Guid referralId, Guid arrangementId)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void RecordArrangementMonitoringStep(Guid referralId, Guid arrangementId)
        //{
        //    throw new NotImplementedException();
        //}

        //internal void RecordArrangementCloseoutStep()
        //{
        //    throw new NotImplementedException();
        //}

        //#endregion

        //#region Arrangement Notes

        //internal IEnumerable<Note> GetArrangementNotes(Guid arrangementId)
        //{
        //    var workflow = workflowsResourceAccess.GetWorkflowState(arrangementId);
        //    workflow.ValidateUserAccess();
        //    return notesResourceAccess.GetNotes(arrangementId);
        //}

        //internal Guid RecordArrangementNote(Guid arrangementId, NoteContents contents)
        //{
        //    var workflow = workflowsResourceAccess.GetWorkflowState(arrangementId);
        //    workflow.ValidateUserAccess();
        //    workflow.ValidateAction();
        //    var noteId = notesResourceAccess.CreateDraftNote(arrangementId, contents);
        //    return noteId;
        //}

        //internal void EditArrangementNote(Guid arrangementId, Guid noteId, NoteContents updatedContents)
        //{
        //    var workflow = workflowsResourceAccess.GetWorkflowState(arrangementId);
        //    workflow.ValidateUserAccess();
        //    var note = notesResourceAccess.GetNote(arrangementId, noteId);
        //    workflow.ValidateAction(note);
        //    notesResourceAccess.EditDraftNote(arrangementId, noteId, updatedContents);
        //}

        //internal void ApproveArrangementNote(Guid arrangementId, Guid noteId)
        //{
        //    var workflow = workflowsResourceAccess.GetWorkflowState(arrangementId);
        //    workflow.ValidateUserAccess();
        //    var note = notesResourceAccess.GetNote(arrangementId, noteId);
        //    workflow.ValidateAction(note);
        //    notesResourceAccess.ApproveDraftNote(arrangementId, noteId);
        //    notificationsUtility.NotifyUser(new Notifications.NoteApproved(arrangementId, noteId));
        //}

        //internal void RejectArrangementNote(Guid arrangementId, Guid noteId, DraftNoteDenialReason reason)
        //{
        //    var workflow = workflowsResourceAccess.GetWorkflowState(arrangementId);
        //    workflow.ValidateUserAccess();
        //    var note = notesResourceAccess.GetNote(arrangementId, noteId);
        //    workflow.ValidateAction(note);
        //    notesResourceAccess.DenyDraftNote(arrangementId, noteId, reason);
        //    notificationsUtility.NotifyUser(new Notifications.NoteRejected(arrangementId, noteId));
        //}

        //#endregion
    }
}
