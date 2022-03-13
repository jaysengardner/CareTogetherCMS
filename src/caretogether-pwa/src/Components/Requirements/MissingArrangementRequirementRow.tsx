import { format } from "date-fns";
import { useState } from "react";
import { useRecoilValue } from "recoil";
import { MissingArrangementRequirement, Permission } from "../../GeneratedClient";
import { policyData } from "../../Model/ConfigurationModel";
import { usePermissions } from "../../Model/SessionModel";
import { IconRow } from "../IconRow";
import { MissingRequirementDialog } from "./MissingRequirementDialog";
import { RequirementContext } from "./RequirementContext";

type MissingArrangementRequirementRowProps = {
  requirement: MissingArrangementRequirement;
  context: RequirementContext;
};

export function MissingArrangementRequirementRow({ requirement, context }: MissingArrangementRequirementRowProps) {
  const policy = useRecoilValue(policyData);
  const permissions = usePermissions();

  const requirementPolicy = policy.actionDefinitions![requirement.actionName!];

  const [dialogOpen, setDialogOpen] = useState(false);
  const openDialog = () => setDialogOpen(true);

  const canComplete = context.kind === 'Referral' || context.kind === 'Arrangement'
    ? true //TODO: Implement these permissions!
    : permissions(Permission.EditApprovalRequirementCompletion);

  return (
    <>
      {requirement.dueBy
        ? <IconRow icon='📅' onClick={canComplete ? openDialog : undefined}>
          {requirement.actionName}&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          <span style={{ float: 'right' }}>{format(requirement.dueBy, "M/d/yy h:mm a")}</span>
        </IconRow>
        : <IconRow icon='❌' onClick={canComplete ? openDialog : undefined}>
          {requirement.actionName}&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
          {requirement.pastDueSince && <span style={{ float: 'right' }}>{format(requirement.pastDueSince, "M/d/yy h:mm a")}</span>}
        </IconRow>}
      <MissingRequirementDialog open={dialogOpen} onClose={() => setDialogOpen(false)}
        requirement={requirement} context={context} policy={requirementPolicy} />
    </>
  );
}
