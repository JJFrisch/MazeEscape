/**
 * Layouts for the campaign maps drawn the way the native (.NET MAUI) app draws
 * them. Every number comes from CampaignPage.xaml(.cs) for World 1, as it stood
 * with the full 67 levels (before commit ebe3396), and from
 * World2CampaignPage.xaml(.cs) for World 2. Positions are in the native canvas's
 * own units; the component scales the canvas to the height it is given.
 */

export type Cell = [number, number];

export interface ClassicArea {
	/** Number of the first level and first bonus level in this area. */
	first: number;
	main: Cell[];
	firstBonus: number;
	bonus: Cell[];
}

export interface ClassicGate {
	x: number; y: number; w: number; h: number;
	rotate: number;
	stars: number;
	small?: boolean;
	/** Levels that must be reached before the gate opens. */
	needs: string[];
	/** Opens with the key and the last level instead of stars. */
	final?: boolean;
}

export interface ClassicTreasure {
	id: string;
	cell: Cell;
	/** The level that leads to it. */
	after: string;
	kind?: 'chest' | 'key' | 'skin';
	skinId?: number;
	skinName?: string;
}

export interface ClassicMapConfig {
	mapW: number;
	mapH: number;
	cols: number;
	rows: number;
	orb: number;
	orbFont: number;
	/** Colour of the number on a locked level. */
	lockedNumColor: string;
	/** Files under static/images. */
	mazeImage: string;
	backdrop: string;
	dockImage: string;
	dockDark: boolean;
	/** Icon file prefixes: `${prefix}_0..3` and `${prefix}_locked`. */
	mainIcon: string;
	bonusIcon: string;
	gateImage: string;
	gateLabelColor: string;
	chestImage: string;
	openedChestImage: string;
	areas: ClassicArea[];
	/** Level to reach and stars to hold before each later area's fog lifts. Empty = no fog. */
	areaEntry: { level: string; stars: number }[];
	/** Fog banks anchored to the right edge. Bank n stays until area n + 1 opens, so they stack. */
	fogWidths: number[];
	gates: ClassicGate[];
	treasures: ClassicTreasure[];
	portal: { cell: Cell; stars: number; finalLevel: string; needsKey: boolean };
}

const WORLD_1: ClassicMapConfig = {
	mapW: 1700,
	mapH: 470,
	cols: 20,
	rows: 5,
	orb: 80,
	orbFont: 16,
	lockedNumColor: '#d3d3d3',
	mazeImage: 'classic/campaign_background_1.png',
	backdrop: 'background_blue_1.png',
	dockImage: 'classic/background_blue_3.png',
	dockDark: false,
	mainIcon: 'classic/level',
	bonusIcon: 'classic/bonus',
	gateImage: 'classic/gate4.png',
	gateLabelColor: 'gold',
	chestImage: 'chest.png',
	openedChestImage: 'classic/opened_chest.png',
	areas: [
		{
			first: 1,
			main: [[3, 3], [3, 4], [2, 4], [2, 3], [2, 2], [1, 2], [0, 2], [0, 1], [0, 0], [1, 0], [2, 0], [3, 0], [3, 1], [3, 2]],
			firstBonus: 1,
			bonus: [[0, 3], [0, 4], [1, 4], [2, 1]]
		},
		{
			first: 15,
			main: [[4, 2], [4, 1], [5, 1], [5, 0], [6, 0], [6, 1], [6, 2], [5, 2], [5, 3], [4, 3], [4, 4], [5, 4], [6, 4], [7, 4]],
			firstBonus: 5,
			bonus: [[7, 1], [7, 2], [7, 3]]
		},
		{
			first: 29,
			main: [[8, 4], [9, 4], [9, 3], [9, 2], [8, 2], [8, 1], [9, 1], [10, 1], [10, 0], [11, 0], [12, 0], [12, 1], [11, 1], [11, 2], [12, 2]],
			firstBonus: 8,
			bonus: [[9, 0], [8, 0]]
		},
		{
			first: 44,
			main: [[13, 2], [14, 2], [14, 1], [13, 1], [13, 0], [14, 0], [15, 0], [15, 1], [15, 2], [16, 2], [16, 3], [16, 4]],
			firstBonus: 10,
			bonus: [[15, 4], [14, 4], [13, 4], [12, 4], [11, 4], [10, 4], [10, 3], [11, 3], [12, 3], [13, 3], [14, 3]]
		},
		{
			first: 56,
			main: [[17, 4], [17, 3], [18, 3], [18, 4], [19, 4], [19, 3], [19, 2], [19, 1], [18, 1], [17, 1], [17, 0], [18, 0]],
			firstBonus: 21,
			bonus: [[17, 2], [16, 0]]
		}
	],
	areaEntry: [
		{ level: '15', stars: 30 },
		{ level: '29', stars: 80 },
		{ level: '44', stars: 120 },
		{ level: '56', stars: 200 }
	],
	fogWidths: [1360, 1020, 590, 248],
	gates: [
		{ x: 24, y: 243, w: 35, h: 80, rotate: 270, stars: 20, needs: ['1b'] },
		{ x: 235, y: 91, w: 35, h: 98, rotate: 0, stars: 45, needs: ['4b'] },
		{ x: 319, y: 187, w: 35, h: 98, rotate: 0, stars: 30, needs: ['15'] },
		{ x: 570, y: 94, w: 55, h: 96, rotate: 0, stars: 60, needs: ['5b'] },
		{ x: 654.5, y: 375, w: 55, h: 96, rotate: 0, stars: 80, needs: ['29'] },
		{ x: 824, y: 0, w: 55, h: 96, rotate: 0, stars: 100, small: true, needs: ['8b'] },
		{ x: 1078, y: 188, w: 55, h: 96, rotate: 0, stars: 120, small: true, needs: ['44'] },
		{ x: 1332.5, y: 375, w: 55, h: 96, rotate: 0, stars: 150, small: true, needs: ['10b'] },
		{ x: 1419, y: 375, w: 55, h: 96, rotate: 0, stars: 200, small: true, needs: ['56'] },
		{ x: 1460, y: 145, w: 55, h: 88, rotate: 270, stars: 230, small: true, needs: ['21b'] },
		{ x: 1418.5, y: 0, w: 55, h: 96, rotate: 0, stars: 240, small: true, needs: ['22b'] },
		{ x: 1585.5, y: 0, w: 55, h: 96, rotate: 0, stars: 0, small: true, needs: [], final: true }
	],
	treasures: [
		{ id: 'c1', cell: [1, 3], after: '3b' },
		{ id: 'c2', cell: [1, 1], after: '4b' },
		{ id: 'c3', cell: [4, 0], after: '18' },
		{ id: 'c4', cell: [6, 3], after: '7b' },
		{ id: 'c5', cell: [8, 3], after: '33' },
		{ id: 'c6', cell: [7, 0], after: '9b' },
		{ id: 'c7', cell: [10, 2], after: '16b' },
		{ id: 'c8', cell: [15, 3], after: '20b', kind: 'key' },
		{ id: 'c9', cell: [18, 2], after: '21b' },
		{ id: 'c10', cell: [16, 1], after: '22b' }
	],
	portal: { cell: [19, 0], stars: 250, finalLevel: '67', needsKey: true }
};

