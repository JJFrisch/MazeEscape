<svelte:head>
	<title>Download – Maze Escape: Pathbound</title>
</svelte:head>

<div class="download-page">

	<!-- ── Hero ─────────────────────────────────── -->
	<section class="dl-hero">
		<div class="dl-hero-badge">Desktop Application</div>
		<h1 class="dl-title">Maze Escape: Pathbound</h1>
		<p class="dl-subtitle">
			Play the full game natively on your machine. The desktop build includes the complete campaign,
			offline play, and all maze types with no browser required.
		</p>
		<p class="dl-byline">
			Developed and maintained by
			<a href="https://jakefrischman.me" target="_blank" rel="noopener noreferrer" class="dl-author-link">Jake Frischmann</a>
		</p>

		<a
			href="https://github.com/JJFrisch/MazeEscape/releases"
			target="_blank"
			rel="noopener noreferrer"
			class="btn-download"
		>
			<svg class="btn-icon" viewBox="0 0 24 24" fill="none" aria-hidden="true">
				<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
				<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
			</svg>
			Download from GitHub Releases
		</a>
		<p class="dl-note">
			Opens the GitHub Releases page. Download the latest <code>.exe</code> or <code>.zip</code>
			installer for your platform.
		</p>
	</section>

	<!-- ── What's included ─────────────────────── -->
	<section class="dl-features">
		<h2 class="dl-section-title">What&apos;s Included</h2>
		<div class="features-grid">
			<div class="feature-card">
				<span class="feature-icon" aria-hidden="true">🗺️</span>
				<h3>Full Campaign</h3>
				<p>All worlds and levels, including bonus stages and multi-shape maze types.</p>
			</div>
			<div class="feature-card">
				<span class="feature-icon" aria-hidden="true">📶</span>
				<h3>Offline Play</h3>
				<p>No internet connection required. Progress is saved locally to your machine.</p>
			</div>
			<div class="feature-card">
				<span class="feature-icon" aria-hidden="true">🔷</span>
				<h3>All Maze Shapes</h3>
				<p>Rectangular, hexagonal, circular, and triangular mazes across the campaign.</p>
			</div>
			<div class="feature-card">
				<span class="feature-icon" aria-hidden="true">🏆</span>
				<h3>Star System</h3>
				<p>Earn up to 3 stars per level based on move count and completion time.</p>
			</div>
		</div>
	</section>

	<!-- ── Requirements ───────────────────────── -->
	<section class="dl-requirements">
		<h2 class="dl-section-title">System Requirements</h2>
		<div class="req-table">
			<div class="req-row">
				<span class="req-label">Platform</span>
				<span class="req-value">Windows 10 / 11</span>
			</div>
			<div class="req-row">
				<span class="req-label">Runtime</span>
				<span class="req-value">.NET 8 (MAUI)</span>
			</div>
			<div class="req-row">
				<span class="req-label">Architecture</span>
				<span class="req-value">x64</span>
			</div>
			<div class="req-row">
				<span class="req-label">Storage</span>
				<span class="req-value">~50 MB</span>
			</div>
		</div>
		<p class="req-note">macOS and Linux builds are not currently available. Play the
			<a href="/" class="dl-link">web version</a> from any browser on any platform.</p>
	</section>

	<!-- ── Source ─────────────────────────────── -->
	<section class="dl-source">
		<h2 class="dl-section-title">Open Source</h2>
		<p>
			Maze Escape: Pathbound is open source. The full source code is available on
			<a href="https://github.com/JJFrisch/MazeEscape" target="_blank" rel="noopener noreferrer" class="dl-link">GitHub</a>
			under the repository <code>JJFrisch/MazeEscape</code>. Issues, pull requests, and feedback
			are welcome.
		</p>
		<p>
			This project is a solo effort by
			<a href="https://jakefrischman.me" target="_blank" rel="noopener noreferrer" class="dl-link">Jake Frischmann</a>.
			If you enjoy the game, share it or leave a star on the repo.
		</p>
	</section>

</div>

