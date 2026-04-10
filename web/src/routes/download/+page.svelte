<svelte:head>
	<title>Download – Maze Escape: Pathbound</title>
</svelte:head>

<div class="download-page">

	<!-- ── Hero ───────────────────────────────────── -->
	<section class="dl-hero">
		<!-- Nebula decorations -->
		<div class="hero-glow-1" aria-hidden="true"></div>
		<div class="hero-glow-2" aria-hidden="true"></div>
		<div class="hero-grid" aria-hidden="true"></div>

		<div class="hero-content">
			<div class="hero-badge">
				<span class="badge-dot"></span>
				Desktop Application
			</div>

			<h1 class="dl-title">
				Maze Escape:<br/>
				<span class="dl-title-sub">Pathbound</span>
			</h1>

			<p class="dl-subtitle">
				Play the full game natively on your machine — complete campaign, offline play,
				and all maze types with no browser required.
			</p>

			<p class="dl-byline">
				Developed and maintained by
				<a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer" class="author-link">Jake Frischmann</a>
			</p>

			<a
				href="https://github.com/JJFrisch/MazeEscape/releases"
				target="_blank"
				rel="noopener noreferrer"
				class="btn-download"
			>
				<svg class="btn-icon" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<path d="M12 3v12M7 11l5 5 5-5" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
					<rect x="3" y="18" width="18" height="3" rx="1.5" stroke="currentColor" stroke-width="2"/>
				</svg>
				Download from GitHub Releases
				<svg class="btn-arrow" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<path d="M7 17L17 7M17 7H7M17 7v10" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
				</svg>
			</a>

			<p class="dl-note">
				Grab the latest <code>.exe</code> or <code>.zip</code> installer from GitHub Releases.
			</p>
		</div>
	</section>

	<!-- ── Feature cards ──────────────────────────── -->
	<section class="dl-section">
		<div class="section-label">What's Included</div>
		<h2 class="section-title">Everything in one download</h2>
		<div class="features-grid">
			{#each [
				{ icon: '🗺️', title: 'Full Campaign', desc: 'All worlds and levels, including bonus stages and multi-shape maze types.' },
				{ icon: '📶', title: 'Offline Play',  desc: 'No internet required. Progress is saved locally to your machine.' },
				{ icon: '🔷', title: 'All Maze Shapes', desc: 'Rectangular, hexagonal, circular, and triangular mazes across the campaign.' },
				{ icon: '🏆', title: 'Star System',   desc: 'Earn up to 3 stars per level based on move count and completion time.' },
			] as feat, i}
				<div class="feature-card" style="--i: {i}">
					<div class="feat-icon-wrap">
						<span role="img" aria-hidden="true">{feat.icon}</span>
					</div>
					<h3 class="feat-title">{feat.title}</h3>
					<p class="feat-desc">{feat.desc}</p>
				</div>
			{/each}
		</div>
	</section>

	<!-- ── Requirements + Source ──────────────────── -->
	<div class="lower-grid">
		<section class="dl-card">
			<div class="card-header-row">
				<div class="card-icon">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<rect x="2" y="3" width="20" height="14" rx="2" stroke="currentColor" stroke-width="2"/>
						<path d="M8 21h8M12 17v4" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
					</svg>
				</div>
				<h2 class="card-title">System Requirements</h2>
			</div>
			<div class="req-table">
				{#each [
					{ label: 'Platform',      val: 'Windows 10 / 11' },
					{ label: 'Runtime',       val: '.NET 8 (MAUI)' },
					{ label: 'Architecture',  val: 'x64' },
					{ label: 'Storage',       val: '~50 MB' },
				] as req}
					<div class="req-row">
						<span class="req-label">{req.label}</span>
						<span class="req-val">{req.val}</span>
					</div>
				{/each}
			</div>
			<p class="req-note">
				macOS & Linux builds are not currently available.
				Play the <a href="/">web version</a> from any browser.
			</p>
		</section>

		<section class="dl-card">
			<div class="card-header-row">
				<div class="card-icon">
					<svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden="true">
						<path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 00-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0020 4.77 5.07 5.07 0 0019.91 1S18.73.65 16 2.48a13.38 13.38 0 00-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 005 4.77a5.44 5.44 0 00-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 009 18.13V22" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
					</svg>
				</div>
				<h2 class="card-title">Open Source</h2>
			</div>
			<p class="card-body-text">
				Maze Escape: Pathbound is fully open source.
				Browse the code, report issues, or submit pull requests on GitHub.
			</p>
			<div class="source-actions">
				<a
					href="https://github.com/JJFrisch/MazeEscape"
					target="_blank"
					rel="noopener noreferrer"
					class="source-btn"
				>
					<svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
						<path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 00-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0020 4.77 5.07 5.07 0 0019.91 1S18.73.65 16 2.48a13.38 13.38 0 00-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 005 4.77a5.44 5.44 0 00-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 009 18.13V22"/>
					</svg>
					JJFrisch/MazeEscape
				</a>
			</div>
			<p class="card-body-text small">
				A solo project by <a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer">Jake Frischmann</a>.
				If you enjoy the game, leave a ⭐ on the repo!
			</p>
		</section>
	</div>

