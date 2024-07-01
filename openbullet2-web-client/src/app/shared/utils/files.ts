import { HttpResponse } from '@angular/common/http';

export function saveFile(response: HttpResponse<Blob>) {
  // Grab the filename from the Content-Disposition header
  const filename: string = getFileName(response);

  if (response.body === null) {
    throw new Error('No body found in response');
  }

  const downloadLink = document.createElement('a');
  downloadLink.href = window.URL.createObjectURL(new Blob([response.body], { type: response.body.type }));

  downloadLink.setAttribute('download', filename);
  document.body.appendChild(downloadLink);
  downloadLink.click();
}

function getFileName(response: HttpResponse<Blob>) {
  try {
    const contentDisposition = response.headers.get('content-disposition');

    if (contentDisposition === null) {
      throw new Error('Could not find content-disposition header');
    }

    return /filename="?([^"|;]+)/.exec(contentDisposition)![1];
  } catch (e) {
    return 'file';
  }
}
