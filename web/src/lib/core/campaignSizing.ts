import type { CampaignLevel } from './types';

export interface CampaignViewport {
	width: number;
	height: number;
}

export interface ResponsiveCampaignLevelConfig {
	width: number;
	height: number;
	twoStarMoves: number;
	threeStarTime: number;
	fiveStarMoves: number;
	fiveStarTime: number;
}

export function shouldUseWideCampaignLayout(viewport?: CampaignViewport): boolean {
	if (!viewport) return false;
	if (viewport.width < 1100 || viewport.height < 700) return false;
	return viewport.width / Math.max(viewport.height, 1) >= 1.45;
}

function scaleThreshold(baseValue: number, scaleFactor: number, minimumIncrease: number): number {
	if (scaleFactor <= 1) return baseValue;
	return Math.max(baseValue + minimumIncrease, Math.round(baseValue * scaleFactor));
}

export function getResponsiveCampaignLevelConfig(
	level: CampaignLevel,
	viewport?: CampaignViewport
): ResponsiveCampaignLevelConfig {
	let width = level.width;
	let height = level.height;

	if (shouldUseWideCampaignLayout(viewport)) {
		switch (level.mazeShape) {
			case 'rectangular': {
				height = Math.max(level.height + 2, Math.round(level.height * 1.28));
				width = Math.max(level.width + 4, Math.round(height * 1.9));
				break;
			}
			case 'hexagonal': {
				height = Math.max(level.height + 1, Math.round(level.height * 1.18));
				width = Math.max(level.width + 2, Math.round(level.width * 1.22));
				break;
			}
			case 'triangular': {
				height = Math.max(level.height + 2, Math.round(level.height * 1.24));
				width = Math.max(level.width + 3, Math.round(level.width * 1.28));
				break;
			}
			case 'circular': {
				const scaledRings = Math.max(level.height + 1, Math.round(level.height * 1.2));
				width = scaledRings;
				height = scaledRings;
				break;
			}
		}
	}

	const scaleFactor = (width * height) / Math.max(level.width * level.height, 1);

	return {
		width,
		height,
		twoStarMoves: scaleThreshold(level.twoStarMoves, scaleFactor, 2),
		threeStarTime: scaleThreshold(level.threeStarTime, scaleFactor, 1),
		fiveStarMoves: scaleThreshold(level.fiveStarMoves, scaleFactor, 1),
		fiveStarTime: scaleThreshold(level.fiveStarTime, scaleFactor, 1)
	};
}