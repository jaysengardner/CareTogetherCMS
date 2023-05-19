import { useState } from 'react';
import { Button, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Divider, FormControl, FormControlLabel, FormGroup, FormLabel, Grid, InputAdornment, InputLabel, MenuItem, Radio, RadioGroup, Select, TextField } from '@mui/material';
import { ExactAge, Gender, PhoneNumberType, EmailAddressType } from '../GeneratedClient';
import { useDirectoryModel } from '../Model/DirectoryModel';
import WarningIcon from '@mui/icons-material/Warning';
import { DatePicker } from '@mui/x-date-pickers';
import { useRecoilValue } from 'recoil';
import { adultFamilyRelationshipsData, ethnicitiesData } from '../Model/ConfigurationModel';
import { useBackdrop } from '../Hooks/useBackdrop';
import { subYears } from 'date-fns';

interface CreateVolunteerFamilyDialogProps {
  onClose: (volunteerFamilyId?: string) => void
}

function optional(arg: string) {
  return arg.length > 0 ? arg : null;
}

export function CreateVolunteerFamilyDialog({onClose}: CreateVolunteerFamilyDialogProps) {
  const [fields, setFields] = useState({
    firstName: '',
    lastName: '',
    gender: null as Gender | null,
    dateOfBirth: null as Date | null,
    ethnicity: '',
    isInHousehold: true,
    relationshipToFamily: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    county: '',
    state: '',
    postalCode: '',
    country: 'United States',
    phoneNumber: '',
    phoneType: PhoneNumberType.Mobile,
    emailAddress: '',
    emailType: EmailAddressType.Personal,
    notes: null as string | null,
    concerns: null as string | null
  });
  const {
    firstName, lastName, gender, dateOfBirth, ethnicity,
    isInHousehold, relationshipToFamily,
    addressLine1, addressLine2, city, county, state, postalCode, country,
    phoneNumber, phoneType, emailAddress, emailType,
    notes, concerns } = fields;
  const directoryModel = useDirectoryModel();

  const relationshipTypes = useRecoilValue(adultFamilyRelationshipsData);
  const ethnicities = useRecoilValue(ethnicitiesData);
  
  const withBackdrop = useBackdrop();

  async function save() {
    await withBackdrop(async () => {
      if (firstName.length <= 0 || lastName.length <= 0) {
        alert("First and last name are required. Try again.");
      } else if (relationshipToFamily === '') { //TODO: Actual validation!
        alert("Family relationship was not selected. Try again.");
      } else {
        let age = dateOfBirth == null ? null : new ExactAge();
        if (dateOfBirth != null)
          age!.dateOfBirth = dateOfBirth;
        const familyId = crypto.randomUUID();
        await directoryModel.createVolunteerFamilyWithNewAdult(familyId,
          firstName, lastName, gender, age, optional(ethnicity),
          isInHousehold, relationshipToFamily,
          optional(addressLine1), optional(addressLine2), optional(city), optional(county), optional(state), optional(postalCode), optional(country),
          optional(phoneNumber), phoneType, optional(emailAddress), emailType,
          (notes == null ? undefined : notes), (concerns == null ? undefined : concerns));
        //TODO: Error handling (start with a basic error dialog w/ request to share a screenshot, and App Insights logging)
        onClose(familyId);
      }
    });
  }

  return (
    <Dialog open={true} onClose={() => onClose()} scroll='body' aria-labelledby="create-family-title">
      <DialogTitle id="create-family-title">
        Create Volunteer Family - First Adult
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          Provide the basic information needed for the first adult in the family.
        </DialogContentText>
        <form noValidate autoComplete="off">
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <TextField required id="first-name" label="First Name" fullWidth size="small"
                value={firstName} onChange={e => setFields({...fields, firstName: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField required id="last-name" label="Last Name" fullWidth size="small"
                value={lastName} onChange={e => setFields({...fields, lastName: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl required fullWidth size="small">
                <InputLabel id="family-relationship-label">Relationship to Family</InputLabel>
                <Select
                  labelId="family-relationship-label" id="family-relationship"
                  value={relationshipToFamily}
                  onChange={e => setFields({...fields, relationshipToFamily: e.target.value as string})}>
                    <MenuItem key="placeholder" value="" disabled>
                      Select a relationship type
                    </MenuItem>
                    {relationshipTypes.map(relationshipType =>
                      <MenuItem key={relationshipType} value={relationshipType}>{relationshipType}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormGroup row>
                <FormControlLabel
                  control={<Checkbox checked={isInHousehold} onChange={e => setFields({...fields, isInHousehold: e.target.checked})}
                    name="isInHousehold" color="primary" size="small" />}
                  label="In Household"
                />
              </FormGroup>
            </Grid>
            <Grid item xs={12}>
              <Divider />
            </Grid>
            <Grid item xs={12}>
              <FormControl component="fieldset">
                <FormLabel component="legend">Gender:</FormLabel>
                <RadioGroup aria-label="genderType" name="genderType" row
                  value={gender == null ? null : Gender[gender]} onChange={e => setFields({...fields, gender: Gender[e.target.value as keyof typeof Gender]})}>
                  <FormControlLabel value={Gender[Gender.Male]} control={<Radio size="small" />} label="Male" />
                  <FormControlLabel value={Gender[Gender.Female]} control={<Radio size="small" />} label="Female" />
                  <FormControlLabel value={Gender[Gender.SeeNotes]} control={<Radio size="small" />} label="See Notes" />
                </RadioGroup>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <DatePicker
                label="Date of birth"
                value={dateOfBirth} maxDate={subYears(new Date(), 18)} openTo="year"
                format="MM/dd/yyyy"
                onChange={(date: any) => date && setFields({...fields, dateOfBirth: date})}
                slotProps={{ textField: { size: "small", fullWidth: true}}} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth size="small">
                <InputLabel id="ethnicity-label">Ethnicity</InputLabel>
                <Select
                  labelId="ethnicity-label" id="ethnicity"
                  value={ethnicity}
                  onChange={e => setFields({...fields, ethnicity: e.target.value as string})}>
                    <MenuItem key="placeholder" value="" disabled>
                      Select an ethnicity
                    </MenuItem>
                    {ethnicities.map(ethnicity =>
                      <MenuItem key={ethnicity} value={ethnicity}>{ethnicity}</MenuItem>)}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField id="phone-number" label="Phone Number" fullWidth size="small" type="tel"
                value={phoneNumber} onChange={e => setFields({...fields, phoneNumber: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl component="fieldset">
                <RadioGroup aria-label="phoneType" name="phoneType" row
                  value={PhoneNumberType[phoneType]} onChange={e => setFields({...fields, phoneType: PhoneNumberType[e.target.value as keyof typeof PhoneNumberType]})}>
                  <FormControlLabel value={PhoneNumberType[PhoneNumberType.Mobile]} control={<Radio size="small" />} label="Mobile" />
                  <FormControlLabel value={PhoneNumberType[PhoneNumberType.Home]} control={<Radio size="small" />} label="Home" />
                  <FormControlLabel value={PhoneNumberType[PhoneNumberType.Work]} control={<Radio size="small" />} label="Work" />
                  {/* <FormControlLabel value={PhoneNumberType[PhoneNumberType.Fax]} control={<Radio size="small" />} label="Fax" /> */}
                </RadioGroup>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField id="email-address" label="Email Address" fullWidth size="small" type="email"
                value={emailAddress} onChange={e => setFields({...fields, emailAddress: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl component="fieldset">
                <RadioGroup aria-label="emailType" name="emailType" row
                  value={EmailAddressType[emailType]} onChange={e => setFields({...fields, emailType: EmailAddressType[e.target.value as keyof typeof EmailAddressType]})}>
                  <FormControlLabel value={EmailAddressType[EmailAddressType.Personal]} control={<Radio size="small" />} label="Personal" />
                  <FormControlLabel value={EmailAddressType[EmailAddressType.Work]} control={<Radio size="small" />} label="Work" />
                </RadioGroup>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
            </Grid>
            <Grid item xs={12}>
              <TextField id="address-line1" label="Address Line 1" fullWidth size="small"
                value={addressLine1} onChange={e => setFields({...fields, addressLine1: e.target.value})} />
            </Grid>
            <Grid item xs={12}>
              <TextField id="address-line2" label="Address Line 2" fullWidth size="small"
                value={addressLine2} onChange={e => setFields({...fields, addressLine2: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField id="address-city" label="City" fullWidth size="small"
                value={city} onChange={e => setFields({...fields, city: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={2}>
              <TextField id="address-state" label="State" fullWidth size="small"
                value={state} onChange={e => setFields({...fields, state: e.target.value})} />
            </Grid>
            <Grid item xs={12} sm={4}>
              <TextField id="address-postalcode" label="ZIP/Postal Code" fullWidth size="small"
                value={postalCode} onChange={e => setFields({...fields, postalCode: e.target.value})} />
            </Grid>
            <Grid item xs={12}>
            </Grid>
            <Grid item xs={12}>
              <TextField
                id="concerns"
                label="Concerns" placeholder="Note any safety risks, allergies, etc."
                multiline fullWidth variant="outlined" minRows={2} maxRows={5} size="small"
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <WarningIcon />
                    </InputAdornment>
                  ),
                }}
                value={concerns == null ? "" : concerns} onChange={e => setFields({...fields, concerns: e.target.value})}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                id="notes"
                label="Notes" placeholder="Space for any general notes"
                multiline fullWidth variant="outlined" minRows={2} maxRows={5} size="small"
                value={notes == null ? "" : notes} onChange={e => setFields({...fields, notes: e.target.value})}
              />
            </Grid>
          </Grid>
        </form>
      </DialogContent>
      <DialogActions sx={{marginBottom: 4}}>
        <Button onClick={() => onClose()} color="secondary">
          Cancel
        </Button>
        <Button onClick={save} variant="contained" color="primary">
          Create Family
        </Button>
      </DialogActions>
    </Dialog>
  );
}
