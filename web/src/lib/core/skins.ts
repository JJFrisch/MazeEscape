/**
 * Skin catalog.
 * Ported from C# PlayerDataModel.InitializeSkins().
 */

import type { SkinModel } from './types';

export const SKIN_CATALOG: SkinModel[] = [
	{ id: 0, name: 'Maze Solver', imageUrl: 'player_image0', coinPrice: 0, gemPrice: 0, isUnlocked: true, isEquipped: true, isSpecialSkin: false },
	{ id: 1, name: 'Cool Lion', imageUrl: 'player_image1', coinPrice: 500, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 2, name: 'Sunset Swirl', imageUrl: 'player_image2', coinPrice: 1000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 3, name: 'Pink Sunset', imageUrl: 'player_image3', coinPrice: 2000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 4, name: 'Diskes', imageUrl: 'player_image4', coinPrice: 5000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 5, name: 'Fireball', imageUrl: 'player_image5', coinPrice: 22000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 6, name: 'Galaxy Ball', imageUrl: 'player_image6', coinPrice: 50000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 7, name: 'Elemental Mixer', imageUrl: 'player_image7', coinPrice: 6500, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 8, name: 'Ivy Eye', imageUrl: 'player_image8', coinPrice: 3400, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 9, name: 'Ruffles', imageUrl: 'player_image9', coinPrice: 7120, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 10, name: 'Rocco', imageUrl: 'player_image10', coinPrice: 3000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 11, name: 'Pugsley', imageUrl: 'player_image11', coinPrice: 4375, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 12, name: 'Kowalski', imageUrl: 'player_image12', coinPrice: 9000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 13, name: 'Space Maze', imageUrl: 'player_image13', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true },
	{ id: 14, name: 'Brain', imageUrl: 'player_image14', coinPrice: 19000, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 15, name: 'Chucky', imageUrl: 'player_image15', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true },
	{ id: 16, name: 'Fire Elemental', imageUrl: 'player_image16', coinPrice: 8500, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: false },
	{ id: 17, name: 'Da Butler', imageUrl: 'player_image17', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true },
	{ id: 100, name: 'Serpent Ascendant', imageUrl: 'player_image13', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'backtracking' },
	{ id: 101, name: 'Obsidian Pursuer', imageUrl: 'player_image15', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'huntAndKill' },
	{ id: 102, name: 'Seedforged Bloom', imageUrl: 'player_image16', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'prims' },
	{ id: 103, name: 'Threadbound Prism', imageUrl: 'player_image17', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'kruskals' },
	{ id: 104, name: 'Balanced Reliquary', imageUrl: 'player_image13', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'growingTree_50_50' },
	{ id: 105, name: 'Favored Remnant', imageUrl: 'player_image15', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'growingTree_75_25' },
	{ id: 106, name: 'Wild Probability', imageUrl: 'player_image16', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'growingTree_25_75' },
	{ id: 107, name: 'Lasting Echo', imageUrl: 'player_image17', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'growingTree_50_0' },
	{ id: 108, name: 'Erased Atlas', imageUrl: 'player_image13', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'wilsons' },
	{ id: 109, name: 'Forgotten Drift', imageUrl: 'player_image15', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'aldousBroder' },
	{ id: 110, name: 'Fork-Tongue Vestment', imageUrl: 'player_image16', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'binaryTree' },
	{ id: 111, name: 'Currentborne Flow', imageUrl: 'player_image17', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'sidewinder' },
	{ id: 112, name: 'Row-Stitched Mantle', imageUrl: 'player_image13', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'ellers' },
	{ id: 113, name: 'Fractured Bastion', imageUrl: 'player_image15', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'recursiveDivision' },
	{ id: 114, name: 'Coiling Reverie', imageUrl: 'player_image16', coinPrice: 0, gemPrice: 0, isUnlocked: false, isEquipped: false, isSpecialSkin: true, masteryAlgorithm: 'spiralBacktracker' }
];

export function getSkinById(id: number): SkinModel | undefined {
	return SKIN_CATALOG.find((s) => s.id === id);
}
