import { IconButton, List, ListItem, ListItemButton, ListItemIcon, ListItemText, useTheme } from '@mui/material';
import { CombinedFamilyInfo, CommunityInfo, Permission, RemoveCommunityMemberFamily } from '../GeneratedClient';
import { useCommunityCommand, visibleFamiliesQuery } from '../Model/DirectoryModel';
import { useCommunityPermissions } from '../Model/SessionModel';
import PeopleIcon from '@mui/icons-material/People';
import DeleteIcon from '@mui/icons-material/Delete';
import { useBackdrop } from '../Hooks/useBackdrop';
import { useRecoilValue } from 'recoil';
import { familyNameString } from '../Families/FamilyName';
import { useNavigate } from 'react-router-dom';

interface CommunityMemberFamiliesProps {
  communityInfo: CommunityInfo;
}
export function CommunityMemberFamilies({ communityInfo }: CommunityMemberFamiliesProps) {
  const permissions = useCommunityPermissions(communityInfo);
  const community = communityInfo.community!;

  const visibleFamilies = useRecoilValue(visibleFamiliesQuery);

  const memberFamilies = (community?.memberFamilies || []).map(familyId =>
    visibleFamilies.find(family => family.family?.id === familyId)).filter(family => family) as CombinedFamilyInfo[];
  
  const removeMemberFamily = useCommunityCommand((communityId, familyId: string) => {
    const command = new RemoveCommunityMemberFamily();
    command.communityId = communityId;
    command.familyId = familyId;
    return command;
  });

  const withBackdrop = useBackdrop();
  async function remove(family: CombinedFamilyInfo) {
    //TODO: Use the DeleteDocumentDialog approach - potentially making it reusable?
    if (window.confirm("Are you sure you want to remove this member family?\n\n" + familyNameString(family))) {
      await withBackdrop(async () => {
        await removeMemberFamily(community.id!, family.family!.id!);
      });
    }
  }

  const theme = useTheme();
  const navigate = useNavigate();

  return <List sx={{ '& .MuiListItemIcon-root': { minWidth: 36 } }}>
    {memberFamilies.map(family => 
      <ListItem key={family.family!.id!} disablePadding
        secondaryAction={permissions(Permission.EditCommunityMemberFamilies)
          ? <IconButton edge="end" aria-label="delete"
              color='primary'
              onClick={() => remove(family)}>
              <DeleteIcon />
            </IconButton>
          : null}>
        <ListItemButton disableGutters sx={{ paddingTop: 0, paddingBottom: 0 }}
          onClick={() => navigate(`/families/${family.family!.id!}`)}>
          <ListItemIcon>
            <PeopleIcon color='primary' />
          </ListItemIcon>
          <ListItemText
            primary={familyNameString(family)} primaryTypographyProps={{ color: theme.palette.primary.main }}>
          </ListItemText>
        </ListItemButton>
      </ListItem>)}
  </List>;
}
