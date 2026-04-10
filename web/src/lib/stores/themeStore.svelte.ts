type Theme = 'dark' | 'light';

function createThemeStore() {
	let theme = $state<Theme>('dark');

	function apply(t: Theme) {
		theme = t;
		document.documentElement.dataset.theme = t;
		try { localStorage.setItem('theme', t); } catch { /* storage unavailable */ }
	}

	function init() {
		let resolved: Theme = 'dark';
		try {
			const stored = localStorage.getItem('theme');
			if (stored === 'light' || stored === 'dark') {
				resolved = stored;
			} else if (window.matchMedia('(prefers-color-scheme: light)').matches) {
				resolved = 'light';
			}
		} catch { /* SSR or storage unavailable */ }
		apply(resolved);
	}

	function toggle() {
		apply(theme === 'dark' ? 'light' : 'dark');
	}

	return {
		get theme() { return theme; },
		init,
		toggle
	};
}

export const themeStore = createThemeStore();
