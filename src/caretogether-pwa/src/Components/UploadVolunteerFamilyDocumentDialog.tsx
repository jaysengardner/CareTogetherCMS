import { useState } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from '@material-ui/core';
import { VolunteerFamily } from '../GeneratedClient';
import { useVolunteerFamiliesModel } from '../Model/VolunteerFamiliesModel';

const useStyles = makeStyles((theme) => ({
  fileInput: {
  }
}));

interface UploadVolunteerFamilyDocumentDialogProps {
  volunteerFamily: VolunteerFamily,
  onClose: () => void
}

export function UploadVolunteerFamilyDocumentDialog({volunteerFamily, onClose}: UploadVolunteerFamilyDocumentDialogProps) {
  const classes = useStyles();
  const [documentFile, setDocumentFile] = useState<File>();
  const volunteerFamiliesModel = useVolunteerFamiliesModel();

  async function uploadDocument() {
    if (!documentFile) {
      alert("No file was selected. Try again.");
    } else {
      await volunteerFamiliesModel.uploadDocument(volunteerFamily.family?.id as string, documentFile);
      //TODO: Error handling (start with a basic error dialog w/ request to share a screenshot, and App Insights logging)
      onClose();
    }
  }

  return (
    <Dialog open={true} onClose={onClose} aria-labelledby="upload-family-document-title">
      <DialogTitle id="upload-family-document-title">Upload Volunteer Family Document</DialogTitle>
      <DialogContent>
        <DialogContentText>Do you want to upload a new document for this family?</DialogContentText>
        <input
          accept="*/*"
          className={classes.fileInput}
          multiple={false}
          id="family-document-file"
          type="file"
          onChange={async (e) => {if (e.target.files && e.target.files.length > 0) {
            setDocumentFile(e.target.files[0]);
          }}}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="secondary">
          Cancel
        </Button>
        <Button onClick={uploadDocument} variant="contained" color="primary">
          Upload
        </Button>
      </DialogActions>
    </Dialog>
  );
}
