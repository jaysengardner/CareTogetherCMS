import { Backdrop, CircularProgress } from "@material-ui/core";
import { atom, useRecoilCallback, useRecoilValue } from "recoil";

const showBackdropData = atom({
  key: 'showBackdropData',
  default: false
});

export function useBackdrop() {
  const showBackdrop = useRecoilCallback(({snapshot, set}) => {
    return async () => {
      set(showBackdropData, true);
    };
  });
  const hideBackdrop = useRecoilCallback(({snapshot, set}) => {
    return async () => {
      set(showBackdropData, false);
    };
  });
  return async (asyncFunction: () => Promise<void>) => {
    await showBackdrop();
    await asyncFunction();
    await hideBackdrop();
  }
}

export default function RequestBackdrop() {
  const showBackdrop = useRecoilValue(showBackdropData);

  return (
    <Backdrop
      style={{zIndex: 10000}}
      open={showBackdrop}>
      <CircularProgress color="inherit" />
    </Backdrop>
  );
};
