document.addEventListener('click', (event) => {
  const button = event.target.closest('.filetree-download-btn');
  if (button) {
    const containerId = button.dataset.containerId;
    const basePath = button.dataset.basePath;
    const zipFileName = button.dataset.zipFilename;
    downloadTreeAsZip(event, containerId, basePath, zipFileName);
  }
});

async function downloadTreeAsZip(event, containerId, basePath, zipFileName) {
  event.preventDefault();
  const container = document.getElementById(containerId);
  if (!container) {
    console.error('Filetree container not found:', containerId);
    return;
  }

  // Load JSZip if not already loaded
  if (typeof JSZip === 'undefined') {
    const script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js';
    document.head.appendChild(script);
    await new Promise(resolve => {
      script.onload = resolve;
    });
  }

  const button = event.target;
  button.textContent = 'Zipping...';
  button.disabled = true;

  const zip = new JSZip();
  const fileLinks = container.querySelectorAll('a');
  const promises = [];

  for (const link of fileLinks) {
    const url = link.href;
    const urlPath = new URL(url).pathname;
    const searchPath = basePath.startsWith('/') ? basePath : '/' + basePath;

    let pathInZip = '';
    if (urlPath.startsWith(searchPath)) {
      pathInZip = urlPath.substring(searchPath.length).replace(/^\//, '');
    } else {
      console.warn('Skipping URL not within basePath:', urlPath, 'Base path:', searchPath);
      continue; // Skip to the next link
    }

    const promise = fetch(url)
      .then(response => {
        if (!response.ok) {
          throw new Error(`Failed to fetch ${url}: ${response.statusText}`);
        }
        return response.blob();
      })
      .then(blob => {
        const dirName = zipFileName.replace('.zip', '');
        const finalZipPath = `${dirName}/${pathInZip}`;
        zip.file(finalZipPath, blob);
      });
    promises.push(promise);
  }

  try {
    await Promise.all(promises);
    const content = await zip.generateAsync({ type: 'blob' });
    
    const link = document.createElement('a');
    link.href = URL.createObjectURL(content);
    link.download = zipFileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

  } catch (error) {
    console.error('Error creating zip file:', error);
    alert('Failed to create zip file. See console for details.');
  } finally {
    button.textContent = 'Download as zip';
    button.disabled = false;
  }
}