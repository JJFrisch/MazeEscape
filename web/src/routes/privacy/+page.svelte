<svelte:head>
	<title>Privacy Policy – Maze Escape: Pathbound</title>
</svelte:head>

<div class="policy-page">
	<h1 class="page-title">Privacy Policy</h1>
	<p class="page-meta">Effective date: April 9, 2026 &middot; Last updated: April 9, 2026</p>

	<p class="intro">
		This Privacy Policy describes how <strong>Maze Escape: Pathbound</strong> (available at
		<a href="https://mazeescapepathbound.com" class="policy-link">mazeescapepathbound.com</a>),
		created and maintained by
		<a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer" class="policy-link">Jake Frischmann</a>,
		handles information about visitors and players.
	</p>

	<section class="policy-section">
		<h2>1. Information We Collect</h2>
		<p>
			<strong>Account data.</strong> If you choose to create an account, we collect your email address
			via Supabase (our authentication and database provider). You may also optionally set a player
			display name stored in the database.
		</p>
		<p>
			<strong>Game progress.</strong> Level completions, star counts, coin balances, and equipped
			skins are stored server-side when you are signed in so your progress can sync across devices.
		</p>
		<p>
			<strong>Local storage.</strong> When you play without an account, game progress is stored
			entirely in your browser&apos;s local storage and never leaves your device.
		</p>
		<p>
			<strong>Usage data.</strong> We do not operate any analytics or advertising services. No
			tracking pixels, cookies beyond session tokens, or third-party ad networks are used on this
			site.
		</p>
	</section>

	<section class="policy-section">
		<h2>2. How We Use Your Information</h2>
		<p>
			Your email address is used solely to authenticate your account. Your game data is used to
			display your progress in the game and, when signed in, to restore that progress on other
			devices. We do not sell, rent, or share your data with any third parties for marketing
			purposes.
		</p>
	</section>

	<section class="policy-section">
		<h2>3. Third-Party Services</h2>
		<p>
			This site uses <strong>Supabase</strong> for account authentication and data persistence.
			Supabase operates under its own
			<a href="https://supabase.com/privacy" target="_blank" rel="noopener noreferrer" class="policy-link">Privacy Policy</a>.
			No other third-party services with data-collection capabilities are integrated into this site.
		</p>
		<p>
			Fonts are loaded from Google Fonts. Google may collect usage data per their own policies.
		</p>
	</section>

	<section class="policy-section">
		<h2>4. Data Retention</h2>
		<p>
			Account and game data is retained as long as your account exists. You may delete your account
			at any time by contacting us (see below), at which point all associated data will be removed.
		</p>
	</section>

	<section class="policy-section">
		<h2>5. Children&apos;s Privacy</h2>
		<p>
			This game is intended for general audiences. We do not knowingly collect personal information
			from children under 13. If you believe a child has provided us with personal data, please
			contact us and we will delete it promptly.
		</p>
	</section>

	<section class="policy-section">
		<h2>6. Changes to This Policy</h2>
		<p>
			We may update this policy from time to time. The effective date at the top of this page will
			reflect the most recent revision. Continued use of the site after changes constitutes
			acceptance of the updated policy.
		</p>
	</section>

	<section class="policy-section">
		<h2>7. Contact</h2>
		<p>
			Questions about this Privacy Policy can be directed to Jake Frischmann via
			<a href="https://jakefrischmann.me" target="_blank" rel="noopener noreferrer" class="policy-link">jakefrischmann.me</a>.
		</p>
	</section>
</div>

<style>
	.policy-page {
		max-width: 680px;
		margin: 0 auto;
		padding: var(--space-4);
	}

	.page-title {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-2);
	}

	.page-meta {
		font-size: var(--text-xs);
		color: var(--color-text-muted);
		margin-bottom: var(--space-8);
	}

	.intro {
		font-size: var(--text-base);
		color: var(--color-text-secondary);
		line-height: 1.7;
		margin-bottom: var(--space-10);
		padding-bottom: var(--space-6);
		border-bottom: 1px solid var(--color-border);
	}

	.policy-section {
		margin-bottom: var(--space-8);
	}

	.policy-section h2 {
		font-family: var(--font-display);
		font-size: var(--text-lg);
		margin-bottom: var(--space-3);
	}

	.policy-section p {
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		line-height: 1.7;
		margin-bottom: var(--space-3);
	}

	.policy-link {
		color: var(--color-accent-primary);
		text-decoration: none;
	}

	.policy-link:hover {
		text-decoration: underline;
	}
</style>
