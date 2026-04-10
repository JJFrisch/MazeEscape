export interface BossRelicDefinition {
	id: string;
	name: string;
	title: string;
	description: string;
	origin: string;
	icon: string;
	accent: string;
}

export const BOSS_RELICS: BossRelicDefinition[] = [
	{
		id: 'labyrinth_core',
		name: 'Labyrinth Core',
		title: 'Sentinel Relic',
		description: 'A collapsed command core pulled from the first world boss. Its surface still redraws possible routes in dim orange light.',
		origin: 'World 1 boss, 14-boss',
		icon: '🜂',
		accent: '#f97316'
	},
	{
		id: 'starforged_crown',
		name: 'Starforged Crown',
		title: 'Celestial Relic',
		description: 'A crown of lattice metal and compressed starlight, claimed from the Galactic Grids world boss on first clear.',
		origin: 'World 2 boss, 22-boss',
		icon: '👑',
		accent: '#38bdf8'
	}
];