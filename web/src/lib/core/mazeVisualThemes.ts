export type MazeVisualTheme = 'neon' | 'classic' | 'dotmatrix' | 'retro' | 'blueprint' | 'forest';

export interface MazeVisualThemeOption {
	id: MazeVisualTheme;
	label: string;
	accent: string;
}

export interface MazeThemePalette {
	strokeColor: string;
	bgColor: string;
	cellBgColor: string;
	cellStrokeColor: string;
	startCellColor: string;
	startCellStroke: string;
	exitCellColor: string;
	exitCellStroke: string;
	visitedColor: string;
	wallWidth: number;
	wallGlowWidth: number;
	wallLineCap: 'round' | 'square';
	wallLineJoin: 'round' | 'miter';
	playerColor: string;
	playerStroke: string;
	playerStrokeWidth: number;
	playerHaloColor: string;
	playerGlow: boolean;
	exitDotColor: string;
	exitGlow: boolean;
	gridColor: string;
	intersectionDotColor: string;
	useDotMatrixGrid: boolean;
}

export const MAZE_VISUAL_THEME_OPTIONS: MazeVisualThemeOption[] = [
	{ id: 'neon', label: 'Neon', accent: '#38bdf8' },
	{ id: 'classic', label: 'Classic', accent: '#6366f1' },
	{ id: 'dotmatrix', label: 'Dot Matrix', accent: '#ff3333' },
	{ id: 'retro', label: 'Retro', accent: '#f59e0b' },
	{ id: 'blueprint', label: 'Blueprint', accent: '#bfdbfe' },
	{ id: 'forest', label: 'Forest', accent: '#4ade80' }
];

const FALLBACK_WALL_COLOR = '#38bdf8';

