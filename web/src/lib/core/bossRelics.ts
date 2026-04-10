export interface BossRelicDefinition {
	id: string;
	name: string;
	title: string;
	description: string;
	lore: string;
	inscription: string;
	origin: string;
	icon: string;
	accent: string;
	art: {
		aura: string;
		frame: string;
		viewBox: string;
		sigilPaths: Array<{
			d: string;
			fill?: string;
			opacity?: number;
			strokeWidth?: number;
		}>;
	};
}

export const BOSS_RELICS: BossRelicDefinition[] = [
	{
		id: 'labyrinth_core',
		name: 'Labyrinth Core',
		title: 'Sentinel Relic',
		description: 'A collapsed command core pulled from the first world boss. Its surface still redraws possible routes in dim orange light.',
		lore: 'Recovered from the war chamber beneath the first gate, the Core hums in short recursive bursts, as if still attempting to solve every maze it has ever commanded.',
		inscription: 'It remembers every wrong turn, and none of them are forgiven.',
		origin: 'World 1 boss, 14-boss',
		icon: '🜂',
		accent: '#f97316',
		art: {
			aura: 'radial-gradient(circle at 50% 50%, rgba(249,115,22,0.38), rgba(124,45,18,0.08) 55%, transparent 72%)',
			frame: 'linear-gradient(135deg, rgba(249,115,22,0.75), rgba(251,191,36,0.18))',
			viewBox: '0 0 120 120',
			sigilPaths: [
				{ d: 'M60 18 90 36 90 84 60 102 30 84 30 36Z', opacity: 0.22 },
				{ d: 'M60 30 78 40 78 80 60 90 42 80 42 40Z', opacity: 0.3 },
				{ d: 'M60 36V84M42 40H78M42 80H78M48 52H72M48 68H72', strokeWidth: 5 },
				{ d: 'M50 52V68H70V52', strokeWidth: 5 }
			]
		}
	},
	{
		id: 'starforged_crown',
		name: 'Starforged Crown',
		title: 'Celestial Relic',
		description: 'A crown of lattice metal and compressed starlight, claimed from the Galactic Grids world boss on first clear.',
		lore: 'The Crown was suspended at the center of a living mesh of corridors. Even inert, it bends nearby light into thin blue halos and leaves your hands cold.',
		inscription: 'Rule the lattice, and the lattice will name you in light.',
		origin: 'World 2 boss, 22-boss',
		icon: '👑',
		accent: '#38bdf8',
		art: {
			aura: 'radial-gradient(circle at 50% 42%, rgba(56,189,248,0.34), rgba(30,64,175,0.08) 54%, transparent 74%)',
			frame: 'linear-gradient(135deg, rgba(56,189,248,0.78), rgba(167,139,250,0.18))',
			viewBox: '0 0 120 120',
			sigilPaths: [
				{ d: 'M24 82 36 40 60 62 84 40 96 82Z', strokeWidth: 5 },
				{ d: 'M34 82V94H86V82', strokeWidth: 5 },
				{ d: 'M60 24 65 36 78 37 68 45 71 58 60 50 49 58 52 45 42 37 55 36Z', fill: 'currentColor', opacity: 0.9 },
				{ d: 'M36 40 48 56M84 40 72 56M60 62V82', opacity: 0.55, strokeWidth: 4 }
			]
		}
	}
];