<script lang="ts">
	import { goto } from '$app/navigation';
	import { base } from '$app/paths';
	import { gameStore } from '$lib/stores/gameStore.svelte';
	import { authStore } from '$lib/supabase/authStore.svelte';

	let mode = $state<'sign-in' | 'sign-up'>('sign-in');
	let email = $state('');
	let password = $state('');
	let confirmPassword = $state('');
	let localError = $state('');

	async function submit() {
		localError = '';
		authStore.clearMessages();

		if (!email.trim() || !password.trim()) {
			localError = 'Email and password are required.';
			return;
		}

		if (mode === 'sign-up' && password !== confirmPassword) {
			localError = 'Passwords do not match.';
			return;
		}

		const success = mode === 'sign-in'
			? await authStore.signIn(email.trim(), password)
			: await authStore.signUp(email.trim(), password);

		if (success && authStore.isAuthenticated) {
			await goto(`${base}/settings`);
		}
	}

	async function signOut() {
		const success = await authStore.signOut();
		if (success) {
			email = '';
			password = '';
			confirmPassword = '';
		}
	}
</script>

<svelte:head>
	<title>Account - MazeEscape</title>
</svelte:head>

<section class="auth-page">
	<div class="auth-card">
		<h1>{authStore.isAuthenticated ? 'Account' : mode === 'sign-in' ? 'Sign In' : 'Create Account'}</h1>
		<p class="subtitle">
			{#if authStore.isAuthenticated}
				Signed in and syncing progress to Supabase.
			{:else}
				Sign in to sync progress, skins, coins, and daily results across devices.
			{/if}
		</p>

		{#if authStore.notice}
			<p class="notice">{authStore.notice}</p>
		{/if}
		{#if localError || authStore.error}
			<p class="error">{localError || authStore.error}</p>
		{/if}

		{#if authStore.isAuthenticated && authStore.user}
			<div class="account-summary">
				<div>
					<span class="label">Email</span>
					<strong>{authStore.user.email}</strong>
				</div>
				<div>
					<span class="label">Sync</span>
					<strong>{gameStore.cloudSyncStatusLabel}</strong>
				</div>
			</div>
			<div class="actions">
				<a class="secondary-link" href="{base}/settings">Manage settings</a>
				<button class="primary-btn" onclick={signOut} disabled={authStore.loading}>Sign Out</button>
			</div>
		{:else}
			<div class="mode-toggle" role="tablist" aria-label="Authentication mode">
				<button class:active={mode === 'sign-in'} onclick={() => mode = 'sign-in'}>Sign In</button>
				<button class:active={mode === 'sign-up'} onclick={() => mode = 'sign-up'}>Sign Up</button>
			</div>

			<form class="auth-form" onsubmit={(event) => { event.preventDefault(); void submit(); }}>
				<label for="auth-email">Email</label>
				<input id="auth-email" type="email" bind:value={email} autocomplete="email" />

				<label for="auth-password">Password</label>
				<input id="auth-password" type="password" bind:value={password} autocomplete={mode === 'sign-in' ? 'current-password' : 'new-password'} />

				{#if mode === 'sign-up'}
					<label for="auth-confirm-password">Confirm Password</label>
					<input id="auth-confirm-password" type="password" bind:value={confirmPassword} autocomplete="new-password" />
				{/if}

				<button class="primary-btn" type="submit" disabled={authStore.loading}>
					{authStore.loading ? 'Working...' : mode === 'sign-in' ? 'Sign In' : 'Create Account'}
				</button>
			</form>
		{/if}
	</div>
</section>

<style>
	.auth-page {
		display: grid;
		place-items: center;
		padding: var(--space-8) var(--space-4);
	}

	.auth-card {
		width: min(100%, 32rem);
		padding: var(--space-8);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-xl);
		background: var(--color-bg-card);
	}

	h1 {
		font-family: var(--font-display);
		font-size: var(--text-2xl);
		margin-bottom: var(--space-2);
	}

	.subtitle {
		color: var(--color-text-secondary);
		margin-bottom: var(--space-4);
	}

	.notice,
	.error {
		padding: var(--space-3);
		border-radius: var(--radius-md);
		margin-bottom: var(--space-4);
	}

	.notice {
		background: rgba(52, 211, 153, 0.15);
		color: var(--color-accent-green);
	}

	.error {
		background: rgba(239, 68, 68, 0.15);
		color: var(--color-accent-red);
	}

	.mode-toggle {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: var(--space-2);
		margin-bottom: var(--space-4);
	}

	.mode-toggle button,
	.primary-btn {
		padding: var(--space-3) var(--space-4);
		border-radius: var(--radius-md);
		font-weight: 600;
		cursor: pointer;
	}

	.mode-toggle button {
		background: var(--color-bg-secondary);
		border: 1px solid var(--color-border);
		color: var(--color-text-secondary);
	}

	.mode-toggle button.active {
		background: var(--color-accent-primary);
		border-color: var(--color-accent-primary);
		color: white;
	}

	.auth-form {
		display: grid;
		gap: var(--space-3);
	}

	label,
	.label {
		font-size: var(--text-sm);
		color: var(--color-text-muted);
	}

	input {
		width: 100%;
		padding: var(--space-3);
		border-radius: var(--radius-md);
		border: 1px solid var(--color-border);
		background: var(--color-bg-secondary);
		color: var(--color-text-primary);
	}

	.primary-btn {
		background: var(--color-accent-primary);
		border: none;
		color: white;
		margin-top: var(--space-2);
	}

	.primary-btn:disabled {
		opacity: 0.6;
		cursor: default;
	}

	.account-summary {
		display: grid;
		gap: var(--space-4);
		padding: var(--space-4);
		border-radius: var(--radius-lg);
		background: var(--color-bg-secondary);
		margin-bottom: var(--space-4);
	}

	.actions {
		display: flex;
		justify-content: space-between;
		align-items: center;
		gap: var(--space-3);
	}

	.secondary-link {
		color: var(--color-accent-secondary);
	}
</style>