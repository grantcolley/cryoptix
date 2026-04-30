import { useAuth0 } from "@auth0/auth0-react";
import { Button } from "@/components/ui/button";
import { Icon } from "@/components/icon/icon";
import { icons } from "@/components/icon/icons";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";

const Login = () => {
  const { loginWithRedirect } = useAuth0();

  return (
    <TooltipProvider>
      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="outline"
            size="icon"
            onClick={() => {
              void loginWithRedirect();
            }}
          >
            <Icon icon={icons.logOut} />
            <span className="sr-only">Login</span>
          </Button>
        </TooltipTrigger>
        <TooltipContent>Login</TooltipContent>
      </Tooltip>
    </TooltipProvider>
  );
};

export default Login;
