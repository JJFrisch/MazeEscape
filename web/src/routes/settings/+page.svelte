<script lang="ts">
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';

	let nameInput = $state(gameStore.player.playerName);
	let wallColor = $state(gameStore.player.wallColor || '#6366f1');
	let saved = $state(false);

	function saveName() {
		gameStore.setPlayerName(nameInput.trim());
		flash();
	}

	function saveWallColor(e: Event) {
		const target = e.target as HTMLInputElement;
		wallColor = target.value;
		gameStore.setWallColor(wallColor);
		flash();
	}

	function flash() {
		saved = true;
		setTimeout(() => saved = false, 1500);
	}

	async function syncNow() { await gameStore.syncNow(); }
	async function signOut()  { await authStore.signOut(); }

	const presetColors = ['#6366f1', '#8b5cf6', '#ec4899', '#ef4444', '#f59e0b', '#10b981', '#06b6d4', '#ffffff', '#000000'];
</script>

<svelte:head>
	<title>Settings – Maze Escape: Pathbound</title>
</svelte:head>

{#if saved}
	<div class="toast">
		<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
			<path d="M5 13l4 4L19 7" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
		</svg>
		Saved!
	</div>
{/if}

<div class="settings-page">
	<div class="settings-header">
		<div class="page-eyebrow">
			<span class="eyebrow-dot"></span>
			Preferences
		</div>
		<h1 class="page-title">Settings</h1>
		<p class="page-sub">Customize your gameplay experience.</p>
	</div>

	<div class="settings-body">

		<!-- Player Name -->
		<div class="setting-card">
			<div class="setting-card-header">
				<div class="setting-icon">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="8" r="4" stroke="currentColor" stroke-width="2"/>
						<path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
					</svg>
				</div>
				<div>
					<label class="setting-title" for="player-name">Player Name</label>
					<p class="setting-desc">How you appear in the game.</p>
				</div>
			</div>
			<div class="input-row">
				<input
					id="player-name"
					class="text-input"
					type="text"
					bind:value={nameInput}
					maxlength="24"
					placeholder="Enter your name"
					onkeydown={(e) => e.key === 'Enter' && saveName()}
				/>
				<button class="primary-btn" onclick={saveName}>Save</button>
			</div>
		</div>

		<!-- Wall Color -->
		<div class="setting-card">
			<div class="setting-card-header">
				<div class="setting-icon">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
						<path d="M12 2a10 10 0 010 20" fill="currentColor" opacity="0.3"/>
					</svg>
				</div>
				<div>
					<div class="setting-title">Wall Color</div>
					<p class="setting-desc">Maze wall color during gameplay.</p>
				</div>
			</div>
			<div class="color-presets">
				{#each presetColors as color}
					<button
						class="color-swatch"
						class:active={wallColor === color}
						style="background: {color}; {color === '#ffffff' ? 'border-color: rgba(255,255,255,0.3);' : ''}"
						onclick={() => { wallColor = color; gameStore.setWallColor(color); flash(); }}
						aria-label="Select color {color}"
					></button>
				{/each}
				<label class="color-picker-wrap" aria-label="Custom color">
					<svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M12 20h9M16.5 3.5a2.121 2.121 0 013 3L7 19l-4 1 1-4L16.5 3.5z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
					</svg>
					<input
						type="color"
						class="color-picker-input"
						value={wallColor}
						oninput={saveWallColor}
					/>
				</label>
			</div>
			<div class="color-preview">
				<span class="preview-label">Preview</span>
				<div class="wall-preview">
					<div class="wall-line" style="background: {wallColor};"></div>
					<div class="wall-line short" style="background: {wallColor};"></div>
					<div class="wall-line" style="background: {wallColor};"></div>
				</div>
			</div>
		</div>

		<!-- Data & Sync -->
		<div class="setting-card">
			<div class="setting-card-header">
				<div class="setting-icon">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M20 12V22H4V12" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
						<path d="M22 7H2v5h20V7z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
						<path d="M12 22V7M12 7H7.5a2.5 2.5 0 010-5C11 2 12 7 12 7zM12 7h4.5a2.5 2.5 0 000-5C13 2 12 7 12 7z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
					</svg>
				</div>
				<div>
					<div class="setting-title">Data & Sync</div>
					<p class="setting-desc">
						{#if authStore.isAuthenticated}
							Cloud sync enabled for <strong>{authStore.user?.email}</strong>
						{:else}
							Progress saved locally. Sign in to sync across devices.
						{/if}
					</p>
				</div>
			</div>

			<div class="data-stats-grid">
				<div class="data-stat">
					<img src="{base}/images/coin.png" alt="" class="stat-icon" aria-hidden="true"/>
					<span class="stat-val">{gameStore.player.coinCount.toLocaleString()}</span>
					<span class="stat-lbl">Coins</span>
				</div>
				<div class="data-stat">
					<span class="stat-emoji">🎨</span>
					<span class="stat-val">{gameStore.player.unlockedSkinIds.length}</span>
					<span class="stat-lbl">Skins</span>
				</div>
				<div class="data-stat">
					<span class="stat-emoji">{authStore.isAuthenticated ? '☁️' : '💾'}</span>
					<span class="stat-val status-val">{gameStore.cloudSyncStatusLabel}</span>
					<span class="stat-lbl">Status</span>
				</div>
				{#if gameStore.lastSyncedAtLabel}
					<div class="data-stat">
						<span class="stat-emoji">🕐</span>
						<span class="stat-val small-val">{gameStore.lastSyncedAtLabel}</span>
						<span class="stat-lbl">Last Sync</span>
					</div>
				{/if}
			</div>

			{#if gameStore.cloudSyncError}
				<div class="sync-error">
					<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
						<path d="M12 8v4M12 16v.01" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
					</svg>
					{gameStore.cloudSyncError}
				</div>
			{/if}

			<div class="sync-actions">
				{#if authStore.isAuthenticated}
					<button class="primary-btn" onclick={syncNow} disabled={gameStore.cloudSyncBusy}>
						{gameStore.cloudSyncBusy ? 'Syncing…' : 'Sync Now'}
					</button>
					<button class="ghost-btn" onclick={signOut} disabled={authStore.loading}>Sign Out</button>
				{:else}
					<a class="primary-btn" href="{base}/auth">Sign in to enable sync</a>
				{/if}
			</div>
		</div>

	</div>
</div>

<style>
	@keyframes fade-up { from { opacity:0; transform:translateY(16px); } to { opacity:1; transform:translateY(0); } }
	@keyframes pulse   { 0%,100% { opacity:1; } 50% { opacity:0.4; } }

	.settings-page {
		max-width: 580px;
		margin: 0 auto;
		animation: fade-up 0.4s ease both;
	}

	/* ── Header ─────────────────────────────────── */
	.settings-header {
		margin-bottom: var(--space-10);
	}
	.page-eyebrow {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 5px 14px;
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.25);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		color: #93c5fd;
		margin-bottom: var(--space-3);
	}
	.eyebrow-dot {
		width: 6px; height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 6px var(--color-accent-primary);
		animation: pulse 2s ease-in-out infinite;
	}
	.page-title {
		font-family: var(--font-display);
		font-size: clamp(1.8rem, 4vw, 2.5rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		margin-bottom: var(--space-2);
	}
	.page-sub { color: var(--color-text-secondary); font-size: var(--text-base); }

	/* ── Settings cards ─────────────────────────── */
	.settings-body {
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
	}

	.setting-card {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border-subtle);
		border-radius: var(--radius-xl);
		padding: var(--space-6);
		backdrop-filter: blur(12px);
		box-shadow: var(--shadow-card);
		display: flex;
		flex-direction: column;
		gap: var(--space-5);
		transition: border-color var(--transition-base);
	}
	.setting-card:hover { border-color: rgba(56,189,248,0.15); }

	.setting-card-header {
		display: flex;
		align-items: flex-start;
		gap: var(--space-4);
	}
	.setting-icon {
		width: 40px;
		height: 40px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.2);
		border-radius: var(--radius-lg);
		color: var(--color-accent-primary);
		flex-shrink: 0;
	}
	.setting-title {
		display: block;
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: var(--color-text-primary);
		margin-bottom: 3px;
	}
	.setting-desc {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.5;
	}
	.setting-desc strong { color: var(--color-text-primary); }

	/* ── Inputs ──────────────────────────────────── */
	.input-row {
		display: flex;
		gap: var(--space-2);
	}
	.text-input {
		flex: 1;
		background: rgba(255,255,255,0.04);
		border: 1px solid rgba(255,255,255,0.1);
		border-radius: var(--radius-lg);
		padding: 11px var(--space-4);
		color: var(--color-text-primary);
		font-size: var(--text-base);
		font-family: var(--font-body);
		transition: border-color var(--transition-fast);
	}
	.text-input:focus {
		outline: none;
		border-color: var(--color-accent-primary);
		box-shadow: 0 0 0 3px rgba(56,189,248,0.1);
	}
	.text-input::placeholder { color: rgba(255,255,255,0.25); }

	/* Buttons */
	.primary-btn {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 11px 22px;
		background: var(--color-accent-primary);
		color: #fff;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		cursor: pointer;
		text-decoration: none;
		transition: all var(--transition-fast);
		box-shadow: 0 0 16px rgba(56,189,248,0.2);
		white-space: nowrap;
	}
	.primary-btn:hover:not(:disabled) { background: #0ea5e9; box-shadow: 0 0 24px rgba(56,189,248,0.35); }
	.primary-btn:disabled { opacity: 0.4; cursor: not-allowed; box-shadow: none; }

	.ghost-btn {
		padding: 11px 22px;
		background: rgba(255,255,255,0.05);
		border: 1px solid rgba(255,255,255,0.12);
		border-radius: var(--radius-lg);
		color: var(--color-text-secondary);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		cursor: pointer;
		transition: all var(--transition-fast);
	}
	.ghost-btn:hover:not(:disabled) { background: rgba(255,255,255,0.1); color: var(--color-text-primary); }
	.ghost-btn:disabled { opacity: 0.4; cursor: not-allowed; }

	/* ── Color picker ───────────────────────────── */
	.color-presets {
		display: flex;
		gap: var(--space-2);
		flex-wrap: wrap;
		align-items: center;
	}
	.color-swatch {
		width: 34px;
		height: 34px;
		border-radius: var(--radius-full);
		border: 2px solid transparent;
		cursor: pointer;
		transition: all var(--transition-fast);
		box-shadow: 0 2px 8px rgba(0,0,0,0.3);
	}
	.color-swatch:hover { transform: scale(1.12); }
	.color-swatch.active {
		border-color: #fff;
		box-shadow: 0 0 0 3px var(--color-accent-primary), 0 2px 8px rgba(0,0,0,0.3);
	}
	.color-picker-wrap {
		width: 34px;
		height: 34px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(255,255,255,0.07);
		border: 1px solid rgba(255,255,255,0.15);
		border-radius: var(--radius-full);
		color: var(--color-text-secondary);
		cursor: pointer;
		transition: all var(--transition-fast);
		position: relative;
		overflow: hidden;
	}
	.color-picker-wrap:hover { background: rgba(255,255,255,0.12); color: var(--color-text-primary); }
	.color-picker-input {
		position: absolute;
		width: 200%;
		height: 200%;
		top: -50%;
		left: -50%;
		opacity: 0;
		cursor: pointer;
	}

	.color-preview {
		display: flex;
		align-items: center;
		gap: var(--space-3);
	}
	.preview-label {
		font-size: var(--text-xs);
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: rgba(255,255,255,0.3);
	}
	.wall-preview {
		display: flex;
		flex-direction: column;
		gap: 5px;
	}
	.wall-line {
		height: 4px;
		width: 100px;
		border-radius: var(--radius-full);
		box-shadow: 0 0 8px currentColor;
		transition: background var(--transition-fast);
	}
	.wall-line.short { width: 60px; }

	/* ── Data stats ─────────────────────────────── */
	.data-stats-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
		gap: var(--space-3);
	}
	.data-stat {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 4px;
		padding: var(--space-3);
		background: rgba(255,255,255,0.03);
		border: 1px solid rgba(255,255,255,0.06);
		border-radius: var(--radius-lg);
		text-align: center;
	}
	.stat-icon { width: 18px; height: 18px; object-fit: contain; }
	.stat-emoji { font-size: 18px; line-height: 1; }
	.stat-val {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: var(--color-text-primary);
	}
	.status-val { font-size: var(--text-sm); }
	.small-val  { font-size: 11px; }
	.stat-lbl {
		font-size: 10px;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: rgba(255,255,255,0.3);
	}

	.sync-error {
		display: flex;
		align-items: center;
		gap: var(--space-2);
		padding: var(--space-3) var(--space-4);
		background: rgba(244,63,94,0.1);
		border: 1px solid rgba(244,63,94,0.3);
		border-radius: var(--radius-lg);
		color: #fca5a5;
		font-size: var(--text-sm);
	}
	.sync-actions {
		display: flex;
		gap: var(--space-2);
		flex-wrap: wrap;
	}
</style>
