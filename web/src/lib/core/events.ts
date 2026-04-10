import { POWERUP_COSTS, type EventProgress, type PowerupCost, type MazeAlgorithm } from './types';

export interface EventMilestone {
	at: number;
	label: string;
}

export interface SeasonalEventDefinition {
	id: string;
	name: string;
	description: string;
	themeAccent: string;
	startsAt: string;
	endsAt: string;
	dailyAlgorithmPool?: MazeAlgorithm[];
	shopOverrides?: Partial<Record<PowerupCost['name'], Partial<PowerupCost>>>;
	trackerLabel: string;
	milestones: EventMilestone[];
}

export const SEASONAL_EVENTS: SeasonalEventDefinition[] = [
	{
		id: 'spring-labyrinth-2026',
		name: 'Spring Labyrinth',
		description: 'Bloom-touched corridors bring brighter daily mazes, event stock, and a shared progress trail.',
		themeAccent: '#34d399',
		startsAt: '2026-04-01T00:00:00.000Z',
		endsAt: '2026-05-01T00:00:00.000Z',
		dailyAlgorithmPool: ['prims', 'growingTree_50_50', 'huntAndKill'],
		shopOverrides: {
			compass: {
				displayName: 'Bloom Compass',
				description: 'Flashes a flower-lit route toward the exit for 3 seconds.',
				cost: 60,
				tags: ['event']
			},
			hint: {
				displayName: 'Petal Hint',
				description: 'Reveals a spring-marked path through the thicket.',
				cost: 175,
				tags: ['event']
			}
		},
		trackerLabel: 'Spring progress',
		milestones: [
			{ at: 3, label: 'Complete 3 themed runs' },
			{ at: 7, label: 'Complete 7 themed runs' },
			{ at: 12, label: 'Complete 12 themed runs' }
		]
	}
];

export function getActiveEvent(now = new Date()): SeasonalEventDefinition | null {
	const ts = now.getTime();
	return SEASONAL_EVENTS.find((event) => {
		const starts = Date.parse(event.startsAt);
		const ends = Date.parse(event.endsAt);
		return ts >= starts && ts < ends;
	}) ?? null;
}

export function getEventPowerupCatalog(now = new Date()): PowerupCost[] {
	const activeEvent = getActiveEvent(now);
	if (!activeEvent?.shopOverrides) return POWERUP_COSTS;

	return POWERUP_COSTS.map((item) => {
		const override = activeEvent.shopOverrides?.[item.name];
		if (!override) return item;
		return {
			...item,
			...override,
			tags: Array.from(new Set([...(item.tags ?? []), ...(override.tags ?? []), 'event']))
		};
	});
}

export function getCompletedEventMilestones(eventId: string, progress: number): number[] {
	const event = SEASONAL_EVENTS.find((entry) => entry.id === eventId);
	if (!event) return [];
	return event.milestones.filter((milestone) => progress >= milestone.at).map((milestone) => milestone.at);
}

export function getEventProgressPercent(event: SeasonalEventDefinition, progress: EventProgress | null | undefined): number {
	const target = event.milestones[event.milestones.length - 1]?.at ?? 1;
	return Math.min(100, Math.round((((progress?.progress ?? 0) / target) * 100)));
}