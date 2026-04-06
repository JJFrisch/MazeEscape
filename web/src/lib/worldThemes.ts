/**
 * Per-world visual art direction for the web UI.
 * Maps world IDs to background art, accent colours, overlays, and motif hints.
 * Used by the landing page, world cards, and level grid pages.
 */

export interface WorldTheme {
	worldId: number;
	name: string;
	tagline: string;
	/** Filename inside web/static/images/ – empty string = CSS gradient fallback */
	bgImageFile: string;
	/** Primary accent colour (light, used on dark backgrounds) */
	accentColor: string;
	/** Dimmed version for backgrounds / rings */
	accentDim: string;
	/** CSS gradient overlaid on the bg image so text stays readable */
	overlayGradient: string;
	/** Accent text colour on dark card surfaces */
	textAccent: string;
	motif: 'tech' | 'space' | 'elemental';
}

export const WORLD_THEMES: WorldTheme[] = [
	{
		worldId: 1,
		name: 'Cybernetic Labyrinths',
		tagline: 'Navigate neon circuits through digital mazes',
		bgImageFile: 'background_blue_1.png',
		accentColor: '#38bdf8',
		accentDim: 'rgba(56,189,248,0.18)',
		overlayGradient:
			'linear-gradient(160deg, rgba(3,14,36,0.84) 0%, rgba(4,26,56,0.72) 100%)',
		textAccent: '#bae6fd',
		motif: 'tech',
	},
	{
		worldId: 2,
		name: 'Galactic Grids',
		tagline: 'Chart a path through cosmic labyrinths',
		bgImageFile: 'space_background11.png',
		accentColor: '#c084fc',
		accentDim: 'rgba(192,132,252,0.18)',
		overlayGradient:
			'linear-gradient(160deg, rgba(8,3,28,0.82) 0%, rgba(14,5,48,0.70) 100%)',
		textAccent: '#e9d5ff',
		motif: 'space',
	},
	{
		worldId: 3,
		name: 'Elemental Whispers',
		tagline: 'Ancient paths through living lands',
		bgImageFile: '',
		accentColor: '#34d399',
		accentDim: 'rgba(52,211,153,0.18)',
		overlayGradient:
			'linear-gradient(160deg, rgba(2,20,10,0.90) 0%, rgba(4,36,18,0.80) 100%)',
		textAccent: '#a7f3d0',
		motif: 'elemental',
	},
];

export function getWorldTheme(worldId: number): WorldTheme {
	return WORLD_THEMES.find((t) => t.worldId === worldId) ?? WORLD_THEMES[0];
}
