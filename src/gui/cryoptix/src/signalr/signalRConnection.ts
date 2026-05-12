import * as signalR from "@microsoft/signalr";

export const createSignalRConnection = (
  baseUrl: string,
  route: string,
  accessTokenFactory: () => string | Promise<string>
) => {
  const normalizedBaseUrl = baseUrl.endsWith("/") ? baseUrl : `${baseUrl}/`;
  const fullUrl = new URL(route, normalizedBaseUrl).toString();

  console.log("[SignalR] Connecting to:", fullUrl);

  return new signalR.HubConnectionBuilder()
    .withUrl(fullUrl, { accessTokenFactory })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build();
};
