import { MAZE_VISUAL_THEME_OPTIONS, type MazeVisualTheme } from '$lib/core/mazeVisualThemes';

const STORAGE_KEY = 'mazeescape_maze_theme';

function createMazeThemeStore() {
	let theme = $state<MazeVisualTheme>('neon');

	function apply(t: MazeVisualTheme) {
		theme = t;
		try { localStorage.setItem(STORAGE_KEY, t); } catch { /* storage unavailable */ }
	}

	function init() {
		try {
			const stored = localStorage.getItem(STORAGE_KEY) as MazeVisualTheme | null;
			const valid = MAZE_VISUAL_THEME_OPTIONS.map((entry) => entry.id);
			if (stored && valid.includes(stored)) {
				theme = stored;
			}
		} catch { /* SSR or storage unavailable */ }
	}

	return {
		get theme() { return theme; },
		init,
		set: apply,
	};
}

export const mazeThemeStore = createMazeThemeStore();
