import { useRouteError, isRouteErrorResponse } from "react-router-dom";

function getErrorMessage(data: unknown): string | undefined {
  if (
    typeof data === "object" &&
    data !== null &&
    "message" in data &&
    typeof (data as { message?: unknown }).message === "string"
  ) {
    return (data as { message: string }).message;
  }

  return undefined;
}

export default function ErrorPage() {
  const error = useRouteError();

  let title = "Something went wrong";
  let message = "An unexpected error occurred.";

  if (isRouteErrorResponse(error)) {
    title = `${error.status} ${error.statusText}`;

    const safeMessage = getErrorMessage(error.data);
    if (safeMessage) {
      message = safeMessage;
    }
  } else if (error instanceof Error) {
    message = error.message;
  }

  return (
    <div className="flex h-screen items-center justify-center flex-col gap-4">
      <h1 className="text-2xl font-bold">{title}</h1>
      <p className="text-muted-foreground">{message}</p>

      <button
        onClick={() => window.location.reload()}
        className="px-4 py-2 bg-black text-white rounded"
      >
        Reload
      </button>
    </div>
  );
}
