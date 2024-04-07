import { getBaseUrl } from './host';

export function applyAppTheme(name: string | null = null) {
  let path = `${getBaseUrl()}/settings/theme`;

  if (name !== null) {
    path += `?name=${encodeURIComponent(name)}`;
  }

  const theme = document.getElementById('app-theme');

  if (theme) {
    theme.setAttribute('href', path);
  }
}
