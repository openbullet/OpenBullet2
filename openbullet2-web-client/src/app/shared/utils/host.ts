import { environment } from '../../../environments/environment';

function getBaseHost() {
  return environment.production ? window.location.origin : 'http://localhost:5000';
}

export function getBaseUrl() {
  // Gets the base URL for API calls
  return `${getBaseHost()}/api/v1`;
}

export function getBaseHubUrl() {
  return `${getBaseHost()}/hubs`;
}

export function getSwaggerUrl() {
  return `${getBaseHost()}/swagger/index.html`;
}
