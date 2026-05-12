// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
	const coverInput = document.getElementById('coverImage');
	const preview = document.getElementById('coverPreview');

	if (!coverInput || !preview) {
		return;
	}

	const defaultCover = '/images/books/default.svg';

	coverInput.addEventListener('change', () => {
		const file = coverInput.files && coverInput.files[0];
		if (!file) {
			preview.src = preview.getAttribute('data-original-src') || defaultCover;
			return;
		}

		preview.src = URL.createObjectURL(file);
	});

	if (!preview.getAttribute('data-original-src')) {
		preview.setAttribute('data-original-src', preview.getAttribute('src') || defaultCover);
	}
});
