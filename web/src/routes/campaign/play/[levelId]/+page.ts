import { getAllWorlds } from '$lib/core/levels';

// Build a page for every level so a direct visit is served as a real page
// rather than through the 404 fallback.
export const entries = () =>
	getAllWorlds().flatMap((world) =>
		world.levels.map((level) => ({ levelId: `${world.worldId}-${level.levelNumber}` }))
	);