<style>
	.download-page {
		max-width: 800px;
		margin: 0 auto;
		padding: var(--space-4);
		display: flex;
		flex-direction: column;
		gap: var(--space-14);
	}

	/* ── Hero ──────────────────────────────────── */
	.dl-hero {
		display: flex;
		flex-direction: column;
		align-items: center;
		text-align: center;
		padding: var(--space-12) var(--space-4);
		background: linear-gradient(160deg, #030b1a 0%, #061226 100%);
		border: 1px solid rgba(56, 189, 248, 0.15);
		border-radius: var(--radius-xl);
	}

	.dl-hero-badge {
		display: inline-block;
		padding: var(--space-1) var(--space-3);
		background: rgba(56, 189, 248, 0.1);
		border: 1px solid rgba(56, 189, 248, 0.35);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		color: #93c5fd;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		margin-bottom: var(--space-5);
	}

	.dl-title {
		font-family: var(--font-display);
		font-size: clamp(2rem, 6vw, 3.5rem);
		font-weight: 700;
		color: #f0f6ff;
		letter-spacing: -0.02em;
		line-height: 1.1;
		margin-bottom: var(--space-4);
	}

	.dl-subtitle {
		font-size: var(--text-base);
		color: #94a3b8;
		max-width: 520px;
		line-height: 1.7;
		margin-bottom: var(--space-3);
	}

	.dl-byline {
		font-size: var(--text-sm);
		color: rgba(148, 163, 184, 0.6);
		margin-bottom: var(--space-8);
	}

	.dl-author-link {
		color: #d97706;
		text-decoration: none;
		transition: color var(--transition-fast);
	}

	.dl-author-link:hover {
		color: #fbbf24;
		text-decoration: underline;
	}

	.btn-download {
		display: inline-flex;
		align-items: center;
		gap: var(--space-3);
		padding: var(--space-4) var(--space-8);
		background: var(--color-accent-primary);
		color: #fff;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-lg);
		text-decoration: none;
		box-shadow: 0 0 28px rgba(21, 101, 192, 0.5);
		transition: all var(--transition-base);
		margin-bottom: var(--space-4);
	}

	.btn-download:hover {
		background: #1976d2;
		transform: translateY(-2px);
		box-shadow: 0 0 40px rgba(21, 101, 192, 0.7);
		color: #fff;
	}

	.btn-icon {
		width: 22px;
		height: 22px;
		flex-shrink: 0;
	}

	.dl-note {
		font-size: var(--text-xs);
		color: rgba(148, 163, 184, 0.5);
		margin-top: var(--space-2);
	}

	code {
		background: rgba(255, 255, 255, 0.08);
		border: 1px solid rgba(255, 255, 255, 0.12);
		border-radius: var(--radius-sm);
		padding: 1px 6px;
		font-size: 0.9em;
	}

	/* ── Features ──────────────────────────────── */
	.dl-section-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		margin-bottom: var(--space-6);
	}

	.features-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
		gap: var(--space-4);
	}

	.feature-card {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		padding: var(--space-5);
	}

	.feature-card h3 {
		font-family: var(--font-display);
		font-size: var(--text-base);
		font-weight: 600;
		margin: var(--space-2) 0 var(--space-2);
	}

	.feature-card p {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.6;
	}

	.feature-icon {
		font-size: 1.8rem;
	}

	/* ── Requirements ──────────────────────────── */
	.req-table {
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		overflow: hidden;
		margin-bottom: var(--space-4);
	}

	.req-row {
		display: flex;
		gap: var(--space-4);
		padding: var(--space-3) var(--space-5);
		border-bottom: 1px solid var(--color-border);
		font-size: var(--text-sm);
	}

	.req-row:last-child {
		border-bottom: none;
	}

	.req-label {
		width: 140px;
		font-weight: 600;
		color: var(--color-text-secondary);
		flex-shrink: 0;
	}

	.req-value {
		color: var(--color-text-primary);
	}

	.req-note {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.6;
	}

	/* ── Source ────────────────────────────────── */
	.dl-source p {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.7;
		margin-bottom: var(--space-3);
	}

	.dl-link {
		color: var(--color-accent-primary);
		text-decoration: none;
	}

	.dl-link:hover {
		text-decoration: underline;
	}
</style>
