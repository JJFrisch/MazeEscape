import { getAllWorlds } from '$lib/core/levels';

// Build a page for every world so a direct visit is served as a real page
// rather than through the 404 fallback.
export const entries = () => getAllWorlds().map((world) => ({ worldId: String(world.worldId) }));
