﻿
class PreviewController{
    textArea = null;
    previewSpan = null;
    constructor(textAreaId, previewSpanId) {
        this.textArea = document.getElementById(textAreaId);
        this.previewSpan = document.getElementById(previewSpanId);
        this.textArea.addEventListener('input', () => this.updatePreview());
        this.updatePreview();
    }

    async updatePreview() {
        let about = this.textArea.value;
        try {
            const response = await fetch('/exercises/render-about-preview', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `about=${encodeURIComponent(about)}`
            });
            if (!response.ok) {
                throw new Error('Network response was not ok.');
            }
            this.previewSpan.innerHTML = await response.text();
        } catch (error) {
            console.error('Error fetching preview:', error);
            this.previewSpan.innerText = '<p class="text-danger">Error rendering preview</p>';
        }
    }
}