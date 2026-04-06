<svelte:head>
	<title>Info – MazeEscape</title>
</svelte:head>

<div class="info-page">
	<h1 class="page-title">ℹ️ How to Play</h1>

	<section class="info-section">
		<h2>🎯 Goal</h2>
		<p>Navigate through the maze from the <strong>start</strong> (blue glow) to the <strong>exit</strong> (green glow) as quickly and efficiently as possible.</p>
	</section>

	<section class="info-section">
		<h2>🎮 Controls</h2>
		<div class="controls-list">
			<div class="control-item">
				<span class="control-keys">Arrow Keys / WASD</span>
				<span class="control-desc">Move through the maze</span>
			</div>
			<div class="control-item">
				<span class="control-keys">Swipe</span>
				<span class="control-desc">Touch controls on mobile</span>
			</div>
			<div class="control-item">
				<span class="control-keys">D-Pad</span>
				<span class="control-desc">On-screen directional buttons</span>
			</div>
		</div>
	</section>

	<section class="info-section">
		<h2>⭐ Stars</h2>
		<p>Earn up to 3 stars per level:</p>
		<ul>
			<li><strong>★ (1 star)</strong> – Complete the level</li>
			<li><strong>★★ (2 stars)</strong> – Finish within the move limit</li>
			<li><strong>★★★ (3 stars)</strong> – Finish within the time limit</li>
		</ul>
		<p>Stars unlock gates to progress through the campaign.</p>
	</section>

	<section class="info-section">
		<h2>🗺️ Campaign</h2>
		<p>Three worlds of increasing difficulty, each with main and bonus levels. Earn enough stars to unlock gates and progress to new areas.</p>
	</section>

	<section class="info-section">
		<h2>📅 Daily Maze</h2>
		<p>A unique maze generated each day. Complete daily mazes to build your streak and earn coins. Unlocked after completing World 1, Level 10.</p>
	</section>

	<section class="info-section">
		<h2>💡 Powerups</h2>
		<ul>
			<li><strong>Hint (200 🪙)</strong> – Shows the next step on the optimal path</li>
			<li><strong>Extra Time (150 🪙)</strong> – Extra seconds for the 3-star threshold</li>
			<li><strong>Extra Moves (50 🪙)</strong> – Extra moves for the 2-star threshold</li>
		</ul>
	</section>

	<section class="info-section">
		<h2>🎨 Skins</h2>
		<p>Customize your character. Purchase skins from the shop or unlock special skins by opening campaign chests.</p>
	</section>

	<section class="info-section credits">
		<h2>Credits</h2>
		<p>MazeEscape – A maze puzzle game.</p>
		<p class="credit-tech">Built with SvelteKit • TypeScript • Canvas</p>
	</section>
</div>

<style>
	.info-page {
		max-width: 600px;
		margin: 0 auto;
		padding: var(--space-4);
	}

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-8);
	}

	.info-section {
		margin-bottom: var(--space-8);
	}

	.info-section h2 {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		margin-bottom: var(--space-3);
	}

	.info-section p,
	.info-section li {
		color: var(--color-text-secondary);
		line-height: 1.6;
		font-size: var(--text-sm);
	}

	.info-section ul {
		list-style: none;
		padding: 0;
		display: flex;
		flex-direction: column;
		gap: var(--space-2);
	}

	.info-section li {
		padding-left: var(--space-4);
		position: relative;
	}

	.info-section li::before {
		content: '→';
		position: absolute;
		left: 0;
		color: var(--color-accent-primary);
	}

	.controls-list {
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
	}

	.control-item {
		display: flex;
		align-items: center;
		gap: var(--space-4);
	}

	.control-keys {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-md);
		padding: var(--space-2) var(--space-3);
		font-family: var(--font-mono);
		font-size: var(--text-sm);
		min-width: 140px;
		text-align: center;
	}

	.control-desc {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
	}

	.credits {
		border-top: 1px solid var(--color-border);
		padding-top: var(--space-8);
		text-align: center;
	}

	.credit-tech {
		color: var(--color-text-muted) !important;
		font-size: var(--text-xs) !important;
		margin-top: var(--space-2);
	}
</style>
