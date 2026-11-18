/**
 * Which hub zones open after which World 1 level. The hub cards, the header
 * coin chip, the map dock and the route guard in the root layout all read this,
 * so a zone is locked the same way wherever it is reached from.
 *
 * The Spire keeps its own lock screen on /daily (gameStore.isDailyMazeUnlocked).
 * The Algorithms page stays open at every level.
 */
import { gameStore } from '$lib/stores/gameStore.svelte';

export type GatedZone = 'vault' | 'spire' | 'arena' | 'archives';

export const ZONE_UNLOCK_LEVEL: Record<GatedZone, string> = {
	vault: '1',
	spire: '10',
	arena: '20',
	archives: '5'
};

/** Routes the layout guard sends back to the hub while their zone is locked. */
const GUARDED_ROUTES: Array<[segment: string, zone: GatedZone]> = [
	['/shop', 'vault'],
	['/equip', 'vault'],
	['/leaderboard', 'arena'],
	['/codex', 'archives'],
	['/stats', 'archives']
];

export function isZoneUnlocked(zone: GatedZone): boolean {
	return !!gameStore.getLevelProgress(1, ZONE_UNLOCK_LEVEL[zone])?.completed;
}

// Wording approved by Jake on 2026-11-18. Follows the Spire's existing lock line.
export function zoneLockReason(zone: GatedZone): string {
	return `Complete Level ${ZONE_UNLOCK_LEVEL[zone]} to unlock`;
}

export function guardedZoneForPath(path: string): GatedZone | null {
	return GUARDED_ROUTES.find(([segment]) => path.includes(segment))?.[1] ?? null;
}
