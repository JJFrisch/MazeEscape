<!--
  MazeRenderer: Canvas-based maze rendering with smooth player animations.
  Handles drawing walls, cells, start/end markers, player, and hint path.
  Responsive to container size.
-->
<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import type { MazeData, Position } from '$lib/core/types';

	let {
		maze,
		playerPos,
		wallColor = '#000000',
		hintPath = null,
		skinEmoji = '🟣',
		showVisited = false,
		visitedCells = new Set<string>()
	}: {
		maze: MazeData;
		playerPos: Position;
		wallColor?: string;
		hintPath?: Position[] | null;
		skinEmoji?: string;
		showVisited?: boolean;
		visitedCells?: Set<string>;
	} = $props();

	let canvas = $state<HTMLCanvasElement | undefined>(undefined);
	let container = $state<HTMLDivElement | undefined>(undefined);
	let animFrame: number;
	let targetPlayerX = $derived(playerPos.x);
	let targetPlayerY = $derived(playerPos.y);
	let animatedPlayerX = $state(0);
	let animatedPlayerY = $state(0);

	const WALL_WIDTH = 2;
	const CELL_PADDING = 0.1;
	const ANIMATION_SPEED = 0.15; // lerp factor

	function lerp(a: number, b: number, t: number): number {
		return a + (b - a) * t;
	}

	function draw() {
		if (!canvas || !container || !maze) return;

		const ctx = canvas.getContext('2d');
		if (!ctx) return;

		const rect = container.getBoundingClientRect();
		const dpr = window.devicePixelRatio || 1;
		const displayWidth = rect.width;
		const displayHeight = rect.height;

		canvas.width = displayWidth * dpr;
		canvas.height = displayHeight * dpr;
		canvas.style.width = `${displayWidth}px`;
		canvas.style.height = `${displayHeight}px`;
		ctx.scale(dpr, dpr);

		// Calculate cell size to fit the container
		const padding = 8;
		const availW = displayWidth - padding * 2;
		const availH = displayHeight - padding * 2;
		const cellSize = Math.min(availW / maze.width, availH / maze.height);
		const offsetX = (displayWidth - cellSize * maze.width) / 2;
		const offsetY = (displayHeight - cellSize * maze.height) / 2;

		// Clear
		ctx.fillStyle = '#0f172a';
		ctx.fillRect(0, 0, displayWidth, displayHeight);

		// Draw cells
		for (let y = 0; y < maze.height; y++) {
			for (let x = 0; x < maze.width; x++) {
				const cell = maze.cells[y][x];
				const cx = offsetX + x * cellSize;
				const cy = offsetY + y * cellSize;
				const inner = cellSize * CELL_PADDING;

				// Cell background
				if (cell.value === 2) {
					// Start cell
					ctx.fillStyle = 'rgba(99, 102, 241, 0.2)';
					ctx.fillRect(cx + inner, cy + inner, cellSize - inner * 2, cellSize - inner * 2);
				} else if (cell.value === 3) {
					// End cell
					ctx.fillStyle = 'rgba(52, 211, 153, 0.25)';
					ctx.fillRect(cx + inner, cy + inner, cellSize - inner * 2, cellSize - inner * 2);

					// Goal marker
					ctx.fillStyle = '#34d399';
					const markerSize = cellSize * 0.3;
					ctx.beginPath();
					ctx.arc(
						cx + cellSize / 2,
						cy + cellSize / 2,
						markerSize / 2,
						0,
						Math.PI * 2
					);
					ctx.fill();
				} else if (showVisited && visitedCells.has(`${x},${y}`)) {
					ctx.fillStyle = 'rgba(167, 139, 250, 0.08)';
					ctx.fillRect(cx + inner, cy + inner, cellSize - inner * 2, cellSize - inner * 2);
				}
			}
		}

		// Draw hint path
		if (hintPath && hintPath.length > 1) {
			ctx.strokeStyle = 'rgba(251, 191, 36, 0.4)';
			ctx.lineWidth = cellSize * 0.15;
			ctx.lineCap = 'round';
			ctx.lineJoin = 'round';
			ctx.beginPath();
			ctx.moveTo(
				offsetX + hintPath[0].x * cellSize + cellSize / 2,
				offsetY + hintPath[0].y * cellSize + cellSize / 2
			);
			for (let i = 1; i < hintPath.length; i++) {
				ctx.lineTo(
					offsetX + hintPath[i].x * cellSize + cellSize / 2,
					offsetY + hintPath[i].y * cellSize + cellSize / 2
				);
			}
			ctx.stroke();
		}

		// Draw walls
		ctx.strokeStyle = wallColor || '#e2e8f0';
		ctx.lineWidth = WALL_WIDTH;
		ctx.lineCap = 'round';

		for (let y = 0; y < maze.height; y++) {
			for (let x = 0; x < maze.width; x++) {
				const cell = maze.cells[y][x];
				const cx = offsetX + x * cellSize;
				const cy = offsetY + y * cellSize;

				// North wall
				if (cell.north) {
					ctx.beginPath();
					ctx.moveTo(cx, cy);
					ctx.lineTo(cx + cellSize, cy);
					ctx.stroke();
				}

				// East wall
				if (cell.east) {
					ctx.beginPath();
					ctx.moveTo(cx + cellSize, cy);
					ctx.lineTo(cx + cellSize, cy + cellSize);
					ctx.stroke();
				}

				// South border (last row)
				if (y === maze.height - 1) {
					ctx.beginPath();
					ctx.moveTo(cx, cy + cellSize);
					ctx.lineTo(cx + cellSize, cy + cellSize);
					ctx.stroke();
				}

				// West border (first column)
				if (x === 0) {
					ctx.beginPath();
					ctx.moveTo(cx, cy);
					ctx.lineTo(cx, cy + cellSize);
					ctx.stroke();
				}
			}
		}

		// Draw player (animated position)
		animatedPlayerX = lerp(animatedPlayerX, targetPlayerX, ANIMATION_SPEED);
		animatedPlayerY = lerp(animatedPlayerY, targetPlayerY, ANIMATION_SPEED);

		const px = offsetX + animatedPlayerX * cellSize + cellSize / 2;
		const py = offsetY + animatedPlayerY * cellSize + cellSize / 2;
		const playerRadius = cellSize * 0.35;

		// Player glow
		const gradient = ctx.createRadialGradient(px, py, 0, px, py, playerRadius * 2);
		gradient.addColorStop(0, 'rgba(167, 139, 250, 0.3)');
		gradient.addColorStop(1, 'rgba(167, 139, 250, 0)');
		ctx.fillStyle = gradient;
		ctx.fillRect(px - playerRadius * 2, py - playerRadius * 2, playerRadius * 4, playerRadius * 4);

		// Player body
		ctx.fillStyle = '#a78bfa';
		ctx.beginPath();
		ctx.arc(px, py, playerRadius, 0, Math.PI * 2);
		ctx.fill();

		// Player emoji/icon
		ctx.font = `${cellSize * 0.45}px sans-serif`;
		ctx.textAlign = 'center';
		ctx.textBaseline = 'middle';
		ctx.fillText(skinEmoji, px, py + 1);

		// Check if animation needs to continue
		const dx = Math.abs(animatedPlayerX - targetPlayerX);
		const dy = Math.abs(animatedPlayerY - targetPlayerY);
		if (dx > 0.01 || dy > 0.01) {
			animFrame = requestAnimationFrame(draw);
		}
	}

	$effect(() => {
		// Redraw whenever maze, playerPos, hintPath, or wallColor change
		if (canvas && maze) {
			targetPlayerX; // track
			targetPlayerY; // track
			hintPath; // track
			wallColor; // track
			cancelAnimationFrame(animFrame);
			animFrame = requestAnimationFrame(draw);
		}
	});

	onMount(() => {
		animatedPlayerX = targetPlayerX;
		animatedPlayerY = targetPlayerY;

		if (!container) {
			return () => {
				cancelAnimationFrame(animFrame);
			};
		}

		const observer = new ResizeObserver(() => {
			cancelAnimationFrame(animFrame);
			animFrame = requestAnimationFrame(draw);
		});
		observer.observe(container);
		animFrame = requestAnimationFrame(draw);

		return () => {
			observer.disconnect();
			cancelAnimationFrame(animFrame);
		};
	});

	onDestroy(() => {
		cancelAnimationFrame(animFrame);
	});
</script>

<div class="maze-container" bind:this={container} role="img" aria-label="Maze grid">
	<canvas bind:this={canvas}></canvas>
</div>

<style>
	.maze-container {
		width: 100%;
		height: 100%;
		min-height: 200px;
		border-radius: var(--radius-lg);
		overflow: hidden;
		background: #0f172a;
	}

	canvas {
		display: block;
		width: 100%;
		height: 100%;
	}
</style>
