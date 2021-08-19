import { useState } from 'react';
import { makeStyles } from '@material-ui/core/styles';
import { Button, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, FormControl, FormControlLabel, FormGroup, FormLabel, Grid, InputAdornment, InputLabel, MenuItem, Radio, RadioGroup, Select, TextField } from '@material-ui/core';
import { Age, ExactAge, FamilyAdultRelationshipType, AgeInYears } from '../GeneratedClient';
import { useVolunteerFamiliesModel } from '../Model/VolunteerFamiliesModel';
import WarningIcon from '@material-ui/icons/Warning';
import { DatePicker } from '@material-ui/pickers';

const useStyles = makeStyles((theme) => ({
  form: {
    '& .MuiFormControl-root': {
    }
  },
  ageYears: {
    width: '20ch'
  }
}));

interface CreateVolunteerFamilyDialogProps {
  open: boolean,
  onClose: () => void
}

export function CreateVolunteerFamilyDialog({open, onClose}: CreateVolunteerFamilyDialogProps) {
  const classes = useStyles();
  const [fields, setFields] = useState({
    firstName: '',
    lastName: '',
    dateOfBirth: new Date(),
    ageInYears: undefined as number | undefined,
    isInHousehold: true,
    isPrimaryFamilyContact: true,
    relationshipToFamily: '' as FamilyAdultRelationshipType | '',
    familyRelationshipNotes: undefined as string | undefined,
    safetyRiskNotes: undefined as string | undefined
  });
  const {
    firstName, lastName, dateOfBirth, ageInYears,
    isInHousehold, isPrimaryFamilyContact, relationshipToFamily,
    familyRelationshipNotes, safetyRiskNotes } = fields;
  const [ageType, setAgeType] = useState<'exact' | 'inYears'>('exact');
  const volunteerFamiliesModel = useVolunteerFamiliesModel();

  const relationshipTypes = Object.keys(FamilyAdultRelationshipType).filter(k => {
    const entry: any = FamilyAdultRelationshipType[k as any];
    return typeof entry === "number"
    || entry === k
    || FamilyAdultRelationshipType[entry]?.toString() !== k
  });

  async function addAdult() {
    if (firstName.length <= 0 || lastName.length <= 0) {
      alert("First and last name are required. Try again.");
    } else if (relationshipToFamily === '') { //TODO: Actual validation!
      alert("Family relationship was not selected. Try again.");
    } else {
      let age: Age;
      if (ageType === 'exact') {
        age = new ExactAge();
        (age as ExactAge).dateOfBirth = dateOfBirth;
      } else {
        age = new AgeInYears();
        (age as AgeInYears).years = ageInYears;
        (age as AgeInYears).asOf = new Date();
      }
      /*const newFamily =*/ await volunteerFamiliesModel.createVolunteerFamilyWithNewAdult(
        firstName, lastName, age,
        isInHousehold, isPrimaryFamilyContact, relationshipToFamily as FamilyAdultRelationshipType,
        familyRelationshipNotes, safetyRiskNotes);
      //TODO: Error handling (start with a basic error dialog w/ request to share a screenshot, and App Insights logging)
      //TODO: Retrieve the created volunteer family and return it through this onClose callback!
      onClose();
      // Since this dialog can be kept around, reset the state so the user can't accidentally submit previous values again.
      setFields({
        firstName: '',
        lastName: '',
        dateOfBirth: new Date(),
        ageInYears: undefined as number | undefined,
        isInHousehold: true,
        isPrimaryFamilyContact: true,
        relationshipToFamily: '' as FamilyAdultRelationshipType | '',
        familyRelationshipNotes: undefined as string | undefined,
        safetyRiskNotes: undefined as string | undefined
      });
    }
  }

  return (
    <Dialog open={open} onClose={onClose} aria-labelledby="create-family-title">
      <DialogTitle id="create-family-title">
        Create Volunteer Family - First Adult
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          Provide the basic information needed for the first adult in the family.
        </DialogContentText>
        <form className={classes.form} noValidate autoComplete="off">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField required id="first-name" label="First Name" fullWidth size="small"
                value={firstName} onChange={e => setFields({...fields, firstName: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField required id="last-name" label="Last Name" fullWidth size="small"
                value={lastName} onChange={e => setFields({...fields, lastName: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <FormControl component="fieldset">
                <FormLabel component="legend">Specify age as:</FormLabel>
                <RadioGroup aria-label="ageType" name="ageType"
                  value={ageType} onChange={e => setAgeType(e.target.value as 'exact' | 'inYears')}>
                  <FormControlLabel value="exact" control={<Radio size="small" />} label="Date of birth:" />
                  <FormControlLabel value="inYears" control={<Radio size="small" />} label="Years old today:" />
                </RadioGroup>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={8} container direction="column" spacing={0}>
              <Grid item>
                <DatePicker
                  label="Date of birth" size="small"
                  value={dateOfBirth}
                  required disableFuture disabled={ageType !== 'exact'} format="yyyy/MM/dd"
                  onChange={(date) => date && setFields({...fields, dateOfBirth: date})}
                  />
              </Grid>
              <Grid item>
                <TextField
                  id="age-years" label="Age" className={classes.ageYears} size="small"
                  required type="number" disabled={ageType !== 'inYears'}
                  value={ageInYears} onChange={e => setFields({...fields, ageInYears: Number.parseInt(e.target.value)})}
                  InputProps={{
                    endAdornment: <InputAdornment position="end">years</InputAdornment>,
                  }} />
                </Grid>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth size="small">
                <InputLabel id="family-relationship-label">Relationship to Family</InputLabel>
                <Select
                  labelId="family-relationship-label" id="family-relationship"
                  value={relationshipToFamily}
                  onChange={e => setFields({...fields, relationshipToFamily: e.target.value as FamilyAdultRelationshipType})}>
                    <MenuItem key="placeholder" value="" disabled>
                      Select a relationship type
                    </MenuItem>
                    {relationshipTypes.map(relationshipType =>
                      <MenuItem key={relationshipType} value={FamilyAdultRelationshipType[relationshipType as any]}>{relationshipType}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormGroup row>
                <FormControlLabel
                  control={<Checkbox checked={isInHousehold} onChange={e => setFields({...fields, isInHousehold: e.target.checked})}
                    name="isInHousehold" color="primary" size="small" />}
                  label="In Household"
                />
                <FormControlLabel
                  control={<Checkbox checked={isPrimaryFamilyContact} onChange={e => setFields({...fields, isPrimaryFamilyContact: e.target.checked})}
                    name="isPrimaryFamilyContact" color="primary" size="small" />}
                  label="Primary Family Contact"
                />
              </FormGroup>
            </Grid>
            <Grid item xs={12}>
              <TextField
                id="family-relationship-notes"
                label="Family Relationship Notes"
                multiline fullWidth variant="outlined" rows={2} rowsMax={5} size="small"
                value={familyRelationshipNotes} onChange={e => setFields({...fields, familyRelationshipNotes: e.target.value})}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                id="safety-risk-notes"
                label="Safety Risk Notes"
                multiline fullWidth variant="outlined" rows={2} rowsMax={5} size="small"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <WarningIcon />
                    </InputAdornment>
                  ),
                }}
                value={safetyRiskNotes} onChange={e => setFields({...fields, safetyRiskNotes: e.target.value})}
              />
            </Grid>
          </Grid>
        </form>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} color="secondary">
          Cancel
        </Button>
        <Button onClick={addAdult} variant="contained" color="primary">
          Create Family
        </Button>
      </DialogActions>
    </Dialog>
  );
}