const WORLD_2: ClassicMapConfig = {
	mapW: 3000,
	mapH: 470,
	cols: 40,
	rows: 6,
	orb: 70,
	orbFont: 14,
	lockedNumColor: '#fff',
	mazeImage: 'classic/w2_campaign_background.png',
	backdrop: 'space_background7.png',
	dockImage: 'space_background11.png',
	dockDark: true,
	// The native page gives levels the planet art and bonus levels the galaxy art.
	mainIcon: 'classic/w2_level',
	bonusIcon: 'classic/w2_bonus',
	gateImage: 'classic/w2_gate.png',
	gateLabelColor: '#fff',
	chestImage: 'world2_chest.png',
	openedChestImage: 'classic/w2_opened_chest.png',
	areas: [
		{
			first: 1,
			main: [[0, 3], [0, 2], [1, 2], [1, 1], [2, 1], [2, 2], [2, 3], [1, 3], [1, 4], [2, 4], [3, 4], [3, 3], [4, 3], [4, 4], [5, 4], [5, 5], [6, 5], [6, 4], [7, 4], [7, 3], [8, 3], [8, 4]],
			firstBonus: 1,
			bonus: [[0, 4], [0, 5], [1, 5], [2, 5], [3, 5], [2, 0], [1, 0], [0, 0], [4, 2], [5, 2], [5, 3], [6, 3], [6, 2], [6, 1], [6, 0], [5, 0], [4, 0], [3, 0], [3, 1], [4, 1], [7, 0], [8, 2], [8, 1], [7, 1], [8, 5]]
		},
		{
			first: 23,
			main: [[9, 4], [10, 4], [10, 5], [11, 5], [11, 4], [12, 4], [12, 5], [13, 5], [13, 4], [13, 3], [12, 3], [12, 2], [11, 2], [11, 1], [12, 1], [13, 1], [13, 0], [14, 0], [15, 0], [15, 1], [14, 1], [14, 2], [15, 2]],
			firstBonus: 26,
			bonus: [[14, 4], [14, 5], [15, 5], [15, 4], [15, 3], [11, 0], [10, 0], [9, 0], [9, 1], [10, 1], [10, 2], [10, 3], [9, 3]]
		},
		{
			first: 46,
			main: [[16, 2], [17, 2], [17, 1], [16, 1], [16, 0], [17, 0], [18, 0], [19, 0], [19, 1], [19, 2], [20, 2], [20, 3], [21, 3], [21, 2], [22, 2], [23, 2], [23, 3], [24, 3], [24, 4], [24, 5]],
			firstBonus: 39,
			bonus: [[20, 0], [20, 1], [21, 1], [21, 0], [22, 0], [23, 0], [23, 1], [24, 1], [19, 3], [18, 3], [18, 2], [22, 3], [22, 4], [21, 4], [20, 4], [20, 5], [19, 5], [19, 4], [18, 4], [18, 5], [17, 5], [17, 4], [16, 4], [16, 3], [21, 5], [22, 5], [23, 5]]
		},
		{
			first: 66,
			main: [[25, 5], [25, 4], [25, 3], [25, 2], [25, 1], [25, 0], [26, 0], [27, 0], [27, 1], [28, 1], [29, 1], [29, 0], [30, 0], [30, 1], [31, 1], [31, 2]],
			firstBonus: 66,
			bonus: [[26, 1], [26, 2], [26, 3], [27, 3], [28, 3], [28, 2], [29, 3], [29, 2], [27, 4], [26, 4], [26, 5], [27, 5], [28, 5], [28, 4], [29, 4], [30, 4], [30, 3], [31, 3], [31, 4], [31, 5], [30, 5]]
		},
		{
			first: 82,
			main: [[32, 2], [32, 3], [32, 4], [32, 5], [33, 5], [34, 5], [35, 5], [36, 5], [37, 5], [37, 4], [36, 4], [35, 4], [35, 3], [35, 2], [36, 2], [36, 1], [37, 1], [38, 1], [39, 1], [39, 0], [38, 0], [37, 0], [36, 0], [35, 0], [34, 0], [34, 1], [33, 1], [33, 0], [32, 0]],
			firstBonus: 87,
			bonus: [[34, 2], [34, 3], [33, 3], [33, 4], [37, 2], [38, 2], [39, 2], [39, 3], [38, 3], [37, 3], [39, 4], [39, 5], [38, 5]]
		}
	],
	areaEntry: [],
	fogWidths: [],
	gates: [
		{ x: 18, y: 273, w: 35, h: 80, rotate: 270, stars: 50, needs: ['1b'] },
		{ x: 167, y: 41, w: 35, h: 80, rotate: 270, stars: 80, needs: ['6b'] },
		{ x: 317, y: 191, w: 35, h: 80, rotate: 90, stars: 120, needs: ['9b'] }
	],
	treasures: [
		{ id: 'c1', cell: [4, 5], after: '5b' },
		{ id: 'c2', cell: [0, 1], after: '8b' },
		{ id: 'c3', cell: [3, 2], after: '6' },
		{ id: 'c4', cell: [5, 1], after: '20b' },
		{ id: 'c5', cell: [8, 0], after: '21b' },
		{ id: 'c6', cell: [7, 2], after: '24b' },
		{ id: 'c7', cell: [7, 5], after: '25b' },
		{ id: 'c8', cell: [9, 5], after: '25' },
		{ id: 'c9', cell: [14, 3], after: '30b' },
		{ id: 'c10', cell: [13, 2], after: '32' },
		{ id: 'c11', cell: [11, 3], after: '35' },
		{ id: 'c12', cell: [9, 2], after: '38b', kind: 'skin', skinId: 13, skinName: 'Space Maze' },
		{ id: 'c13', cell: [12, 0], after: '37' },
		{ id: 'c14', cell: [22, 1], after: '43b' },
		{ id: 'c15', cell: [24, 2], after: '46b' },
		{ id: 'c16', cell: [18, 1], after: '49b' },
		{ id: 'c17', cell: [16, 5], after: '61b' },
		{ id: 'c18', cell: [17, 3], after: '62b' },
		{ id: 'c19', cell: [23, 4], after: '65b' },
		{ id: 'c20', cell: [24, 0], after: '71' },
		{ id: 'c21', cell: [28, 0], after: '75' },
		{ id: 'c22', cell: [31, 0], after: '78' },
		{ id: 'c23', cell: [27, 2], after: '71b' },
		{ id: 'c24', cell: [30, 2], after: '73b' },
		{ id: 'c25', cell: [29, 5], after: '86b', kind: 'skin', skinId: 15, skinName: 'Chucky' },
		{ id: 'c26', cell: [33, 2], after: '82' },
		{ id: 'c27', cell: [34, 4], after: '90b' },
		{ id: 'c28', cell: [36, 3], after: '96b', kind: 'skin', skinId: 17, skinName: 'Da Butler' },
		{ id: 'c29', cell: [38, 4], after: '99b' },
		{ id: 'c30', cell: [35, 1], after: '107' }
	],
	portal: { cell: [32, 1], stars: 20, finalLevel: '110', needsKey: false }
};

export function getClassicMap(worldId: number): ClassicMapConfig | null {
	if (worldId === 1) return WORLD_1;
	if (worldId === 2) return WORLD_2;
	return null;
}
