import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { useAuth0 } from "@auth0/auth0-react";

type Props = {
  children: React.ReactNode;
};

export function RequireAuth({ children }: Props) {
  const { isAuthenticated, isLoading, loginWithRedirect } = useAuth0();
  const location = useLocation();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      void loginWithRedirect({
        appState: {
          returnTo: location.pathname + location.search,
        },
      });
    }
  }, [isLoading, isAuthenticated, loginWithRedirect, location]);

  if (isLoading || !isAuthenticated) {
    return null; // or a spinner
  }

  return <>{children}</>;
}
