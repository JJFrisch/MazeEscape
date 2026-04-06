/**
 * Seeded pseudo-random number generator (Mulberry32).
 * Produces deterministic sequences from a 32-bit seed.
 * Used for reproducible maze generation (daily mazes, campaign levels).
 */
export class SeededRandom {
	private state: number;

	constructor(seed: number) {
		this.state = seed | 0;
	}

	/** Returns a float in [0, 1) */
	next(): number {
		let t = (this.state += 0x6d2b79f5);
		t = Math.imul(t ^ (t >>> 15), t | 1);
		t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
		return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
	}

	/** Returns an integer in [min, max) */
	nextInt(min: number, max: number): number {
		return min + Math.floor(this.next() * (max - min));
	}

	/** Shuffles an array in-place using Fisher-Yates */
	shuffle<T>(arr: T[]): T[] {
		for (let i = arr.length - 1; i > 0; i--) {
			const j = this.nextInt(0, i + 1);
			[arr[i], arr[j]] = [arr[j], arr[i]];
		}
		return arr;
	}
}

/**
 * Creates a deterministic seed from a date string.
 * Used for daily mazes so all players get the same maze on the same day.
 */
export function dateSeed(dateStr: string): number {
	let hash = 0;
	for (let i = 0; i < dateStr.length; i++) {
		const char = dateStr.charCodeAt(i);
		hash = (hash << 5) - hash + char;
		hash |= 0;
	}
	return Math.abs(hash);
}