export function getMazeThemePalette(theme: MazeVisualTheme, wallColor?: string): MazeThemePalette {
	const resolvedWallColor = !wallColor || wallColor === '#000000' ? FALLBACK_WALL_COLOR : wallColor;

	switch (theme) {
		case 'classic':
			return {
				strokeColor: '#1a1a1a',
				bgColor: '#ffffff',
				cellBgColor: '#f8fafc',
				cellStrokeColor: 'rgba(99,102,241,0.10)',
				startCellColor: 'rgba(99,102,241,0.12)',
				startCellStroke: 'rgba(99,102,241,0.3)',
				exitCellColor: 'rgba(52,211,153,0.15)',
				exitCellStroke: 'rgba(52,211,153,0.4)',
				visitedColor: 'rgba(99,102,241,0.06)',
				wallWidth: 3.5,
				wallGlowWidth: 0,
				wallLineCap: 'square',
				wallLineJoin: 'miter',
				playerColor: '#4f46e5',
				playerStroke: '#312e81',
				playerStrokeWidth: 1.5,
				playerHaloColor: 'rgba(99,102,241,0.12)',
				playerGlow: false,
				exitDotColor: '#059669',
				exitGlow: false,
				gridColor: 'rgba(15,23,42,0.05)',
				intersectionDotColor: '#1a1a1a',
				useDotMatrixGrid: false
			};
		case 'dotmatrix':
			return {
				strokeColor: '#ff3333',
				bgColor: '#0a0a0a',
				cellBgColor: '#111',
				cellStrokeColor: 'rgba(255,255,255,0.04)',
				startCellColor: 'rgba(255,255,255,0.08)',
				startCellStroke: 'rgba(255,255,255,0.16)',
				exitCellColor: 'rgba(52,211,153,0.12)',
				exitCellStroke: 'rgba(52,211,153,0.35)',
				visitedColor: 'rgba(255,80,80,0.06)',
				wallWidth: 2,
				wallGlowWidth: 2.5,
				wallLineCap: 'round',
				wallLineJoin: 'round',
				playerColor: '#ff3333',
				playerStroke: 'none',
				playerStrokeWidth: 0,
				playerHaloColor: 'rgba(255,51,51,0.16)',
				playerGlow: false,
				exitDotColor: '#22c55e',
				exitGlow: false,
				gridColor: 'rgba(255,255,255,0.15)',
				intersectionDotColor: '#ff3333',
				useDotMatrixGrid: true
			};
		case 'retro':
			return {
				strokeColor: '#f59e0b',
				bgColor: '#0a0600',
				cellBgColor: '#110900',
				cellStrokeColor: 'rgba(245,158,11,0.08)',
				startCellColor: 'rgba(245,158,11,0.10)',
				startCellStroke: 'rgba(245,158,11,0.22)',
				exitCellColor: 'rgba(52,211,153,0.10)',
				exitCellStroke: 'rgba(52,211,153,0.28)',
				visitedColor: 'rgba(245,158,11,0.05)',
				wallWidth: 2,
				wallGlowWidth: 2.5,
				wallLineCap: 'round',
				wallLineJoin: 'round',
				playerColor: '#f59e0b',
				playerStroke: 'none',
				playerStrokeWidth: 0,
				playerHaloColor: 'rgba(245,158,11,0.16)',
				playerGlow: true,
				exitDotColor: '#34d399',
				exitGlow: false,
				gridColor: 'rgba(245,158,11,0.05)',
				intersectionDotColor: '#f59e0b',
				useDotMatrixGrid: false
			};
		case 'blueprint':
			return {
				strokeColor: '#bfdbfe',
				bgColor: '#1e3a5f',
				cellBgColor: 'rgba(191,219,254,0.04)',
				cellStrokeColor: 'rgba(191,219,254,0.09)',
				startCellColor: 'rgba(147,197,253,0.15)',
				startCellStroke: 'rgba(191,219,254,0.28)',
				exitCellColor: 'rgba(52,211,153,0.15)',
				exitCellStroke: 'rgba(52,211,153,0.28)',
				visitedColor: 'rgba(147,197,253,0.06)',
				wallWidth: 1.5,
				wallGlowWidth: 0,
				wallLineCap: 'square',
				wallLineJoin: 'miter',
				playerColor: '#fbbf24',
				playerStroke: 'none',
				playerStrokeWidth: 0,
				playerHaloColor: 'rgba(251,191,36,0.12)',
				playerGlow: false,
				exitDotColor: '#34d399',
				exitGlow: false,
				gridColor: 'rgba(191,219,254,0.06)',
				intersectionDotColor: '#bfdbfe',
				useDotMatrixGrid: false
			};
		case 'forest':
			return {
				strokeColor: '#4ade80',
				bgColor: '#ffffff',
				cellBgColor: 'rgba(34,197,94,0.035)',
				cellStrokeColor: 'rgba(22,163,74,0.10)',
				startCellColor: 'rgba(34,197,94,0.12)',
				startCellStroke: 'rgba(21,128,61,0.24)',
				exitCellColor: 'rgba(187,247,208,0.75)',
				exitCellStroke: 'rgba(22,163,74,0.26)',
				visitedColor: 'rgba(34,197,94,0.06)',
				wallWidth: 2,
				wallGlowWidth: 2.5,
				wallLineCap: 'round',
				wallLineJoin: 'round',
				playerColor: '#15803d',
				playerStroke: '#dcfce7',
				playerStrokeWidth: 1,
				playerHaloColor: 'rgba(34,197,94,0.14)',
				playerGlow: true,
				exitDotColor: '#22c55e',
				exitGlow: false,
				gridColor: 'rgba(22,163,74,0.08)',
				intersectionDotColor: '#4ade80',
				useDotMatrixGrid: false
			};
		case 'neon':
		default:
			return {
				strokeColor: resolvedWallColor,
				bgColor: '#080e1e',
				cellBgColor: 'rgba(255,255,255,0.015)',
				cellStrokeColor: 'rgba(255,255,255,0.04)',
				startCellColor: 'rgba(109,40,217,0.18)',
				startCellStroke: 'rgba(139,92,246,0.3)',
				exitCellColor: 'rgba(52,211,153,0.15)',
				exitCellStroke: 'rgba(52,211,153,0.35)',
				visitedColor: 'rgba(139,92,246,0.06)',
				wallWidth: 2,
				wallGlowWidth: 2.5,
				wallLineCap: 'round',
				wallLineJoin: 'round',
				playerColor: 'url(#mgrad-player)',
				playerStroke: 'none',
				playerStrokeWidth: 0,
				playerHaloColor: 'rgba(139,92,246,0.18)',
				playerGlow: true,
				exitDotColor: 'url(#mgrad-exit)',
				exitGlow: true,
				gridColor: 'rgba(255,255,255,0.04)',
				intersectionDotColor: resolvedWallColor,
				useDotMatrixGrid: false
			};
	}
}