</div>

<style>
	@keyframes fade-up { from { opacity:0; transform:translateY(20px); } to { opacity:1; transform:translateY(0); } }
	@keyframes card-rise { from { opacity:0; transform:translateY(24px); } to { opacity:1; transform:translateY(0); } }
	@keyframes badge-pulse { 0%,100%{opacity:1;transform:scale(1);} 50%{opacity:0.4;transform:scale(0.85);} }
	@keyframes ring-pulse { 0%{transform:translate(-50%,-50%) scale(1);opacity:0.5;} 100%{transform:translate(-50%,-50%) scale(2.2);opacity:0;} }

	.download-page {
		max-width: 860px;
		margin: 0 auto;
		display: flex;
		flex-direction: column;
		gap: var(--space-10);
	}

	/* ── Hero ───────────────────────────────────── */
	.dl-hero {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
		text-align: center;
		padding: 72px var(--space-8) 64px;
		background: linear-gradient(160deg, rgba(6,13,26,1) 0%, rgba(8,18,40,1) 100%);
		border: 1px solid rgba(56,189,248,0.15);
		border-radius: var(--radius-2xl);
		overflow: hidden;
		animation: fade-up 0.5s ease both;
	}

	.hero-glow-1 {
		position: absolute;
		width: 500px; height: 350px;
		top: -120px; left: -80px;
		background: radial-gradient(ellipse, rgba(56,189,248,0.1), transparent 70%);
		filter: blur(60px);
		pointer-events: none;
	}
	.hero-glow-2 {
		position: absolute;
		width: 400px; height: 300px;
		bottom: -100px; right: -60px;
		background: radial-gradient(ellipse, rgba(245,158,11,0.09), transparent 70%);
		filter: blur(60px);
		pointer-events: none;
	}
	.hero-grid {
		position: absolute;
		inset: 0;
		background-image:
			linear-gradient(rgba(56,189,248,0.03) 1px, transparent 1px),
			linear-gradient(90deg, rgba(56,189,248,0.03) 1px, transparent 1px);
		background-size: 32px 32px;
		pointer-events: none;
	}

	.hero-content {
		position: relative;
		z-index: 1;
		max-width: 600px;
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: var(--space-3);
	}

	.hero-badge {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 6px 16px;
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.35);
		border-radius: var(--radius-full);
		font-size: var(--text-xs);
		font-weight: 700;
		color: #93c5fd;
		letter-spacing: 0.10em;
		text-transform: uppercase;
		margin-bottom: var(--space-2);
	}
	.badge-dot {
		width: 6px; height: 6px;
		background: var(--color-accent-primary);
		border-radius: 50%;
		box-shadow: 0 0 8px var(--color-accent-primary);
		animation: badge-pulse 2s ease-in-out infinite;
	}

	.dl-title {
		font-family: var(--font-display);
		font-size: clamp(2.2rem, 6vw, 3.8rem);
		font-weight: 700;
		color: #f0f6ff;
		letter-spacing: -0.03em;
		line-height: 1.1;
		margin-bottom: var(--space-2);
	}
	.dl-title-sub {
		color: var(--color-accent-gold);
		text-shadow: 0 0 32px rgba(245,158,11,0.45);
	}

	.dl-subtitle {
		font-size: var(--text-base);
		color: #94a3b8;
		max-width: 480px;
		line-height: 1.7;
	}

	.dl-byline {
		font-size: var(--text-sm);
		color: rgba(148,163,184,0.55);
		margin-bottom: var(--space-2);
	}
	.author-link {
		color: var(--color-accent-gold);
		transition: color var(--transition-fast);
	}
	.author-link:hover { color: #fcd34d; }

	.btn-download {
		position: relative;
		display: inline-flex;
		align-items: center;
		gap: var(--space-3);
		padding: 16px 36px;
		background: var(--color-accent-primary);
		color: #fff;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		text-decoration: none;
		box-shadow: 0 0 36px rgba(56,189,248,0.35);
		transition: all var(--transition-base);
		overflow: visible;
		margin: var(--space-2) 0;
	}
	.btn-download::before {
		content: '';
		position: absolute;
		top: 50%; left: 50%;
		width: 100%; height: 100%;
		border: 2px solid rgba(56,189,248,0.5);
		border-radius: var(--radius-lg);
		animation: ring-pulse 2.5s ease-out infinite;
		pointer-events: none;
	}
	.btn-download:hover {
		background: #0ea5e9;
		box-shadow: 0 0 52px rgba(56,189,248,0.55);
		transform: translateY(-2px);
		color: #fff;
	}
	.btn-icon { width: 20px; height: 20px; flex-shrink: 0; }
	.btn-arrow { width: 16px; height: 16px; flex-shrink: 0; opacity: 0.7; }

	.dl-note {
		font-size: var(--text-xs);
		color: rgba(148,163,184,0.45);
	}
	code {
		background: rgba(255,255,255,0.08);
		border: 1px solid rgba(255,255,255,0.12);
		border-radius: var(--radius-sm);
		padding: 1px 6px;
		font-size: 0.9em;
		font-family: var(--font-mono);
	}

	/* ── Features section ───────────────────────── */
	.dl-section {
		animation: fade-up 0.5s ease 0.15s both;
	}
	.section-label {
		font-size: var(--text-xs);
		font-weight: 700;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		color: var(--color-accent-primary);
		margin-bottom: var(--space-2);
	}
	.section-title {
		font-family: var(--font-display);
		font-size: clamp(1.4rem, 3vw, 2rem);
		font-weight: 700;
		color: var(--color-text-primary);
		letter-spacing: -0.02em;
		margin-bottom: var(--space-6);
	}

	.features-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
		gap: var(--space-4);
	}

	.feature-card {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border-subtle);
		border-radius: var(--radius-xl);
		padding: var(--space-6) var(--space-5);
		backdrop-filter: blur(12px);
		box-shadow: var(--shadow-card);
		transition: all var(--transition-base);
		animation: card-rise 0.5s ease calc(var(--i, 0) * 80ms) both;
	}
	.feature-card:hover {
		border-color: rgba(56,189,248,0.25);
		transform: translateY(-4px);
		box-shadow: var(--shadow-card), 0 0 24px rgba(56,189,248,0.1);
	}

	.feat-icon-wrap {
		font-size: 2rem;
		margin-bottom: var(--space-3);
		line-height: 1;
	}
	.feat-title {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		color: var(--color-text-primary);
		margin-bottom: var(--space-2);
	}
	.feat-desc {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.6;
	}

	/* ── Lower grid ─────────────────────────────── */
	.lower-grid {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: var(--space-4);
		animation: fade-up 0.5s ease 0.25s both;
	}

	.dl-card {
		background: var(--color-bg-card);
		border: 1px solid var(--color-border-subtle);
		border-radius: var(--radius-xl);
		padding: var(--space-6);
		backdrop-filter: blur(12px);
		box-shadow: var(--shadow-card);
		display: flex;
		flex-direction: column;
		gap: var(--space-4);
	}
	.card-header-row {
		display: flex;
		align-items: center;
		gap: var(--space-3);
	}
	.card-icon {
		width: 38px; height: 38px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(56,189,248,0.08);
		border: 1px solid rgba(56,189,248,0.2);
		border-radius: var(--radius-lg);
		color: var(--color-accent-primary);
		flex-shrink: 0;
	}
	.card-title {
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-lg);
		color: var(--color-text-primary);
	}

	/* Requirements table */
	.req-table {
		border: 1px solid rgba(255,255,255,0.06);
		border-radius: var(--radius-lg);
		overflow: hidden;
	}
	.req-row {
		display: flex;
		gap: var(--space-4);
		padding: 11px var(--space-4);
		font-size: var(--text-sm);
		border-bottom: 1px solid rgba(255,255,255,0.05);
	}
	.req-row:last-child { border-bottom: none; }
	.req-row:nth-child(odd) { background: rgba(255,255,255,0.02); }
	.req-label {
		width: 120px;
		font-weight: 600;
		color: var(--color-text-secondary);
		flex-shrink: 0;
	}
	.req-val { color: var(--color-text-primary); }
	.req-note {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.6;
	}
	.req-note a { color: var(--color-accent-primary); }

	/* Source card */
	.card-body-text {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.7;
	}
	.card-body-text.small { font-size: var(--text-xs); }
	.card-body-text a { color: var(--color-accent-primary); }

	.source-actions { display: flex; }
	.source-btn {
		display: inline-flex;
		align-items: center;
		gap: var(--space-2);
		padding: 10px 18px;
		background: rgba(255,255,255,0.05);
		border: 1px solid rgba(255,255,255,0.12);
		border-radius: var(--radius-lg);
		font-family: var(--font-mono);
		font-size: var(--text-sm);
		font-weight: 600;
		color: var(--color-text-primary);
		text-decoration: none;
		transition: all var(--transition-fast);
	}
	.source-btn:hover {
		background: rgba(255,255,255,0.09);
		border-color: rgba(255,255,255,0.2);
		color: var(--color-text-primary);
	}

	@media (max-width: 640px) {
		.lower-grid { grid-template-columns: 1fr; }
		.dl-hero { padding: 48px var(--space-6) 40px; }
		.btn-download { padding: 14px 24px; font-size: var(--text-base); }
	}
</style>
