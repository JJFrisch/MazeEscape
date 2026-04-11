<script lang="ts">
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { onMount } from 'svelte';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';

	type AuthMode = 'sign-in' | 'sign-up' | 'reset-request' | 'recovery';

	let mode = $state<AuthMode>('sign-in');
	let email = $state('');
	let password = $state('');
	let confirmPassword = $state('');
	let localError = $state('');

	onMount(() => {
		const hash = new URLSearchParams(window.location.hash.replace(/^#/, ''));
		const search = new URLSearchParams(window.location.search);
		const errorDescription = hash.get('error_description') ?? search.get('error_description');
		if (errorDescription) {
			localError = decodeURIComponent(errorDescription.replace(/\+/g, ' '));
		}
	});

	$effect(() => {
		if (authStore.recoveryMode) mode = 'recovery';
	});

	async function submit() {
		localError = '';
		authStore.clearMessages();
		if (mode === 'reset-request') {
			if (!email.trim()) { localError = 'Email is required.'; return; }
			await authStore.requestPasswordReset(email.trim());
			return;
		}
		if (mode === 'recovery') {
			if (!password.trim()) { localError = 'A new password is required.'; return; }
			if (password !== confirmPassword) { localError = 'Passwords do not match.'; return; }
			const success = await authStore.updatePassword(password);
			if (success && authStore.isAuthenticated) await goto(`${base}/settings`);
			return;
		}
		if (!email.trim() || !password.trim()) { localError = 'Email and password are required.'; return; }
		if (mode === 'sign-up' && password !== confirmPassword) { localError = 'Passwords do not match.'; return; }
		const success = mode === 'sign-in'
			? await authStore.signIn(email.trim(), password)
			: mode === 'sign-up' ? await authStore.signUp(email.trim(), password) : false;
		if (success && authStore.isAuthenticated) await goto(`${base}/settings`);
	}

	async function signOut() {
		const success = await authStore.signOut();
		if (success) { mode = 'sign-in'; email = ''; password = ''; confirmPassword = ''; }
	}

	const pageTitle = $derived(
		authStore.isAuthenticated ? 'Account' :
		mode === 'sign-up' ? 'Create Account' :
		mode === 'reset-request' ? 'Reset Password' :
		mode === 'recovery' ? 'New Password' : 'Sign In'
	);
	const pageSubtitle = $derived(
		authStore.isAuthenticated ? 'Signed in and syncing your progress.' :
		mode === 'reset-request' ? "We'll email you a link to reset your password." :
		mode === 'recovery' ? 'Set a new password for your account.' :
		'Sync progress, skins, coins, and daily results across all your devices.'
	);
</script>

<svelte:head>
	<title>Account – Maze Escape: Pathbound</title>
</svelte:head>

<div class="auth-shell">
	<!-- Background decorations -->
	<div class="bg-glow-1" aria-hidden="true"></div>
	<div class="bg-glow-2" aria-hidden="true"></div>

	<div class="auth-card">
		<!-- Card header -->
		<div class="card-top">
			<div class="card-logo" aria-hidden="true">
				<svg width="22" height="22" viewBox="0 0 28 28" fill="none">
					<rect x="1.5" y="1.5" width="25" height="25" rx="4" stroke="currentColor" stroke-width="2"/>
					<path d="M7 7h14v5H12v4h9v5H7v-5h5v-4H7z" fill="none" stroke="currentColor" stroke-width="2" stroke-linejoin="round" stroke-linecap="round"/>
					<circle cx="19" cy="20" r="2" fill="var(--color-accent-gold)"/>
				</svg>
			</div>
			<div>
				<h1 class="card-title">{pageTitle}</h1>
				<p class="card-subtitle">{pageSubtitle}</p>
			</div>
		</div>

		<!-- Notices -->
		{#if authStore.notice}
			<div class="notice-banner">
				<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<path d="M5 13l4 4L19 7" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"/>
				</svg>
				{authStore.notice}
			</div>
		{/if}
		{#if localError || authStore.error}
			<div class="error-banner">
				<svg width="14" height="14" viewBox="0 0 24 24" fill="none" aria-hidden="true">
					<circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
					<path d="M12 8v4M12 16v.01" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
				</svg>
				{localError || authStore.error}
			</div>
		{/if}

		<!-- Authenticated state -->
		{#if authStore.isAuthenticated && authStore.user}
			<div class="account-info">
				<div class="account-row">
					<span class="info-label">Email</span>
					<span class="info-val">{authStore.user.email}</span>
				</div>
				<div class="account-row">
					<span class="info-label">Sync Status</span>
					<span class="info-val sync-status">{gameStore.cloudSyncStatusLabel}</span>
				</div>
			</div>
			<div class="auth-actions">
				<a class="ghost-btn" href="{base}/settings">Manage Settings</a>
				<button class="primary-btn" onclick={signOut} disabled={authStore.loading}>Sign Out</button>
			</div>

		<!-- Auth form -->
		{:else}
			{#if mode === 'sign-in' || mode === 'sign-up'}
				<div class="mode-tabs" role="tablist" aria-label="Sign in or sign up">
					<button
						class="mode-tab"
						class:active={mode === 'sign-in'}
						role="tab"
						aria-selected={mode === 'sign-in'}
						onclick={() => mode = 'sign-in'}
					>Sign In</button>
					<button
						class="mode-tab"
						class:active={mode === 'sign-up'}
						role="tab"
						aria-selected={mode === 'sign-up'}
						onclick={() => mode = 'sign-up'}
					>Sign Up</button>
				</div>
			{/if}

			<form class="auth-form" onsubmit={(e) => { e.preventDefault(); void submit(); }}>
				{#if mode !== 'recovery'}
					<div class="field">
						<label class="field-label" for="auth-email">Email address</label>
						<input
							id="auth-email"
							class="field-input"
							type="email"
							bind:value={email}
							autocomplete="email"
							placeholder="you@example.com"
						/>
					</div>
				{/if}

				{#if mode !== 'reset-request'}
					<div class="field">
						<label class="field-label" for="auth-password">
							{mode === 'recovery' ? 'New password' : 'Password'}
						</label>
						<input
							id="auth-password"
							class="field-input"
							type="password"
							bind:value={password}
							autocomplete={mode === 'sign-in' ? 'current-password' : 'new-password'}
							placeholder="••••••••"
						/>
					</div>
				{/if}

				{#if mode === 'sign-up' || mode === 'recovery'}
					<div class="field">
						<label class="field-label" for="auth-confirm">Confirm password</label>
						<input
							id="auth-confirm"
							class="field-input"
							type="password"
							bind:value={confirmPassword}
							autocomplete="new-password"
							placeholder="••••••••"
						/>
					</div>
				{/if}

				<button class="submit-btn" type="submit" disabled={authStore.loading}>
					{#if authStore.loading}
						<span class="loading-dots">Working</span>
					{:else if mode === 'sign-in'}
						Sign In
					{:else if mode === 'sign-up'}
						Create Account
					{:else if mode === 'reset-request'}
						Send Reset Email
					{:else}
						Update Password
					{/if}
				</button>

				{#if mode === 'sign-in'}
					<button class="text-btn" type="button" onclick={() => mode = 'reset-request'}>
						Forgot your password?
					</button>
				{:else if mode === 'reset-request'}
					<button class="text-btn" type="button" onclick={() => { mode = 'sign-in'; authStore.clearMessages(); }}>
						← Back to sign in
					</button>
				{/if}
			</form>
		{/if}
	</div>
</div>

<style>
	@keyframes fade-up { from { opacity:0; transform:translateY(20px); } to { opacity:1; transform:translateY(0); } }
	@keyframes loading { 0%,100%{opacity:1;} 50%{opacity:0.4;} }

	.auth-shell {
		position: relative;
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 60vh;
		padding: var(--space-8) var(--space-4);
	}

	/* Background glows */
	.bg-glow-1 {
		position: fixed;
		width: 600px; height: 500px;
		top: -100px; left: -150px;
		background: radial-gradient(ellipse, rgba(56,189,248,0.06), transparent 65%);
		filter: blur(80px);
		pointer-events: none;
		z-index: 0;
	}
	.bg-glow-2 {
		position: fixed;
		width: 500px; height: 400px;
		bottom: -80px; right: -100px;
		background: radial-gradient(ellipse, rgba(245,158,11,0.05), transparent 65%);
		filter: blur(80px);
		pointer-events: none;
		z-index: 0;
	}

	/* ── Card ───────────────────────────────────── */
	.auth-card {
		position: relative;
		z-index: 1;
		width: min(100%, 26rem);
		padding: var(--space-8);
		background: var(--color-bg-card);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-2xl);
		backdrop-filter: blur(24px);
		box-shadow: var(--shadow-card);
		display: flex;
		flex-direction: column;
		gap: var(--space-5);
		animation: fade-up 0.4s ease both;
	}

	/* ── Card top ───────────────────────────────── */
	.card-top {
		display: flex;
		align-items: flex-start;
		gap: var(--space-3);
		padding-bottom: var(--space-4);
		border-bottom: 1px solid var(--color-border-subtle);
	}
	.card-logo {
		width: 44px; height: 44px;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(56,189,248,0.1);
		border: 1px solid rgba(56,189,248,0.25);
		border-radius: var(--radius-xl);
		color: var(--color-accent-primary);
		flex-shrink: 0;
	}
	.card-title {
		font-family: var(--font-display);
		font-size: var(--text-xl);
		font-weight: 700;
		color: var(--color-text-primary);
		margin-bottom: 3px;
	}
	.card-subtitle {
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		line-height: 1.5;
	}

	/* ── Banners ────────────────────────────────── */
	.notice-banner, .error-banner {
		display: flex;
		align-items: flex-start;
		gap: var(--space-2);
		padding: var(--space-3) var(--space-4);
		border-radius: var(--radius-lg);
		font-size: var(--text-sm);
		line-height: 1.5;
	}
	.notice-banner {
		background: rgba(16,185,129,0.1);
		border: 1px solid rgba(16,185,129,0.3);
		color: var(--color-accent-green);
	}
	.error-banner {
		background: rgba(244,63,94,0.1);
		border: 1px solid rgba(244,63,94,0.3);
		color: var(--color-accent-red);
	}

	/* ── Account info ───────────────────────────── */
	.account-info {
		background: var(--color-bg-glass);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		overflow: hidden;
	}
	.account-row {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 11px var(--space-4);
		font-size: var(--text-sm);
		border-bottom: 1px solid var(--color-border-subtle);
	}
	.account-row:last-child { border-bottom: none; }
	.info-label { color: var(--color-text-secondary); font-weight: 500; }
	.info-val { color: var(--color-text-primary); font-weight: 600; }
	.sync-status { color: var(--color-accent-primary); }

	.auth-actions {
		display: flex;
		gap: var(--space-2);
		flex-wrap: wrap;
	}

	/* ── Mode tabs ──────────────────────────────── */
	.mode-tabs {
		display: flex;
		gap: 4px;
		background: var(--color-bg-glass);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		padding: 4px;
	}
	.mode-tab {
		flex: 1;
		padding: 8px var(--space-3);
		border: none;
		border-radius: calc(var(--radius-lg) - 2px);
		font-size: var(--text-sm);
		font-weight: 600;
		cursor: pointer;
		color: var(--color-text-secondary);
		background: transparent;
		transition: all var(--transition-fast);
	}
	.mode-tab:hover:not(.active) { color: var(--color-text-primary); background: var(--color-bg-glass-hover); }
	.mode-tab.active {
		background: var(--color-accent-primary);
		color: #fff;
		box-shadow: 0 0 12px rgba(56,189,248,0.3);
	}

	/* ── Form ───────────────────────────────────── */
	.auth-form {
		display: flex;
		flex-direction: column;
		gap: var(--space-3);
	}

	.field {
		display: flex;
		flex-direction: column;
		gap: var(--space-1);
	}
	.field-label {
		font-size: var(--text-sm);
		font-weight: 500;
		color: var(--color-text-secondary);
	}
	.field-input {
		width: 100%;
		padding: 11px var(--space-4);
		border-radius: var(--radius-lg);
		border: 1px solid var(--color-border);
		background: var(--color-bg-glass);
		color: var(--color-text-primary);
		font-size: var(--text-base);
		font-family: var(--font-body);
		transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
	}
	.field-input:focus {
		outline: none;
		border-color: var(--color-accent-primary);
		box-shadow: 0 0 0 3px var(--color-accent-glow);
	}
	.field-input::placeholder { color: var(--color-text-muted); }

	/* Buttons */
	.submit-btn {
		margin-top: var(--space-1);
		width: 100%;
		padding: 13px;
		background: var(--color-accent-primary);
		color: #fff;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-base);
		cursor: pointer;
		transition: all var(--transition-fast);
		box-shadow: 0 0 24px rgba(56,189,248,0.25);
	}
	.submit-btn:hover:not(:disabled) { background: #0ea5e9; box-shadow: 0 0 36px rgba(56,189,248,0.4); }
	.submit-btn:disabled { opacity: 0.45; cursor: not-allowed; box-shadow: none; }

	.text-btn {
		background: none;
		border: none;
		color: var(--color-text-secondary);
		font-size: var(--text-sm);
		cursor: pointer;
		padding: var(--space-1);
		text-align: center;
		transition: color var(--transition-fast);
	}
	.text-btn:hover { color: var(--color-accent-primary); }

	.primary-btn {
		padding: 11px 20px;
		background: var(--color-accent-primary);
		color: #fff;
		border: none;
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 700;
		font-size: var(--text-sm);
		cursor: pointer;
		transition: all var(--transition-fast);
	}
	.primary-btn:hover:not(:disabled) { background: #0ea5e9; }
	.primary-btn:disabled { opacity: 0.45; cursor: not-allowed; }

	.ghost-btn {
		padding: 11px 20px;
		background: var(--color-bg-glass);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-lg);
		font-family: var(--font-display);
		font-weight: 600;
		font-size: var(--text-sm);
		color: var(--color-text-secondary);
		cursor: pointer;
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		transition: all var(--transition-fast);
	}
	.ghost-btn:hover { background: var(--color-bg-glass-hover); color: var(--color-text-primary); }

	.loading-dots { animation: loading 1.2s ease-in-out infinite; }
</style>
